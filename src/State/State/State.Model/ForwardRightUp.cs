#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

namespace State.Model
{
    public struct ForwardRightUp
    {
        public static ForwardRightUp Zero;
        public float Forward;
        public float Right;
        public float Up;

        static ForwardRightUp()
        {
            Zero = new ForwardRightUp();
        }

        public ForwardRightUp(float forward, float right, float up)
        {
            Forward = forward;
            Right = right;
            Up = up;
        }

        public override string ToString()
        {
            return string.Format("Forward: {0} Right: {1} Up: {2}", Forward, Right, Up);
        }
    }
}