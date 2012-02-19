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

using System.Collections.Generic;
using Anj.XNA.Joysticks.Wizard;

#endregion

namespace Anj.XNA.Joysticks
{
    public struct Axis
    {
        public JoystickAxis Name;
        public JoystickAxisAction Action;
        public bool IsInverted;
    }

    public class JoystickDevice
    {
        public readonly IList<Axis> Axes;
        public string Name;

        public JoystickDevice()
        {
            Axes = new List<Axis>();
        }
    }

    public class JoystickSetup
    {
        public readonly IList<JoystickDevice> Devices;
        public string Name;

        public JoystickSetup()
        {
            Devices = new List<JoystickDevice>();
        }
    }
}