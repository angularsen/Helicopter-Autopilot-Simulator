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
using NUnit.Framework;
using Sensors.SampleDistortion;

#endregion

namespace HelicopterSim.Tests.Sensors
{
    [TestFixture]
    public class NonLinearityTest
    {
        private static void TestCurve(Vector2 start, Vector2 end)
        {
            const float amplitude = 1;
            const float waveLength = 1;

            // Linear and non-linear curves should start at same position
            float progress = 0;
            Vector2 lerpValue = Vector2.Lerp(start, end, progress);
            Vector2 sCurveValue = NonLinearity.SinusOffset(lerpValue, start, end, amplitude, waveLength);
            Assert.AreEqual(sCurveValue.Y, lerpValue.Y);

//             S-Curve should start out below the linear ascent
            progress = 0.25f;
            lerpValue = Vector2.Lerp(start, end, progress);
            sCurveValue = NonLinearity.SinusOffset(lerpValue, start, end, amplitude, waveLength);
            Assert.Less(sCurveValue.Y, lerpValue.Y);

//             After catching up the S-Curve should now be above the linear interpolation
            progress = 0.75f;
            lerpValue = Vector2.Lerp(start, end, progress);
            sCurveValue = NonLinearity.SinusOffset(lerpValue, start, end, amplitude, waveLength);
            Assert.Greater(sCurveValue.Y, lerpValue.Y);

//             Linear and non-linear curves should end at same position
            progress = 1.0f;
            lerpValue = Vector2.Lerp(start, end, progress);
            sCurveValue = NonLinearity.SinusOffset(lerpValue, start, end, amplitude, waveLength);
            Assert.AreEqual(sCurveValue.Y, lerpValue.Y);
        }

        [Test]
        public void SCurveTest()
        {
//             Testing S-curve against a linear interpolation.
//             It is a requirement that the linear interpolation has increasing X and Y values.
            TestCurve(new Vector2(0, 0), new Vector2(100, 100));
            TestCurve(new Vector2(0, 0), new Vector2(100, 200));
            TestCurve(new Vector2(-100, -50), new Vector2(100, 200));
        }
    }
}