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
    public class IMU : SensorBase
    {
        private readonly Accelerometer3Axis _accelerometer;
        private readonly Gyroscope3Axis _gyroscope;

        // TODO startPosition not used, because Kalman Filter handles this. Either IMU should handle pos/rot integration or the Kalman Filter should. Not both.

        /// <summary>
        /// World frame acceleration vector.
        /// </summary>
        public Vector3 AccelerationWorld;

        /// <summary>
        /// Local acceleration.
        /// </summary>
        public ForwardRightUp AccelerationLocal;

        /// <summary>
        /// Based on an initial orientation and accumulated gyro measurements, we can estimate a current orientation.
        /// The error is likely to drift if the sensor data is noisy or infrequent (or for virtual sensors; out of sync with simulator game loop).
        /// </summary>
        public Quaternion AccumulatedOrientation;

        /// <summary>
        /// World frame angular delta (change in angle).
        /// </summary>
        public AngularValues AngularDeltaBody;

        /// <summary>
        /// World frame angular rate (angle velocity).
        /// </summary>
        public AngularValues AngularRateBody;

        public IMU(SensorSpecifications sensorSpecifications, Quaternion startOrientation, bool isPerfect)
            : base(isPerfect)
        {
            _accelerometer = new Accelerometer3Axis(sensorSpecifications, isPerfect);
            _gyroscope = new Gyroscope3Axis(sensorSpecifications, isPerfect);

            AccumulatedOrientation = startOrientation;
        }

        #region Implementation of ISensor

        public override void Update(PhysicalHeliState startState, PhysicalHeliState endState, JoystickOutput output, TimeSpan startTime, TimeSpan endTime)
        {
            _accelerometer.Update(startState, endState, output, startTime, endTime);
            _gyroscope.Update(startState, endState, output, startTime, endTime);

            // Make sure we use orientation at start of timestep to transform to world, since IMU should be able to calculate accumulated position 
            // by this world acceleration, and according to my physics simulation the position is calculated before rotation is.
            AccelerationWorld = _accelerometer.AccelerationWorld;
            AccelerationLocal = new ForwardRightUp(_accelerometer.Forward, _accelerometer.Right, _accelerometer.Up);
            AngularRateBody = AngularValues.FromRadians(_gyroscope.Rate);
            AngularDeltaBody = AngularValues.FromRadians(_gyroscope.Delta);

            PitchRollYaw delta = AngularDeltaBody.Radians;
            AccumulatedOrientation =
                VectorHelper.AddPitchRollYaw(AccumulatedOrientation, delta.Pitch, delta.Roll, delta.Yaw);
        }

        #endregion



        /// <summary>
        /// Correct orientation by providing a measured gravity vector in the body frame.
        /// Typically we use accelerometers for this. If the vehicle's non-gravity acceleration is near zero 
        /// then the measured acceleration vector has length close to 1G. This only happens sporadically for a 
        /// helicopter, but once they do they are often reliable and can be used to correct drift in the accumulated
        /// orientation of the gyros.
        /// </summary>
        /// <param name="gravityBF"></param>
        public void CorrectOrientation(Vector3 gravityBF)
        {
            // TODO Remove orientation code from Kalman Filter and use IMU's accumulated and gravity corrected orientation instead
            throw new NotImplementedException("CorrectOrientation()");
        }
    }
}