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

namespace Physics.Common
{
    public static class PhysicsConstants
    {
        public const float MaxPitchRate = MathHelper.Pi;
        public const float MaxRollRate = MathHelper.Pi;
        public const float MaxYawRate = MathHelper.Pi;

        /// <summary>
        /// Reference: http://en.wikipedia.org/wiki/Earth's_gravity
        /// </summary>
        public static readonly Vector3 Gravity = new Vector3(0, -9.80665f, 0);
    }
}