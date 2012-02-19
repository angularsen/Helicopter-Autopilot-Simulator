#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

namespace Control.Common
{
    public struct JoystickOutput
    {
        public float Pitch;
        public float Roll;
        public float Throttle;
        public float Yaw;

        public JoystickOutput(float pitch, float roll, float yaw, float throttle)
        {
            Pitch = pitch;
            Roll = roll;
            Yaw = yaw;
            Throttle = throttle;
        }
    }
}