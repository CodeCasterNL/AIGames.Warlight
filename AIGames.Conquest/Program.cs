using AIGames.Conquest.Bot;
using AIGames.Conquest.Engine;
using AIGames.Conquest.SampleBot;
using AIGames.Conquest.Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace AIGames.Conquest
{
    public static class Program
    {
        static void Main(string[] args)
        {
            // Uncomment `StartConsoleServer()` to create a bot that reads from and writes to the console.

            //StartConsoleServer();
            StartMultiplayerServer();
        }

        private static void StartConsoleServer()
        {
            // Your bot here! Or begin by adapting the StarterBot.
            IGameplayEngine bot = new StarterBot();
            IConquestServer server = new ConsoleServer(bot);

            server.Start();
        }

        private static void StartMultiplayerServer()
        {
            // Small map with 6 regions:
            //string mapFile = @"Games\Sample-map.txt";
            
            // Official Warlight world map:
            string mapFile = @"Games\Warlight-map.txt";

            var testBot1 = new StarterBot("player1");
            //var testBot1 = new NoMoveBot("player1");

            //var testBot2 = new StarterBot("player2");
            var testBot2 = new NoMoveBot("player2");

            // No state file:
            var stateFiles = new string[] { null };

            // Single state file:
            //var stateFiles = new string[] { @"Games\\53277b354b5ab27d7f937930_Round7.txt" };

            // Multiple state files:
            //var stateFiles = Directory.GetFiles(@"Games\").Where(f => f.Contains("_Round")).ToList();

            foreach (string stateFile in stateFiles)
            {
                var server = new TextFileMapServer(mapFile, stateFile, testBot1, testBot2, runUntilFullyConquered: true, runUntilWin: true);
                server.Start();

                Console.WriteLine("Game '{0}' finished.", stateFile ?? "(stateless)");
                Console.ReadKey(true);
            }
        }
    }
}
