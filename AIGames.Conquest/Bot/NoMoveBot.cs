using AIGames.Conquest.Engine;
using AIGames.Conquest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIGames.Conquest.Bot
{
    public class NoMoveBot : IGameplayEngine
    {
        public string Name { get; set; }
                
        public NoMoveBot(string name = "NoMoveBot")
        {
            Name = name;
            State = new GameState();
        }

        public List<Region> GetPreferredStartingRegions(List<Region> startingRegions, int timeout)
        {            
            return new List<Region>();
        }

        public List<PlaceArmiesMove> GetPlaceArmiesMoves(int timeOut)
        {
            return new List<PlaceArmiesMove>();
        }

        public List<AttackTransferMove> GetAttackTransferMoves(int timeOut)
        {
            return new List<AttackTransferMove>();
        }

        public void ProcessOpponentMoves(List<Move> allMoves)
        {            
        }

        public GameState State { get; private set; }
    }
}
