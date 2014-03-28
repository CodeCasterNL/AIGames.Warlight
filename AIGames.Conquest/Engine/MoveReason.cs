using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIGames.Conquest.Engine
{
    public enum MoveReason
    {
        Unknown,
        Random, 
        AttackNeutral,
        AttackHostile,
        PulledInForHelp,
        MovingToFrontLine,
    }
}
