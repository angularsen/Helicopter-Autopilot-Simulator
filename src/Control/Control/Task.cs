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
using Control.Common;

#endregion

namespace Control
{
    public class Task : IEnumerable<Waypoint>
    {
        private readonly IList<Waypoint> _waypoints;
        private int _currentWaypointIndex;
        private List<Waypoint> _allWaypoints;
        public IEnumerable<Waypoint> AllWaypoints { get { return _allWaypoints; } }

            public Task()
        {
            _waypoints = new List<Waypoint>();
            _allWaypoints = new List<Waypoint>();
        }

        /// <summary>
        /// [Meters]. Any positive value larger than 0 will tell the autopilot to maintain this height above ground.
        /// </summary>
        public int HoldHeightAboveGround { get; set; }
        public bool Loop { get; set; }

        public Waypoint Current
        {
            get
            {
                if (_waypoints.Count == 0)
                    return null;

                return _waypoints[_currentWaypointIndex];
            }
        }


        public void AddWaypoint(Waypoint wp)
        {
            _waypoints.Add(wp);
            _allWaypoints.Add(wp);
        }

        /// <summary>
        /// Progress to the next waypoint.
        /// </summary>
        public void Next()
        {
            if (_waypoints.Count == 0) throw new Exception("Cannot proceed to next waypoint. No more waypoints left!");


            // A looping task never removes any waypoints as a result of proceeding to next waypoint,
            // but a non-looping task simply removes waypoints until there is no more left.
            if (Loop)
                _currentWaypointIndex = (_currentWaypointIndex + 1)%_waypoints.Count;
            else
                _waypoints.RemoveAt(0);
        }

        public Task Clone()
        {
            var r = new Task();

            foreach (Waypoint wp in this)
                r.AddWaypoint(wp);

            r.HoldHeightAboveGround = HoldHeightAboveGround;
            r.Loop = Loop;
            r._currentWaypointIndex = _currentWaypointIndex;
            r._allWaypoints = _allWaypoints;

            return r;
        }

        #region Implementation of IEnumerable

        public IEnumerator<Waypoint> GetEnumerator()
        {
            return _waypoints.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}