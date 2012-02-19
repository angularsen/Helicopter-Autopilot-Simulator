using System;
using System.Collections.Generic;

namespace Anj.XNA.Joysticks.Wizard
{
    public class JoystickWizardController
    {
        public event Action ConfigurationComplete;
        public event Action<JoystickAction, JoystickAction> StepCompleted;

        private readonly Queue<JoystickAction> _actionsToConfigure;

        public JoystickAction CurrentAction { get; private set; }
        public JoystickAction CurrentOppositeAction { get; private set; }

        private readonly IList<MonitoredJoystick> _joystickMonitors;
        private readonly Queue<JoystickAction> _oppositeActions;
        public readonly Dictionary<JoystickAxis, AxisMapping> AxisToMapping;
        private readonly List<JoystickAxis> _alreadyAssignedAxes;

        public JoystickWizardController()
        {
            AxisToMapping = new Dictionary<JoystickAxis, AxisMapping>();
            _alreadyAssignedAxes = new List<JoystickAxis>();
            _joystickMonitors = new List<MonitoredJoystick>();
            _actionsToConfigure = new Queue<JoystickAction>();
            _actionsToConfigure.Enqueue(JoystickAction.PitchNoseUp);
            _actionsToConfigure.Enqueue(JoystickAction.RollRight);
            _actionsToConfigure.Enqueue(JoystickAction.YawRight);
            _actionsToConfigure.Enqueue(JoystickAction.MoreThrottle);

            _oppositeActions = new Queue<JoystickAction>();
            _oppositeActions.Enqueue(JoystickAction.PitchNoseDown);
            _oppositeActions.Enqueue(JoystickAction.RollLeft);
            _oppositeActions.Enqueue(JoystickAction.YawLeft);
            _oppositeActions.Enqueue(JoystickAction.LessThrottle);

            if (!Next())
                throw new Exception("No actions to configure! Next() return false in constructor.");
        }

        private bool Next()
        {
            if (_actionsToConfigure.Count == 0 || _oppositeActions.Count == 0)
                return false;

            CurrentAction = _actionsToConfigure.Dequeue();
            CurrentOppositeAction = _oppositeActions.Dequeue();

            return true;
        }

        public void AddJoystick(Joystick joystick)
        {
            _joystickMonitors.Add(new MonitoredJoystick(joystick));
        }

        public void Update()
        {
            foreach (var monitor in _joystickMonitors)
            {
                monitor.Update();

                bool isInverted;
                JoystickAxis axis;

                var axisFound = IsAxisRecognized(monitor, _alreadyAssignedAxes, out axis, out isInverted);
                if (!axisFound || 
                    _alreadyAssignedAxes.Contains(axis))
                    continue;

                _alreadyAssignedAxes.Add(axis);

                var map = new AxisMapping();
                map.Axis = axis;

                if (isInverted)
                {
                    map.Negative = CurrentAction;
                    map.Positive = CurrentOppositeAction;
                }
                else
                {
                    map.Positive = CurrentAction;
                    map.Negative = CurrentOppositeAction;
                }

                AxisToMapping[map.Axis] = map;

                var finished = !Next();
                if (finished)
                {
                    if (ConfigurationComplete != null)
                        ConfigurationComplete();
                }
                else
                {
                    if (StepCompleted != null)
                        StepCompleted(map.Positive, map.Negative);
                }
            }
        }

        private static bool IsAxisRecognized(MonitoredJoystick monitor, IList<JoystickAxis> ignoreAxes, out JoystickAxis axis, out bool isInverted)
        {
            // When exiting in false, we still need to set some default values that should never be used
            axis = JoystickAxis.X;
            isInverted = false;

//            if (IsInitialState(monitor))
//                return false;

            // Max possible deviation is Int16.MaxValue - Int16.MinValue
            // Most likely deviation is from 0 to Int16.MaxValue or 0 to Int16.MinValue
            // as the joystick is typically centered.
            const int threshold = (int)(0.9*Int16.MaxValue);
            if (monitor.Deviation.X > threshold && !ignoreAxes.Contains(JoystickAxis.X))
            {
                axis = JoystickAxis.X;
                isInverted = monitor.Current.X > monitor.MinValue.X;
            }
            else if (monitor.Deviation.Y > threshold && !ignoreAxes.Contains(JoystickAxis.Y))
            {
                axis = JoystickAxis.Y;
                isInverted = monitor.Current.Y > monitor.MinValue.Y;
            }
            else if (monitor.Deviation.Z > threshold && !ignoreAxes.Contains(JoystickAxis.Z))
            {
                axis = JoystickAxis.Z;
                isInverted = monitor.Current.Z > monitor.MinValue.Z;
            }
            else if (monitor.Deviation.Rx > threshold && !ignoreAxes.Contains(JoystickAxis.Rx))
            {
                axis = JoystickAxis.Rx;
                isInverted = monitor.Current.Rx > monitor.MinValue.Rx;
            }
            else if (monitor.Deviation.Ry > threshold && !ignoreAxes.Contains(JoystickAxis.Ry))
            {
                axis = JoystickAxis.Ry;
                isInverted = monitor.Current.Ry > monitor.MinValue.Ry;
            }
            else if (monitor.Deviation.Rz > threshold && !ignoreAxes.Contains(JoystickAxis.Rz))
            {
                axis = JoystickAxis.Rz;
                isInverted = monitor.Current.Rz > monitor.MinValue.Rz;
            }
            else if (monitor.Deviation.U > threshold && !ignoreAxes.Contains(JoystickAxis.U))
            {
                axis = JoystickAxis.U;
                isInverted = monitor.Current.U > monitor.MinValue.U;
            }
            else if (monitor.Deviation.V > threshold && !ignoreAxes.Contains(JoystickAxis.V))
            {
                axis = JoystickAxis.V;
                isInverted = monitor.Current.V > monitor.MinValue.V;
            }
            else
                return false;

            return true;
        }

        private static bool IsInitialState(MonitoredJoystick monitor)
        {
            return monitor.Current.Equals(monitor.Prev);
//                IsInitialState(monitor.Current.X) &&
//                IsInitialState(monitor.Current.Y) &&
//                IsInitialState(monitor.Current.Z) &&
//                IsInitialState(monitor.Current.Rx) &&
//                IsInitialState(monitor.Current.Ry) &&
//                IsInitialState(monitor.Current.Rz) &&
//                IsInitialState(monitor.Current.U) &&
//                IsInitialState(monitor.Current.V);
        }

//        private static bool IsInitialState(int axisValue)
//        {
//            return axisValue == 0 || axisValue == Int16.MaxValue || axisValue == Int16.MinValue;
//        }
    }
}