using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AIGames.Conquest.Engine
{
    /// <summary>
    /// A silly attack handler for the silly server implementation.
    /// </summary>
    public static class AttackHandler
    {
        private static readonly Random _random = new Random();

        public static void HandleAttack(AttackTransferMove move, GameState gameState)
        {
            var fromRegion = gameState.FullMap.GetRegionByID(move.FromRegion.ID);
            var toRegion = gameState.FullMap.GetRegionByID(move.ToRegion.ID);

            ConsoleExtensions.WriteColoredLine(ConsoleColor.Yellow, "Player '{0}' attacks region {1} ({2} armies) from region {3} ({4} armies) with {5} armies for {6}.", move.PlayerName, toRegion.ID, toRegion.Armies, fromRegion.ID, fromRegion.Armies, move.Armies, move.MoveReason);

            int attackers = move.Armies;
            int defenders = toRegion.Armies;

            int attackersLost;
            int defendersLost;

            if (toRegion.PlayerName == "neutral" && toRegion.Armies == 2)
            {
                // Not random to properly determine growth changes instead of relying on random.
                switch (move.Armies)
                {
                    case 1:
                        attackersLost = 1;
                        defendersLost = 0;
                        break;
                    case 2:
                        attackersLost = 1;
                        defendersLost = 1;
                        break;
                    case 3:
                        attackersLost = 2;
                        defendersLost = 2;
                        break;
                    case 4:
                        attackersLost = 1;
                        defendersLost = 2;
                        break;
                    default:
                        attackersLost = 0;
                        defendersLost = 2;
                        break;
                }
            }
            else
            {
                // Aaand this one is wrong, but it was the first I came up with without reading the rules.
                // TODO: it's 70% chance 1 defender kills an attacker and 60% chance 1 attacker kills 1 defender, but do surviving armies get another go if any opposing armies left?
                attackersLost = Math.Abs(defenders - (attackers / _random.Next(1, attackers)));
                defendersLost = Math.Abs(attackers - (defenders / _random.Next(1, defenders)));

                if (attackersLost > attackers)
                {
                    attackersLost = attackers;
                }
                if (defendersLost > defenders)
                {
                    defendersLost = defenders;
                }
            }

            attackers -= attackersLost;
            defenders -= defendersLost;

            if (defenders <= 0)
            {
                fromRegion.Armies -= move.Armies;
                fromRegion.ArmiesAvailable -= move.Armies;

                string defenderName = toRegion.PlayerName;

                // change ownership and transfer attackers
                toRegion.PlayerName = move.PlayerName;
                toRegion.Armies = move.Armies - attackersLost;

                if (toRegion.Armies < 1)
                {
                    toRegion.Armies = 1;
                }

                ConsoleExtensions.WriteColoredLine(ConsoleColor.Green, "Player '{0}' won and lost {1} armies, defender '{2}' lost {3}.", move.PlayerName, attackersLost, defenderName, defendersLost);
            }
            else
            {
                fromRegion.Armies -= attackersLost;
                fromRegion.ArmiesAvailable -= attackersLost;
                
                toRegion.Armies -= defendersLost;

                ConsoleExtensions.WriteColoredLine(ConsoleColor.Green, "Player '{0}' lost, losing {1} armies,  defender '{2}' lost {3}.", move.PlayerName, attackersLost, toRegion.PlayerName, defendersLost);
            }
        }
    }
}
