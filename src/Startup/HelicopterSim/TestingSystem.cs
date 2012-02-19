#if !XBOX

#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

using System.Windows.Forms;
using System;
using Simulator;
using Simulator.Parsers;
using Simulator.Resources;
using Simulator.Testing;
using System.IO;

namespace HelicopterSim
{
    internal static class TestingSystem
    {
        private const RenderModes RenderMode = RenderModes.Normal;

        [STAThread]
        public static void Main(string[] args)
        {
            Run(new[] {"-test", "TODO"});
        }

        internal static void Run(string[] runtimeArgs)
        {
            if (runtimeArgs == null || runtimeArgs.Length != 3)
            {
                MessageBox.Show(@"Expected arguments to be '-test <config file path> <output directory path>'.");
                return;
            }

            string testConfigurationFilePath = runtimeArgs[1];
            if (!File.Exists(testConfigurationFilePath))
            {
                MessageBox.Show(@"No test configuration file found at: " + testConfigurationFilePath);
                return;
            }

            string relativeOutputPath = runtimeArgs[2];

            try
            {
                TestConfiguration testConf = SimulatorResources.GetTestConfiguration(testConfigurationFilePath);
                var simSettings = new SimulationSettings();
                simSettings.RenderMode = RenderModes.Normal;

                // Start the simulator game entry point
                using (var game = new SimulatorGame(simSettings, IntPtr.Zero, testConf, relativeOutputPath)) 
                {
                    // Enables cameras (poor design, I know..)
                    game.IsCapturingMouse = true;

                    // Make sure the XNA game window (left eye) displays as a full screen window
                    // just as the one we just created for the right eye
                    var gameForm = (Form)System.Windows.Forms.Control.FromHandle(game.Window.Handle);
                    gameForm.FormBorderStyle = FormBorderStyle.None;
                    gameForm.Text = @"A²DS Test Mode";

                    gameForm.LostFocus += (sender, e) => game.IsCapturingMouse = false;
                    gameForm.GotFocus += (sender, e) => game.IsCapturingMouse = true;

                    // Run the game code
                    game.Run();
                }

                string configfileCopy = Path.Combine(relativeOutputPath, @"TestConfiguration.xml");
                File.Copy(testConfigurationFilePath, configfileCopy);
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Error occured." + e);
            }
        }
    }
}
#endif