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

using System;
using Control;
using System.Text;

#endregion

namespace Simulator.Testing
{
    public class AutopilotConfiguration
    {
        public float MaxHVelocity;
        public PIDSetup PIDSetup;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Autopilot MaxHVelocity: {0} km/h \r\n", Math.Round(MaxHVelocity*3.6, 1));
            sb.AppendFormat("Autopilot PIDSetup: {0}", PIDSetup);
            return sb.ToString();
        }
    }
}