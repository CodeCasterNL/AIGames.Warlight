using AIGames.Conquest.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AIGames.Conquest.Server
{
    public interface IConquestServer
    {
        void Start();

        GameState GameState { get; }

        string Winner { get; set; }
    }
}
