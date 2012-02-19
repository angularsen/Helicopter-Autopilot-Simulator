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

namespace State.Common
{
    public static class StateHelper
    {
        public static Angles ToDegrees(Angles r)
        {
            var degrees = new Angles
                              {
                                  BearingAngle = MathHelper.ToDegrees(r.BearingAngle),
                                  BearingErrorAngle = MathHelper.ToDegrees(r.BearingErrorAngle),
                                  GoalAngle = MathHelper.ToDegrees(r.GoalAngle),
                                  HeadingAngle = MathHelper.ToDegrees(r.HeadingAngle),
                                  PitchAngle = MathHelper.ToDegrees(r.PitchAngle),
                                  RollAngle = MathHelper.ToDegrees(r.RollAngle)
                              };
            return degrees;
        }

        public static PhysicalHeliState ToPhysical(HeliState state)
        {
            // TODO Why is roll angle inverted?
//            var orientation = Quaternion.CreateFromYawPitchRoll(
//                state.Radians.HeadingAngle,
//                state.Radians.PitchAngle,
//                -state.Radians.RollAngle);

            return new PhysicalHeliState(state.Orientation, state.Position, state.Velocity, state.Acceleration);
        }

        /// <summary>
        ///   Returns a new physical state where the values represent the error in the estimated state to
        ///   the given true state. I.e. position = estimatedPosition - truePosition.
        /// </summary>
        /// <param name="trueState"></param>
        /// <param name="estimatedState"></param>
        /// <returns></returns>
        public static PhysicalHeliState GetError(PhysicalHeliState trueState, PhysicalHeliState estimatedState)
        {
            var axesError = new Axes
                                {
                                    Forward = estimatedState.Axes.Forward - trueState.Axes.Forward,
                                    Right = estimatedState.Axes.Right - trueState.Axes.Right,
                                    Up = estimatedState.Axes.Up - trueState.Axes.Up
                                };

//            var rotationError = new Matrix {Forward = axesError.Forward, Right = axesError.Right, Up = axesError.Up};

            // TODO Verify this works, since axes were originally used to compute state instead of orientation (but should equivalent)
            return new PhysicalHeliState(
                Quaternion.Identity, // TODO Not sure how to represent rotation error in quaternion representation
                estimatedState.Position - trueState.Position,
                estimatedState.Velocity - trueState.Velocity,
                estimatedState.Acceleration - trueState.Acceleration);
        }

        /// <summary>
        /// TODO Temporary. We might later distinguish between physical state and navigation state.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="heightAboveGround"></param>
        /// <param name="waypoint"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static HeliState ToHeliState(PhysicalHeliState s, float heightAboveGround, Waypoint waypoint, JoystickOutput output)
        {
            // Update state
            var r = new HeliState
                        {
                            HeightAboveGround = heightAboveGround,
                            Orientation = s.Orientation,
                            Forward = s.Axes.Forward,
                            Up = s.Axes.Up,
                            Right = s.Axes.Right,
                            Output = output,
                            Position = s.Position,
                            Velocity = s.Velocity,
                            Acceleration = s.Acceleration,
                            Waypoint = waypoint,
                            HPositionToGoal = VectorHelper.ToHorizontal(waypoint.Position - s.Position),
                        };


            // Update angles from current state
            var radians = new Angles
                              {
                                  PitchAngle = VectorHelper.GetPitchAngle(s.Orientation),
                                  RollAngle = VectorHelper.GetRollAngle(s.Orientation),
                                  HeadingAngle = VectorHelper.GetHeadingAngle(s.Orientation),
                              };

            // Velocity can be zero, and thus have no direction (NaN angle)
            radians.BearingAngle = (s.Velocity.Length() < 0.001f)
                                       ? radians.HeadingAngle
                                       : VectorHelper.GetHeadingAngle(s.Velocity);
                //GetAngle(VectorHelper.ToHorizontal(s.Velocity));

            radians.GoalAngle = VectorHelper.GetAngle(r.HPositionToGoal);
            radians.BearingErrorAngle = VectorHelper.DiffAngle(radians.BearingAngle, radians.GoalAngle);

            // Store angles as both radians and degrees for ease-of-use later
            r.Radians = radians;
            r.Degrees = ToDegrees(radians);
            return r;
        }
    }
}