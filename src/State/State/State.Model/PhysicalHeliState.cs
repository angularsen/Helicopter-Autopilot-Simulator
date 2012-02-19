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

using Anj.Helpers.XNA;
using Microsoft.Xna.Framework;

#endregion

namespace State.Model
{
    public struct PhysicalHeliState
    {
        public readonly float HeadingAngle;
        public readonly float PitchAngle;
        public readonly float RollAngle;
        public Vector3 Acceleration;
        public Axes Axes;
        public Quaternion Orientation;
        public Vector3 Position;
        public Vector3 Velocity;

        public PhysicalHeliState(Quaternion orientation, Vector3 pos, Vector3 velocity, Vector3 acceleration)
        {
            Orientation = orientation;
            Position = pos;
            Velocity = velocity;
            Acceleration = acceleration;

            Matrix orientationMatrix = Matrix.CreateFromQuaternion(orientation);
            Axes = VectorHelper.GetAxes(orientation);
            PitchAngle = VectorHelper.GetPitchAngle(orientationMatrix);
            RollAngle = VectorHelper.GetRollAngle(orientationMatrix);
            HeadingAngle = VectorHelper.GetHeadingAngle(orientationMatrix);
        }
    }
}