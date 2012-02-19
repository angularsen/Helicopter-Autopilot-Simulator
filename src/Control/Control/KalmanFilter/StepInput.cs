#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

namespace Control.KalmanFilter
{
    public struct StepInput<T>
    {
        public T PostP;
        public T PostX;
        public T X;

        public StepInput(StepOutput<T> prev)
        {
            PostP = prev.PostP;
            PostX = prev.PostX;
            X = prev.X;
        }

        public StepInput(T x0, T postX0, T postP0)
        {
            X = x0;
            PostX = postX0;
            PostP = postP0;
        }
    }
}