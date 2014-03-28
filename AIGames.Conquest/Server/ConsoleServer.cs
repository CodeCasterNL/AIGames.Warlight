using AIGames.Conquest.Engine;
using AIGames.Conquest.SampleBot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;


namespace AIGames.Conquest.Server
{
    public class ConsoleServer : IConquestServer
    {
        private int _round = 0;
        private IGameplayEngine _bot;

        public GameState GameState { get { return _bot.State; } }
        public string Winner { get; set; }

        public ConsoleServer(IGameplayEngine bot)
        {
            _bot = bot;
        }

        public virtual void Start()
        {
            // To enable pasting more console input, from http://stackoverflow.com/a/16638000/266143.
            int bufSize = 1024;
            Stream inStream = Console.OpenStandardInput(bufSize);
            Console.SetIn(new StreamReader(inStream, Console.InputEncoding, false, bufSize));

            while (true)
            {
                _bot.State.RoundNumber = ++_round;
#if !DEBUG
                try
                {
#endif
                    HandleConsoleInput();
#if !DEBUG
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Exception occurred: {0}", ex.Message);
                }
#endif
                
            }
        }

        private void HandleConsoleInput()
        {
            string line;
            if ((line = ReadLine()) == null)
            {
                return;
            }

            string response = InputParser.Parse(line, _bot);

            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine(response);
            }
        }

        private string ReadLine()
        {
            string line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
            {
                // null flags exit.
                return null;
            }
            return line;
        }
    }
}
