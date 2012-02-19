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
using Anj.Helpers.NET;
using Anj.Helpers.XNA;
using Control.Common;
using Microsoft.Xna.Framework;
using Physics.Common;
using State.Model;

#endregion

namespace Physics
{
    /// <summary>
    ///   Resources:
    ///   - Drag and air resistance
    ///   http://en.wikipedia.org/wiki/Drag_(physics)
    ///   http://en.wikipedia.org/wiki/Drag_equation
    /// 
    ///   - Principles of helicopter aerodynamics (J. Gordon Leishman)
    ///   http://books.google.no/books?id=nMV-TkaX-9cC&printsec=frontcover 
    ///   Preview page 218 chapter 6.5 about how rotor wind pulls fuselage down towards ground
    ///   by as much as 5% of fuselage weight.
    ///   Tail rotor typically uses ~10% of power.
    /// </summary>
    public class HeliPhysics : IHeliPhysics
    {
        private readonly TerrainCollision _collision;
        private readonly bool _useTerrainCollision;
        private const float Mass = 0.7f; // kilos

        private bool _isInitialized;
        private TimestepResult _prevTimestepResult;

        public HeliPhysics(TerrainCollision collision, bool useTerrainCollision)
        {
            _collision = collision;
            _useTerrainCollision = useTerrainCollision;
        }

        private static float Weight
        {
            get { return Mass*PhysicsConstants.Gravity.Length(); }
        }

        private static float MaxThrust
        {
            get { return 1.7f*Weight; }
        }

        #region IHeliPhysics Members

        /// <summary>Calculates the input and external forces.</summary>
        /// <returns>The new orientation and the total acceleration for this orientation in this timestep.</returns>
        public SimulationStepResults PerformTimestep(PhysicalHeliState prev, JoystickOutput output, TimeSpan stepDuration,
                                                     TimeSpan stepEndTime)
        {
            if (!_isInitialized)
            {
//                startCondition.Acceleration = CalculateAcceleration(startCondition.Orientation, startCondition.Velocity, input);
                _prevTimestepResult = new TimestepResult(prev.Position, prev.Velocity, prev.Orientation,
                                                         stepEndTime - stepDuration);

                _isInitialized = true;
            }

            // If the number of substeps is 0 then only the state at the end of the timestep will be calculated.
            // If the number is greater than 1, then the timestep will 1 then a substep will be calculated in the middle of the timestep.
            const int substeps = 0;
            if (substeps < 0)
                throw new Exception("The number of substeps is invalid.");

            TimeSpan substepDuration = stepDuration.Divide(1 + substeps);

            Vector3 initialAcceleration = CalculateAcceleration(prev.Orientation, prev.Velocity, output);
            var initialState = new TimestepStartingCondition(_prevTimestepResult, initialAcceleration);
                //_prevTimestepResult.Result;
//            var substepResults = new List<SubstepResults> {initialState};

            // We always need to calculate at least the timestep itself, plus any optional substeps.
            // Substeps are used to provide sensors with a higher frequency of data than the simulator is capable of rendering real-time.
//            const int stepsToCalculate = substeps + 1;   
//            SubstepResults prevSubstep = initialState;
//            for (int i = 0; i < stepsToCalculate; i++)
//            {
//                prevSubstep.Acceleration = CalculateAcceleration(prevSubstep.Orientation, prevSubstep.Velocity, input);
//                SubstepResults r = SimulateStep(prevSubstep, prevSubstep.Acceleration, input, substepDuration, stepEndTime);
//
//                substepResults.Add(r);
//                prevSubstep = r;
//            }


            TimestepResult result = SimulateStep(initialState, output, substepDuration, stepEndTime);
            //new SimulationStepResults(stepDuration, substepDuration, substepResults);
            _prevTimestepResult = result;

//            DebugInformation.Time1 = stepEndTime;
//            DebugInformation.Q1 = result.Orientation;
//
//            DebugInformation.Vectors["Pos"] = result.Position;
//            DebugInformation.Vectors["Vel"] = result.Velocity;
//            DebugInformation.Vectors["Acc"] = initialAcceleration;

            return new SimulationStepResults(initialState, result, stepEndTime - stepDuration, stepEndTime);
        }

        #endregion

        private static Vector3 CalculateAcceleration(Quaternion orientation, Vector3 velocity, JoystickOutput output)
        {
            Axes prevAxes = VectorHelper.GetAxes(orientation);

            Vector3 liftDir = prevAxes.Up;
            float liftForce = output.Throttle*MaxThrust; // 100% throttle => max thrust, simple but works OK
            Vector3 rotorLift = liftForce*liftDir;
            var wind = new Vector3(0, 0, 0); // TODO Use real winds here..
            Vector3 drag = CalculateDrag(velocity, prevAxes);
            Vector3 totalForces = drag + wind + rotorLift;
            Vector3 constantTimestepAcceleration = PhysicsConstants.Gravity + totalForces/Mass;

            return constantTimestepAcceleration;
        }

        private TimestepResult SimulateStep(TimestepStartingCondition startCondition, JoystickOutput output,
                                                   TimeSpan stepDuration, TimeSpan stepEndTime)
        {
            // Find new position and velocity from a constant acceleration over timestep
            var dt = (float) stepDuration.TotalSeconds;

//            TimestepResult result;
            if (_useTerrainCollision)
            {
                // Note we override gravity here because the gravity acceleration is already accounted for in startCondition.Acceleration vector
                _collision.SetGravity(Vector3.Zero);

                float heliMass = _collision.HelicopterBody.Mass;
                _collision.HelicopterBody.Position = Conversion.ToJitterVector(startCondition.Position);
                _collision.HelicopterBody.LinearVelocity = Conversion.ToJitterVector(startCondition.Velocity);
                _collision.HelicopterBody.AddForce(Conversion.ToJitterVector(heliMass * startCondition.Acceleration));

                var localAngVelocity = new Vector3(
                    output.Pitch * PhysicsConstants.MaxPitchRate,
                    output.Yaw * PhysicsConstants.MaxYawRate,
                    -output.Roll * PhysicsConstants.MaxRollRate);

                var worldAngVelocity = VectorHelper.MapFromWorld(localAngVelocity, startCondition.Orientation);

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
            else
            {
                Vector3 newPosition;
                Vector3 newVelocity;
                ClassicalMechanics.ConstantAcceleration(
                    dt, 
                    startCondition.Position, 
                    startCondition.Velocity,
                    startCondition.Acceleration,
                    out newPosition, out newVelocity);

                // TODO Add wind
                // Rotate helicopter by joystick input after moving it
                //            Vector3 airVelocity = -startCondition.Velocity + Vector3.Zero; 

                // TODO Re-apply rotation by air friction as soon as the Kalman Filter works fine without it
                Quaternion newOrientation, afterJoystick;
                RotateByJoystickInput(startCondition.Orientation, dt, output, out afterJoystick);
                //            RotateByAirFriction(afterJoystick, airVelocity, dt, out newOrientation);
                newOrientation = afterJoystick;
                return new TimestepResult(newPosition, newVelocity, newOrientation, stepEndTime);

            }
        }

        private static void RotateByAirFriction(Quaternion currentOrientation, Vector3 airVelocity, float dt,
                                                out Quaternion newOrientation)
        {
            float headingAngle = VectorHelper.GetHeadingAngle(currentOrientation);
            float airflowHeadingAngle = VectorHelper.GetHeadingAngle(-airVelocity);
            float relativeAirflowAngle = airflowHeadingAngle - headingAngle;

            // Note: Simplificiation, we assume helicopter is nearly level.
            // This way any vertical wind speeds won't cause the helicopter to rotate.
            float horizontalWindMagnitudeFactor = VectorHelper.ToHorizontal3D(airVelocity).Length();

            // Wind angle causes max rotation when wind is perfectly to the Right or Left of helicopter
            // since the arm of the tail rotor is the longest then, as seen from the wind. 
            // As the wind approaches the frontside or tailside, the effect decreases.
            var windAngleFactor = (float) Math.Abs(Math.Sin(relativeAirflowAngle));

//            float windFactor = horizontalWindMagnitudeFactor*windAngleFactor;

            // The yaw rate [rad/s] per unit of wind velocity [m/s]
            const float yawRatePerVelocity = 0.1f;
            float yawRate = windAngleFactor*horizontalWindMagnitudeFactor*yawRatePerVelocity;
                // TODO temp //0.1745f;  // ~10 degrees per second
            float deltaYaw = yawRate*dt;
            if (relativeAirflowAngle < 0) deltaYaw = -deltaYaw;

            newOrientation = VectorHelper.AddPitchRollYaw(currentOrientation, 0, 0, deltaYaw);
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


        /// <summary>Air density</summary>
        /// <param name="velocity"></param>
        /// <param name="axes"></param>
        /// <returns></returns>
        private static Vector3 CalculateDrag(Vector3 velocity, Axes axes)
        {
            // p = air density per 20' Celcius (http://en.wikipedia.org/wiki/Density)
            // v = velocity vector
            // A = cross-section of helicopter as a function of main direction of wind
            // Cd = drag coefficient (non-dimensional) as a function of fuselage form factor
            float p = 1.204f;
            float v = velocity.Length();
            float A = CalculateCrossSection(velocity, axes);
            float Cd = CalculateDragCoefficient();
            Vector3 frictionDirection = -Vector3.Normalize(velocity);

            // TODO Consider if Stokes' drag model fits better for RC helicopters
            // This formula is for drag at high velocity
            float magnitude = 0.5f*p*v*v*A*Cd;
            if (magnitude == 0) return Vector3.Zero;

            // Return a vector with the direction opposite of velocity and magnitude of the drag
            return magnitude*frictionDirection;
        }

        private static float CalculateDragCoefficient()
        {
            // TODO Refine
            // A streamlined form will be close to zero while a cube is ~1.05
            // http://en.wikipedia.org/wiki/Drag_coefficient
            // For now we assume the fuselage is somewhere between those two
            // This will later be a function of direction of the wind to the fuselage.
            return 1.0f;
        }

        private static float CalculateCrossSection(Vector3 velocity, Axes axes)
        {
            // TODO Refine
            //Estimate fuselage cross section as 10 x 20 cm.
            // This will of course vary if the wind comes sideways, front or back or
            // anywhere in-between
            // This will later be a function of direction of the wind to the fuselage.
            return 0.1f*0.2f;
//            return 20*0.1f*0.2f;
        }
    }
}