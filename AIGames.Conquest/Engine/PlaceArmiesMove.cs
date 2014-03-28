using AIGames.Conquest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIGames.Conquest.Engine
{
    public class PlaceArmiesMove : Move
    {
        public Region Region { get; set; }

        public PlaceArmiesMove(String playerName, Region region, int armies)
        {
            Armies = armies; 
            Region = region;
            PlayerName = playerName;
        }

        public override string GetCommandString()
        {
            if (string.IsNullOrEmpty(IllegalMove))
            {
                return PlayerName + " place_armies " + Region.ID + " " + Armies;
            }
            else
            {
                return PlayerName + " illegal_move " + IllegalMove;
            }
        }

        public override string ToString()
        {
            return string.Format("Place {0} armies on region {1}", Armies, Region.ID);
        }
    }
}
