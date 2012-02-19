using System;
using Anj.Helpers.XNA;
using Control.Common;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Physics;
using Physics.Common;
using Simulator;
using Simulator.Parsers;

namespace HelicopterSim.Tests
{
    [TestFixture]
    public class JitterPhysicsTest
    {
        private TerrainCollision _collision;

        private TimestepResult ByPhysics(TimestepStartingCondition startCondition, JoystickOutput output,
                                         TimeSpan stepEndTime, float dt)
        {
            Vector3 newPosition;
            Vector3 newVelocity;
            ClassicalMechanics.ConstantAcceleration(
                dt,
                startCondition.Position,
                startCondition.Velocity,
                startCondition.Acceleration,
                out newPosition, out newVelocity);


            Quaternion newOrientation, afterJoystick;
            RotateByJoystickInput(startCondition.Orientation, dt, output, out afterJoystick);
            newOrientation = afterJoystick;
            return new TimestepResult(newPosition, newVelocity, newOrientation, stepEndTime);
        }

        /// <summary>
        ///   Rotate helicopter entirely by joystick input (major simplification of physics).
        /// </summary>
        /// <param name="currentOrientation"></param>
        /// <param name="dt"></param>
        /// <param name="output"></param>
        /// <param name="newOrientation"></param>
        private static void RotateByJoystickInput(Quaternion currentOrientation, float dt, JoystickOutput output,
                                                  out Quaternion newOrientation)
        {
            // Move helicopter from previous state according to user/autopilot input
            // Change roll, pitch and yaw according to input
            // Invert yaw, so positive is clockwise
            float deltaRoll = output.Roll*PhysicsConstants.MaxRollRate*dt;
            float deltaPitch = output.Pitch*PhysicsConstants.MaxPitchRate*dt;
            float deltaYaw = output.Yaw*PhysicsConstants.MaxYawRate*dt;

            // Add deltas of pitch, roll and yaw to the rotation matrix
            newOrientation = VectorHelper.AddPitchRollYaw(currentOrientation, deltaPitch, deltaRoll, deltaYaw);

            // Get axis vectors for the new orientation
            //            newAxes = VectorHelper.GetAxes(newOrientation);
        }

        private TimestepResult ByJitter(TimestepStartingCondition startCondition, JoystickOutput output,
                                        TimeSpan stepEndTime, float dt)
        {
            // Note we override gravity here because the gravity acceleration is already accounted for in startCondition.Acceleration vector
            _collision.SetGravity(Vector3.Zero);

            float heliMass = _collision.HelicopterBody.Mass;
            _collision.HelicopterBody.Position = Conversion.ToJitterVector(startCondition.Position);
            _collision.HelicopterBody.LinearVelocity = Conversion.ToJitterVector(startCondition.Velocity);
            _collision.HelicopterBody.AddForce(Conversion.ToJitterVector(heliMass*startCondition.Acceleration));

            var localAngVelocity = new Vector3(
                output.Pitch*PhysicsConstants.MaxPitchRate,
                output.Yaw*PhysicsConstants.MaxYawRate,
                -output.Roll*PhysicsConstants.MaxRollRate);

            Vector3 worldAngVelocity = VectorHelper.MapFromWorld(localAngVelocity, startCondition.Orientation);

            _collision.HelicopterBody.AngularVelocity = Conversion.ToJitterVector(worldAngVelocity);

            // Simulate physics
            //                Vector3 preForward = Vector3.Transform(Vector3.Forward, startCondition.Orientation);
            _collision.World.Step(dt, false);

            // TODO Testing with Jitter Physics
            return new TimestepResult(
                Conversion.ToXNAVector(_collision.HelicopterBody.Position),
                Conversion.ToXNAVector(_collision.HelicopterBody.LinearVelocity),
                Quaternion.CreateFromRotationMatrix(Conversion.ToXNAMatrix(_collision.HelicopterBody.Orientation)),
                stepEndTime);

            //                Vector3 postForward = Vector3.Transform(Vector3.Forward, result.Orientation);
        }

        [Test]
        [STAThread]
        public void Test()
        {
            var simSettings = new SimulationSettings();
            simSettings.RenderMode = RenderModes.Normal;

            var game = new SimulatorGame(simSettings, IntPtr.Zero);
            _collision = new TerrainCollision(game, game);
            _collision.Initialize();
            _collision.TmpBuildScene();

            // Find new position and velocity from a constant acceleration over timestep
            const float dt = 0.017f;

            var a = new Vector3(0, 9.81f, 0);
            var startCondition1 = new TimestepStartingCondition(Vector3.Zero, Vector3.Zero, a,
                                                                Quaternion.Identity, TimeSpan.Zero);
            TimestepStartingCondition startCondition2 = startCondition1;

            var joystickOutput = new JoystickOutput(0.1f, 0.1f, 0, 0.5f);

            for (int i = 0; i < 100; i++)
            {
                TimestepResult jitterResult = ByJitter(startCondition1, joystickOutput,
                                                       startCondition1.StartTime + TimeSpan.FromSeconds(dt), dt);

                TimestepResult physicsResult = ByPhysics(startCondition2, joystickOutput,
                                                         startCondition2.StartTime + TimeSpan.FromSeconds(dt), dt);

                Vector3 dPos = jitterResult.Position - physicsResult.Position;
                Vector3 dVel = jitterResult.Velocity - physicsResult.Velocity;


                if (jitterResult.Orientation != physicsResult.Orientation)
                {
                    float dPitchDeg =
                        MathHelper.ToDegrees(VectorHelper.GetPitchAngle(jitterResult.Orientation) -
                                             VectorHelper.GetPitchAngle(physicsResult.Orientation));

                    float dRollDeg =
                        MathHelper.ToDegrees(VectorHelper.GetRollAngle(jitterResult.Orientation) -
                                             VectorHelper.GetRollAngle(physicsResult.Orientation));

                    float dYawDeg =
                        MathHelper.ToDegrees(VectorHelper.GetHeadingAngle(jitterResult.Orientation) -
                                             VectorHelper.GetHeadingAngle(physicsResult.Orientation));


                    Console.WriteLine("YPR delta " + dPitchDeg + " " + dRollDeg + " " + dYawDeg);
                }

                TimeSpan nextStartTime = physicsResult.EndTime;
                startCondition1 = new TimestepStartingCondition(jitterResult.Position, jitterResult.Velocity,
                                                                a, jitterResult.Orientation, nextStartTime);

                startCondition2 = new TimestepStartingCondition(physicsResult.Position, physicsResult.Velocity,
                                                                a, physicsResult.Orientation, nextStartTime);
            }
        }
    }
}