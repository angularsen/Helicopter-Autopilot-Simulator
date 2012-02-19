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
using System.Text;
using Anj.Helpers.XNA;
using Control.Common;
using State.Model;

#endregion

// TODO Implement effect-correcting, because it is not given that the hard coded PID values will work on any model.
// The effect of an output over time should be measured, and if the effect is too little it should be amplified and vice versa.

// Disable warnings about "code not reachable".
// I have a few of those because of hardcoded constant settings.
#pragma warning disable 0162

namespace Control
{
    public class Autopilot
    {
        private const bool IsLogging = false;
        public readonly PIDSetup PIDSetup;
        public bool IsAtDestination;

        /// <summary>
        /// Set to true to use IsTruePositionWithinRadius instead of estimated state when determining
        /// if withing radius of a waypoint.
        /// </summary>
        public bool IsTestMode;

        /// <summary>
        /// Set to true to indicate that the true position is within the radius of the current waypoint.
        /// Only the simulator can determine this, so it will set this field accordingly.
        /// </summary>
        public bool IsTruePositionWithinRadius;

        public float MaxHVelocity = 10f;

        public NavigationMap Map { get; set; }

        #region Constructor

        public Autopilot(Task t, PIDSetup pidSetup)
        {
            PIDSetup = pidSetup;
            Navigation = NavigationState.EnRoute;
            Actions = Actions.None;
            Task = t;
            Output = new OutputController(pidSetup);
            IsTruePositionWithinRadius = false;
        }

        #endregion

        #region Properties

        public Task Task { get; set; }

        public Waypoint CurrentWaypoint
        {
            get { return Task.Current; }
        }

        public NavigationState Navigation { get; private set; }
        public Actions Actions { get; private set; }

        public OutputController Output { get; private set; }

        #endregion

        private static void Log(string t)
        {
            if (IsLogging)
                Console.WriteLine(t);
        }

        private static bool IsBitSet(Enum source, Enum lookingFor)
        {
            int s = Convert.ToInt32(source);
            int l = Convert.ToInt32(lookingFor);
            return ((s & l) > 0);
        }

        public static string ActionsToString(Actions actions)
        {
            if (actions == Actions.None) return "None";

            var sb = new StringBuilder();

#if XBOX
            foreach (Actions action in Xbox360Helper.GetValues<Actions>())
                if (IsBitSet(actions, action)) sb.Append(action + ", ");
#else
            foreach (Actions action in Enum.GetValues(typeof (Actions)))
                if (IsBitSet(actions, action)) sb.Append(action + ", ");
#endif

            return sb.ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="ticks">TimeSpan ticks (100ns).</param>
        /// <param name="control"></param>
        /// <returns></returns>
        public JoystickOutput GetOutput(HeliState s, long ticks, out ControlGoal control)
        {
            JoystickOutput result;

            // Re-fill the list of actions that are being performed in this input
            Actions = Actions.None;


            //            float hDistanceToGoal = Vector2.Distance(VectorHelper.ToHorizontal(s.Position),
            //                                                     VectorHelper.ToHorizontal(CurrentWaypoint.Position));

            // Simplified: Is the position in one second from now a crash?
            // If not, are we at the destination?
            //            HeliState nextState = GetEstimatedState(s, TimeSpan.FromSeconds(1));
            //            Navigation = nextState.Position.Y < 0 
            //                ? NavigationState.Recovery 
            //                : NavigationState.EnRoute;

            // TODO Insert some recovery code as well, commented out now because it triggers too often
            Navigation = NavigationState.EnRoute;
            //                Navigation = (hDistanceToGoal < GoalDistanceTolerance)
            //                                 ? NavigationState.AtDestination
            //                                 : NavigationState.EnRoute;


            //            if (CurrentWaypoint.Type != WaypointType.Start) //&&
//            if (CurrentWaypoint.Type == WaypointType.Land)
//            {
//                result = new JoystickInput();
//                control = new ControlGoal();
//            }
//            else
            {
                switch (Navigation)
                {
                    case NavigationState.Recovery:
                        Log("Nav: Recovery");
                        result = GetOutput_Recovery(s, ticks, out control);
                        break;

                    case NavigationState.EnRoute:
                        Log("Nav: EnRoute");
                        result = GetOutput_EnRoute(s, ticks, out control);
                        break;

                    case NavigationState.AtDestination:
                        Log("Nav: AtDestination");
                        result = GetHoverOutput(s, ticks, out control);
                        break;

                    default:
                        throw new NotImplementedException("Should return before here.");
                }

                Log("Actions: " + ActionsToString(Actions));

                Log(String.Format("Input: PRYT {0},{1},{2},{3}",
                                  (int) (result.Pitch*100),
                                  (int) (result.Roll*100),
                                  (int) (result.Yaw*100),
                                  (int) (result.Throttle*100)));
            }

            ProcessNavigation(s, ticks);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userInput"></param>
        /// <param name="s"></param>
        /// <param name="ticks">TimeSpan ticks (100ns).</param>
        /// <param name="control"></param>
        /// <returns></returns>
        public JoystickOutput GetAssistedOutput(JoystickOutput userInput, HeliState s, long ticks,
                                                out ControlGoal control)
        {
            Navigation = NavigationState.AssistedAutopilot;
            JoystickOutput result = GetOutput_AssistedAutopilot(userInput, s, ticks, out control);
            ProcessNavigation(s, ticks);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="ticks">TimeSpan ticks (100ns).</param>
        private void ProcessNavigation(HeliState s, long ticks)
        {
            // Note: Changing current waypoint must be done after input processing, because
            // modifying the waypoint will not update the helicopter state until next time - 
            // including states about angle and distance to waypoint.

            // Make sure we use 2D position (map) when holding height above terrain, in order to pass waypoints.
            if (Task.HoldHeightAboveGround > 0)
            {
                float groundHeightAtWaypoint = Map.GetAltitude(VectorHelper.ToHorizontal(CurrentWaypoint.Position));
                CurrentWaypoint.Position.Y = groundHeightAtWaypoint + Task.HoldHeightAboveGround;
            }

            bool isWithinRadius = IsTestMode
                                      ? IsTruePositionWithinRadius
                                      : CurrentWaypoint.IsWithinRadius(s.Position);

            if (isWithinRadius)
                CurrentWaypoint.SecondsWaited += ticks/1000.0f;
            else
                CurrentWaypoint.SecondsWaited = 0; // Seconds waited should be in an uninterrupted sequence

            // Progress to next waypoint if currently at start point or have been sufficiently long at a stop point
            if ( //CurrentWaypoint.Type == WaypointType.Start ||
                CurrentWaypoint.Type == WaypointType.Hover && CurrentWaypoint.DoneWaiting ||
                CurrentWaypoint.Type == WaypointType.Intermediate && isWithinRadius)
            {
                Task.Next();
            }
            else if (CurrentWaypoint.Type == WaypointType.TestDestination &&
                     isWithinRadius)
            {
                if (Task.Loop)
                    Task.Next();
                else
                    IsAtDestination = true;
            }

            if (Task.Current == null)
                throw new Exception("Waypoint should never be null! Always end task with an end-waypoint.");

            //            s.Waypoint = Task.Current;
            //            s.HPositionToGoal = VectorHelper.ToHorizontal(s.Waypoint.Position - s.Position);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="totalTicks">TimeSpan ticks (100ns).</param>
        /// <param name="control"></param>
        /// <returns></returns>
        private JoystickOutput GetOutput_EnRoute(HeliState s, long totalTicks, out ControlGoal control)
        {
            return Output.MoveTowardsGoal(MaxHVelocity, out control, s, totalTicks, Task.HoldHeightAboveGround);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userInput"></param>
        /// <param name="s"></param>
        /// <param name="totalTicks">TimeSpan ticks (100ns).</param>
        /// <param name="control"></param>
        /// <returns></returns>
        private JoystickOutput GetOutput_AssistedAutopilot(JoystickOutput userInput, HeliState s, long totalTicks,
                                                           out ControlGoal control)
        {
            // Command the output controller based on the user input.
            // Joystick commands:
            // Forward/backward - Move forwards/backwards
            // Right/left - Move rightwards/leftwards
            // Throttle  - Set the height above the ground to hold
            float heightAboveGroundToHold = MyMathHelper.Lerp(userInput.Throttle, 0, 1, 1, 10);
            float forwardsVelocity = MyMathHelper.Lerp(-userInput.Pitch, -1, 1, -10, 10);
            float rightwardsVelocity = MyMathHelper.Lerp(userInput.Roll, -1, 1, -10, 10);
            float headingDeg = s.Degrees.HeadingAngle - 15*userInput.Yaw;

            return Output.MoveRelatively(s, forwardsVelocity, rightwardsVelocity, headingDeg, heightAboveGroundToHold,
                                         totalTicks, out control);
        }

        public JoystickOutput GetHoverOutput(HeliState s, long totalTicks, out ControlGoal control)
        {
            Actions |= Actions.Hover;

            //Log("Hovering!");

            control = new ControlGoal
                          {
                              HVelocity = 0,
                              PitchAngle = 0,
                              RollAngle = 0,
                              HeadingAngle = 0
                          };

            return new JoystickOutput
                       {
                           // TODO We need to store an initial s.Position.Y instead of using the most current one, so we need a "Hover" command that does this
                           Throttle = Output.Throttle(s.Position.Y, s.Position.Y, totalTicks),
                           Roll = Output.Roll(s.Degrees.RollAngle, control.RollAngle, totalTicks),
                           Pitch = Output.Pitch(s.Degrees.PitchAngle, control.PitchAngle, totalTicks),
                           Yaw = 0.0f
                       };
        }

        private JoystickOutput GetOutput_Recovery(HeliState s, long totalTicks, out ControlGoal control)
        {
            Actions |= Actions.Hover;

            Log("Crash imminent, trying to recover!");

            control = new ControlGoal
                          {
                              HVelocity = 0,
                              PitchAngle = 0,
                              RollAngle = 0,
                              HeadingAngle = 0
                          };

            return new JoystickOutput
                       {
                           Throttle = 1.0f,
                           Roll = Output.Roll(s.Degrees.RollAngle, control.RollAngle, totalTicks),
                           Pitch = Output.Pitch(s.Degrees.PitchAngle, control.PitchAngle, totalTicks),
                           Yaw = 0.0f
                       };
        }

        //        private static HeliState GetEstimatedState(HeliState currentState, TimeSpan timeFromNow)
        //        {
        //            HeliState s = currentState;
        //
        //            var t = (float) timeFromNow.TotalSeconds;
        //            Vector3 a = s.TotalAcceleration;
        //            Vector3 v0 = s.Velocity;
        //
        //            // Note Did not work well, jittery behavior. Maybe smooth?
        //            // Estimate using constant acceleration.
        //            // distance = v0t + 0.5at^2
        //            Vector3 estMoveDistance = (v0 /*+ 0.5f*a*t*/)*t;
        //            s.Position += estMoveDistance;
        //
        //            // v = v0 + at
        //            s.Velocity += a*t;
        //
        //            // TODO Improve estimate taking rotational velocity into account and current user input maybe?
        //            return s;
        //        }

        public void Reset()
        {
            Navigation = NavigationState.EnRoute;
            Actions = Actions.None;
        }

        public Autopilot Clone()
        {
            var r = new Autopilot(Task, Output.PIDSetup.Clone())
                        {
                            Map = Map,
                            IsTestMode = IsTestMode,
                            MaxHVelocity = MaxHVelocity,
                        };
            return r;
        }
    }
}