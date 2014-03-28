using AIGames.Conquest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AIGames.Conquest.Parsers
{
    static class MapParser
    {
        public static void ParseMapInfo(Map map, string[] mapInput)
        {
            int i, regionId, superRegionId, reward;

            if (mapInput[1].Equals("super_regions"))
            {
                for (i = 2; i < mapInput.Length; i++)
                {
                    try
                    {
                        superRegionId = int.Parse(mapInput[i]);
                        i++;
                        reward = int.Parse(mapInput[i]);
                        map.AddSuperRegion(new SuperRegion(superRegionId, reward));
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Unable to parse SuperRegions: " + ex);
                    }
                }
            }
            else if (mapInput[1].Equals("regions"))
            {
                for (i = 2; i < mapInput.Length; i++)
                {
                    try
                    {
                        regionId = int.Parse(mapInput[i]);
                        i++;
                        superRegionId = int.Parse(mapInput[i]);
                        SuperRegion superRegion = map.GetSuperRegionByID(superRegionId);
                        map.AddRegion(new Region(regionId, superRegion, "neutral", 2));
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Unable to parse Regions: " + ex);
                    }
                }
            }
            else if (mapInput[1].Equals("neighbors"))
            {
                for (i = 2; i < mapInput.Length; i++)
                {
                    try
                    {
                        Region region = map.GetRegionByID(int.Parse(mapInput[i]));
                        i++;
                        String[] neighborIds = mapInput[i].Split(',');
                        for (int j = 0; j < neighborIds.Length; j++)
                        {
                            Region neighbor = map.GetRegionByID(int.Parse(neighborIds[j]));
                            region.AddNeighbor(neighbor);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Unable to parse Neighbors: " + ex);
                    }
                }
            }
        }
    }
}
