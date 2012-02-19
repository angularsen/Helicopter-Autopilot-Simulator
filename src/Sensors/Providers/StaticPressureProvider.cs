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
using Sensors.Helpers;
using Sensors.SampleDistortion;

#endregion

namespace Sensors.Providers
{
    public class StaticPressureProvider
    {
        public StaticPressureProvider()
        {
            // Default value
            SeaLevelStaticPresure = 101325;
        }

        public float SeaLevelStaticPresure { get; set; }

        public float GetSimulatedStaticPressure(float trueAltitude)
        {
            float truePressure = StaticPressureHelper.GetPressure(trueAltitude, SeaLevelStaticPresure);

            // If we assume the pressure is rising linearly (although it is not) we would want to further distort the
            // measured pressure by applying non-linearity to the readings.
            Vector2 lowestPressurePlot = GetPressurePlot(StaticPressureHelper.MinAltitude);
            Vector2 highestPressurePlot = GetPressurePlot(StaticPressureHelper.MaxAltitude);
            var pressurePlot = new Vector2(trueAltitude, truePressure);
//            float nonLinearPressure = NonLinearity.SinusOffset(pressurePlot, lowestPressurePlot, highestPressurePlot).Y;
            float nonLinearPressure =
                NonLinearity.SinusOffset(pressurePlot, lowestPressurePlot, highestPressurePlot, 5f, 100).Y;

            // Noise amplitude is selected to give a few meters of variation, both positive and negative
            const float pressurePerMeter = 11.989f;
            const float noiseAmplitudeMeters = 0.5f;
            const float noiseAmplitudePressure = noiseAmplitudeMeters*pressurePerMeter;
            return Noise.WhiteNoise(nonLinearPressure, noiseAmplitudePressure);
        }

        private Vector2 GetPressurePlot(float altitude)
        {
            return new Vector2(altitude, StaticPressureHelper.GetPressure(altitude, SeaLevelStaticPresure));
        }
    }
}