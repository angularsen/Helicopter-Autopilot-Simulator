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

namespace State.Model
{
    public struct AngularValues
    {
        public PitchRollYaw Degrees;
        public PitchRollYaw Radians;

        public static AngularValues FromRadians(PitchRollYaw radians)
        {
            return new AngularValues
                       {
                           Radians = radians,
                           Degrees = new PitchRollYaw(
                               MathHelper.ToDegrees(radians.Pitch),
                               MathHelper.ToDegrees(radians.Roll),
                               MathHelper.ToDegrees(radians.Yaw))
                       };
        }

        public static AngularValues FromDegrees(PitchRollYaw degrees)
        {
            return new AngularValues
                       {
                           Degrees = degrees,
                           Radians = new PitchRollYaw(
                               MathHelper.ToRadians(degrees.Pitch),
                               MathHelper.ToRadians(degrees.Roll),
                               MathHelper.ToRadians(degrees.Yaw))
                       };
        }
    }
}