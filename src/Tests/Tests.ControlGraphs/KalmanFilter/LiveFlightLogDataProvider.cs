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

using System.Windows.Media;
using Microsoft.Xna.Framework;
using State.Model;
using Swordfish.WPF.Charts;
using Point = System.Windows.Point;

#endregion

namespace Tests.ControlGraphs.KalmanFilter
{
    public class LiveFlightLogDataProvider //: GraphTestBase
    {
        private readonly ChartPrimitive _accelForwardGraph;
        private readonly ChartPrimitive _accelRightGraph;
        private readonly ChartPrimitive _accelUpGraph;
        private readonly XYLineChart _accelerometerChart;
        private readonly ChartPrimitive _estimatedGraph;
        private readonly ChartPrimitive _observedGraph;
        private readonly XYLineChart _positionChart;
        private readonly ChartPrimitive _trueGraph;
        private readonly ChartPrimitive _blindEstimatedGraph;

        private readonly ChartPrimitive _groundHeightGraph;
        private readonly ChartPrimitive _helicopterHeightGraph;
        private readonly ChartPrimitive _estimatedHelicopterHeightGraph;
        private readonly XYLineChart _heightChart;
        private const int MaxPlots = 500;

        public LiveFlightLogDataProvider(XYLineChart positionChart, XYLineChart accelerometerChart, XYLineChart heightChart)
        {
            _positionChart = positionChart;
            _accelerometerChart = accelerometerChart;
            _heightChart = heightChart;

            #region Position Top View setup

            _positionChart.UniformScaling = true;
            _positionChart.Title = "Position Top View";
            _positionChart.XAxisLabel = "X [m]";
            _positionChart.YAxisLabel = "Z [m]";

            _estimatedGraph = new ChartPrimitive
                                  {
                                      Filled = false,
                                      Dashed = false,
                                      ShowInLegend = true,
                                      LineThickness = 1.5,
                                      HitTest = true,
                                      Color = Colors.Navy,
                                      Label = "Estimated"
                                  };

            _blindEstimatedGraph = new ChartPrimitive
                                       {
                                           Filled = false,
                                           Dashed = false,
                                           ShowInLegend = true,
                                           LineThickness = 1.5,
                                           HitTest = true,
                                           Color = Colors.LightBlue,
                                           Label = "Blind estimated"
                                       };

            _trueGraph = new ChartPrimitive
                             {
                                 Filled = false,
                                 Dashed = false,
                                 ShowInLegend = true,
                                 LineThickness = 1.5,
                                 HitTest = true,
                                 Color = Colors.DarkRed,
                                 Label = "True"
                             };

            _observedGraph = new ChartPrimitive
                                 {
                                     Filled = false,
                                     Dashed = false,
                                     ShowInLegend = true,
                                     LineThickness = 1.5,
                                     HitTest = true,
                                     Color = Colors.Orange,
                                     Label = "GPS"
                                 };

            #endregion

            #region Height Data setup

            _heightChart.Title = "Height data";
            _heightChart.XAxisLabel = "Time [s]";
            _heightChart.YAxisLabel = "Altitude [m]";

            _helicopterHeightGraph = new ChartPrimitive
                                         {
                                             Filled = false,
                                             Dashed = false,
                                             ShowInLegend = true,
                                             LineThickness = 1.5,
                                             HitTest = true,
                                             Color = Colors.DarkRed,
                                             Label = "True"
                                         };

            _estimatedHelicopterHeightGraph = new ChartPrimitive
            {
                Filled = false,
                Dashed = false,
                ShowInLegend = true,
                LineThickness = 1.5,
                HitTest = true,
                Color = Colors.Navy,
                Label = "Estimated"
            };

            _groundHeightGraph = new ChartPrimitive
                                     {
                                         Filled = false,
                                         Dashed = false,
                                         ShowInLegend = true,
                                         LineThickness = 1.5,
                                         HitTest = true,
                                         Color = Colors.DarkGray,
                                         Label = "Ground"
                                     };

            #endregion

            #region Acceleration Data setup

            _accelerometerChart.Title = "Accelerometer data";
            _accelerometerChart.XAxisLabel = "Time [s]";
            _accelerometerChart.YAxisLabel = "Acceleration [m/s²]";

            _accelForwardGraph = new ChartPrimitive
                                     {
                                         Filled = false,
                                         Dashed = false,
                                         ShowInLegend = true,
                                         LineThickness = 1.5,
                                         HitTest = true,
                                         Color = Colors.Navy,
                                         Label = "Accel forward"
                                     };

            _accelRightGraph = new ChartPrimitive
                                   {
                                       Filled = false,
                                       Dashed = false,
                                       ShowInLegend = true,
                                       LineThickness = 1.5,
                                       HitTest = true,
                                       Color = Colors.DarkRed,
                                       Label = "Accel right"
                                   };

            _accelUpGraph = new ChartPrimitive
                                {
                                    Filled = false,
                                    Dashed = false,
                                    ShowInLegend = true,
                                    LineThickness = 1.5,
                                    HitTest = true,
                                    Color = Colors.Orange,
                                    Label = "Accel up"
                                };

            #endregion

        }

        public void Add(HelicopterLogSnapshot snapshot)
        {
            // GPS observations only occur every 1 second, so ignore Vector3.Zero positions (not 100% safe, but close enough)
            // Note uncomment this to add GPS observations to live flight log window
//            if (snapshot.Observed.Position != Vector3.Zero)
//                AppendLineXZ(_observedGraph, snapshot.Observed.Position);

            AppendLineXZ(_estimatedGraph, snapshot.Estimated.Position);
            AppendLineXZ(_blindEstimatedGraph, snapshot.BlindEstimated.Position);
            AppendLineXZ(_trueGraph, snapshot.True.Position);

            double seconds = snapshot.Time.TotalSeconds;

            AppendLine(_helicopterHeightGraph, seconds, snapshot.True.Position.Y);
            AppendLine(_estimatedHelicopterHeightGraph, seconds, snapshot.Estimated.Position.Y);
            AppendLine(_groundHeightGraph, seconds, snapshot.GroundAltitude);

            AppendLine(_accelForwardGraph, seconds, snapshot.Accelerometer.Forward);
            AppendLine(_accelRightGraph, seconds, snapshot.Accelerometer.Right);
            AppendLine(_accelUpGraph, seconds, snapshot.Accelerometer.Up);

            AddGraphsToChartsIfNecessary();

            // TODO Force area to show 500 samples of width even if we have less than 500 samples to plot
//            _heightChart.PlotAreaOverride = new Rect(_heightChart.P);


            _positionChart.RedrawPlotLines();
            _heightChart.RedrawPlotLines();
            _accelerometerChart.RedrawPlotLines();
        }

        

        /// <summary>
        /// Temporary method. Our charts crashes if they contain 0 or 1 elements because the
        /// calculated size of the canvas is 0. This should be fixed in the chart component later.
        /// </summary>
        private void AddGraphsToChartsIfNecessary()
        {
            // The chart crashes if we add empty primitives or that has only one plot, 
            // because it auto zooms and won't know what scale to use with only one plot.
            if (_trueGraph.Points.Count == 2)
            {
                _positionChart.Primitives.Add(_estimatedGraph);
                _positionChart.Primitives.Add(_observedGraph);
                _positionChart.Primitives.Add(_trueGraph);
                _positionChart.Primitives.Add(_blindEstimatedGraph);
            }

            if (_helicopterHeightGraph.Points.Count == 2)
            {
                _heightChart.Primitives.Add(_helicopterHeightGraph);
                _heightChart.Primitives.Add(_estimatedHelicopterHeightGraph);
                _heightChart.Primitives.Add(_groundHeightGraph);
            }

            if (_accelForwardGraph.Points.Count == 2)
            {
                _accelerometerChart.Primitives.Add(_accelForwardGraph);
                _accelerometerChart.Primitives.Add(_accelRightGraph);
                _accelerometerChart.Primitives.Add(_accelUpGraph);
            }
        }

        private static void AppendLineXZ(ChartPrimitive graph, Vector3 plot)
        {
            AppendLine(graph, plot.X, plot.Z);
        }

        private static void AppendLine(ChartPrimitive graph, double x, float y)
        {
            graph.AddPoint(new Point(x, y));
            if (graph.Points.Count > MaxPlots)
                graph.Points.RemoveAt(0);
        }

        public void Clear()
        {
            _estimatedGraph.Points.Clear();
            _blindEstimatedGraph.Points.Clear();
            _trueGraph.Points.Clear();
            _observedGraph.Points.Clear();

            _accelForwardGraph.Points.Clear();
            _accelRightGraph.Points.Clear();
            _accelUpGraph.Points.Clear();

            _positionChart.Primitives.Clear();
            _accelerometerChart.Primitives.Clear();
        }
    }
}