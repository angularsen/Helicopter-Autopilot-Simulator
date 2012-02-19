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
using State.Model;

#endregion

namespace Sensors
{
    public abstract class SensorBase : ISensor
    {
        protected SensorBase(bool isPerfect)
        {
            IsPerfect = isPerfect;
        }

        /// <summary>
        ///   Set to true to return sensor data from perfect/true state instead of measured values.
        /// </summary>
        public bool IsPerfect { get; set; }

        #region ISensor Members

        public abstract void Update(PhysicalHeliState startState, PhysicalHeliState endState, JoystickOutput output, TimeSpan totalSimulationTime, TimeSpan endTime);

        #endregion
    }
}