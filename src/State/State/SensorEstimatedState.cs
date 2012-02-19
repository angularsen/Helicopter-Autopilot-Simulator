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
using System.Collections.Generic;
using Control.Common;
using Control.KalmanFilter;
using Microsoft.Xna.Framework;
using Physics.Common;
using Sensors;
using Sensors.Model;
using State.Model;

#endregion

namespace State
{
//    internal struct InputSample
//    {
//        public JoystickInput Input;
//        public long Millis;
//
//        public InputSample(long millis, JoystickInput input)
//        {
//            Millis = millis;
//            Input = input;
//        }
//    }

    /// <summary>
    ///   Notes:
    ///   Extended Kalman Filter
    ///   Adaptive Fuzzy Kalman Filter
    /// </summary>
    public class SensorEstimatedState : IStateProvider
    {
        /// <summary>
        /// A log of true, observed and estimated states over time.
        /// </summary>
        public readonly List<HelicopterLogSnapshot> Log;

        private readonly PhysicalHeliState _initialState;
        private readonly SensorModel _sensors;

        /// <summary>
        /// For debugging purposes only! Compare estimation to true state. This will be set externally from Helicopterbase.Update() each loop iteration.
        /// </summary>
        public SimulationStepResults CheatingTrueState;

        private PhysicalHeliState _currentEstimate;
        private PhysicalHeliState _currentObservation;
        private PhysicalHeliState _estimated;
#if !XBOX
        private GPSINSFilter _gpsins;
#endif

        private bool _isInitialized;
        private Vector3 _prevGPSPos;
        private PhysicalHeliState _currentBlindEstimate;


        public SensorEstimatedState(SensorModel sensorModel, PhysicalHeliState initialState)
        {
            _sensors = sensorModel;
            _initialState = initialState;
            _estimated = new PhysicalHeliState();
            Log = new List<HelicopterLogSnapshot>();
        }

        public bool Ready
        {
            get { return _sensors.Ready && _isInitialized; }
        }

        #region IStateProvider Members

        public void GetState(out PhysicalHeliState estimated, out PhysicalHeliState observed, out PhysicalHeliState blindEstimatedState)
        {
            if (!Ready) throw new Exception("Can't call GetNextState() when Ready is false.");

            estimated = _currentEstimate;
            observed = _currentObservation;
            blindEstimatedState = _currentBlindEstimate;
        }

        public void Update(SimulationStepResults trueState, long elapsedMillis, long totalElapsedMillis,
                           JoystickOutput currentOutput)
        {
#if XBOX
            // TODO Xbox equivalent or fix the GPSINSFilter dependency on Iridium Math.Net
            _isInitialized = true;
#else

            // TODO If sensors are not ready we cannot produce an estimated state
            // However, currently the helicopter starts mid-air so we have no time to wait for it to initialize.. so this is cheating.
            // When the helicopter starts on the ground we can properly implement this process.
            if (!_isInitialized)
            {
                if (!_sensors.Ready) return;

                _estimated.Position = _initialState.Position;
                _estimated.Velocity = _initialState.Velocity;
                _estimated.Acceleration = _initialState.Acceleration;
                _estimated.Orientation = _initialState.Orientation;
                _prevGPSPos = _sensors.GPS.Output.Position;

                _gpsins = new GPSINSFilter(
                    TimeSpan.FromMilliseconds(totalElapsedMillis - elapsedMillis),
                    _initialState.Position,
                    _initialState.Velocity,
                    _initialState.Orientation,
                    _sensors.Specifications);

                _isInitialized = true;
            }


            TimeSpan totalTime = TimeSpan.FromMilliseconds(totalElapsedMillis);

            // Setup observations
            var observations = new GPSINSObservation();
            observations.Time = totalTime;
            observations.RangeFinderHeightOverGround = _sensors.GroundRangeFinder.FlatGroundHeight; // TODO Update frequency? Noise?

            if (_sensors.GPS.IsUpdated)
            {
                observations.GotGPSUpdate = true;
                observations.GPSPosition = _sensors.GPS.Output.Position;
                observations.GPSHVelocity = _sensors.GPS.Output.Velocity;
            }

            // Setup input to filter's internal model
            var input = new GPSINSInput
                            {
                                AccelerationWorld = _sensors.IMU.AccelerationWorld,
                                Orientation = _sensors.IMU.AccumulatedOrientation,
//                                PitchDelta = _sensors.IMU.AngularDeltaBody.Radians.Pitch,
//                                RollDelta = _sensors.IMU.AngularDeltaBody.Radians.Roll,
//                                YawDelta = _sensors.IMU.AngularDeltaBody.Radians.Yaw,
//                                PitchRate = _sensors.IMU.Output.AngularRate.Radians.Pitch,
//                                RollRate = _sensors.IMU.Output.AngularRate.Radians.Roll,
//                                YawRate = _sensors.IMU.Output.AngularRate.Radians.Yaw,
                            };

            // Estimate
            StepOutput<GPSINSOutput> gpsinsEstimate = _gpsins.Filter(observations, input);

//            Vector3 deltaPosition = gpsinsEstimate.PostX.Position - _currentEstimate.Position;
//            if (deltaPosition.Length() > 40)
//                deltaPosition = deltaPosition;                

//            var trueState = CheatingTrueState.Result;
            GPSINSOutput observed = gpsinsEstimate.Z;
            GPSINSOutput estimated = gpsinsEstimate.PostX;
            GPSINSOutput blindEstimated = gpsinsEstimate.X;

            _currentEstimate = new PhysicalHeliState(estimated.Orientation, estimated.Position, estimated.Velocity,
                                                     input.AccelerationWorld);

            _currentBlindEstimate = new PhysicalHeliState(blindEstimated.Orientation, blindEstimated.Position, blindEstimated.Velocity,
                                                     input.AccelerationWorld);

            _currentObservation = new PhysicalHeliState(observed.Orientation, observed.Position, observed.Velocity,
                                                        Vector3.Zero);
#endif
        }

        #endregion
    }
}