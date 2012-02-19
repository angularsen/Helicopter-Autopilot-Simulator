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
using Microsoft.Xna.Framework;

#endregion

namespace Simulator.Tracking
{
    public struct TrackedObject
    {
        public Vector3 Acceleration;
        public Quaternion Orientation;
        public Vector3 Position;
        public DateTime Time;
        public Vector3 Velocity;

//        public TrackedObject(Vector3 position, Vector3 velocity, Vector3 acceleration, Quaternion orientation, DateTime time)
//        {
//            Position = position;
//            Velocity = velocity;
//            Acceleration = acceleration;
//            Orientation = orientation;
//            Time = time;
//        }
    }
}