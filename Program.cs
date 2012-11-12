using System;

namespace CS6613_Final
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (CheckersGame game = new CheckersGame())
            {
                game.Run();
            }
        }
    }
#endif
}

