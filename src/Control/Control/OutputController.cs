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
using State.Model;

#endregion

// TODOS 
// 1. Don't use modulo 360.0f
// 2. Avoid floating point operations where unnecessary
// 3. Document how MoveTowardsGoal avoids the circle-problem when correcting the bearing

namespace Control
{
    public class OutputController
    {
        public OutputController(PIDSetup setup)
        {
            PIDSetup = setup;
        }

        /// <summary>
        ///   Custom output controller by given PID configurations.
        /// </summary>
        public OutputController(PID pitchAnglePID, PID rollAnglePID, PID yawAnglePID, PID throttlePID,
                                PID horizontalForwardVelocityPID, PID horizontalRightwardVelocityPID)
            : this(new PIDSetup
                       {
                           PitchAngle = pitchAnglePID,
                           RollAngle = rollAnglePID,
                           YawAngle = yawAnglePID,
                           Throttle = throttlePID,
                           ForwardsAccel = horizontalForwardVelocityPID,
                           RightwardsAccel = horizontalRightwardVelocityPID,
                       })
        {
        }

        public PIDSetup PIDSetup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxHVelocity"></param>
        /// <param name="control"></param>
        /// <param name="s"></param>
        /// <param name="totalTicks">TimeSpan ticks (100ns).</param>
        /// <param name="holdHeightAboveGround">If larger than zero, this will override the throttle to hold an altitude above ground.</param>
        /// <returns></returns>
        public JoystickOutput MoveTowardsGoal(float maxHVelocity, out ControlGoal control, HeliState s, long totalTicks, float holdHeightAboveGround)
        {
//            Vector2 bearing = VectorHelper.ToHorizontal(s.Velocity);



            float hGoalDistance = s.HPositionToGoal.Length();//VectorHelper.ToHorizontal(s.Position - s.Waypoint.Position).Length();
            const float secondsToStop = 1.0f;
            float metersToStop = secondsToStop * maxHVelocity;
            float wantedHVelocity = MathHelper.Lerp(0, maxHVelocity, MathHelper.Clamp(hGoalDistance / metersToStop, 0, 1));

            // When flying in sloped terrains we risk crashing if the terrain altitude changes quicker than
            // the autopilot can follow at the current horizontal velocity.
            // If that is the case, stop horizontal motion while trying to stabilize height above ground.
//            if (moveToTargetInput.Throttle == 1 || moveToTargetInput.Throttle == 0)
//                wantedHVelocity = 0;

            Vector2 hForwardNorm = Vector2.Normalize(VectorHelper.ToHorizontal(s.Forward));
            Vector2 hRightNorm = Vector2.Normalize(VectorHelper.ToHorizontal(s.Right));

            Vector2 wantedHVelocityVector = wantedHVelocity * Vector2.Normalize(s.HPositionToGoal);
            float wantedHVelocityForward = (wantedHVelocity != 0) 
                ? VectorHelper.Project(wantedHVelocityVector, hForwardNorm) : 0;
            float wantedHVelocityRight = (wantedHVelocity != 0) 
                ? VectorHelper.Project(wantedHVelocityVector, hRightNorm) : 0;


            // If helicopter is nearing a waypoint at which it should hover for a while, then
            // let the helicopter yaw to the waypoint heading angle while hovering.
            // Otherwise we want the helicopter to head in the bearing direction.
            Vector2 hVelocityVector = VectorHelper.ToHorizontal(s.Velocity); 
            float hVelocity = hVelocityVector.Length();
            float wantedYawAngleDeg = (s.Waypoint.Type == WaypointType.Hover && hVelocity < 1)
                                          ? MathHelper.ToDegrees(s.Waypoint.HeadingAngle)
                                          : s.Degrees.GoalAngle;

            return MoveRelatively(s, wantedHVelocityForward, wantedHVelocityRight,
                                  wantedYawAngleDeg, holdHeightAboveGround, totalTicks, out control);
        }


        public JoystickOutput MoveRelatively(HeliState s, float wantedHVelocityForward, 
            float wantedHVelocityRight, float wantedYawAngleDeg, float wantedHeightAboveGround, 
            long totalTicks, out ControlGoal control)
        {
            // Max degrees to tilt the cyclic when accelerating/decelerating
            const float maxCyclicAngleDeg = 10.0f;

            Vector2 hVelocityVector = VectorHelper.ToHorizontal(s.Velocity);
            Vector2 hForwardNorm = Vector2.Normalize(VectorHelper.ToHorizontal(s.Forward));
            Vector2 hRightNorm = Vector2.Normalize(VectorHelper.ToHorizontal(s.Right));
            float hVelocityForward = VectorHelper.Project(hVelocityVector, hForwardNorm);
            float hVelocityRight = VectorHelper.Project(hVelocityVector, hRightNorm);

            // Errors in velocity (derivative of position)
            float hVelocityForwardError = hVelocityForward - wantedHVelocityForward;
            float hVelocityRightError = hVelocityRight - wantedHVelocityRight;

            // Too much velocity should trigger deceleration and vice versa.
            // Forwards acceleration means pitching nose down, and rightwards acceleration means rolling right.
            // -1 == max deceleration, +1 == max acceleration
            float wantedForwardsAcceleration = PIDSetup.ForwardsAccel.ComputeExplicit(0, 0, hVelocityForwardError);
            float wantedRightwardsAcceleration = PIDSetup.RightwardsAccel.ComputeExplicit(0, 0, hVelocityRightError);

            // Determine the pitching/rolling angles to achieve wanted acceleration/deceleration in either direction
            // Invert pitch; negative angle (nose down) to accelerate.
            float wantedPitchAngleDeg = -wantedForwardsAcceleration * maxCyclicAngleDeg;
            float wantedRollAngleDeg = wantedRightwardsAcceleration * maxCyclicAngleDeg;



            // Determine the current errors in pitch and roll
            // Then adjust the inputs accordingly
            var moveToTargetInput = new JoystickOutput();
            moveToTargetInput.Pitch = Pitch(s.Degrees.PitchAngle, wantedPitchAngleDeg, totalTicks);
            moveToTargetInput.Roll = Roll(s.Degrees.RollAngle, wantedRollAngleDeg, totalTicks);
            moveToTargetInput.Yaw = Yaw(s.Degrees.HeadingAngle, wantedYawAngleDeg, totalTicks);
            moveToTargetInput.Throttle = (wantedHeightAboveGround > 0)
                ? Throttle(s.HeightAboveGround, wantedHeightAboveGround, totalTicks)
                : Throttle(s.Position.Y, s.Waypoint.Position.Y, totalTicks);

            // This is used for debugging and visual feedback
            control = new ControlGoal
            {
                HVelocity = 0,//wantedVelocity,
                PitchAngle = MathHelper.ToRadians(wantedPitchAngleDeg),
                RollAngle = MathHelper.ToRadians(wantedRollAngleDeg),
                HeadingAngle = 0,
            };
            return moveToTargetInput;
        }


//        private JoystickInput Hover(HeliState s, out ControlGoal control)
//        {
//
//            control = new ControlGoal
//            {
//                HVelocity = 0,
//                PitchAngle = MathHelper.ToRadians(wantedPitchAngleDeg),
//                RollAngle = MathHelper.ToRadians(wantedRollAngleDeg),
//            };
//        }

        /// <summary></summary>
        /// <returns>
        ///   -1 for pitching the nose downwards, 1 for pitching the nose upwards
        /// </returns>
        public float Pitch(float pitchAngleDeg, float wantedPitchAngleDeg, long totalTicks)
        {
            float pitchAngleErrorDeg = pitchAngleDeg - wantedPitchAngleDeg;
            return -(PIDSetup.PitchAngle.Compute(pitchAngleErrorDeg, totalTicks));
        }

        /// <summary></summary>
        /// <returns>
        ///   -1 for max roll speed to the left, 1 for max roll speed to the right
        /// </returns>
        public float Roll(float rollAngleDeg, float wantedRollAngleDeg, long totalTicks)
        {
            float rollAngleErrorDeg = rollAngleDeg - wantedRollAngleDeg;
            return -(PIDSetup.RollAngle.Compute(rollAngleErrorDeg, totalTicks));
        }

        /// <summary></summary>
        /// <returns>
        ///   -1 for full counterclockwise yawing, 1 for full clockwise yawing
        /// </returns>
        public float Yaw(float headingAngleDeg, float wantedYawAngleDeg, long totalTicks)
        {
            float headingAngle = MathHelper.ToRadians(headingAngleDeg);
            float wantedYawAngle = MathHelper.ToRadians(wantedYawAngleDeg);
            float yawErrorDeg = MathHelper.ToDegrees(VectorHelper.DiffAngle(headingAngle, wantedYawAngle));
            return PIDSetup.YawAngle.Compute(yawErrorDeg, totalTicks);
        }

        /// <summary></summary>
        /// <returns>
        ///   0 for no throttle, 1 for full throttle
        /// </returns>
        public float Throttle(float altitude, float wantedAltitude, long totalTicks)
        {
            float altitudeError = altitude - wantedAltitude;

            // Throttle should go from 0 to 1, not -1 to 1
            return PIDSetup.Throttle.ComputePositive(altitudeError, totalTicks);
        }


//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="s"></param>
//        /// <param name="wantedVelocity"></param>
//        /// <returns></returns>
//        private static Vector2 GetBearingCorrection(HeliState s, float wantedVelocity)
//        {
//            // P: A deviation of N m/s should give maximum output.
//            var hVelocityPID = new PID(5.0f, 0, 0);
//
//            // Project the horizontal velocity vector onto the target vector, so the result
//            float hVelocityToTarget = VectorHelper.Project(VectorHelper.ToHorizontal(s.Velocity), s.HPositionToGoal);
//            float hVelocityError = hVelocityToTarget - wantedVelocity;
//
//            // Invert velocity correction. Too much velocity should trigger less acceleration.
//            // -1 == max deceleration, +1 == max acceleration
//            var hVelocityCorrection = MathHelper.Clamp(-hVelocityError*hVelocityPID.P, -1, 1);
//
//            // 180 degrees error = 0 means the opposite bearing vector will be used (when moving exactly the direction away from the goal, start moving directly towards the goal)
//            // 90 degrees error = 0.5 means the helicopter will accelerate one part towards opposite bearing (come to a stop) and one part towards the goal (move towards goal)
//            // 0 degrees errror = 1 means the goal vector will be used, because the helicopter is already bearing towards the goal
//            // This interpolation will cause the helicopter to smoothly decrease any mis-bearing and come to a bearing towards the goal.
//            float invCorrectionFactor = MathHelper.Clamp(1 - Math.Abs(s.Radians.BearingErrorAngle/MathHelper.Pi), 0, 1);
//
//            // Use correction factor to linearly interpolate between opposite bearing vector and goal vector
//            Vector2 invBearing = -VectorHelper.ToHorizontal(s.Radians.BearingAngle);
//            Vector2 bearingCorrection = Vector2.Lerp(Vector2.Normalize(invBearing), 
//                                                     Vector2.Normalize(s.HPositionToGoal),
//                                                     invCorrectionFactor);
//            
//            float bearingCorrectionAngle = VectorHelper.GetAngle(bearingCorrection);
//            
//            return bearingCorrection;
//        }
//
//        // Obsolete code. Used in MoveTowardsGoal().
//        private static void LinearBearingCorrection()
//        {
//
//            //            // Project the horizontal velocity vector onto the target vector, so the result
//            //            float hVelocityToTarget = VectorHelper.Project(VectorHelper.ToHorizontal(s.Velocity), s.HPositionToGoal);
//            //            float hVelocityError = hVelocityToTarget - wantedVelocity;
//            //
//            //            // Invert velocity correction. Too much velocity should trigger less acceleration.
//            //            // -1 == max deceleration, +1 == max acceleration
//            //            float hVelocityCorrection = MathHelper.Clamp(-hVelocityError*hVelocityPID.P, -1, 1);
//            //
//            //            // 180 degrees error = 0 means the opposite bearing vector will be used (when moving exactly the direction away from the goal, start moving directly towards the goal)
//            //            // 90 degrees error = 0.5 means the helicopter will accelerate one part towards opposite bearing (come to a stop) and one part towards the goal (move towards goal)
//            //            // 0 degrees errror = 1 means the goal vector will be used, because the helicopter is already bearing towards the goal
//            //            // This interpolation will cause the helicopter to smoothly decrease any mis-bearing and come to a bearing towards the goal.
//            //            float invCorrectionFactor = MathHelper.Clamp(1 - Math.Abs(s.BearingErrorAngle/MathHelper.Pi), 0, 1);
//            //
//            //            // Use correction factor to linearly interpolate between opposite bearing vector and goal vector
//            //            Vector2 invBearing = -VectorHelper.ToHorizontal(s.BearingAngle);
//            //            Vector2 bearingCorrection = Vector2.Lerp(Vector2.Normalize(invBearing), 
//            //                                                     Vector2.Normalize(s.HPositionToGoal),
//            //                                                     invCorrectionFactor);
//            //            float bearingCorrectionAngle = VectorHelper.GetAngle(bearingCorrection);
//            //
//            //            // Calculate the amount of pitch and roll to apply in order to tilt the cyclic towards the bearing correction angle.
//            //            // The amount of tilt depends on the PID for the velocity error.
//            //            // Right angle (roll) is always 90 degrees clockwise of heading angle (pitch)
//            //            float headingAngleError = VectorHelper.DiffAngle(s.HeadingAngle, bearingCorrectionAngle);
//            //            float rightAngleError = (headingAngleError - MathHelper.PiOver2)%MathHelper.TwoPi;
//            //
//            //            // Determine how much the pitch and roll each affect the velocity in the desired correction vector.
//            //            // pitch = 0.2, roll = 0.8 means the pitch should be 1/4th of the roll in order to direct the cyclic tilt towards the wanted correction vector.
//            //            // If heading is already along correction vector, the pitch would be 1.0 and only pitch should be used to accelerate/decelerate 
//            //            // along the vector.
//            //            // If heading is opposite of correction vector, the pitch would be -1.0 meaning it would need to pitch its nose up.
//            //            var a = Math.Sin(headingAngleError);
//            //            var b = Math.Sin(rightAngleError);
//            //            double denominator = Math.Abs(a) + Math.Abs(b);
//            //            var pitchCorrectionEffect = -(float)(a / denominator);
//            //            var rollCorrectionEffect = -(float)(b / denominator);
//            //
//            //            // Determine the wanted cyclic tilt, and what pitch and roll is necessary to achieve it
//            //            float wantedCyclicTiltAngleDeg = Math.Abs(hVelocityCorrection * maxCyclicAngleDeg);
//            //            float wantedPitchAngleDeg = pitchCorrectionEffect*wantedCyclicTiltAngleDeg;
//            //            float wantedRollAngleDeg = rollCorrectionEffect*wantedCyclicTiltAngleDeg;
//        }
        
        
    }
}