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
using Microsoft.Xna.Framework;
using Physics.Common;
using Sensors.Model;
using State.Model;

#endregion

namespace Sensors
{
    public class SensorModel
    {
        private readonly SensorSpecifications _sensorSpecifications;
        private readonly IEnumerable<ISensor> _sensors;

        public SensorModel(SensorSpecifications sensorSpecifications, NavigationMap map, Vector3 startPosition, Quaternion startOrientation)
        {
            _sensorSpecifications = sensorSpecifications;

            Console.WriteLine("Using sensors:\n" + sensorSpecifications);


            // Instantiate sensors and populate properties and sensor list
            const bool isPerfect = false;
            _sensors = new List<ISensor>
                           {
                               (GPS = new GPS(sensorSpecifications, isPerfect, true)),
                               (IMU = new IMU(sensorSpecifications, startOrientation, isPerfect)),
                               (Magnetometer = new Magnetometer3Axis(sensorSpecifications, isPerfect)),
                               (GroundRangeFinder = new SonicRangeFinder(sensorSpecifications, isPerfect, map)),
                               (PressureGauge = new StaticPressureGauge(sensorSpecifications, isPerfect)),
                           };
        }

        public GPS GPS { get; private set; }
        public IMU IMU { get; private set; }
        public Magnetometer3Axis Magnetometer { get; private set; }
//        public Accelerometer3Axis Accelerometer { get; private set; }
//        public Gyroscope3Axis Gyroscope { get; private set; }
//        public DifferentialPressureGauge AirSpeed { get; private set; }
        public StaticPressureGauge PressureGauge { get; private set; }
        public SonicRangeFinder GroundRangeFinder { get; private set; }

        public bool Ready
        {
            get { return GPS.Ready; }
        }

        public SensorSpecifications Specifications { get { return _sensorSpecifications; } }

        public void Update(SimulationStepResults step, JoystickOutput output)
        {
            // TODO Use substeps in physics simulation when sensors require higher frequency than the main-loop runs at
            // Currently we simply use the state at the start of the timestep to feed into the sensors, so they read the state
            // that was before any motion took place in that timestep.
            // Later we may need to update sensors multiple times for each timestep.
            TimestepStartingCondition start = step.StartingCondition;
            TimestepResult end = step.Result;

            var startPhysicalState = new PhysicalHeliState(start.Orientation, start.Position, start.Velocity,
                                                      start.Acceleration);

            var endPhysicalState = new PhysicalHeliState(end.Orientation, end.Position, end.Velocity,
                                                      new Vector3(float.NaN));

            // Update all sensors
            foreach (ISensor sensor in _sensors)
                sensor.Update(startPhysicalState, endPhysicalState, output, step.StartTime, step.EndTime);
        }
    }
}