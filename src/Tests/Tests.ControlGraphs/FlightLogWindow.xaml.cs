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

using System.Windows;
using Tests.ControlGraphs.KalmanFilter;

#endregion

namespace Tests.ControlGraphs
{
    /// <summary>
    ///   Interaction logic for FlightLogWindow.xaml
    /// </summary>
    public partial class FlightLogWindow : Window
    {
        public FlightLogWindow()
        {
            InitializeComponent();

//            new ScalarTest(XYLineChart).FirstOrderOutputTest();
//            new ScalarTest(XYLineChart).ConstantOutputTest();
//            new ScalarTest(XYLineChart).GaussianDistributionTest();

//            new GPS2DTest(Graph.XYLineChart).GPSFilter2DTest();
            //new GPS_INSTest(Graph.XYLineChart).Filter2DTest();
//            new FlightLoggerTest(Graph.XYLineChart).HorizontalFlightTest();
        }
    }
}