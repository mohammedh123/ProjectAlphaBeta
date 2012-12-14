// Mohammed Hossain 12/12/12

namespace CS6613_Final
{
#if WINDOWS || XBOX
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            using (var game = new XnaCheckersDriver())
            {
                game.Run();
            }
        }
    }
#endif
}