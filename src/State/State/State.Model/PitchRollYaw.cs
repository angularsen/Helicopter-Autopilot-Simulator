#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

namespace State.Model
{
    public struct PitchRollYaw
    {
        public float Pitch;
        public float Roll;
        public float Yaw;

        public PitchRollYaw(float pitch, float roll, float yaw)
        {
            Pitch = pitch;
            Roll = roll;
            Yaw = yaw;
        }
    }
}