using AIGames.Conquest.Engine;
using AIGames.Conquest.Models;
using AIGames.Conquest.Parsers;
using AIGames.Conquest.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace AIGames.Conquest.SampleBot
{
    public static class InputParser
    {
        public static string Parse(string line, IGameplayEngine bot)
        {
            string result = null;

            if (line == null)
            {
                // null indicates exit
                return null;
            }

            line = line.Trim();

            if (line.Length == 0)
            {
                return "";
            }

            String[] parts = InputParser.Split(line);

            if (parts[0].Equals("pick_starting_regions"))
            {
                result = PickStartingRegions(bot, parts);
            }
            else if (parts.Length == 3 && parts[0].Equals("go"))
            {
                result = Go(bot, parts);
            }
            else if (parts.Length == 3 && parts[0].Equals("settings"))
            {
                ProcessSettings(bot, parts);
            }
            else if (parts[0].Equals("setup_map"))
            {
                bot.State.SetupMap(parts);
            }
            else if (parts[0].Equals("update_map"))
            {
                //all visible regions are given
                bot.State.UpdateMap(parts);
            }
            else if (parts[0].Equals("opponent_moves"))
            {
                //all visible opponent moves are given
                bot.ProcessOpponentMoves(MoveParser.ParseMoves(bot.State.FullMap, line));
            }
            else
            {
                Console.Error.WriteLine("Unable to parse line '{0}'", line);
            }

            return result;
        }

        private static void ProcessSettings(IGameplayEngine bot, string[] parts)
        {
            if (parts[1] == "your_bot")
            {
                bot.Name = parts[2];
            }

            bot.State.UpdateSettings(parts[1], parts[2]);
        }

        /// <summary>
        /// Parses the "pick_starting_regions" line parts and issues the GetPreferredStartingRegions() method on the bot.
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="parts"></param>
        /// <returns></returns>
        private static string PickStartingRegions(IGameplayEngine bot, string[] parts)
        {
            var pickable = GetPickableStartingRegions(bot.State.FullMap, parts);

            int timeout = int.Parse(parts[1]);
            var preferredStartingRegions = bot.GetPreferredStartingRegions(pickable, timeout);

            return string.Join(" ", preferredStartingRegions.Select(r => r.ID.ToString()).ToArray());
        }

        /// <summary>
        /// Called when the bot needs to make a place_armies or attack/transfer move.
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="parts"></param>
        /// <returns></returns>
        private static string Go(IGameplayEngine bot, string[] parts)
        {
            IEnumerable<Move> moves = null;
            int timeout = int.Parse(parts[2]);

            if (parts[1].Equals("place_armies"))
            {
                moves = bot.GetPlaceArmiesMoves(timeout).Cast<Move>();
            }
            else if (parts[1].Equals("attack/transfer"))
            {
                moves = bot.GetAttackTransferMoves(timeout).Cast<Move>();
            }
            else
            {
                throw new Exception(string.Format("Unkown go command '{0}'.", parts[1]));
            }

            if (moves == null || !moves.Any())
            {
                return "No moves";
            }
            else
            {
                string output = string.Join(",", moves.Select(m => m.GetCommandString()).ToArray());
                return output;
            }
        }

        /// <summary>
        /// Parses the `pick_starting_regions $timeout $id{12}` line and returns a list of Regions from the FullMap the bot can choose from.
        /// </summary>
        /// <param name="pickStartingRegionsLineParts"></param>
        private static List<Region> GetPickableStartingRegions(Map map, string[] pickStartingRegionsLineParts)
        {
            var pickableStartingRegions = new List<Region>();

            for (int i = 2; i < pickStartingRegionsLineParts.Length; i++)
            {
                int regionId;
                try
                {
                    regionId = int.Parse(pickStartingRegionsLineParts[i]);
                    Region pickableRegion = map.GetRegionByID(regionId);
                    pickableStartingRegions.Add(pickableRegion);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Unable to parse pickable regions: " + ex);
                }
            }

            return pickableStartingRegions;
        }

        internal static string[] Split(string line)
        {
            return line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
        }
    }
}