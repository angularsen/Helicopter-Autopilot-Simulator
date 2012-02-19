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

using Swordfish.WPF.Charts;

#endregion

namespace Tests.ControlGraphs
{
    public class GraphTestBase
    {
        protected readonly XYLineChart Chart;

        protected GraphTestBase(XYLineChart positionChart)
        {
            Chart = positionChart;
        }

//        public static ChartPrimitive GetLine(IEnumerable<Point> points)
//        {
//            var line = new ChartPrimitive
//                           {
//                               Filled = false,
//                               Dashed = false,
//                               ShowInLegend = true,
//                               LineThickness = 1.5,
//                               HitTest = true
//                           };
//
//            foreach (Point point in points)
//                line.AddPoint(point);
//
//            return line;
//        }

//        protected void AddLineXY(string label, Color color, IEnumerable<Vector3> points)
//        {
//            AddLineXY(label, color, points.To Select(e => new Point(e.X, e.Y)));
//        }
//
//        protected void AddLineXZ(string label, Color color, IEnumerable<Vector3> points)
//        {
//            AddLineXY(label, color, points.Select(e => new Point(e.X, e.Z)));
//        }
//
//        protected static float[] CreateFloatArray(int length, int fillValue)
//        {
//            var array = new float[length];
//            for (int i = 0; i < array.Length; i++)
//                array[i] = fillValue;
//
//            return array;
//        }
//
//        protected void AddLineXY(string label, Color color, IEnumerable<Point> points)
//        {
//            ChartPrimitive line = GetLine(points);
//            line.Color = color;
//            line.Label = label;
//
//            if (Chart != null)
//                Chart.Primitives.Add(line);
//        }
//
//        protected void AddLineXY(string label, Color color, IEnumerable<float> values)
//        {
//            IEnumerable<Point> points = values.Select((val, index) => new Point(index, val));
//            AddLineXY(label, color, points);
//        }
    }
}