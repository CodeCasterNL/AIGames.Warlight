using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIGames.Conquest
{
    public static class ConsoleExtensions
    {
        public static void WriteColoredLine(ConsoleColor color, string format, params object[] arg)
        {
            //if (
            //    color != ConsoleColor.Green 
            //    && color != ConsoleColor.White 
            //    && color != ConsoleColor.Cyan
            //   )
            //{
            //    return;
            //}

            var oldColor = Console.ForegroundColor;

            Console.ForegroundColor = color;            
            Console.WriteLine(format, arg);
            Console.ForegroundColor = oldColor;
        }
    }
}
