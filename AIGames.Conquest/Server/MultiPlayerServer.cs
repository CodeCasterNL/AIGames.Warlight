using AIGames.Conquest.Bot;
using AIGames.Conquest.Engine;
using AIGames.Conquest.Models;
using AIGames.Conquest.SampleBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace AIGames.Conquest.Server
{
    /// <summary>
    /// This is a bit of a silly server, but it is used to let two bots compete agains each other without abusing the console.
    /// 
    /// TODO: This server is not compliant to the specs on at least the following:
    ///     - sends two opponent_moves, one for placing and one for moving. The real server combines them.
    ///     - first executes all moves of the first player per round (p1 or p2, randomized), then all moves of the second one. Real server interleaves attacks: firstPlayer.Attacks[0], secondPlayer.Attacks[0], firstPlayer.Attacks[1], secondPlayer.Attacks[1], ...
    ///     - it doesn't enforce the timeout.
    /// </summary>
    public class MultiPlayerServer : IConquestServer
    {
        protected int _maxRounds = 100;
        protected int _round;
        protected GameState _gameState;
        public GameState GameState { get { return _gameState; } }

        protected ServerState _serverState;
        protected int _timeout;

        protected Random _random = new Random();

        protected IGameplayEngine _player1;
        protected IGameplayEngine _player2;

        public string Winner { get; set; }

        protected bool _runUntilFullyConquered;
        protected bool _runUntilWin;
        protected int _pass;

        public MultiPlayerServer(IGameplayEngine player1, IGameplayEngine player2, bool runUntilFullyConquered, bool runUntilWin)
        {
            _gameState = new GameState();
            _timeout = 2000;
            _player1 = player1;
            _player2 = player2;
            _runUntilFullyConquered = runUntilFullyConquered;
            _runUntilWin = runUntilWin;
        }

        public virtual void Start()
        {
            while (true)
            {
                _pass++;

                RunGame();

                if (_runUntilWin && string.IsNullOrEmpty(Winner))
                {
                    ConsoleExtensions.WriteColoredLine(ConsoleColor.Red, "Draw in pass {0}, resetting...", _pass);

                    // Reset
                    _round = 0;
                    _gameState.FullMap.Clear();
                }
                else
                {
                    break;
                }
            }

            ConsoleExtensions.WriteColoredLine(ConsoleColor.Red, "Finished in round {0} of pass {1}.", _round, _pass);
        }

        /// <summary>
        /// Runs a single game.
        /// </summary>
        protected virtual void RunGame()
        {
            _round = 0;
            Winner = null;

            InitializeMap();
            InitializePlayers();

            while (++_round <= _maxRounds)
            {
                int startingPlayer = _random.Next(0, 2);
                var firstPlayer = (startingPlayer == 0) ? _player1 : _player2;
                var secondPlayer = (startingPlayer == 0) ? _player2 : _player1;

                ConsoleExtensions.WriteColoredLine(ConsoleColor.Cyan, "Round {0}, player {1} starts!", _round, startingPlayer + 1);

                // Place armies always player1, then player2.                
                _player1.State.StartingArmies = _gameState.GetStartingArmies(_player1.Name);
                _player1.State.RoundNumber = _round;
                ProcessPlayerMoves(_player1, _player2, _player1.GetPlaceArmiesMoves(_timeout).Select(m => (Move)m).ToList());

                _player2.State.StartingArmies = _gameState.GetStartingArmies(_player2.Name);
                _player2.State.RoundNumber = _round;
                ProcessPlayerMoves(_player2, _player1, _player2.GetPlaceArmiesMoves(_timeout).Select(m => (Move)m).ToList());

                bool finished = HandleAttackTransferMoves(firstPlayer, secondPlayer);

                ConsoleExtensions.WriteColoredLine(ConsoleColor.White, _gameState.FullMap.GetMapString());

                if (finished)
                {
                    return;
                }
            }

            ConsoleExtensions.WriteColoredLine(ConsoleColor.White, "It's a draw after {0} turns.", _round - 1);
        }

        public List<Region> GetMapUpdate(string playerName)
        {
            var visibleRegions = _gameState.FullMap.Regions.OwnedBy(playerName).ToList();

            foreach (var ownedRegion in visibleRegions.ToList())
            {
                foreach (var neighbor in ownedRegion.Neighbors.ToList())
                {
                    if (!visibleRegions.Contains(neighbor))
                    {
                        visibleRegions.Add(neighbor);
                    }
                }
            }

            return visibleRegions;
        }

        protected virtual List<Region> GetStartingRegions()
        {
            // pick_starting_regions $timeout [$regionId]{6}

            var allRegions = _gameState.FullMap.Regions.ToList();

            if (!allRegions.Any())
            {
                throw new Exception("Map contains no regions");
            }

            // Fill up with duplicated IDs when necessary to make it 6.
            while (allRegions.Count < 6)
            {
                allRegions.AddRange(allRegions);
            }

            _serverState = ServerState.StartingRegionsSelected;

            return allRegions.OrderBy(r => _random.Next())
                             .Take(6)
                             .ToList();
        }

        /// <summary>
        /// Initializes each region in the state's map with 2 neutral armies.
        /// </summary>
        protected virtual void InitializeMap()
        {
            foreach (var region in _gameState.FullMap.Regions)
            {
                region.PlayerName = "neutral";
                region.Armies = 2;
            }
        }

        /// <summary>
        /// If the map doesn't contain any regions belonging to player1, this method:
        /// - sends the pickable starting regions to the bots and asks for preferred starting regions in return
        /// - processes those replies
        /// </summary>
        protected virtual void InitializePlayers()
        {
            if (!_gameState.FullMap.Regions.Any(r => r.PlayerName == _player1.Name))
            {
                var startingRegions = GetStartingRegions();

                var player1Regions = GetAndPrintStartingRegions(_player1, startingRegions, _timeout);
                var player2Regions = GetAndPrintStartingRegions(_player2, startingRegions, _timeout);

                ProcessStartingRegions(player1Regions, player2Regions);

                SendMapUpdateAndOutputMap(_player1);
                SendMapUpdateAndOutputMap(_player2);
            }

            if (_player1.Name == _player2.Name)
            {
                _player2.Name += "2";
            }

            _player1.State.OpponentName = _player2.Name;
            _player2.State.OpponentName = _player1.Name;
        }

        protected virtual void ProcessStartingRegions(List<Region> player1Regions, List<Region> player2Regions)
        {
            // TODO: do something with `player1Regions` and `player2Regions`. For now just select all regions.
            var pickableRegions = _gameState.FullMap.Regions.ToList();

            // To make sure that when testing on smaller maps, we don't give out too many regions and a maximum of 3 per player.
            int toPick = Math.Min(pickableRegions.Count / 2, 6);
            toPick = 2 * (toPick / 2);

            // And pick _any_ neutral region for each bot until enough have been chosen.
            for (int i = 0; i < toPick; i++)
            {
                int freeRegion = -1;
                do
                {
                    int index = _random.Next(0, pickableRegions.Count);
                    if (pickableRegions[index].PlayerName.Equals("neutral"))
                    {
                        freeRegion = index;
                    }
                }
                while (freeRegion < 0);

                pickableRegions[freeRegion].PlayerName = i % 2 == 0 ? _player1.Name : _player2.Name;
            }

            _serverState = ServerState.StartingRegionsAssigned;
        }

        protected virtual bool DetectGameEnd()
        {
            if ((_runUntilFullyConquered && _gameState.FullMap.Regions.GroupBy(r => r.PlayerName).Count() == 1)
             || !_runUntilFullyConquered && _gameState.PlayerHasWon)
            {
                Winner = _gameState.FullMap.Regions.FirstOrDefault(r => r.PlayerName != "neutral").PlayerName;
                ConsoleExtensions.WriteColoredLine(ConsoleColor.White, new string('*', 60));
                ConsoleExtensions.WriteColoredLine(ConsoleColor.White, "Player '{0}' has won in round {1} by capturing all regions.", _gameState.FullMap.Regions.FirstOrDefault(r => r.PlayerName != "neutral").PlayerName, _round);
                ConsoleExtensions.WriteColoredLine(ConsoleColor.White, new string('*', 60));

                return true;
            }

            return false;
        }

        protected virtual bool HandleAttackTransferMoves(IGameplayEngine firstPlayer, IGameplayEngine secondPlayer)
        {
            var moves = firstPlayer.GetAttackTransferMoves(_timeout); ;
            ProcessPlayerMoves(firstPlayer, secondPlayer, moves.Cast<Move>().ToList());
            if (DetectGameEnd())
            {
                return true;
            }

            moves = secondPlayer.GetAttackTransferMoves(_timeout);
            ProcessPlayerMoves(secondPlayer, firstPlayer, moves.Cast<Move>().ToList());
            if (DetectGameEnd())
            {
                return true;
            }

            firstPlayer.State.UpdateMap(GetMapUpdate(firstPlayer.Name));
            secondPlayer.State.UpdateMap(GetMapUpdate(secondPlayer.Name));

            return false;
        }

        protected virtual void ProcessPlayerMoves(IGameplayEngine actingPlayer, IGameplayEngine opponent, List<Move> moves)
        {
            if (!moves.Any())
            {
                ConsoleExtensions.WriteColoredLine(ConsoleColor.Red, "Player '{0}' made no moves!", actingPlayer.Name);
            }

            ApplyMovesToState(moves, actingPlayer);

            opponent.ProcessOpponentMoves(moves);
        }

        protected virtual void ApplyMovesToState(List<Move> moves, IGameplayEngine actingPlayer)
        {
            GameState.ResetRegions(_gameState.FullMap.Regions, resetOwnerAndArmies: false);

            int startingArmies = _gameState.GetStartingArmies(actingPlayer.Name);

            int armiesPlaced = 0;
            foreach (var move in moves)
            {
                if (move is AttackTransferMove)
                {
                    Move(actingPlayer, move as AttackTransferMove);
                }
                else
                {
                    int armiesPlacing = Place(actingPlayer, move as PlaceArmiesMove, startingArmies - armiesPlaced);
                    armiesPlaced += armiesPlacing;
                }
            }
        }

        protected virtual int Place(IGameplayEngine actingPlayer, PlaceArmiesMove move, int armiesLeft)
        {
            Region region = _gameState.FullMap.GetRegionByID(move.Region.ID);

            if (region == null)
            {
                ConsoleExtensions.WriteColoredLine(ConsoleColor.Red, "Player '{0}' tried to place {1} armies on region {2}, which does not exist. Move discarded.", move.PlayerName, move.Armies, move.Region.ID, move.Region.PlayerName);
                return 0;
            }
            if (move.PlayerName != region.PlayerName)
            {
                ConsoleExtensions.WriteColoredLine(ConsoleColor.Red, "Player '{0}' tried to place {1} armies on region {2}, which is owned by '{3}' instead. Move discarded.", move.PlayerName, move.Armies, move.Region.ID, move.Region.PlayerName);
                return 0;
            }
            if (move.Armies < 1)
            {
                ConsoleExtensions.WriteColoredLine(ConsoleColor.Red, "Player '{0}' tried to place {1} armies on region {2}, which is a bit silly. Move discarded.", move.PlayerName, move.Armies, move.Region.ID);
                return 0;
            }
            if (move.Armies > armiesLeft)
            {
                ConsoleExtensions.WriteColoredLine(ConsoleColor.Red, "Player '{0}' places {1} armies on region {2}, but only {3} armies were remaining. Move discarded.", move.PlayerName, move.Armies, move.Region.ID, armiesLeft);
                return 0;
            }

            ConsoleExtensions.WriteColoredLine(ConsoleColor.DarkGreen, "Player '{0}' places {1} armies on region {2}.", move.PlayerName, move.Armies, move.Region.ID);

            region.Armies += move.Armies;
            region.ArmiesAvailable += move.Armies;

            ConsoleExtensions.WriteColoredLine(ConsoleColor.Magenta, "Region {0} ('{1}'): {2} armies",
                                                                           region.ID,
                                                                           region.PlayerName,
                                                                           region.Armies);
            return move.Armies;
        }

        protected virtual void Move(IGameplayEngine actingPlayer, AttackTransferMove move)
        {
            var fromRegion = _gameState.FullMap.GetRegionByID(move.FromRegion.ID);
            var toRegion = _gameState.FullMap.GetRegionByID(move.ToRegion.ID);

            if (move.Armies < 1)
            {
                ConsoleExtensions.WriteColoredLine(ConsoleColor.Red, "Player '{0}' tried to move {1} armies from region {2} to {3}, which is a bit silly. Move discarded.", move.PlayerName, move.Armies, fromRegion.ID, toRegion.ID, fromRegion.Armies);
                return;
            }
            if (fromRegion.PlayerName != move.PlayerName)
            {
                ConsoleExtensions.WriteColoredLine(ConsoleColor.Red, "Player '{0}' tried to move {1} armies from region {2} to {3}, but region {2} is not theirs. Move discarded.", move.PlayerName, move.Armies, fromRegion.ID, toRegion.ID);
                return;
            }
            if (move.Armies > fromRegion.ArmiesAvailable)
            {
                ConsoleExtensions.WriteColoredLine(ConsoleColor.Red, "Player '{0}' tried to move {1} armies from region {2} to {3}, but only {4} armies were remaining. Move discarded.", move.PlayerName, move.Armies, fromRegion.ID, toRegion.ID, fromRegion.Armies);
                return;
            }
            if (!fromRegion.Neighbors.Contains(toRegion))
            {
                ConsoleExtensions.WriteColoredLine(ConsoleColor.Red, "Player '{0}' tried to move {1} armies from region {2} to {3}, but the regions are no neighbors. Move discarded.", move.PlayerName, move.Armies, fromRegion.ID, toRegion.ID);
                return;
            }

            if (move.PlayerName == toRegion.PlayerName)
            {
                MoveArmies(move.PlayerName, fromRegion, toRegion, move.Armies, move.MoveReason);
            }
            else
            {
                AttackHandler.HandleAttack(move, _gameState);
            }

            ConsoleExtensions.WriteColoredLine(ConsoleColor.Magenta, "Region {0} ('{1}'): {2} armies, Region {3} ('{4}'): {5} armies",
                                                                       fromRegion.ID,
                                                                       fromRegion.PlayerName,
                                                                       fromRegion.Armies,
                                                                       toRegion.ID,
                                                                       toRegion.PlayerName,
                                                                       toRegion.Armies);
        }

        protected virtual void MoveArmies(string playerName, Region fromRegion, Region toRegion, int armies, MoveReason moveReason)
        {
            ConsoleExtensions.WriteColoredLine(ConsoleColor.Yellow, "Player '{0}' moves {1} armies from region {2} to {3} for {4}.", playerName, armies, fromRegion.ID, toRegion.ID, moveReason);

            fromRegion.Armies -= armies;
            fromRegion.ArmiesAvailable -= armies;

            toRegion.Armies += armies;
        }

        protected virtual void SendMapUpdateAndOutputMap(IGameplayEngine forPlayer)
        {
            forPlayer.State.UpdateMap(GetMapUpdate(forPlayer.Name));
            ConsoleExtensions.WriteColoredLine(ConsoleColor.White, "Player `{0}` map: {1}", forPlayer.Name, forPlayer.State.FullMap.GetMapString());
        }

        protected virtual List<Region> GetAndPrintStartingRegions(IGameplayEngine forPlayer, List<Region> startingRegions, int _timeout)
        {
            var regions = forPlayer.GetPreferredStartingRegions(startingRegions, _timeout);
            if (regions == null || !regions.Any())
            {
                ConsoleExtensions.WriteColoredLine(ConsoleColor.Red, "Player `{0}` didn't pick any starting regions!", forPlayer.Name);
            }
            else
            {
                string selected = string.Join(" ", regions.Select(r => r.ID.ToString()).ToArray());
                ConsoleExtensions.WriteColoredLine(ConsoleColor.Green, "Player `{0}` picked starting regions: {1}", forPlayer.Name, selected);
            }

            return regions;
        }
    }
}
