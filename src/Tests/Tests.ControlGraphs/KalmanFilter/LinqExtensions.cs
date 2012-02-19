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
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Tests.ControlGraphs.KalmanFilter
{
    /// <summary>
    /// http://www.codeproject.com/KB/linq/LinqStatistics.aspx
    /// </summary>
    public static class LinqExtensions
    {
        public static float Variance(this IEnumerable<float> source)
        {
            float avg = source.Average();
            float d = source.Aggregate(0.0f,
                                       (total, next) => total + (float) Math.Pow(next - avg, 2));
            return d/(source.Count() - 1);
        }

        public static float StandardDeviation(this IEnumerable<float> source)
        {
            return (float) Math.Sqrt(source.Variance());
        }
    }
}