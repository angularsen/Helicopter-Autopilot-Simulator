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

namespace Sensors.SampleDistortion
{
    public static class Noise
    {
        private static readonly Random Rand;

        static Noise()
        {
            Rand = new Random();
        }

        public static float WhiteNoise(float value, float noiseAmplitude)
        {
            var randomPlusMinusOne = (float) (2*Rand.NextDouble() - 1);
            return value + noiseAmplitude*randomPlusMinusOne;
        }
    }
}