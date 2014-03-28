using AIGames.Conquest.Engine;
using AIGames.Conquest.Parsers;
using AIGames.Conquest.SampleBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace AIGames.Conquest.Server
{
    public class TextFileMapServer : MultiPlayerServer
    {
        private string _mapFilename;
        private string _stateFilename;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapFilename">Map file name.</param>
        /// <param name="stateFile">State file name, pass null for no state.</param>
        /// <param name="player1">Instance of bot for player1.</param>
        /// <param name="player2">Instance of bot for player2.</param>
        /// <param name="runUntilFullyConquered">If false and either bot loses its last region, the game ends. If true, then one player must conquer the entire map.</param>
        /// <param name="runUntilWin">When a draw occurs after _maxTurns turns (default: 100) and this parameter is set to false, the game ends. If set to true, the server will restart the game until either bot wins.</param>
        public TextFileMapServer(string mapFilename, string stateFile, IGameplayEngine player1, IGameplayEngine player2, bool runUntilFullyConquered, bool runUntilWin)
            : base(player1, player2, runUntilFullyConquered, runUntilWin)
        {
            _mapFilename = mapFilename;
            _stateFilename = stateFile;            
        }        

        protected override void InitializeMap()
        {
            _gameState.FullMap.Clear();
            _player1.State.FullMap.Clear();
            _player2.State.FullMap.Clear();

            var lines = File.ReadAllLines(_mapFilename);
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var lineParts = line.Split(' ');

                if (lineParts[0] == "settings")
                {
                    _gameState.UpdateSettings(lineParts[1], lineParts[2]);

                    _player1.State.UpdateSettings(lineParts[1], lineParts[2]);

                    lineParts[1] = lineParts[1] == "your_bot" ? "opponent_bot" : "your_bot";
                    _player2.State.UpdateSettings(lineParts[1], lineParts[2]);
                }
                else if (lineParts[0] == "setup_map")
                {
                    MapParser.ParseMapInfo(_gameState.FullMap, lineParts);
                    MapParser.ParseMapInfo(_player1.State.FullMap, lineParts);
                    MapParser.ParseMapInfo(_player2.State.FullMap, lineParts);
                }
                else
                {
                    ConsoleExtensions.WriteColoredLine(ConsoleColor.Gray, "Ignoring map line '{0}'", line);
                }
            }

            base.InitializeMap();
        }

        protected override void InitializePlayers()
        {
            if (!string.IsNullOrEmpty(_stateFilename))
            {
                using (var reader = new StreamReader(_stateFilename))
                {
                    // First line: "round X"
                    _round = int.Parse(reader.ReadLine().Split(' ')[1]) - 1;

                    foreach (var line in reader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (line.StartsWith("update_map"))
                        {
                            _gameState.UpdateMap(InputParser.Split(line), resetOwnerAndArmies: false);
                        }
                    }
                }
            }

            base.InitializePlayers();

            _player1.State.UpdateMap(base.GetMapUpdate(_player1.Name));
            _player2.State.UpdateMap(base.GetMapUpdate(_player2.Name));

            ConsoleExtensions.WriteColoredLine(ConsoleColor.DarkGray, "Loaded state file '{0}'.", _stateFilename);
            ConsoleExtensions.WriteColoredLine(ConsoleColor.White, _gameState.FullMap.GetMapString());
        }
    }
}
