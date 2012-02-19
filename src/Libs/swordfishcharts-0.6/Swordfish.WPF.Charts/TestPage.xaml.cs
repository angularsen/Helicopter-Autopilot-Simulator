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

using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;

#endregion

namespace Swordfish.WPF.Charts
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class TestPage
    {
        public TestPage()
        {
            InitializeComponent();
            ChartUtilities.AddTestLines(xyLineChart);
            xyLineChart.SubNotes = new string[]
                                       {"Right Mouse Button To Zoom, Left Mouse Button To Pan, Double-Click To Reset"};
            copyToClipboard.CopyToClipboardDelegate = CopyChartToClipboard;
            copyToClipboard.SaveToFileDelegate = SaveToFile;
            //xyLineChart.FlipYAxis = true;
            //xyLineChart.PlotAreaOverride = new Rect(new Point(0, -2), new Point(5, 2));
        }

        public XYLineChart XYLineChart
        {
            get { return xyLineChart; }
        }

        /// <summary>
        /// Copies the chart to the clipboard
        /// </summary>
        /// <param name="element"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected void CopyChartToClipboard(FrameworkElement element, double width, double height)
        {
            try
            {
                ChartUtilities.CopyChartToClipboard(plotToCopy, xyLineChart, width, height);
            }
            catch
            {
                // the above might throw a security exception is used in an xbap
                Trace.WriteLine("Error copying to clipboard");
            }
        }

        protected void SaveToFile(FrameworkElement element, double width, double height)
        {
            var saveFile = new SaveFileDialog();
            saveFile.Filter = "png files (*.png)|*.png";
            saveFile.AddExtension = true;
            saveFile.OverwritePrompt = true;
            saveFile.Title = "Save Chart To a File";
            saveFile.ValidateNames = true;

            if (saveFile.ShowDialog() == true)
            {
                ChartUtilities.CopyFrameworkElemenToPNGFile(plotToCopy, width, height, saveFile.FileName);
            }
        }
    }
}