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
using System.Windows;
using System.Windows.Media;
using Anj.Helpers.Swordfish;
using Control.KalmanFilter;
using Microsoft.Xna.Framework;
using Sensors.Model;
using Swordfish.WPF.Charts;

#endregion

namespace Tests.ControlGraphs.KalmanFilter
{
    internal class GPS_INSTest : GraphTestBase
    {
        public GPS_INSTest(XYLineChart chart)
            : base(chart)
        {
        }

        /// <summary></summary>
        public void Filter2DTest(SensorSpecifications sensorSpecifications)
        {
            try
            {
                if (Chart == null)
                    return;

                // Add test Lines to demonstrate the control
                Chart.Primitives.Clear();


                throw new NotImplementedException(
                    "input.Orientation is not working any more as we no longer uses deltas but rather explicit orientation as input. We need to update orientation for each iteration for this to work again.");
//                var startPos = new Vector3(0, 0, 0);
//                var startVelocity = new Vector3(1, 0, 0);
//                var input = new GPSINSInput
//                                {
//                                    AccelerationWorld = new Vector3(0, +0.02f, 0),
//                                    Orientation = Quaternion.Identity,
//                    PitchDelta = 0,
//                    RollDelta = +MathHelper.ToRadians(10),
//                    YawDelta = 0,
//                    PitchRate = 0,
//                    RollRate = +MathHelper.ToRadians(10),
//                    YawRate = 0,
//                                };
//                var filter = new GPS_INSFilter(TimeSpan.Zero, startPos, startVelocity, 0, 0, 0);
//                var filter = new GPSINSFilter(TimeSpan.Zero, startPos, startVelocity,
//                                               Quaternion.CreateFromYawPitchRoll(0, 0, 0), sensorSpecifications);
                    // TODO GPS noise?
//
//                var inValues = new List<GPSINSObservation>();
//                for (int t = 0; t < 100; t++)
//                {
//                    var obs = new GPSINSObservation
//                                  {
//                                      GPSPosition = startVelocity*t + 0.5f*input.AccelerationWorld*t*t,
//                                      Time = TimeSpan.FromSeconds(t)
//                                  };
//
//
//                    inValues.Add(obs);
//                }
//
//
//                var outValues = new List<StepOutput<GPSINSOutput>>();
//                foreach (GPSINSObservation observation in inValues)
//                    outValues.Add(filter.Filter(observation, input));
//
//                IEnumerable<StepOutput<GPS_INSFilter2DInput>> outValues = filter.Filter(inValues);
//
                //            AddLineXY("N-Window Smoothing", Colors.DarkGoldenrod, originalZValues);//smoothedZValues);
                //            AddLineXY("Noise W", Colors.White, outValues.Select(element => element.W));
                //                AddLineXY("Noise V", Colors.LightGray, outValues.Select(e => e.V.Position));
                //            AddLineXY("k", Colors.Cyan, outValues.Select(element => element.K));
//                AddLineXY("A Priori X", Colors.LightBlue, outValues.Select(e => e.PriX.Position));
//
                // TODO Commented out after converting to quaternions. Need to compute Roll somehow.
//                AddLineXY("Roll", Colors.DeepPink, outValues.Select(e => e.PostX.Orientation.Roll));
//
//                SwordfishGraphHelper.AddLineXY(Chart, "Estimate", Colors.Navy, outValues.Select(e => e.PostX.Position));
//                SwordfishGraphHelper.AddLineXY(Chart, "True", Colors.DarkRed, outValues.Select(e => e.X.Position));
//                SwordfishGraphHelper.AddLineXY(Chart, "GPS", Colors.Yellow, outValues.Select(e => e.Z.Position));
                //                AddLineXY(Chart, "True", Color.FromArgb(50, 150, 0, 0), inValues.Select(e => e.Position));
//
//                Chart.Title = "Filter2DTest()";
//                Chart.XAxisLabel = "X [m]";
//                Chart.YAxisLabel = "Y [m]";
//
//                Chart.RedrawPlotLines();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}