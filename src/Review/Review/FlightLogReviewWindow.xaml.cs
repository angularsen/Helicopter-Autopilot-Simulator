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
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using Microsoft.Xna.Framework.Graphics;
using Simulator.Utils;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using System.Collections.Generic;

#endregion

namespace Review
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class FlightLogReviewWindow : Window
    {
        private readonly FlightLogDataProvider _dataProvider;

        public FlightLogReviewWindow()
        {
            InitializeComponent();

//            MouseDoubleClick += (sender, e) => SaveScreenshot();
            _dataProvider = new FlightLogDataProvider(PositionTopView.XYLineChart, AccelerometerView.XYLineChart,
                                                      HeightView.XYLineChart);

            // By default load the last flightlog
            const string defaultFileLog = @"flightpath.xml";
            if (File.Exists(defaultFileLog))
                LoadFlightLog(defaultFileLog);
        }

        private void SaveScreenshot()
        {
            //byte[] screenshot = GraphsPanel.GetPngImage(1.0);
            //var fileStream = new FileStream(@"GraphsScreenshot.jpg", FileMode.Create, FileAccess.ReadWrite);
            //var binaryWriter = new BinaryWriter(fileStream);
            //binaryWriter.Write(screenshot);
            //binaryWriter.Close();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            var data = ((DataObject) e.Data);
            if (data == null)
                return;

            StringCollection filepaths = data.GetFileDropList();
            if (filepaths.Count == 0)
                return;

            if (filepaths.Count == 1)
                LoadFlightLog(filepaths[0]);
            else
                LogFlightLogComparison(filepaths);
        }

        /// <summary>
        /// Assumes filepaths to the four ex1.xml ex2.xml ex3.xml and ex4.xml files are passed in.
        /// </summary>
        /// <param name="filepaths"></param>
        private void LogFlightLogComparison(StringCollection filepaths)
        {
//            if (!filepaths.Contains("ex1") || !filepaths.Contains("ex2") || !filepaths.Contains("ex3") ||
//                !filepaths.Contains("ex4")) return;

            _dataProvider.Clear();
            var colors = new Dictionary<string, Color>();
            colors["ex1"] = Colors.DarkRed;
            colors["ex2"] = Colors.Navy;
            colors["ex3"] = Colors.SkyBlue;
            colors["ex4"] = Colors.Orange;

            foreach (string filepath in filepaths)
            {
                FlightLog flightlog = FlightLogXMLFile.Read(filepath);
                string filename = Path.GetFileNameWithoutExtension(filepath);
                double thickness = (filename == "ex1") ? 3 : 1.5;

                _dataProvider.AddToComparison(flightlog, colors[filename], filename, thickness);
            }
        }

        private void LoadFlightLog(string filepath)
        {
            _dataProvider.Set(FlightLogXMLFile.Read(filepath));
        }
    }
}