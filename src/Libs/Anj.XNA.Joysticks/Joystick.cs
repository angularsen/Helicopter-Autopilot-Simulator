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
using System.Diagnostics;
using Microsoft.DirectX.DirectInput;

#endregion

namespace Anj.XNA.Joysticks
{
    public struct AxisStates : IEquatable<AxisStates>
    {
        public int X;
        public int Y;
        public int Z;
        public int Rx;
        public int Ry;
        public int Rz;
        public int U;
        public int V;

        public bool Equals(AxisStates other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z &&
                   Rx == other.Rx &&
                   Ry == other.Ry &&
                   Rz == other.Rz &&
                   U == other.U &&
                   V == other.V;
        }
    }


    /// <summary>
    /// Class to interface with a joystick device.
    /// </summary>
    public class Joystick
    {
        private readonly IntPtr _hWnd;
        private bool[] _buttons;
        public Device DirectInputDevice;
        public JoystickState State;
        public AxisStates Axes;
        public AxisInformation[] AxisInfo;

        /// <summary>
        /// Constructor for the class.
        /// </summary>
        /// <param name="windowHandle">Handle of the window which the joystick will be "attached" to.</param>
        public Joystick(IntPtr windowHandle)
        {
            _hWnd = windowHandle;
            AxisCount = 0;
        }

        /// <summary>
        /// Number of axes on the joystick.
        /// </summary>
        public int AxisCount { get; private set; }

        /// <summary>
        /// Number of buttons on the joystick.
        /// </summary>
        public int ButtonCount { get; private set; }


        private void Poll()
        {
            try
            {
                // poll the joystick
                DirectInputDevice.Poll();
                // update the joystick state field
                State = DirectInputDevice.CurrentJoystickState;
            }
            catch (Exception err)
            {
                // we probably lost connection to the joystick
                // was it unplugged or locked by another application?
                Debug.WriteLine("Poll()");
                Debug.WriteLine(err.Message);
                Debug.WriteLine(err.StackTrace);
            }
        }

        /// <summary>
        /// Retrieves a list of joysticks attached to the computer.
        /// </summary>
        /// <example>
        /// [C#]
        /// <code>
        /// JoystickInterface.Joystick jst = new JoystickInterface.Joystick(this.Handle);
        /// string[] sticks = jst.FindJoysticks();
        /// </code>
        /// </example>
        /// <returns>A list of joysticks as an array of strings.</returns>
        public static string[] FindJoysticks()
        {
            string[] systemJoysticks = null;

            try
            {
                // Find all the GameControl devices that are attached.
                DeviceList gameControllerList = Manager.GetDevices(DeviceClass.GameControl,
                                                                   EnumDevicesFlags.AttachedOnly);

                // check that we have at least one device.
                if (gameControllerList.Count > 0)
                {
                    systemJoysticks = new string[gameControllerList.Count];
                    int i = 0;
                    // loop through the devices.
                    foreach (DeviceInstance deviceInstance in gameControllerList)
                    {
                        // create a device from this controller so we can retrieve info.
                        var joystickDevice = new Device(deviceInstance.InstanceGuid);

                        systemJoysticks[i++] = joystickDevice.DeviceInformation.InstanceName;
                    }
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine("FindJoysticks()");
                Debug.WriteLine(err.Message);
                Debug.WriteLine(err.StackTrace);
            }

            return systemJoysticks;
        }

        /// <summary>
        /// Acquire the named joystick. You can find this joystick through the <see cref="FindJoysticks"/> method.
        /// </summary>
        /// <param name="name">Name of the joystick.</param>
        /// <returns>The success of the connection.</returns>
        public bool AcquireJoystick(string name)
        {
            try
            {
                DeviceList gameControllerList = Manager.GetDevices(DeviceClass.GameControl,
                                                                   EnumDevicesFlags.AttachedOnly);
                bool found = false;
                // loop through the devices.
                foreach (DeviceInstance deviceInstance in gameControllerList)
                {
                    if (deviceInstance.InstanceName != name)
                        continue;
                    
                    // create a device from this controller so we can retrieve info.
                    DirectInputDevice = new Device(deviceInstance.InstanceGuid);
                    DirectInputDevice.SetCooperativeLevel(_hWnd,
                                                       CooperativeLevelFlags.Background |
                                                       CooperativeLevelFlags.NonExclusive);
                    found = true;
                    break;
                }

                if (!found)
                    return false;

                // Tell DirectX that this is a Joystick.
                DirectInputDevice.SetDataFormat(DeviceDataFormat.Joystick);

                // Finally, acquire the device.
                DirectInputDevice.Acquire();

                DeviceCaps cps = DirectInputDevice.Caps;
                AxisCount = cps.NumberAxes;
                ButtonCount = cps.NumberButtons;
                AxisInfo = new AxisInformation[AxisCount];


                DeviceObjectList axisIter = DirectInputDevice.GetObjects(DeviceObjectTypeFlags.Axis);
                int i = 0;
                while (axisIter.MoveNext())
                {
                    var info = (DeviceObjectInstance) axisIter.Current;
                    AxisInfo[i++] = new AxisInformation(info);
                }

                UpdateStatus();
            }
            catch (Exception err)
            {
                Debug.WriteLine("FindJoysticks()");
                Debug.WriteLine(err.Message);
                Debug.WriteLine(err.StackTrace);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Unaquire a joystick releasing it back to the system.
        /// </summary>
        public void ReleaseJoystick()
        {
            if (DirectInputDevice != null) DirectInputDevice.Unacquire();
        }

        public struct AxisInformation
        {
            public string Name;
            public int MaxForce;
            public int ForceResolution;

            public AxisInformation(DeviceObjectInstance axisObject)
            {
                Name = axisObject.Name;
                MaxForce = axisObject.MaxForce;
                ForceResolution = axisObject.ForceResolution;
            }

        }

        /// <summary>
        /// Update the properties of button and axis positions.
        /// </summary>
        public void UpdateStatus()
        {
            Poll();

            Axes.X = State.X;
            Axes.Y = State.Y;
            Axes.Z = State.Z;
            Axes.Rx = State.Rx;
            Axes.Ry = State.Ry;
            Axes.Rz = State.Rz;
            Axes.U = State.GetSlider()[0];
            Axes.V = State.GetSlider()[1];

            // not using buttons, so don't take the tiny amount of time it takes to get/parse
            byte[] jsButtons = State.GetButtons();
            _buttons = new bool[jsButtons.Length];

            int i = 0;
            foreach (byte button in jsButtons)
            {
                _buttons[i] = button >= 128;
                i++;
            }
        }

        public override string ToString()
        {
            return AxisCount + " + axes, " + ButtonCount + " buttons.";
        }
    }
}