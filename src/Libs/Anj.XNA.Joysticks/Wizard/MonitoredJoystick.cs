using System;

namespace Anj.XNA.Joysticks.Wizard
{
    internal class MonitoredJoystick
    {
        public readonly Joystick Joystick;
        public AxisStates MaxValue;
        public AxisStates MinValue;
        public AxisStates Deviation;
        public AxisStates Current;
        public AxisStates Prev;


        public MonitoredJoystick(Joystick joystick)
        {
            Joystick = joystick;
            MaxValue = InitAxes(int.MinValue);
            MinValue = InitAxes(int.MaxValue);
            Current = InitAxes(0);
            Prev = Current;
        }

        public override string ToString()
        {
            return Joystick.DirectInputDevice.DeviceInformation.ProductName;
        }

        private static AxisStates InitAxes(int value)
        {
            return new AxisStates
                       {
                           X = value,
                           Y = value,
                           Z = value,
                           Rx = value,
                           Ry = value,
                           Rz = value,
                           U = value,
                           V = value,
                       };
        }

        private bool _isInitialized = false;
        public void Update()
        {
            AxisStates newValues = Joystick.Axes;

            if (!_isInitialized)
            {
                Prev = Current = newValues;
                _isInitialized = true;
            }
            else
            {
                Prev = Current;
                Current = newValues;
            }

            // Don't update min/max if there has been no change.
            // This is to overcome the bug that joysticks have constant output until the stick is moved
            if (Prev.Equals(Current))
                return;

            MaxValue.X = Math.Max(MaxValue.X, newValues.X);
            MaxValue.Y = Math.Max(MaxValue.Y, newValues.Y);
            MaxValue.Z = Math.Max(MaxValue.Z, newValues.Z);
            MaxValue.Rx = Math.Max(MaxValue.Rx, newValues.Rx);
            MaxValue.Ry = Math.Max(MaxValue.Ry, newValues.Ry);
            MaxValue.Rz = Math.Max(MaxValue.Rz, newValues.Rz);
            MaxValue.U = Math.Max(MaxValue.U, newValues.U);
            MaxValue.V = Math.Max(MaxValue.V, newValues.V);

            MinValue.X = Math.Min(MinValue.X, newValues.X);
            MinValue.Y = Math.Min(MinValue.Y, newValues.Y);
            MinValue.Z = Math.Min(MinValue.Z, newValues.Z);
            MinValue.Rx = Math.Min(MinValue.Rx, newValues.Rx);
            MinValue.Ry = Math.Min(MinValue.Ry, newValues.Ry);
            MinValue.Rz = Math.Min(MinValue.Rz, newValues.Rz);
            MinValue.U = Math.Min(MinValue.U, newValues.U);
            MinValue.V = Math.Min(MinValue.V, newValues.V);

            Deviation.X = Math.Abs(MaxValue.X - MinValue.X);
            Deviation.Y = Math.Abs(MaxValue.Y - MinValue.Y);
            Deviation.Z = Math.Abs(MaxValue.Z - MinValue.Z);
            Deviation.Rx = Math.Abs(MaxValue.Rx - MinValue.Rx);
            Deviation.Ry = Math.Abs(MaxValue.Ry - MinValue.Ry);
            Deviation.Rz = Math.Abs(MaxValue.Rz - MinValue.Rz);
            Deviation.U = Math.Abs(MaxValue.U - MinValue.U);
            Deviation.V = Math.Abs(MaxValue.V - MinValue.V);


        }
    }
}