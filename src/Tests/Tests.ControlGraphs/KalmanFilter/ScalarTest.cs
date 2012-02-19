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
using Anj.Helpers.XNA;
using Control.KalmanFilter;
using Swordfish.WPF.Charts;

#endregion

namespace Tests.ControlGraphs.KalmanFilter
{
    public class ScalarTest : GraphTestBase
    {
        public ScalarTest(XYLineChart chart)
            : base(chart)
        {
        }

        /// <summary>
        ///   According to
        ///   http://www.swarthmore.edu/NatSci/echeeve1/Ref/Kalman/Ex1ScalarKalman.html
        /// </summary>
        public void ConstantOutputTest()
        {
            try
            {
                if (Chart == null)
                    return;

                // Add test Lines to demonstrate the control
                Chart.Primitives.Clear();

                const float a = 1, b = 0, h = 3, Q = 0, R = 1;
                var filter = new ScalarKF(a, b, h, Q, R);
                float[] inValues = SwordfishGraphHelper.CreateFloatArray(100, 1);
                IList<StepOutput<float>> outValues = filter.Filter(inValues);

                var smoothedZValues = new List<float>(outValues.Select(element => element.Z));
                int halfWindowLength = inValues.Length/10;
                for (int i = halfWindowLength; i < smoothedZValues.Count - halfWindowLength; i++)
                {
                    float windowMean = 0;
                    for (int j = -halfWindowLength; j <= halfWindowLength; j++)
                    {
                        int windowIndex = i + j;
                        windowMean += smoothedZValues[windowIndex];
                    }

                    windowMean /= halfWindowLength*2;
                    smoothedZValues[i] = windowMean;
                }

                SwordfishGraphHelper.AddLineXY(Chart, "N-Window Smoothing", Colors.DarkGoldenrod, smoothedZValues);
                SwordfishGraphHelper.AddLineXY(Chart, "Noise V", Colors.LightGray,
                                               outValues.Select(element => element.V));
                SwordfishGraphHelper.AddLineXY(Chart, "k", Colors.Cyan, outValues.Select(element => element.K));
                SwordfishGraphHelper.AddLineXY(Chart, "A Posteriori X", Colors.YellowGreen,
                                               outValues.Select(element => element.PostX));
                SwordfishGraphHelper.AddLineXY(Chart, "A Priori X", Colors.Blue,
                                               outValues.Select(element => element.PriX));
                SwordfishGraphHelper.AddLineXY(Chart, "X", Colors.Red, outValues.Select(element => element.X));

                Chart.Title = "ConstantOutputTest()";
                Chart.XAxisLabel = "X Axis";
                Chart.YAxisLabel = "Y Axis";

                Chart.RedrawPlotLines();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }


        /// <summary>
        ///   According to 
        ///   http://www.swarthmore.edu/NatSci/echeeve1/Ref/Kalman/Ex2ScalarKalman.html
        /// </summary>
        public void FirstOrderOutputTest()
        {
            try
            {
                if (Chart == null)
                    return;

                // Add test Lines to demonstrate the control
                Chart.Primitives.Clear();

                const float a = 0.85f, b = 0, h = 3, Q = 0.1f, R = 1;
                var filter = new ScalarKF(a, b, h, Q, R);
                float[] inValues = SwordfishGraphHelper.CreateFloatArray(100, 1);
                IList<StepOutput<float>> outValues = filter.Filter(inValues);

                var originalZValues = new List<float>(outValues.Select(element => element.Z));
//            var smoothedZValues = new List<float>();
                int halfWindowLength = inValues.Length/10;
                for (int i = halfWindowLength; i < originalZValues.Count - halfWindowLength; i++)
                {
                    float windowMean = 0;
                    for (int j = -halfWindowLength; j <= halfWindowLength; j++)
                    {
                        int windowIndex = i + j;
                        windowMean += originalZValues[windowIndex];
                    }

                    windowMean /= halfWindowLength*2;
                    //                smoothedZValues.Add(windowMean);
                    originalZValues[i] = windowMean;
                }

//            AddLineXY("N-Window Smoothing", Colors.DarkGoldenrod, originalZValues);//smoothedZValues);
//            AddLineXY("Noise W", Colors.White, outValues.Select(element => element.W));
//            AddLineXY("Noise V", Colors.LightGray, outValues.Select(element => element.V));
//            AddLineXY("k", Colors.Cyan, outValues.Select(element => element.K));
                SwordfishGraphHelper.AddLineXY(Chart, "A Posteriori X", Colors.YellowGreen,
                                               outValues.Select(element => element.PostX));
                SwordfishGraphHelper.AddLineXY(Chart, "A Priori X", Colors.Blue,
                                               outValues.Select(element => element.PriX));
                SwordfishGraphHelper.AddLineXY(Chart, "X", Colors.Red, outValues.Select(element => element.X));

                Chart.Title = "FirstOrderOutputTest()";
                Chart.XAxisLabel = "X Axis";
                Chart.YAxisLabel = "Y Axis";

                Chart.RedrawPlotLines();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void GaussianDistributionTest()
        {
            try
            {
                if (Chart == null)
                    return;

                try
                {
                    Chart.Primitives.Clear();

                    var rand = new GaussianRandom();

                    float minY = float.MaxValue;
                    float maxY = float.MinValue;
                    var values = new float[100000];
                    for (int x = 0; x < values.Length; x++)
                    {
                        float val = rand.NextGaussian(0, 1);
                        values[x] = val;
                        if (val > maxY) maxY = val;
                        if (val < minY) minY = val;
                    }


                    const int distributionSteps = 1000;
                    float stepSizeY = (maxY - minY)/distributionSteps;
                    var distributionBucketCount = new Point[distributionSteps];

                    // Set X-positions for all points 
                    for (int i = 0; i < distributionBucketCount.Length; i++)
                        distributionBucketCount[i].X = MyMathHelper.Lerp(i, 0, distributionBucketCount.Length, 0,
                                                                         values.Length);

                    // Increase Y-position by one for each bucket hit
                    foreach (float valY in values)
                    {
                        int bucketIndex = Convert.ToUInt16(Math.Min(distributionSteps - 1, (valY - minY)/stepSizeY));
                        distributionBucketCount[bucketIndex].Y++;
                    }

                    SwordfishGraphHelper.AddLineXY(Chart, "Gaussian values", Colors.Gray, values);
                    SwordfishGraphHelper.AddLineXY(Chart, "Distribution bucket count", Colors.Red,
                                                   distributionBucketCount);

                    Chart.Title = "GaussianDistributionTest()";
                    Chart.XAxisLabel = "X Axis";
                    Chart.YAxisLabel = "Y Axis";

                    Chart.RedrawPlotLines();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    Console.WriteLine(e);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}