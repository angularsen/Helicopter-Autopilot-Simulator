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
    public static class ClassicalMechanics
    {
        /// <summary>
        ///   Calculates new position and velocity as a result of constant acceleration over a timestep.
        /// </summary>
        /// <param name="dt">
        ///   Timestep in fraction of a second
        /// </param>
        /// <param name="p0">Start position</param>
        /// <param name="v0">Start velocity</param>
        /// <param name="a">
        ///   Constant acceleration throughout timestep
        /// </param>
        /// <param name="p">
        ///   Resulting position after timestep
        /// </param>
        /// <param name="v">
        ///   Resulting velocity after timestep
        /// </param>
        public static void ConstantAcceleration(float dt, Vector3 p0, Vector3 v0, Vector3 a, out Vector3 p,
                                                out Vector3 v)
        {
            p = p0 + v0*dt + 0.5f*a*dt*dt;
            v = v0 + a*dt;
        }
    }
}