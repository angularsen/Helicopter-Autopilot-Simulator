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
using Microsoft.Xna.Framework;

#endregion

namespace State.Model
{
    public struct Angles
    {
        public float BearingAngle;
        public float BearingErrorAngle;
        public float GoalAngle;
        public float HeadingAngle;
        public float PitchAngle;
        public float RollAngle;
    }

    public struct HeliState
    {
        public float HeightAboveGround;
        public Vector3 Acceleration;
        public Angles Degrees;
        public Vector3 Forward;
        public Vector2 HPositionToGoal;
        public JoystickOutput Output;
        public Quaternion Orientation;
        public Vector3 Position;
        public Angles Radians;
        public Vector3 Right;
        public Vector3 Up;
        public Vector3 Velocity;
        public Waypoint Waypoint;

        public override string ToString()
        {
            return String.Format("Position {0},{1},{2} - Velocity {3}km/h - Heading {4}",
                                 Math.Round(Position.X,1), Math.Round(Position.Y,1), Math.Round(Position.Z,1),
                                 Math.Round(Velocity.Length()*3.6, 1),
                                 Convert.ToInt32(Degrees.HeadingAngle)
                );
        }
    }
}