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

#endregion

namespace HelicopterSim.Tests
{
    [TestFixture]
    public class QuaternionTest
    {
        private static bool AreEqual(float left, float right, float errorMargin)
        {
            return
                left >= (right - errorMargin) &&
                left <= (right + errorMargin);
        }

        [Test]
        public void Test()
        {
            Matrix startOrientation = Matrix.CreateFromYawPitchRoll(0, 0, 0);

            Quaternion pitchRotation = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), MathHelper.ToRadians(90));
            Quaternion yawRotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(-90));
//            var rollRotation = Quaternion.CreateFromAxisAngle(bodyAxes.Forward, rollDelta);

            Quaternion orientation = Quaternion.CreateFromRotationMatrix(startOrientation);
            orientation *= pitchRotation*yawRotation;
//            orientation = Quaternion.Concatenate(orientation, rollRotation);

            var v = new Vector3(0, 0, -1);
            Console.WriteLine("Expected " + new Vector3(1, 0, 0));
            Console.WriteLine("Got " + Vector3.Transform(v, orientation));
        }

        [Test]
        public void TestBodyToWorld()
        {
            Quaternion orientation = Quaternion.CreateFromYawPitchRoll(0.312f, 1.4423f, 0.7123f);
            var gravityWorld = new Vector3(0, -9.81f, 0);
            Vector3 gravityBF = Vector3.Transform(gravityWorld, orientation);
            Vector3 derivedGravityWorld = Vector3.Transform(gravityBF, Quaternion.Inverse(orientation));

            Assert.That(AreEqual(derivedGravityWorld.X, gravityWorld.X, 0.00001f));
            Assert.That(AreEqual(derivedGravityWorld.Y, gravityWorld.Y, 0.00001f));
            Assert.That(AreEqual(derivedGravityWorld.Z, gravityWorld.Z, 0.00001f));
        }
    }
}