using System;
using System.Collections.Generic;
using System.Linq;
using Anj.XNA.Joysticks.Wizard;
using System.Diagnostics;

namespace Anj.XNA.Joysticks
{
    /// <summary>
    /// Some joysticks (such as Logitech G940 Flight System) are actually more than one joystick device.
    /// For those cases we need the concept of a joystick system, rather than a joystick device.
    /// </summary>
    public class JoystickSystem : IDisposable
    {
        private readonly IntPtr _windowHandle;
        private readonly List<Joystick> _joysticks;
        private readonly Dictionary<JoystickAxisAction, Joystick> _actionToJoystick;
        private readonly Dictionary<JoystickAxisAction, Axis> _actionToAxis;

        #region Constructors

        private JoystickSystem(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            _joysticks = new List<Joystick>();
            _actionToJoystick = new Dictionary<JoystickAxisAction, Joystick>();
            _actionToAxis = new Dictionary<JoystickAxisAction, Axis>();
        }

        public JoystickSystem(IntPtr windowHandle, IEnumerable<JoystickSetup> setups) : this(windowHandle)
        {
            if (setups == null || setups.Count() == 0)
                throw new ArgumentException(@"No setups in collection.", "setups");

            string[] connectedJoystickDeviceNames = Joystick.FindJoysticks();

            // Find the names of the currently connected devices that have a name-matched setup defined in JoystickSetups.XML
//            foreach (var joystickSetup in setups)
//            {
//                JoystickSetup setup = joystickSetup;
            IEnumerable<JoystickSetup> deviceAndSetupMatches = from setup in setups
                          from setupDevice in setup.Devices
                          from connectedDevName in connectedJoystickDeviceNames
                          where setupDevice.Name == connectedDevName
//                          join connectedDevName in connectedJoystickDeviceNames on dev.Name equals connectedDevName
                          select setup;
//            }
//            IEnumerable<JoystickSetup> deviceAndSetupMatches = from devName in connectedJoystickDeviceNames
//                                                               join setup in setups on devName equals setup.Name
//                                                               select setup;
//            setups.First().Devices[0].Name
            // If no matches were found, fall back to the first joystick setup.
            // Fhe first setup should be a very ordinary joystick setup that matches most models.
            if (deviceAndSetupMatches.Count() == 0)
            {
                Debug.WriteLine(
                    "Joysticks were connected, but no matching joystick setup configuration was found. Using the first joystick setup and hoping for the best.");
                Init(setups.First());
            }

            // If one or several joysticks matching a setup in XML were found, use the first one.
            Init(deviceAndSetupMatches.First());
        }

        #endregion

        #region Public methods

        public float GetAxisValue(JoystickAxisAction action)
        {
            Joystick device = _actionToJoystick[action];
            Axis axis = _actionToAxis[action];
            JoystickAxis axisName = axis.Name;

            float invertion = axis.IsInverted ? -1 : 1;

            float value;
            if (axisName == JoystickAxis.X)
                value = device.Axes.X;
            else if (axisName == JoystickAxis.Y)
                value = device.Axes.Y;
            else if (axisName == JoystickAxis.Z)
                value = device.Axes.Z;
            else if (axisName == JoystickAxis.Rx)
                value = device.Axes.Rx;
            else if (axisName == JoystickAxis.Ry)
                value = device.Axes.Ry;
            else if (axisName == JoystickAxis.Rz)
                value = device.Axes.Rz;
            else if (axisName == JoystickAxis.U)
                value = device.Axes.U;
            else if (axisName == JoystickAxis.V)
                value = device.Axes.V;
            else 
                throw new NotImplementedException();

            value = (value - Int16.MaxValue)/Int16.MaxValue;    // -1 to +1
            return invertion * value; // Inverted axis
        } 

        public void Dispose()
        {
            if (_joysticks != null)
                foreach (var joystick in _joysticks)
                    joystick.ReleaseJoystick();
        }

        public void Update()
        {
            if (_joysticks != null)
                foreach (var joystick in _joysticks)
                {
                    joystick.UpdateStatus();
                }
        }

        #endregion

        #region Private methods

        private void Init(JoystickSetup setup)
        {
            string[] connectedJoystickDeviceNames = Joystick.FindJoysticks();

            foreach (JoystickDevice device in setup.Devices)
            {
                if (!connectedJoystickDeviceNames.Contains(device.Name))
                    throw new Exception("Joystick " + device.Name + " was not connected!");

                var js = new Joystick(_windowHandle);
                js.AcquireJoystick(device.Name);
                _joysticks.Add(js);

                foreach (Axis axis in device.Axes)
                {
                    _actionToAxis[axis.Action] = axis;
                    _actionToJoystick[axis.Action] = js;
                }
            }
        }

        #endregion

        public override string ToString()
        {
            return _joysticks.Count + " joysticks configured.";
        }
    }
}
