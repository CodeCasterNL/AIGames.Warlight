using AIGames.Conquest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIGames.Conquest.Engine
{
    public class AttackTransferMove : Move
    {
        public MoveReason MoveReason { get; set; }
        public Region FromRegion { get; set; }
        public Region ToRegion { get; set; }
        
        public Region ActualTarget { get; set; }


        public AttackTransferMove(String playerName, Region fromRegion, Region toRegion, Region actualTarget, int armies, MoveReason reason)
        {
            PlayerName = playerName;
            FromRegion = fromRegion;
            ToRegion = toRegion;
            Armies = armies;
            MoveReason = reason;
            ActualTarget = actualTarget;
        }

        public override String ToString()
        {
            return string.Format("Attack/transfer with {0} armies from {1} to {2} (actual target: {3}), reason: {4} ",
                                                Armies, FromRegion.ID, ToRegion.ID, ActualTarget.ID, MoveReason);
        }

        public override string GetCommandString()
        {
            if (string.IsNullOrEmpty(IllegalMove))
            {
                return PlayerName + " attack/transfer " + FromRegion.ID + " " + ToRegion.ID + " " + Armies;
            }
            else
            {
                return PlayerName + " illegal_move " + IllegalMove;
            }
        }
    }
}
