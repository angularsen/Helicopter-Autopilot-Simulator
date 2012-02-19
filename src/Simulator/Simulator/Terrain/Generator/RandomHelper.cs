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

#endregion

namespace NINFocusOnTerrain
{
    /// <summary>
    /// Random helper class.
    /// </summary>
    public class RandomHelper
    {
        /// <summary>
        /// Get the random number generator.
        /// </summary>
        public Random Random { get; private set; }

        public RandomHelper() 
        {
            Random = new Random();
        }

        public RandomHelper(int randomSeed)
        {
            Random = new Random(randomSeed);
        }

        /// <summary>
        /// Return a float between min and max.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public float GetFloatInRange(float min, float max)
        {
            return min + (max - min)*(float) Random.NextDouble();
        }

        /// <summary>
        /// Return some noise for a specific seed.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static float Noise(int i)
        {
            i = (i << 13) ^ i;

            return (1.0f - ((i*(i*i*15731 + 789221) + 1376312589) & 0x7FFFFFFF)/1073741824.0f);
        }
    }
}

/*======================================================================================================================

								    NIN - Nerdy Inverse Network - http://nerdy-inverse.com

======================================================================================================================*/