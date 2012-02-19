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
using Microsoft.Xna.Framework;

#endregion

namespace Sensors.SampleDistortion
{
    public static class NonLinearity
    {
        /// <summary>
        ///   Transforms linear values to an S-curve with the same start and end point.
        ///   The goal is to both undershoot and overshoot the line to achieve the S-shape
        ///   and will accomplish so using Hermite curves.
        /// </summary>
        /// <param name="plot"></param>
        /// <param name="start"></param>
        /// <param name="end">
        ///   end.X and end.Y must be larger than start.X and start.Y
        /// </param>
        /// <param name="amplitudeY"></param>
        /// <param name="waveLengthX"></param>
        /// <returns></returns>
        public static Vector2 SinusOffset(Vector2 plot, Vector2 start, Vector2 end, float amplitudeY, float waveLengthX)
        {
//            if (start.X >= end.X) throw new ArgumentException("start.X must be less than end.X");

            // By choosing start and end tangents that are less than the linear tangent,
            // the Hermite curve will get an S-shape.
//            float linearTangent = (end.Y - start.Y)/(end.X - start.X);
//            Vector2 curveTangent = new Vector2(1, 1f * linearTangent);

//            float progress = MyMathHelper.InvLerp(plot.X, start.X, end.X);
//            Vector2 curvePlot = Vector2.Hermite(start, curveTangent, end, curveTangent, progress);
//            Vector2 curvePlot = Vector2.CatmullRom(start + new Vector2(-1, 0), start, end, end + new Vector2(1, 0), progress);
//            Vector2 linearPlot = Vector2.Lerp(start, end, progress);
//            float curveOffsetY = curvePlot.Y - linearPlot.Y;

            // Offset the original plot by a S-curve (which most likely won't follow perfect linear interpolations anyway)
//            plot.Y += curveOffsetY;

            // The sinus will perform a cycle for each period of waveLengthX 
            float distanceX = plot.X - start.X;
            plot.Y += amplitudeY*(float) Math.Sin(distanceX/waveLengthX*2*Math.PI);
            return plot;
        }
    }
}