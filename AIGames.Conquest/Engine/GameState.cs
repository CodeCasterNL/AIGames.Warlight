using AIGames.Conquest.Bot;
using AIGames.Conquest.Models;
using AIGames.Conquest.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AIGames.Conquest.Engine
{
    public class GameState
    {
        public String OpponentName { get; set; }

        public Map FullMap { get; private set; }

        public int StartingArmies { get; set; }

        public int RoundNumber { get; set; }

        public GameState()
        {
            OpponentName = "";
            RoundNumber = 0;
            FullMap = new Map();
        }

        public int GetStartingArmies(string playerName)
        {
            int players = 5;

            foreach (var superRegion in FullMap.SuperRegions)
            {
                if (superRegion.OwnedByPlayer(playerName))
                {
                    players += superRegion.ArmiesReward;
                }
            }

            return players;
        }

        public void UpdateSettings(String key, String value)
        {
            if (key.Equals("your_bot")) { }
            else if (key.Equals("opponent_bot"))
            {
                OpponentName = value;
            }
            else if (key.Equals("starting_armies"))
            {
                StartingArmies = int.Parse(value);

                // "starting_armies" indicates next round.
                RoundNumber++;
            }
            else
            {
                throw new ArgumentException(string.Format("Unknown key '{0}'.", key), "key");
            }
        }

        /// <summary>
        /// The initial map is given to the bot in three lines: superregions, regions, neighbors.
        /// </summary>
        /// <param name="mapInput"></param>
        public void SetupMap(string[] mapInput)
        {
            MapParser.ParseMapInfo(FullMap, mapInput);
        }

        /// <summary>
        /// Visible regions are given to the bot with player and armies info.
        /// </summary>
        /// <param name="mapInput"></param>
        public void UpdateMap(string[] mapInput, bool resetOwnerAndArmies = true)
        {
            // Just reset because of Fog of War. We may have lost a region that is now invisible. 
            // TODO: Ultimately we should remember state apart from region, for example in a list of states where each new round adds a state, which can then be initialized here and be modified by own and opponent moves.
            ResetRegions(FullMap.Regions, resetOwnerAndArmies);

            for (int i = 1; i < mapInput.Length; i++)
            {
                try
                {
                    UpdateRegion(int.Parse(mapInput[i]), mapInput[i + 1], int.Parse(mapInput[i + 2]));

                    i += 2;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Unable to parse Map Update: " + ex);
                }
            }
        }


        /// <summary>
        /// Visible regions are given to the bot with player and armies info.
        /// </summary>
        /// <param name="mapInput"></param>
        public void UpdateMap(List<Region> visibleRegions, bool resetOwnerAndArmies = true)
        {
            // Just reset because of Fog of War. We may have lost a region that is now invisible. 
            // TODO: Ultimately we should remember state apart from region, for example in a list of states where each new round adds a state, which can then be initialized here and be modified by own and opponent moves.
            ResetRegions(FullMap.Regions, resetOwnerAndArmies);

            foreach (var updatedRegion in visibleRegions)
            {
                try
                {
                    UpdateRegion(updatedRegion.ID, updatedRegion.PlayerName, updatedRegion.Armies);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Unable to parse Map Update: " + ex);
                }
            }
        }

        private void UpdateRegion(int regionID, string newPlayerName, int newArmyCount)
        {
            Region targetRegion = FullMap.GetRegionByID(regionID);

            targetRegion.PlayerName = newPlayerName;
            targetRegion.Armies = newArmyCount;
            targetRegion.ArmiesAvailable = targetRegion.Armies - 1;
        }

        /// <summary>
        /// Resets the various counters and properties on the given regions. 
        /// </summary>
        /// <param name="regions"></param>
        /// <param name="resetOwnerAndArmies"></param>
        public static void ResetRegions(List<Region> regions, bool resetOwnerAndArmies)
        {
            foreach (var region in regions)
            {
                if (resetOwnerAndArmies)
                {
                    region.Armies = 2;
                    region.PlayerName = "unknown"; // Fog of war.
                }

                region.ArmiesAvailable = region.Armies - 1;
                region.IncomingArmies = 0;
                region.RequestedArmies = 0;
            }
        }

        public bool PlayerHasWon
        {
            get
            {
                return FullMap.Regions
                              .Where(r => r.PlayerName != "neutral")
                              .GroupBy(r => r.PlayerName)
                              .Count() == 1;
            }
        }
    }
}