using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AIGames.Conquest.Models
{
    public class Map
    {
        public List<Region> Regions { get; set; }
        public List<SuperRegion> SuperRegions { get; set; }

        public Map()
        {
            this.Regions = new List<Region>();
            this.SuperRegions = new List<SuperRegion>();
        }

        public Map(List<Region> regions, List<SuperRegion> superRegions)
        {
            this.Regions = regions;
            this.SuperRegions = superRegions;
        }

        /**
         * add a Region to the map
         * @param region : Region to be added
         */
        public void AddRegion(Region region)
        {
            var existing = GetRegionByID(region.ID);
            if (existing != null)
            {
                Console.Error.WriteLine("Region cannot be added: id already exists.");
                return;
            }

            Regions.Add(region);
        }

        /**
         * add a SuperRegion to the map
         * @param superRegion : SuperRegion to be added
         */
        public void AddSuperRegion(SuperRegion superRegion)
        {
            var existing = GetSuperRegionByID(superRegion.ID);
            if (existing != null)
            {
                Console.Error.WriteLine("SuperRegion cannot be added: id already exists.");
                return;
            }

            SuperRegions.Add(superRegion);
        }

        /// <summary>
        /// Returns a copy of this map and all of its contents, with new instances of each object (Map, SuperRegions, Regions).
        /// </summary>
        /// <returns></returns>
        public Map GetMapCopy()
        {
            Map newMap = new Map();

            foreach (SuperRegion sr in SuperRegions)
            {
                SuperRegion newSuperRegion = new SuperRegion(sr.ID, sr.ArmiesReward);
                newMap.AddSuperRegion(newSuperRegion);
            }

            foreach (Region r in Regions)
            {
                Region newRegion = new Region(r.ID, newMap.GetSuperRegionByID(r.SuperRegion.ID), r.PlayerName, r.Armies);
                newMap.AddRegion(newRegion);
            }

            foreach (Region r in Regions) 
            {
                Region newRegion = newMap.GetRegionByID(r.ID);
                foreach (Region neighbor in r.Neighbors)
                {
                    newRegion.AddNeighbor(newMap.GetRegionByID(neighbor.ID));
                }
            }

            return newMap;
        }

        public Region GetRegionByID(int id)
        {
            return Regions.FirstOrDefault(r => r.ID == id);
        }

        public SuperRegion GetSuperRegionByID(int id)
        {
            return SuperRegions.FirstOrDefault(s => s.ID == id);
        }

        public String GetMapString()
        {
            return string.Join(" - ", Regions.Select(region => string.Format("{0} {1} {2}", region.ID, region.PlayerName, region.Armies)).ToArray());
        }

        public void Clear()
        {
            this.Regions = new List<Region>();
            this.SuperRegions = new List<SuperRegion>();
        }
    }
}
