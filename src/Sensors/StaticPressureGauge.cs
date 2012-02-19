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
using Sensors.Helpers;
using Sensors.Model;
using Sensors.Providers;
using State.Model;

#endregion

namespace Sensors
{
    // A sensor that measures atmospheric pressure. Equations are valid up to 11,000 meters above sea level.
    // The constants are taken from these sources:
    // http://en.wikipedia.org/wiki/Atmospheric_pressure
    // http://en.wikipedia.org/wiki/Lapse_rate
    public class StaticPressureGauge : SensorBase
    {
        private readonly StaticPressureProvider _pressureProvider;

        /// <summary></summary>
        public StaticPressureGauge(SensorSpecifications sensorSpecifications, bool isPerfect)
            : base(isPerfect)
        {
            _pressureProvider = new StaticPressureProvider();
        }

        /// <summary>
        ///   The sea level static pressure may vary on a daily basis, so set this property accordingly if necessary.
        /// </summary>
        public float SeaLevelStaticPressure
        {
            get { return _pressureProvider.SeaLevelStaticPresure; }
            set { _pressureProvider.SeaLevelStaticPresure = value; }
        }

        public float Altitude { get; private set; }

        #region Overrides of SensorBase

        public override void Update(PhysicalHeliState startState, PhysicalHeliState endState, JoystickOutput output, TimeSpan totalSimulationTime, TimeSpan endTime)
        {
            PhysicalHeliState stateToMeasure = endState;
            float trueAltitude = stateToMeasure.Position.Y;

            if (IsPerfect)
                Altitude = trueAltitude;
            else
            {
                float simulatedPressure = _pressureProvider.GetSimulatedStaticPressure(trueAltitude);
                Altitude = StaticPressureHelper.GetAltitude(simulatedPressure, SeaLevelStaticPressure);
            }
        }

        #endregion
    }
}