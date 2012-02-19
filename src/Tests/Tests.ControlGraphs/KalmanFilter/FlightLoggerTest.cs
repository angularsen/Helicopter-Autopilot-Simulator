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
using System.Windows.Media;
using Anj.Helpers.Swordfish;
using Microsoft.Xna.Framework;
using Simulator.Utils;
using State.Model;
using Swordfish.WPF.Charts;

#endregion

namespace Tests.ControlGraphs.KalmanFilter
{
    public class FlightLoggerTest : GraphTestBase
    {
        public FlightLoggerTest(XYLineChart chart)
            : base(chart)
        {
        }

        public void HorizontalFlightTest()
        {
            const string filepath = "../../../../../Startup/HelicopterSim/bin/x86/Release/" + "flightpath.xml";
            FlightLog flightLog = FlightLogXMLFile.Read(filepath);
            if (flightLog == null)
                throw new Exception("Could not read flight log from file: " + filepath);

            HorizontalFlightTest(flightLog.Plots);
        }

        /// <summary></summary>
        public void HorizontalFlightTest(IEnumerable<HelicopterLogSnapshot> flightLog)
        {
            if (flightLog == null)
                return;

            if (Chart == null)
                return;


            // Add test Lines to demonstrate the control
            Chart.Primitives.Clear();

            IEnumerable<Vector3> horizontalGPSSamples = flightLog.Select(e => e.Observed.Position).Where(p => p != Vector3.Zero);

            float radiusVariance = new Vector3(
                horizontalGPSSamples.Select(e => e.X).StandardDeviation(),
                horizontalGPSSamples.Select(e => e.Y).StandardDeviation(),
                horizontalGPSSamples.Select(e => e.Z).StandardDeviation()).Length();

            Console.WriteLine(@"Radius std. dev. of GPS data: " + radiusVariance + @"m.");

            // GPS observations only occur every 1 second, so ignore Vector3.Zero positions (not 100% safe, but close enough)
            SwordfishGraphHelper.AddLineXZ(Chart, "GPS", Colors.Orange, horizontalGPSSamples);
            SwordfishGraphHelper.AddLineXZ(Chart, "Estimated", Colors.Navy, flightLog.Select(e => e.Estimated.Position));
            SwordfishGraphHelper.AddLineXZ(Chart, "True", Colors.DarkRed, flightLog.Select(e => e.True.Position));

            Chart.Title = "HorizontalFlightTest()";
            Chart.XAxisLabel = "X [m]";
            Chart.YAxisLabel = "Z [m]";

            Chart.RedrawPlotLines();
        }
    }
}