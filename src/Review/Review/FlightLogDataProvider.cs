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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Control.Common;
using Microsoft.Xna.Framework;
using Simulator.Utils;
using State.Model;
using Swordfish.WPF.Charts;
using Swordfish.WPF.Charts.Extensions;
using Point = System.Windows.Point;

#endregion

namespace Review
{
    public class FlightLogDataProvider
    {
        private readonly ChartPrimitive _accelForwardGraph;
        private readonly ChartPrimitive _accelRightGraph;
        private readonly ChartPrimitive _accelUpGraph;
        private readonly XYLineChart _accelerometerChart;
        private readonly ChartPrimitive _blindEstimatedGraph;
        private readonly ChartPrimitive _estimatedGraph;
        private readonly ChartPrimitive _estimatedHelicopterHeightGraph;
        private readonly ChartPrimitive _groundHeightGraph;
        private readonly XYLineChart _heightChart;
        private readonly ChartPrimitive _helicopterHeightGraph;
        private readonly ChartPrimitive _observedGraph;
        private readonly XYLineChart _positionChart;
        private readonly ChartPrimitive _trueGraph;
        private ChartMarkerSet _gpsMarkers;
        private ChartMarkerSet _waypointMarkers;
        private ChartMarkerSet _gpsMarkersAltitude;
//        private ChartMarkerSet _waypointMarkersAltitude;
        private readonly ChartPrimitive _observedHeightGraph;
        private readonly ChartPrimitive _waypointGraph;
        private bool _comparisonIsEmpty = true;
//        private readonly List<ChartPrimitive> _positionComparisonGraphs;
//        private readonly List<ChartPrimitive> _altitudeComparisonGraphs;

        public FlightLogDataProvider(XYLineChart positionChart, XYLineChart accelerometerChart, XYLineChart heightChart)
        {
            _positionChart = positionChart;
            _accelerometerChart = accelerometerChart;
            _heightChart = heightChart;

//            _positionComparisonGraphs = new List<ChartPrimitive>();
//            _altitudeComparisonGraphs = new List<ChartPrimitive>();

            #region Position Top View setup

//            _positionChart.UniformScaling = true;
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

            _waypointGraph = new ChartPrimitive
            {
                ShowInLegend = true,
                Color = Colors.Yellow,
                Label = "Waypoint"
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

            _observedHeightGraph = new ChartPrimitive
            {
                ShowInLegend = true,
                Color = Colors.Orange,
                Label = "GPS"
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

        public void Set(FlightLog flightLog)
        {
            Clear();

            PrepareCharts();

            foreach (var snapshot in flightLog.Plots)
                AddPlot(snapshot);

            foreach (Waypoint waypoint in flightLog.Waypoints)
            {
                _waypointMarkers.AddPoint(new ChartPoint("", 
                    new Point(waypoint.Position.X, waypoint.Position.Z)));
            }

            RenderAndSetScale(flightLog.Plots.Last().Time);
        }

        public void AddToComparison(FlightLog flightLog, Color graphColor, string label, double thickness)
        {
            var posGraph = new ChartPrimitive
            {
                Filled = false,
                Dashed = false,
                ShowInLegend = true,
                LineThickness = thickness,
                HitTest = true,
                Color = graphColor,
                Label = label
            };

            var altitudeGraph = new ChartPrimitive
            {
                Filled = false,
                Dashed = false,
                ShowInLegend = true,
                LineThickness = thickness,
                HitTest = true,
                Color = graphColor,
                Label = label
            };

            _positionChart.Primitives.Add(posGraph);
            _heightChart.Primitives.Add(altitudeGraph);

            if (_comparisonIsEmpty)
                _heightChart.Primitives.Add(_groundHeightGraph);

//            _waypointMarkers = new ChartMarkerSet(_waypointGraph)
//            {
//                Stroke = Colors.Black,
//                Fill = Color.FromArgb(150, 255, 255, 0),
//                Size = 20,
//                StrokeWidth = 1,
//            };
//
//            _positionChart.MarkerSets.Add(_waypointMarkers);


            foreach (var snapshot in flightLog.Plots)
            {
                AppendLineXZ(posGraph, snapshot.True.Position);
                AppendLine(altitudeGraph, snapshot.Time.TotalSeconds, snapshot.True.Position.Y);

                if (_comparisonIsEmpty)
                    AppendLine(_groundHeightGraph, snapshot.Time.TotalSeconds, snapshot.GroundAltitude);
            }

            
//            foreach (Waypoint waypoint in flightLog.Waypoints)
//            {
//                _waypointMarkers.AddPoint(new ChartPoint("",
//                    new Point(waypoint.Position.X, waypoint.Position.Z)));
//            }

            RenderAndSetScale(flightLog.Plots.Last().Time);

            _comparisonIsEmpty = false;
        }

        public void Clear()
        {
            _comparisonIsEmpty = true;

//            _positionComparisonGraphs.Clear();
//            _altitudeComparisonGraphs.Clear();

            _positionChart.MarkerSets.Clear();
            _heightChart.MarkerSets.Clear();

            _estimatedGraph.Points.Clear();
            _blindEstimatedGraph.Points.Clear();
            _trueGraph.Points.Clear();
            _observedGraph.Points.Clear();

            _accelForwardGraph.Points.Clear();
            _accelRightGraph.Points.Clear();
            _accelUpGraph.Points.Clear();

            _groundHeightGraph.Points.Clear();
            _helicopterHeightGraph.Points.Clear();
            _estimatedHelicopterHeightGraph.Points.Clear();

            _positionChart.Primitives.Clear();
            _accelerometerChart.Primitives.Clear();
            _heightChart.Primitives.Clear();
        }

        #region Private Helpers

        private void RenderAndSetScale(TimeSpan time)
        {
            _positionChart.RedrawPlotLines();
            _heightChart.RedrawPlotLines();
            _accelerometerChart.RedrawPlotLines();

            _positionChart.PlotAreaOverride = new Rect(0, 0, 256, 256);


            var minPoint = _heightChart.GetMinPoint();
            var maxPoint = _heightChart.GetMaxPoint();

            const double startTime = -1;
            double endTime = 2 + time.TotalSeconds;

            var rect = new Rect(minPoint, maxPoint);
            rect.Y -= 5;
            rect.Height += 10;
            rect.X = startTime;
            rect.Width = endTime;
            _heightChart.PlotAreaOverride = rect;
        }

        private void PrepareCharts()
        {
            _positionChart.Primitives.Add(_estimatedGraph);
            _positionChart.Primitives.Add(_observedGraph);
            _positionChart.Primitives.Add(_trueGraph);
            _positionChart.Primitives.Add(_blindEstimatedGraph);
            _positionChart.Primitives.Add(_waypointGraph);


            _heightChart.Primitives.Add(_helicopterHeightGraph);
            _heightChart.Primitives.Add(_estimatedHelicopterHeightGraph);
            _heightChart.Primitives.Add(_groundHeightGraph);
            _heightChart.Primitives.Add(_observedHeightGraph);

            _accelerometerChart.Primitives.Add(_accelForwardGraph);
            _accelerometerChart.Primitives.Add(_accelRightGraph);
            _accelerometerChart.Primitives.Add(_accelUpGraph);

            _gpsMarkers = new ChartMarkerSet(_observedGraph)
                              {
                                  Fill = _observedGraph.Color,
                                  Stroke = Colors.Black,
                                  StrokeWidth = 1,
                                  Size = 10
                              };

            _waypointMarkers = new ChartMarkerSet(_waypointGraph)
                                   {
                                       Stroke = Colors.Black,
                                       Fill = Color.FromArgb(150, 255, 255, 0),
                                       Size = 20,
                                       StrokeWidth = 1,
                                   };

            _gpsMarkersAltitude = new ChartMarkerSet(_helicopterHeightGraph)
                                      {
                                          Fill = _observedHeightGraph.Color,
                                          Stroke = Colors.Black,
                                          StrokeWidth = 1,
                                          Size = 10
                                      };

//            _waypointMarkersAltitude = new ChartMarkerSet(_helicopterHeightGraph)
//                                           {
//                                               Stroke = Colors.Yellow,
//                                               Fill = Colors.Transparent,
//                                               Size = 20,
//                                               StrokeWidth = 3,
//                                           };

            _positionChart.MarkerSets.Add(_gpsMarkers);
            _positionChart.MarkerSets.Add(_waypointMarkers);

            _heightChart.MarkerSets.Add(_gpsMarkersAltitude);
//            _heightChart.MarkerSets.Add(_waypointMarkersAltitude);
        }


        private void AddPlot(HelicopterLogSnapshot snapshot)
        {
            double seconds = snapshot.Time.TotalSeconds;

            // GPS observations only occur every 1 second, so ignore Vector3.Zero positions (not 100% safe, but close enough)
            // Note uncomment this to add GPS observations 
            if (snapshot.Observed.Position != Vector3.Zero)
            {
                _gpsMarkers.AddPoint(new ChartPoint("", 
                                                    new Point(snapshot.Observed.Position.X, snapshot.Observed.Position.Z)));

                _gpsMarkersAltitude.AddPoint(new ChartPoint("",
                                                            new Point(seconds, snapshot.Observed.Position.Y)));
            }


            AppendLineXZ(_estimatedGraph, snapshot.Estimated.Position);
            AppendLineXZ(_blindEstimatedGraph, snapshot.BlindEstimated.Position);
            AppendLineXZ(_trueGraph, snapshot.True.Position);


            AppendLine(_helicopterHeightGraph, seconds, snapshot.True.Position.Y);
            AppendLine(_estimatedHelicopterHeightGraph, seconds, snapshot.Estimated.Position.Y);
            AppendLine(_groundHeightGraph, seconds, snapshot.GroundAltitude);

            AppendLine(_accelForwardGraph, seconds, snapshot.Accelerometer.Forward);
            AppendLine(_accelRightGraph, seconds, snapshot.Accelerometer.Right);
            AppendLine(_accelUpGraph, seconds, snapshot.Accelerometer.Up);
        }





        private static void AppendLineXZ(ChartPrimitive graph, Vector3 plot)
        {
            AppendLine(graph, plot.X, plot.Z);
            
        }

        private static void AppendLine(ChartPrimitive graph, double x, float y)
        {
            graph.AddPoint(new Point(x, y));
//            if (graph.Points.Count > MaxPlots)
//                graph.Points.RemoveAt(0);
        }

        #endregion
    }
}