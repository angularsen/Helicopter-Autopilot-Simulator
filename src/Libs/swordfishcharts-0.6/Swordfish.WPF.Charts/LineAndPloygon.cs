#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

namespace Swordfish.WPF.Charts
{
    /// <summary>
    /// Holds a line and a polygon pair
    /// </summary>
    public class LineAndPolygon
    {
        public ChartPrimitive Line;
        public ChartPrimitive Polygon;

        public LineAndPolygon()
        {
        }

        public LineAndPolygon(ChartPrimitive line, ChartPrimitive polygon)
        {
            Line = line;
            Polygon = polygon;
        }
    }
}