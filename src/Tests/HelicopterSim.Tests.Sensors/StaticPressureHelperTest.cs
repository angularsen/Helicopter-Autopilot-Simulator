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
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Sensors.Helpers;

#endregion

namespace HelicopterSim.Tests.Sensors
{
    [TestFixture]
    public class StaticPressureHelperTest
    {
        private const float SeaLevelStaticPressure = StaticPressureHelper.DefaultSeaLevelStaticPressure;


        private static void AssertWithin(float value, float reference, float maxError)
        {
            bool result = (value >= reference - maxError) && value <= (reference + maxError);
            Assert.IsTrue(result, String.Format("{0} not within {1} by a max error of {2}.", value, reference, maxError));
        }

        private static void ConversionIntegrityTest(float altitude)
        {
            float derivedPressure = StaticPressureHelper.GetPressure(altitude, SeaLevelStaticPressure);
            float derivedAltitude = StaticPressureHelper.GetAltitude(derivedPressure, SeaLevelStaticPressure);
            AssertWithin(derivedAltitude, altitude, 0.001f);
        }

        [Test]
        public void ConversionIntegrityTest()
        {
            float inBetween = MathHelper.Lerp(StaticPressureHelper.MinAltitude, StaticPressureHelper.MaxAltitude, 0.5f);
            ConversionIntegrityTest(StaticPressureHelper.MinAltitude);
            ConversionIntegrityTest(StaticPressureHelper.MaxAltitude);
            ConversionIntegrityTest(inBetween);
        }

        [Test]
        public void GetAltitudeTest()
        {
            float pressure, expected, actual;


            // All expected values are taken from a lookup table
            pressure = 22632;
            expected = 11000;
            actual = StaticPressureHelper.GetAltitude(pressure, SeaLevelStaticPressure);
            AssertWithin(actual, expected, 100);

            pressure = 101325;
            expected = 0;
            actual = StaticPressureHelper.GetAltitude(pressure, SeaLevelStaticPressure);
            AssertWithin(actual, expected, 100);

            pressure = 54019;
            expected = 5000;
            actual = StaticPressureHelper.GetAltitude(pressure, SeaLevelStaticPressure);
            AssertWithin(actual, expected, 100);
        }

        [Test]
        public void GetPressureTest()
        {
            float altitude, expected, actual;

            // All expected values are taken from a lookup table
            altitude = 11000f;
            expected = 22632f;
            actual = StaticPressureHelper.GetPressure(altitude, SeaLevelStaticPressure);
            AssertWithin(actual, expected, 100);

            altitude = 0f;
            expected = 101325f;
            actual = StaticPressureHelper.GetPressure(altitude, SeaLevelStaticPressure);
            AssertWithin(actual, expected, 100);

            altitude = 5000f;
            expected = 54019f;
            actual = StaticPressureHelper.GetPressure(altitude, SeaLevelStaticPressure);
            AssertWithin(actual, expected, 100);
        }
    }
}