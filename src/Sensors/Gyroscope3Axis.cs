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
using Control.Common;
using Physics.Common;
using Sensors.Model;
using State.Model;

#endregion

namespace Sensors
{
    public class Gyroscope3Axis : SensorBase
    {
        private readonly SensorSpecifications _sensorSpecifications;
        private PitchRollYaw _delta;
        private PitchRollYaw _rate;

        public Gyroscope3Axis(SensorSpecifications sensorSpecifications, bool isPerfect) : base(isPerfect)
        {
            _sensorSpecifications = sensorSpecifications;
        }

        public PitchRollYaw Rate
        {
            get { return _rate; }
        }

        public PitchRollYaw Delta
        {
            get { return _delta; }
        }

        #region Implementation of ISensor

        public override void Update(PhysicalHeliState startState, PhysicalHeliState endState, JoystickOutput output, TimeSpan startTime, TimeSpan endTime)
        {
            var dt = (float) ((endTime - startTime).TotalSeconds);
            _rate.Pitch = output.Pitch*PhysicsConstants.MaxPitchRate;
            _rate.Roll = output.Roll*PhysicsConstants.MaxRollRate;
            _rate.Yaw = output.Yaw*PhysicsConstants.MaxYawRate;

            // Note it is important to explicitly calculate the delta and not reuse the _rate values multiplied by dt
            // See http://stackoverflow.com/questions/2491161/why-differs-floating-point-precision-in-c-when-separated-by-parantheses-and-when
            // and QuaternionPrecisionTest.Test() for more information about precision.
            _delta.Pitch = output.Pitch*PhysicsConstants.MaxPitchRate*dt;
            _delta.Roll = output.Roll*PhysicsConstants.MaxRollRate*dt;
            _delta.Yaw = output.Yaw*PhysicsConstants.MaxYawRate*dt;

            // TODO Handle IsPerfect == false
        }

        #endregion
    }
}