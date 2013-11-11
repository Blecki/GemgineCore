using System;

namespace BaseDemo
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (var game = new Gem.Main(""))
            {
                game.Game = new ConsoleDemo();
                game.Run();
            }
        }
    }
#endif
}

