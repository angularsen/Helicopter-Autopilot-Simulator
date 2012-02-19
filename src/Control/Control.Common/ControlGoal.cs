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
    public struct ControlGoal
    {
        public float Altitude;
        public float HVelocity;
        public float HeadingAngle;
        public float PitchAngle;
        public float RollAngle;
    }
}