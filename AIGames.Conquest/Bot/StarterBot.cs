using AIGames.Conquest.Engine;
using AIGames.Conquest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIGames.Conquest.Bot
{
    /// <summary>
    /// This is the bot that is delivered out-of-the-box. It's stupid, and it's your task to improve it!
    /// </summary>
    public class StarterBot : IGameplayEngine
    {
        public string Name { get; set; }
        public GameState State { get; private set; }
        private readonly Random _random;

        public StarterBot(string name = "StarterBot")
        {
            Name = name;
            _random = new Random(); 
            State = new GameState();            
        }

        /// <summary>
        /// A method used at the start of the game to decide which player start with what Regions. 6 Regions are required to be returned. This example randomly picks 6 regions from the pickable starting Regions given by the engine.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="timeout"></param>
        /// <returns>A list of m (m=6) Regions starting with the most preferred Region and ending with the least preferred Region to start with.</returns>
        public List<Region> GetPreferredStartingRegions(List<Region> pickableStartingRegions, int timeout)
        {
            if (pickableStartingRegions.Count < 6)
            {
                throw new Exception("Not enough regions received to start with.");
            }

            int m = 6;
            List<Region> preferredStartingRegions = new List<Region>();
            for (int i = 0; i < m; i++)
            {
                double rand = _random.NextDouble();
                int r = (int)(rand * pickableStartingRegions.Count);

                var region =  pickableStartingRegions[r];
                
                if (!preferredStartingRegions.Contains(region))
                {
                    preferredStartingRegions.Add(region);
                }
                else
                {
                    i--;
                }
            }

            return preferredStartingRegions;
        }

        /// <summary>
        /// This method is called for at first part of each round. This example puts two armies on random regions until he has no more armies left to place. 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="timeout"></param>
        /// <returns>The list of PlaceArmiesMoves for one round.</returns>
        public List<PlaceArmiesMove> GetPlaceArmiesMoves(int timeout)
        {
            GameState.ResetRegions(State.FullMap.Regions, resetOwnerAndArmies: false);

            List<PlaceArmiesMove> placeArmiesMoves = new List<PlaceArmiesMove>();

            int armies = 2;
            int armiesLeft = State.StartingArmies;
            List<Region> visibleRegions = State.FullMap.Regions;

            if (!visibleRegions.Any(r => r.PlayerName == Name))
            {
                // Can't do anything...
                throw new Exception("I don't believe I have any region to place an army on.");
            }

            if (!visibleRegions.Any(r => r.PlayerName != Name))
            {
                // Can't do anything...
                throw new Exception("I believe I have won!");
            }

            while (armiesLeft > 0)
            {
                double rand = _random.NextDouble();
                int r = (int)(rand * visibleRegions.Count);
                Region region = visibleRegions[r];

                if (region.OwnedByPlayer(Name))
                {
                    placeArmiesMoves.Add(new PlaceArmiesMove(Name, region, Math.Min(armies, armiesLeft)));
                    armiesLeft -= armies;
                }
            }

            // Apply the moves to our own state, because the GetAttackTransferMoves() relies on it and we won't get an update_map in between.
            foreach (var move in placeArmiesMoves)
            {
                var region = State.FullMap.GetRegionByID(move.Region.ID);
                region.Armies += move.Armies;
                region.ArmiesAvailable += move.Armies;
            }

            return placeArmiesMoves;
        }

        /// <summary>
        /// This method is called for at the second part of each round. This example attacks if a region has more than 6 armies on it, and transfers if it has less than 6 and a neighboring owned region.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="timeout"></param>
        /// <returns>The list of PlaceArmiesMoves for one round.</returns>
        public List<AttackTransferMove> GetAttackTransferMoves(int timeout)
        {
            var attackTransferMoves = new List<AttackTransferMove>();

            var regionsByArmySize = State.FullMap.Regions.OrderByDescending(r => r.Armies);

            foreach (Region fromRegion in regionsByArmySize)
            {
                if (!fromRegion.OwnedByPlayer(Name))
                {
                    continue;
                }

                List<Region> possibleToRegions = fromRegion.Neighbors
                                                           .OrderBy(n => n.Armies)
                                                           .ToList();

                while (possibleToRegions.Any())
                {
                    int r = _random.Next(0, possibleToRegions.Count);
                    Region toRegion = possibleToRegions[r];

                    var attackMove = PerhapsGetAttackMove(fromRegion, toRegion, possibleToRegions, attackTransferMoves);
                    if (attackMove != null)
                    {
                        attackTransferMoves.Add(attackMove);
                        break;
                    }

                    var moveMove = PerhapsGetMoveMove(fromRegion, toRegion, possibleToRegions, attackTransferMoves);
                    if (moveMove != null)
                    {
                        attackTransferMoves.Add(moveMove);
                        break;
                    }

                    possibleToRegions.Remove(toRegion);
                }
            }

            return attackTransferMoves;
        }

        /// <summary>
        /// At the end of each turn, you receive a call to this method with the moves the opponent made to and from regions visible to you.
        /// </summary>
        /// <param name="allMoves"></param>
        public void ProcessOpponentMoves(List<Move> allMoves)
        {
        }

        /// <summary>
        /// Evaluates whether a move should occur between to the given region.
        /// </summary>
        /// <param name="fromRegion"></param>
        /// <param name="toRegion"></param>
        /// <param name="possibleToRegions"></param>
        /// <param name="attackTransferMoves"></param>
        /// <returns>null if no move.</returns>
        private AttackTransferMove PerhapsGetMoveMove(Region fromRegion, Region toRegion, List<Region> possibleToRegions, List<AttackTransferMove> attackTransferMoves)
        {
            if (!toRegion.PlayerName.Equals(Name) || fromRegion.Armies <= 1)
            {
                return null;
            }

            int armies = 5;
            return new AttackTransferMove(Name, fromRegion, toRegion, toRegion, Math.Min(fromRegion.Armies - 1, armies), MoveReason.Random);
        }

        /// <summary>
        /// Evaluates whether an attack should happen on the given region.
        /// </summary>
        /// <param name="fromRegion"></param>
        /// <param name="toRegion"></param>
        /// <param name="possibleToRegions"></param>
        /// <param name="attackTransferMoves"></param>
        /// <returns>null if no move.</returns>
        private AttackTransferMove PerhapsGetAttackMove(Region fromRegion, Region toRegion, List<Region> possibleToRegions, List<AttackTransferMove> attackTransferMoves)
        {
            if (fromRegion.PlayerName != Name
              || fromRegion.Armies < 6
              || toRegion.PlayerName == Name)
            {
                return null;
            }

            int difference = fromRegion.Armies - toRegion.Armies;
            if (difference < 0)
            {
                return null;
            }

            int armies = toRegion.Armies + (int)(difference * 1.5);
            armies = Math.Min(armies, fromRegion.Armies - 1);

            return new AttackTransferMove(Name, fromRegion, toRegion, toRegion, armies, MoveReason.Random);
        }
    }
}
