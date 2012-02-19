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

namespace Control.Common
{
    public enum WaypointType
    {
//        Start,
        Intermediate,
        Hover,
        TestDestination,
        Land,
    }

    public class Waypoint
    {
        public float HeadingAngle;
        public Vector3 Position;
        public float Radius;
        public float SecondsToWait; // Used for stop point or start point delay
        public float SecondsWaited;

        public Waypoint(Vector3 pos, float headingAngle, WaypointType type, float radius)
        {
            Type = type;
            Position = pos;
            HeadingAngle = headingAngle;
            Radius = radius;

            if (type == WaypointType.Hover) SecondsToWait = 2.0f;
            else if (type == WaypointType.Land) SecondsToWait = float.MaxValue;
            else SecondsToWait = 0;

            SecondsWaited = 0;
        }

        public WaypointType Type { get; set; }

        public bool DoneWaiting
        {
            get { return SecondsWaited >= SecondsToWait; }
        }

        public bool IsWithinRadius(Vector3 pos)
        {
            return (Position - pos).Length() < Radius;
        }

        public bool IsWithinRadius(Vector2 pos)
        {
            var horPosition = VectorHelper.ToHorizontal(Position);
            return (horPosition - pos).Length() < Radius;
        }
    }
}