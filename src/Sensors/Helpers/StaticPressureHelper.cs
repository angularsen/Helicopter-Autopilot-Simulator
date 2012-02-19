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

namespace Sensors.Helpers
{
    /// <summary>
    /// Simulates the measured atmospheric pressure, including errors and atmospheric variations.
    /// </summary>
    public static class StaticPressureHelper
    {
        public const float DefaultSeaLevelStaticPressure = 101325;
        public const float MaxAltitude = 11000;
        public const float MinAltitude = 0;

        #region Private constants and default values

        private const float MinPressure = 22632;

        /// <summary>
        /// Standard temperature lapse rate [K/m]. 
        /// </summary>
        private const float Lb = -0.00649f;

        /// <summary>
        /// Standard temperature [K]
        /// </summary>
        private const float Tb = 288.15f;

        /// <summary>
        /// Bottom layer height [m].
        /// </summary>
        private const float hb = 0;

        /// <summary>
        /// Universal gas constant [Nm/(K·mol)].
        /// </summary>
        private const float R = 8.31432f;

        /// <summary>
        /// Standard gravity [m/s²].
        /// </summary>
        private const float g0 = 9.80665f;

        /// <summary>
        /// Molar mass of Earth's air [kg/mol].
        /// </summary>
        private const float M = 0.0289644f;

        #endregion

        #region Helper methods

        /// <summary>
        /// Helper method to calculate the altitude above sea level from atmospheric pressure.
        /// </summary>
        /// <param name="staticPressure">Pressure in pascals</param>
        /// <param name="seaLevelStaticPressure"></param>
        /// <returns></returns>
        public static float GetAltitude(float staticPressure, float seaLevelStaticPressure)
        {
            // Limit is imposed by the fact that this equation only holds for b=0 of the barometric formula
            float Pb = seaLevelStaticPressure;
            float p = staticPressure;
            if (p < MinPressure)
                throw new ArgumentException("Can't calculate for higher than 11 000 meters (less than " + MinPressure +
                                            " pascals)!");

            // Equation: http://en.wikipedia.org/wiki/Atmospheric_pressure 
            // Modified to solve for altitude h.
            var A = (float) Math.Exp(Math.Log(p/Pb)*(R*Lb)/(g0*M));
            float h = (Tb - Tb*A)/(Lb*A) + hb;
            return h;
        }

        /// <summary>
        /// Helper method to calculate pressure from altitude above sea level.
        /// </summary>
        /// <param name="altitudeAboveSeaLevel"></param>
        /// <param name="seaLevelStaticPressure"></param>
        /// <returns></returns>
        public static float GetPressure(float altitudeAboveSeaLevel, float seaLevelStaticPressure)
        {
            // Limit is imposed by the fact that this equation only holds for b=0 of the barometric formula
            float Pb = seaLevelStaticPressure;
            float h = altitudeAboveSeaLevel;
            if (h > 11000)
                throw new ArgumentException("Can't calculate for higher than 11 000 meters (22632 pascals)!");

            // Equation: http://en.wikipedia.org/wiki/Atmospheric_pressure
            float p = seaLevelStaticPressure*(float) Math.Pow(Tb/(Tb + Lb*(h - hb)), g0*M/R/Lb);
            return p;
        }

        #endregion
    }
}