using System;

namespace AtmosphericScattering
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SkyDomeDemo game = new SkyDomeDemo())
            {
                game.Run();
            }
        }
    }
}

