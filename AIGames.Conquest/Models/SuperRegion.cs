using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AIGames.Conquest.Models
{
    public class SuperRegion
    {
        private int id;
        private int armiesReward;
        public List<Region> SubRegions { get; set; }

        public SuperRegion(int id, int armiesReward)
        {
            this.id = id;
            this.armiesReward = armiesReward;
            SubRegions = new List<Region>();
        }

        public void addSubRegion(Region subRegion)
        {
            if (!SubRegions.Contains(subRegion))
            {
                SubRegions.Add(subRegion);
            }
        }

        /**
         * @return A string with the name of the player that fully owns this SuperRegion
         */
        public bool OwnedByPlayer(string playerName)
        {
            return !SubRegions.Any(r => r.PlayerName != playerName);
        }

        /**
         * @return The id of this SuperRegion
         */
        public int ID
        {
            get
            {
                return id;
            }
        }

        /**
         * @return The number of armies a Player is rewarded when he fully owns this SuperRegion
         */
        public int ArmiesReward
        {
            get
            {
                return armiesReward;
            }
        }
    }
}
