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

#endregion

namespace State.Model
{
    public struct HelicopterLogSnapshot
    {
        /// <summary>
        /// The estimate when ignoring all observations, relying entirely on the input and model of the Kalman Filter.
        /// </summary>
        public PhysicalHeliState BlindEstimated;

        /// <summary>
        /// Kalman Filter estimated state.
        /// </summary>
        public PhysicalHeliState Estimated;

        /// <summary>
        /// Observed state (may not make sense with overlapping observations..? For now we use only GPS.)
        /// </summary>
        public PhysicalHeliState Observed;

        /// <summary>
        /// Time of simulation snapshot.
        /// </summary>
        public TimeSpan Time;

        /// <summary>
        /// True state.
        /// </summary>
        public PhysicalHeliState True;

        /// <summary>
        /// Accelerometer reading.
        /// </summary>
        public ForwardRightUp Accelerometer;

        /// <summary>
        /// The altitude of the ground below the helicopter.
        /// </summary>
        public float GroundAltitude;

        public HelicopterLogSnapshot(PhysicalHeliState trueState, PhysicalHeliState observedState,
                                    PhysicalHeliState estimatedState, PhysicalHeliState blindEstimatedState, 
                                    ForwardRightUp accelerometer, float groundAltitude, TimeSpan time)
        {
            True = trueState;
            Estimated = estimatedState;
            Observed = observedState;
            Time = time;
            Accelerometer = accelerometer;
            BlindEstimated = blindEstimatedState;
            GroundAltitude = groundAltitude;
        }
    }
}