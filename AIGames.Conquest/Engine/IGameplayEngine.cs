using AIGames.Conquest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AIGames.Conquest.Engine
{
    public interface IGameplayEngine
    {
        /// <summary>
        /// The name assigned to this bot. 
        /// </summary>
        string Name { get; set; }

        List<Region> GetPreferredStartingRegions(List<Region> startingRegions, int timeout);

        List<PlaceArmiesMove> GetPlaceArmiesMoves(int timeout);

        List<AttackTransferMove> GetAttackTransferMoves(int timeout);

        /// <summary>
        /// At the end of each turn, you receive a call to this method with the moves the opponent made to and from regions visible to you.
        /// </summary>
        /// <param name="allMoves"></param>
        void ProcessOpponentMoves(List<Move> allMoves);

        GameState State { get; }        
    }
}
