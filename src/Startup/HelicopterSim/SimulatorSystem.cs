#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

#region Using

#if !XBOX
using System.Drawing;
using System.Windows.Forms;
using Rectangle = System.Drawing.Rectangle;
#endif

using System;
using Microsoft.Xna.Framework;
using Simulator;
using Simulator.Parsers;
using Simulator.Resources;

#endregion

namespace HelicopterSim
{
    internal static class SimulatorSystem
    {

#if XBOX
        public static void Run()
        {
            try
            {
//            SimulationSettings simSettings = SimulatorResources.GetSimulationSettings();
                var simSettings = new SimulationSettings();
                simSettings.RenderMode = RenderModes.Normal;

                // Start the simulator game entry point
                using (var game = new SimulatorGame(simSettings, IntPtr.Zero))
                {
                    game.Run();
                }
            }
            catch (Exception e)
            {
                string msg = "Error occured!\n\n" + e;
                Console.WriteLine(e);
//                MessageBox.Show(msg);
            }
        }
#else
        private static SettingsController _settingsController;

        public static void Run()
        {
            try
            {
                int numScreens = Screen.AllScreens.Length;
                if (numScreens != 1 && numScreens != 2)
                    throw new NotImplementedException("Only supports single and dual monitor setup!");

                SimulationSettings simSettings = SimulatorResources.GetSimulationSettings();

                if (simSettings.RenderMode == RenderModes.Stereo && numScreens < 2)
                {
                    Console.WriteLine(@"Could not run stereo mode. Can't find two monitors connected?");
                    simSettings.RenderMode = RenderModes.Normal;
                }

                IntPtr rightEyeWindow = IntPtr.Zero;
                if (simSettings.RenderMode == RenderModes.Stereo)
                    rightEyeWindow = SpawnRightEyeWindow();

                // Start the simulator game entry point
                using (var game = new SimulatorGame(simSettings, rightEyeWindow))
                {
                    // Make sure the XNA game window (left eye) displays as a full screen window
                    // just as the one we just created for the right eye
                    var gameForm = (Form) System.Windows.Forms.Control.FromHandle(game.Window.Handle);
                    gameForm.FormBorderStyle = FormBorderStyle.None;
                    gameForm.LostFocus += (sender, e) => game.IsCapturingMouse = false;
                    gameForm.GotFocus += (sender, e) => Game_GotFocus(game);
                    gameForm.MouseClick += (sender, e) =>
                                               {
                                                   if (e.Button == MouseButtons.Right)
                                                       Game_MouseRightClick(game);
                                               };

                    // Name the window according to mode
                    gameForm.Text = (simSettings.RenderMode == RenderModes.Stereo)
                                        ? "A²DS Stereoscopy (left eye)"
                                        : "A²DS";

                    _settingsController = new SettingsController(gameForm);

                    // Hook the settings controller to the simulator by events, so changes in PID settings causes
                    // autopilot to use new PID settings.
                    _settingsController.PIDSettingsChanged +=
                        () => game.SetPIDSetup(_settingsController.CurrentPIDSetup);

                    // Run the game code
                    game.Run();
                }
            }
            catch (Exception e)
            {
                string msg = "Error occured!\n\n" + e;
                Console.WriteLine(e);
                MessageBox.Show(msg);
            }
        }

        private static void Game_MouseRightClick(Game game)
        {
            game.IsMouseVisible = true;

            _settingsController.ShowSettingsDialog();
        }

        /// <summary>
        /// Fired when game once again receives focus (by clicking, alt-tabbing...)
        /// </summary>
        /// <param name="game"></param>
        private static void Game_GotFocus(SimulatorGame game)
        {
            // Allow cameras (among other things) to start capturing the mouse again
            game.IsCapturingMouse = true;

            // Hide settings form and mouse cursor
            game.IsMouseVisible = false;

            _settingsController.HideSettingsDialog();
        }

        private static IntPtr SpawnRightEyeWindow()
        {
            // Create a separate window for the right eye
            var form = new Form
                           {
                               Text = @"A²DS Stereoscopy (right eye)",
                               ShowInTaskbar = true,
                               FormBorderStyle = FormBorderStyle.None
                           };
            form.Show();

            // Place the window in the secondary monitor in semi-fullscreen
            foreach (Screen screen in Screen.AllScreens)
                if (screen != Screen.PrimaryScreen)
                {
                    // Set the window to fullscreen size
                    form.ClientSize = new Size(screen.Bounds.Width, screen.Bounds.Height);
                    Rectangle newBounds = form.Bounds;

                    // Place the window in the top-left corner of the screen
                    newBounds.X = screen.Bounds.Left;
                    newBounds.Y = screen.Bounds.Top;
                    form.Bounds = newBounds;
                }

            return form.Handle;
        }
#endif

    }
}