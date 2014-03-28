using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIGames.Conquest.Models
{
    public class Region
    {
        public int ID { get { return _id; } }
        private int _id;

        public List<Region> Neighbors { get; set; }
        public SuperRegion SuperRegion { get; set; }
        public String PlayerName { get; set; }
        public int Armies { get; set; }

        public int ArmiesAvailable { get; set; }
        public int IncomingArmies { get; set; }
        public int RequestedArmies { get; set; }
        public bool HelpNeeded { get { return RequestedArmies > 0; } }            

        public Region(int id, SuperRegion superRegion, String playerName, int armies)
        {
            this._id = id;
            this.SuperRegion = superRegion;
            this.Neighbors = new List<Region>();
            this.PlayerName = playerName;
            Armies = armies;

            superRegion.addSubRegion(this);
        }

        public void AddNeighbor(Region neighbor)
        {
            if (!Neighbors.Contains(neighbor))
            {
                Neighbors.Add(neighbor);
                neighbor.AddNeighbor(this);
            }
        }
        
        public bool IsNeighbor(Region region)
        {
            return Neighbors.Contains(region);
        }

        public bool OwnedByPlayer(String playerName)
        {
            return playerName.Equals(this.PlayerName);
        }

        public override string ToString()
        {
            return string.Format("ID: {0}, Owner: {1}, Armies: {2}, ArmiesAvailable: {3} IncomingArmies: {4}, RequestedArmies: {5}, HelpNeeded: {6}",
                                  ID, PlayerName, Armies, ArmiesAvailable, IncomingArmies, RequestedArmies, HelpNeeded);
        }
    }

    public static class RegionExtensions
    {
        public static IEnumerable<Region> OwnedBy(this IEnumerable<Region> regions, string ownerName)
        {
            return regions.Where(r => r.PlayerName == ownerName);
        }
    }
}
