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

using Microsoft.Xna.Framework;

#endregion

namespace Sensors.Model
{
    public struct GPSOutput
    {
        public Vector3 Velocity;
        public Vector3 Position;

        public GPSOutput(Vector3 position, Vector3 velocity)
        {
            Position = position;
            Velocity = velocity;
        }
    }
}