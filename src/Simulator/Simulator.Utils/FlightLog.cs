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
using Control.Common;
using State.Model;

#endregion

namespace Simulator.Utils
{
    public class FlightLog
    {
        public IEnumerable<HelicopterLogSnapshot> Plots;
        public IEnumerable<Waypoint> Waypoints;

        public FlightLog()
        {
            Plots = new List<HelicopterLogSnapshot>();
            Waypoints = new List<Waypoint>();
        }
    }
}