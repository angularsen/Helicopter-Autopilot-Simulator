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
using State.Model;
using Sensors.Model;

#endregion

namespace Sensors
{
    /// <summary>
    ///   Notes:
    ///   - Conversion from acceleration vector to acceleration sensor axes have an inaccuracy of about 2E-7
    /// </summary>
    public class Accelerometer3Axis : SensorBase
    {
        private readonly GaussianRandom _gaussRand;
        private readonly float _rmsNoiseXY;
        private readonly float _rmsNoiseZ;
        private TimeSpan _prevStartTime;
        private bool _isInitialized;
        private readonly float _frequency;
        private readonly TimeSpan _timeBetweenUpdates;

        public Accelerometer3Axis(SensorSpecifications sensorSpecifications, bool isPerfect) : base(isPerfect)
        {
            _gaussRand = new GaussianRandom();

            _frequency = sensorSpecifications.AccelerometerFrequency;
            _rmsNoiseXY = sensorSpecifications.AccelerometerStdDev.Forward;
            _rmsNoiseZ = sensorSpecifications.AccelerometerStdDev.Up;

            _timeBetweenUpdates = (_frequency > 0) 
                ? TimeSpan.FromSeconds(1.0/_frequency) 
                : TimeSpan.Zero;

            _isInitialized = false;
        }

        /// <summary>
        /// Positive Right
        /// </summary>
        public float Right { get; private set; }

        /// <summary>
        /// Positive Up
        /// </summary>
        public float Up { get; private set; }

        /// <summary>
        /// Positive Forward
        /// </summary>
        public float Forward { get; private set; }

//        public Vector3 Vector { get; private set; }
        public Vector3 AccelerationWorld { get; private set; }


        public override void Update(PhysicalHeliState startState, PhysicalHeliState endState, JoystickOutput output, TimeSpan startTime, TimeSpan endTime)
        {
            if (!_isInitialized)
            {
                _prevStartTime = startTime;
                _isInitialized = true;
            }

            PhysicalHeliState stateToMeasure = startState;

            // Only update according to max frequency
            var timeSinceLastUpdate = startTime - _prevStartTime;
            if (_frequency > 0 && timeSinceLastUpdate < _timeBetweenUpdates) 
                return;

            _prevStartTime = startTime;

            // Project world acceleration onto the helicopter's orientation and its XYZ sensors
            // See how we use the original acceleration vector (the cause) directly instead of
            // deriving it from changes in position over time (the effect) which has 
            // unrealistic delay and would amplify errors
            AccelerationWorld = stateToMeasure.Acceleration;

            // TODO Verify these are correct
            Vector3 accelerationBody = VectorHelper.MapFromWorld(AccelerationWorld, stateToMeasure.Orientation);
            Right = accelerationBody.X;
            Up = accelerationBody.Y;
            Forward = -accelerationBody.Z;

            if (!IsPerfect)
            {
                var noiseRight = _gaussRand.NextGaussian(0, _rmsNoiseXY);
                var noiseUp = _gaussRand.NextGaussian(0, _rmsNoiseXY);
                var noiseForward = _gaussRand.NextGaussian(0, _rmsNoiseZ);

                // Add noise to local axes representing the accelerometer readings
                Forward += noiseForward;
                Right += noiseRight;
                Up += noiseUp;

                // Add noise to world acceleration vector, by transforming noise in local axes to world axes (with some non-important precision loss)
                Vector3 accelerationNoiseWorld =
                    VectorHelper.MapToWorld(new Vector3(noiseRight, noiseUp, noiseForward), stateToMeasure.Orientation);
                AccelerationWorld += accelerationNoiseWorld;
            }
        }
    }
}