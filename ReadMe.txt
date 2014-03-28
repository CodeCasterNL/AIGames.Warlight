For the Warlight AI Challenge (http://theaigames.com/competitions/warlight-ai-challenge):

This project's code contains:
 * A ConsoleServer which reads input from the console, parses it and passes the commands on to a bot implementation, returning the bot output on the console.
 * A NoMoveBot, which does exactly what its name states.
 * A StarterBot that makes random moves, adapted from the Java example (http://theaigames.com/downloads/starterbots/warlight-starterbot-java.zip).
 * A MultiPlayerServer that lets two bot implementations, each with their own world view (state), run against each other on a single map. Their moves are executed by the server and the outcome thereof is applied by the server to each bot's state.
 * A TextFileServer, extending the MultiPlayerServer with the ability to load map and state definitions from files.
 * Various parsers and other helper code. 
 * Probably lots of dead code too, since this code has gone through major refactoring coming from the Java example.

Resources, in the Games directory:
 * Sample-map.txt, a small sample map consisting of six (6) regions. A graph of the map:
 
      [3]
      / \
    [1]-[2]
     |   |
    [4] [6]
     |
    [5]
 
 * Sample-server.txt, texts to copy-paste into a console bot to test it before uploading to make sure it's kind of working.
 * Warlight-map.txt, the official Warlight world map: 42 regions together forming the continents of Earth.
 * Conquest_TestMap_SuperRegions.png: an image representing the region IDs of the Warlight map.
 * 53277b354b5ab27d7f937930_Round7.txt: a state file used to demonstrate loading a specific round in the game.

Interesting breakpoint / starting locations:
 * Program.cs, for the various options this program and the servers have.
 * MultiPlayerServer.Start(), at `if (finished)`: control arrive sthere after both bots have made their moves and those moves have been handled and persisted to the server's state. You can now easily read the map and move output of the last round on the console window.
 * StarterBot.GetPreferredStartingRegions(), .GetPlaceArmiesMoves(), .GetAttackTransferMoves() and .ProcessOpponentMoves(). Here the actual game commands are being processed, and you can do your thing to improve that bot!
 
General remarks:
 * Unfortunately I didn't bother to add _any_ unit tests, though a lot of safeguards are built into the server and parsers. 
 * When adding state files to replay games, make sure to set Copy to Output Directory in the Properties. Also make sure your bot is assigned to `player1`, you may have to do a Find&Replace through the update_map statement(s).
 * This is a working bot! If you make sure that in Program.Main() only StartConsoleServer() is called, you can upload this entire directory in a ZIP file to join the contest!