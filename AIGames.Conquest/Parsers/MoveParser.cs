using AIGames.Conquest.Engine;
using AIGames.Conquest.Models;
using AIGames.Conquest.SampleBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AIGames.Conquest.Parsers
{
    public static class MoveParser
    {
        public static List<Move> ParseMoves(Map fullMap, string line)
        {
            var result = new List<Move>();
            var moveInput = InputParser.Split(line);
            for (int i = 1; i < moveInput.Length; i++)
            {
                try
                {
                    Move move;
                    if (moveInput[i + 1].Equals("place_armies"))
                    {
                        Region region = fullMap.GetRegionByID(int.Parse(moveInput[i + 2]));
                        String playerName = moveInput[i];
                        int armies = int.Parse(moveInput[i + 3]);
                        move = new PlaceArmiesMove(playerName, region, armies);
                        i += 3;
                    }
                    else if (moveInput[i + 1].Equals("attack/transfer"))
                    {
                        Region fromRegion = fullMap.GetRegionByID(int.Parse(moveInput[i + 2]));
                        Region toRegion = fullMap.GetRegionByID(int.Parse(moveInput[i + 3]));

                        String playerName = moveInput[i];
                        int armies = int.Parse(moveInput[i + 4]);
                        move = new AttackTransferMove(playerName, fromRegion, toRegion, toRegion, armies, MoveReason.Unknown);
                        i += 4;
                    }
                    else
                    {
                        // Should never happen.
                        continue;
                    }

                    result.Add(move);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Unable to parse moves: " + ex);
                }
            }

            return result;
        }
    }
}
