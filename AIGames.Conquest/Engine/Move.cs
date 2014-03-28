using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIGames.Conquest.Engine
{
    public abstract class Move
    {
        public int Armies { get; set; }
        public String PlayerName { get; set; }
        public String IllegalMove { get; set; }

        public abstract string GetCommandString();
    }
}
