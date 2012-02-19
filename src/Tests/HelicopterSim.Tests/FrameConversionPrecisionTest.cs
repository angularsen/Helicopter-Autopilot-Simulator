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
using Anj.Helpers.XNA;
using Microsoft.Xna.Framework;
using NUnit.Framework;

#endregion

namespace HelicopterSim.Tests
{
    [TestFixture]
    public class FrameConversionPrecisionTest
    {
        private static void TestOrientation(Vector3 vector, float yaw, float pitch, float roll)
        {
            // Some arbitrary orientation different than the Cartesian coordinate system
            Matrix orientation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

            // Convert to body frame (to get the values the accelerometers measure)
            float Xbf = VectorHelper.Project(vector, orientation.Right);
            float Ybf = VectorHelper.Project(vector, orientation.Up);
            float Zbf = VectorHelper.Project(vector, orientation.Forward);
            var gravityBF = new Vector3(Xbf, Ybf, Zbf);

            // Convert back to Cartesian coordinate system
            Vector3 gravityResult = VectorHelper.MapToWorld(gravityBF, orientation);
            Vector3 result = gravityResult - vector;

            Vector3 gravityResult2 = Vector3.Transform(Vector3.Transform(vector, orientation),
                                                       Matrix.Invert(orientation));
            Vector3 result2 = gravityResult2 - vector;

            Console.WriteLine("Body frame pitch({0}) roll({1}) yaw({2}) gave differences of {3} and {4}.", pitch, roll,
                              yaw, result, result2);
        }

        [Test]
        public void CartesianToBodyFrameTest()
        {
            Vector3 gravity = 9.81f*new Vector3(1.1f, 0.8f, 3f);

            Console.WriteLine("Converting vector {0} to body frame and back.", gravity);

            TestOrientation(gravity, 0, 0, 0);
            TestOrientation(gravity, 1, 0, 0);
            TestOrientation(gravity, 0, 1, 0);
            TestOrientation(gravity, 0, 1, 1);
            TestOrientation(gravity, 2.1f, 0.6f, 1.7f);
        }
    }
}