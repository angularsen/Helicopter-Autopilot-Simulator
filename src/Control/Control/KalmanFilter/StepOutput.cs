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

namespace Control.KalmanFilter
{
    public struct StepOutput<T>
    {
        public T K;
        public T PostP;
        public T PostX;
        public T PostXError;
        public T PriP;
        public T PriX;
        public T PriXError;
        public TimeSpan Time;
        public T V;
        public T W;
        public T X;
        public T Z;
    }
}