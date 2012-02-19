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
    public class Magnetometer3Axis : SensorBase
    {
        private readonly RingBuffer<Axes> _sampleHistory;

        public Magnetometer3Axis(SensorSpecifications sensorSpecifications, bool isPerfect)
            : base(isPerfect)
        {
            // TODO History length?
            _sampleHistory = new RingBuffer<Axes>(100);
        }

        public Axes CurrentMeasuredAxes { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public override void Update(PhysicalHeliState startState, PhysicalHeliState endState, JoystickOutput output, TimeSpan startTime, TimeSpan endTime)
        {
            PhysicalHeliState stateToMeasure = endState;

            // TODO Fill XYZ magnetic readings
            // TODO Simulate sensor lag, inaccuarcy, noise..
            _sampleHistory.Add(stateToMeasure.Axes);

            CurrentMeasuredAxes = IsPerfect ? stateToMeasure.Axes : ComputeMeanAxes();
        }

        /// <summary>
        ///   Computes axes that represent the mean vectors of the history samples.
        /// </summary>
        /// <returns></returns>
        private Axes ComputeMeanAxes()
        {
            var forward = new Vector3();
            var right = new Vector3();
            var up = new Vector3();

            foreach (Axes sample in _sampleHistory)
            {
                forward += sample.Forward;
                right += sample.Right;
                up += sample.Up;
            }

            forward /= _sampleHistory.Count;
            right /= _sampleHistory.Count;
            up /= _sampleHistory.Count;

            return new Axes(forward, right, up);
        }
    }
}