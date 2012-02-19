#if !XBOX

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

using Vrpn;
using System;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using Vector3 = Microsoft.Xna.Framework.Vector3;

#endregion

namespace Simulator.Tracking
{
    public class Tracker
    {
        private const int HMDTrackerID = 3;
        private const int WandTrackerID = 0;

        private const string VPRNHostIP = "129.241.103.142";
        private readonly TrackerRemote _tracker;

        private TrackedObject _hmd;

        public Tracker()
        {
            _tracker = new TrackerRemote("Isense900@" + VPRNHostIP);
            _tracker.PositionChanged += TrackerStateChanged;
            _tracker.VelocityChanged += TrackerVelocityChanged;
            _tracker.AccelerationChanged += TrackerAccelerationChanged;
        }

        public void Update()
        {
            _tracker.Update();
        }

        #region Properties

        private bool _isConnected;

        public bool IsConnected
        {
            get
            {
                Update();
                return _isConnected;
            }
            set { _isConnected = value; }
        }

        /// <summary>
        /// Tracking for Head Mounted Display (helmet).
        /// </summary>
        public TrackedObject HMD
        {
            get { return _hmd; }
            private set { _hmd = value; }
        }


        /// <summary>
        /// Tracking for wand.
        /// </summary>
        public TrackedObject Wand { get; private set; }

        #endregion

        #region Event handlers

        private void TrackerVelocityChanged(object sender, TrackerVelocityChangeEventArgs e)
        {
            if (e.Sensor == HMDTrackerID)
            {
                _hmd.Velocity = ToXNA(e.Velocity);
                _hmd.Time = e.Time;
            }
        }

        private void TrackerAccelerationChanged(object sender, TrackerAccelChangeEventArgs e)
        {
            if (e.Sensor == HMDTrackerID)
            {
                _hmd.Acceleration = ToXNA(e.Acceleration);
                _hmd.Time = e.Time;
            }
        }

        private void TrackerStateChanged(object sender, TrackerChangeEventArgs e)
        {
            if (e.Sensor == HMDTrackerID)
            {
                _hmd.Position = ToXNA(e.Position);
                _hmd.Orientation = ToXNA(e.Orientation);
                _hmd.Time = e.Time;
                IsConnected = true;
            }
        }

        #endregion

        #region Helper methods

        private static Vector3 ToXNA(Vrpn.Vector3 v)
        {
            return new Vector3((float) v.X, (float) v.Y, (float) v.Z);
        }

        private static Quaternion ToXNA(Vrpn.Quaternion q)
        {
            return new Quaternion((float) q.X, (float) q.Y, (float) q.Z, (float) q.W);
        }

        #endregion

        #region Test methods

        public static void Main(string[] args)
        {
            var hmdTracker = new TrackerRemote("Isense900@" + VPRNHostIP);
            hmdTracker.PositionChanged += TestHMDChanged;

//            var wandButtons = new ButtonRemote("Wand0@" + VPRNHostIP);
//            wandButtons.WandButtonsChanged += WandButtonsChanged;
//            wandButtons.MuteWarnings = true;

//            var wandJoystick = new AnalogRemote("Wand0" + VPRNHostIP);
//            wandJoystick.AnalogChanged += WandJoystickChanged;

            while (true)
            {
                hmdTracker.Update();
            }
        }

        private static void WandJoystickChanged(object sender, AnalogChangeEventArgs e)
        {
        }

        private static void WandButtonsChanged(object sender, ButtonChangeEventArgs e)
        {
        }

        private static void TestHMDChanged(object sender, TrackerChangeEventArgs e)
        {
            if (e.Sensor == HMDTrackerID)
                Console.WriteLine("HMD position {0}", e.Position);
        }

        #endregion
    }
}

#endif
