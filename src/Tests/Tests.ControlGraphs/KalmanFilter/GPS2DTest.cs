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
using Swordfish.WPF.Charts;

#endregion

namespace Tests.ControlGraphs.KalmanFilter
{
    internal class GPS2DTest : GraphTestBase
    {
        public GPS2DTest(XYLineChart chart) : base(chart)
        {
        }

        /// <summary></summary>
        public void GPSFilter2DTest()
        {
            try
            {
                if (Chart == null)
                    return;

                // Add test Lines to demonstrate the control
                Chart.Primitives.Clear();

                var filterStartState = new GPSObservation
                                           {
                                               Position = new Vector3(0, 0, 0),
                                               //                                               Velocity = new Vector3(1, 0, 0),
                                               //                                           Acceleration = new Vector3(0, 1, 0),
                                               Time = TimeSpan.Zero,
                                           };

                var filter = new GPSFilter2D(filterStartState);
                var inValues = new List<GPSObservation>();
                for (int t = 0; t < 20; t++)
                {
                    var obs = new GPSObservation
                                  {
                                      Position = new Vector3(t, 2.5f*0.01f*t*t, 0),
                                      //(float) (1 + 1*Math.Cos(2*Math.PI*4*t/100)), 0),
                                      Time = TimeSpan.FromSeconds(t)
                                  };


                    inValues.Add(obs);
                }


                var outValues = new List<StepOutput<GPSFilter2DSample>>();
                foreach (GPSObservation observation in inValues)
                    outValues.Add(filter.Filter(observation, new GPSFilter2DInput
                                                                 {
                                                                     Velocity = new Vector3(1, 0, 0),
                                                                     Acceleration = new Vector3(0, 0.01f, 0),
                                                                 }));

//                IEnumerable<StepOutput<GPSFilter2DSample>> outValues = filter.Filter(inValues);

                //            AddLineXY("N-Window Smoothing", Colors.DarkGoldenrod, originalZValues);//smoothedZValues);
                //            AddLineXY("Noise W", Colors.White, outValues.Select(element => element.W));
                //                AddLineXY("Noise V", Colors.LightGray, outValues.Select(e => e.V.Position));
                //            AddLineXY("k", Colors.Cyan, outValues.Select(element => element.K));
                SwordfishGraphHelper.AddLineXY(Chart, "A Priori X", Colors.Blue, outValues.Select(e => e.PriX.Position));
                SwordfishGraphHelper.AddLineXY(Chart, "A Posteriori X", Colors.YellowGreen, outValues.Select(e => e.PostX.Position));
                SwordfishGraphHelper.AddLineXY(Chart, "X", Color.FromArgb(255, 150, 0, 0), outValues.Select(e => e.X.Position));
                SwordfishGraphHelper.AddLineXY(Chart, "Z", Color.FromArgb(255, 255, 255, 0), outValues.Select(e => e.Z.Position));
                //                AddLineXY(Chart, "True", Color.FromArgb(50, 150, 0, 0), inValues.Select(e => e.Position));

                Chart.Title = "Filter2DTest()";
                Chart.XAxisLabel = "X [m]";
                Chart.YAxisLabel = "Y [m]";

                Chart.RedrawPlotLines();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}