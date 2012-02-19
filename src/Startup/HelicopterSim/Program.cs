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
using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Simulator;
using Rectangle = System.Drawing.Rectangle;
#endif

#endregion

namespace HelicopterSim
{
    internal static class Program
    {

#if !XBOX
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "-test")
                TestingSystem.Run(args);
            else   
                SimulatorSystem.Run();
        }
#else
        private static void Main(string[] args)
        {
            SimulatorSystem.Run();
        }
#endif
    }
}