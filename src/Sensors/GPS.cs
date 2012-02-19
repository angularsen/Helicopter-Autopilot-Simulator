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
using Anj.Helpers.XNA;
using Control.Common;
using Microsoft.Xna.Framework;
using Sensors.Model;
using State.Model;

#endregion

namespace Sensors
{
    /// <summary>
    ///   References on GPS:
    ///   - GPS accuracy: http://www.gpsworld.com/gpsworld/article/articleDetail.jsp?id=388666
    /// </summary>
    public class GPS : ISensor
    {
//        private const float StdDevPerAxis = 4f;
        private const float SecondsPerUpdate = 1.0f;
        private readonly SensorSpecifications _sensorSpecifications;
        private readonly bool _isPerfect;

        // TODO When helicopter starts on the ground we can wait for the GPS to get ready (see SensorEstimatedState.Update())
        private readonly TimeSpan _setupDuration = TimeSpan.FromSeconds(0);
        private readonly object _syncRoot = new object();
        private readonly bool _useUpdateFrequency;
        private TimeSpan _initStartTime;
        private bool _isInitialized;
        private GPSOutput _output;

        private TimeSpan _prevUpdate;

        public GPS(SensorSpecifications sensorSpecifications, bool isPerfect, bool useUpdateFrequency)
        {
            _sensorSpecifications = sensorSpecifications;
            _isPerfect = isPerfect;
            _useUpdateFrequency = useUpdateFrequency;

            _prevUpdate = TimeSpan.MinValue;
            _initStartTime = TimeSpan.Zero;
//            Position = Vector3.Zero;
//            Velocity = Vector3.Zero;
            Ready = false;
        }

        /// <summary>
        ///   Returns whether the GPS has stable satellite data and producing reliable position values.
        /// </summary>
        public bool Ready { get; private set; }

        ///// <summary>
        /////   The GPS position is updated every 1 second, and has a varying degree of inaccuracy
        /////   at best within 5 meter radius.
        ///// </summary>
        //public Vector3 Position { get; private set; }

        ///// <summary>
        /////   The GPS velocity is updated every 1 second from its change in position.
        ///// </summary>
        //public Vector3 Velocity { get; private set; }

        public GPSOutput Output
        {
            get
            {
                lock (_syncRoot)
                {
                    IsUpdated = false;
                    return _output;
                }
            }
            private set
            {
                lock (_syncRoot)
                {
                    _output = value;
                    IsUpdated = true;
                }
            }
        }



        public bool IsUpdated { get; private set; }

        #region ISensor Members

        public void Update(PhysicalHeliState tartState, PhysicalHeliState endState, JoystickOutput output, TimeSpan startTime, TimeSpan endTime)
        {
            if (!_isInitialized)
            {
                _initStartTime = startTime;
                _isInitialized = true;
            }

            PhysicalHeliState stateToMeasure = endState;

            // Only update GPS readout as often as SecondsPerUpdate says
            if (_useUpdateFrequency && startTime < _prevUpdate + TimeSpan.FromSeconds(SecondsPerUpdate))
                return;

            // It takes some time for a GPS to connect and produce positional values
            if (!Ready && startTime >= _initStartTime + _setupDuration)
                Ready = true;

            // Update the GPS information if the device is ready
            if (Ready)
            {
                _prevUpdate = startTime;

                Vector3 position = stateToMeasure.Position;
                Vector3 velocity = stateToMeasure.Velocity;

                if (!_isPerfect)
                {
                    position += VectorHelper.GaussRandom3(_sensorSpecifications.GPSPositionStdDev);
                    velocity += VectorHelper.GaussRandom3(_sensorSpecifications.GPSVelocityStdDev);
                }

                Output = new GPSOutput(position, velocity);
            }
        }

        #endregion
    }
}