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

using Anj.Helpers.XNA;
using Control.Common;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Physics.Common;

#endregion

namespace HelicopterSim.Tests
{
    [TestFixture]
    internal class QuaternionPrecisionTest
    {
        [Test]
        public void Test()
        {
            JoystickOutput output;
            output.Pitch = 0.312312432f;
            output.Roll = 0.512312432f;
            output.Yaw = 0.912312432f;
            const float dt = 0.017001f;

            float pitchRate = output.Pitch*PhysicsConstants.MaxPitchRate;
            float rollRate = output.Roll*PhysicsConstants.MaxRollRate;
            float yawRate = output.Yaw*PhysicsConstants.MaxYawRate;

            Quaternion orient1 = Quaternion.Identity;
            Quaternion orient2 = Quaternion.Identity;

            for (int i = 0; i < 10000; i++)
            {
                float deltaPitch = (output.Pitch*PhysicsConstants.MaxPitchRate)*dt;
                float deltaRoll = (output.Roll*PhysicsConstants.MaxRollRate)*dt;
                float deltaYaw = (output.Yaw*PhysicsConstants.MaxYawRate)*dt;

                // Add deltas of pitch, roll and yaw to the rotation matrix
                orient1 = VectorHelper.AddPitchRollYaw(orient1, deltaPitch, deltaRoll, deltaYaw);

                deltaPitch = pitchRate*dt;
                deltaRoll = rollRate*dt;
                deltaYaw = yawRate*dt;
                orient2 = VectorHelper.AddPitchRollYaw(orient2, deltaPitch, deltaRoll, deltaYaw);
            }

            Assert.AreEqual(orient1.X, orient2.X, "X");
            Assert.AreEqual(orient1.Y, orient2.Y, "Y");
            Assert.AreEqual(orient1.Z, orient2.Z, "Z");
            Assert.AreEqual(orient1.W, orient2.W, "W");
        }
    }
}