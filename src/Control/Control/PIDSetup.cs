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
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Control
{
    public struct PIDSetup : IEnumerable<PID>
    {
        public string Name;
        public PID PitchAngle;
        public PID RollAngle;
        public PID Throttle;
        public PID ForwardsAccel;
        public PID RightwardsAccel;
        public PID YawAngle;

        #region Implementation of IEnumerable

        public IEnumerator<PID> GetEnumerator()
        {
            yield return PitchAngle;
            yield return RollAngle;
            yield return YawAngle;
            yield return ForwardsAccel;
            yield return RightwardsAccel;
            yield return Throttle;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Returns a new PIDSetup with the same PID configuration, but losing the accumulated PID states (diff and integral errors).
        /// </summary>
        /// <returns></returns>
        public PIDSetup Clone()
        {
            return new PIDSetup
                       {
                           Name = Name,
                           PitchAngle = PitchAngle.Clone(),
                           RollAngle = RollAngle.Clone(),
                           Throttle = Throttle.Clone(),
                           ForwardsAccel = ForwardsAccel.Clone(),
                           RightwardsAccel = RightwardsAccel.Clone(),
                           YawAngle = YawAngle.Clone()
                       };
        }

        public override string ToString()
        {
            return String.Format(@"Pitch({0}, {1}, {2}) Roll({3}, {4}, {5}) Yaw({6}, {7}, {8}) Throttle({9}, {10}, {11})", 
                PitchAngle.P, PitchAngle.I, PitchAngle.D,
                RollAngle.P, RollAngle.I, RollAngle.D,
                YawAngle.P, YawAngle.I, YawAngle.D,
                Throttle.P, Throttle.I, Throttle.D);
        }
    }
}