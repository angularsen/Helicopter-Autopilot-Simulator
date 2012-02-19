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
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Swordfish.WPF.Charts.Extensions;

#endregion

namespace Swordfish.WPF.Charts
{
    public partial class XYLineChart
    {
        #region Private Fields

        // user data

        public static readonly string BSDBit = "Swordfish Charts, Copyright (c) 2007, John Stewien";
        private readonly AdornerCursorCoordinateDrawer adorner;
        private readonly PathGeometry chartClip;

        // Helper classes

        private readonly ClosestPointPicker closestPointPicker;

        // Appearance

        private readonly Color gridLineColor = Colors.Silver;
        private readonly Color gridLineLabelColor = Colors.Black;
        private readonly PanZoomCalculator panZoomCalculator;

        /// <summary>
        /// A list of lines to draw on the chart
        /// </summary>
        private readonly List<ChartPrimitive> primitiveList;

        private readonly MatrixTransform shapeTransform;

        private AdornerLayer adornerLayer;

        /// <summary>
        /// Flag that when set flips the Y axis such that zero is in the
        /// top left and positive is down.
        /// </summary>
        private bool flipYAxis;

        /// <summary>
        /// A list of markers to draw on the chart
        /// </summary>
        private readonly List<ChartMarkerSet> markerList;

        /// <summary>
        /// A list of intervals to draw on the chart
        /// </summary>
        private readonly IList<Interval> intervalList;
        private Point optimalGridLineSpacing;

        /// <summary>
        /// Overide the calculated plot area if this isn't null;
        /// </summary>
        private Rect? plotAreaOverride;

        private Point scaledMaxPoint;
        private Point scaledMinPoint;

        private readonly AdornerChartMarkers adornerMarkers;
        // Keep this line for you BSD License obligations


        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Constructor. Initializes all the class fields.
        /// </summary>
        public XYLineChart()
        {
            // This assumes that you are navigating to this scene.
            // If you will normally instantiate it via code and display it
            // manually, you either have to call InitializeComponent by hand or
            // uncomment the following line.

            InitializeComponent();

            TextCanvasInfo = new CanvasTransformInfo();

            primitiveList = new List<ChartPrimitive>();
            markerList = new List<ChartMarkerSet>();
            intervalList = new List<Interval>();
            // Set the Chart Geometry Clip region
            chartClip = new PathGeometry();
            chartClip.AddGeometry(
                new RectangleGeometry(new Rect(0, 0, clippedPlotCanvas.ActualWidth, clippedPlotCanvas.ActualHeight)));
            shapeTransform = new MatrixTransform();
            adornerMarkers = new AdornerChartMarkers(clippedPlotCanvas, shapeTransform, MarkerSets, this);
            adorner = new AdornerCursorCoordinateDrawer(clippedPlotCanvas, shapeTransform);

            optimalGridLineSpacing = new Point(100, 75);

            panZoomCalculator =
                new PanZoomCalculator(new Rect(0, 0, clippedPlotCanvas.ActualWidth, clippedPlotCanvas.ActualHeight));
            panZoomCalculator.Window = new Rect(0, 0, clippedPlotCanvas.ActualWidth, clippedPlotCanvas.ActualHeight);
            panZoomCalculator.PanZoomChanged += panZoomCalculator_PanZoomChanged;

            closestPointPicker = new ClosestPointPicker(new Size(13, 13));
            closestPointPicker.ClosestPointChanged += closestPointPicker_ClosestPointChanged;

            Cursor = Cursors.None;

            TextCanvasInfo.Canvas = textCanvas;

//            UniformScaling = true;

            // Set up all the message handlers for the clipped plot canvas
            AttachEventsToCanvas(this);

            //clippedPlotCanvas.Clip = chartClip;
            clippedPlotCanvas.IsVisibleChanged += clippedPlotCanvas_IsVisibleChanged;
            clippedPlotCanvas.SizeChanged += clippedPlotCanvas_SizeChanged;
        }

        /// <summary>
        /// Attaches mouse handling events to the canvas passed in. The canvas passed in should be the top level canvas.
        /// </summary>
        /// <param name="eventCanvas"></param>
        protected void AttachEventsToCanvas(UIElement eventCanvas)
        {
            eventCanvas.LostMouseCapture += clippedPlotCanvas_LostMouseCapture;
            eventCanvas.MouseMove += clippedPlotCanvas_MouseMove;
            eventCanvas.MouseLeftButtonDown += clippedPlotCanvas_MouseLeftButtonDown;
            eventCanvas.MouseLeftButtonUp += clippedPlotCanvas_MouseLeftButtonUp;
            eventCanvas.MouseRightButtonDown += clippedPlotCanvas_MouseRightButtonDown;
            eventCanvas.MouseRightButtonUp += clippedPlotCanvas_MouseRightButtonUp;
            eventCanvas.MouseEnter += clippedPlotCanvas_MouseEnter;
            eventCanvas.MouseLeave += clippedPlotCanvas_MouseLeave;
        }

        /// <summary>
        /// Reset everything from the current collection of Chart Primitives
        /// </summary>
        public void RedrawPlotLines()
        {
            closestPointPicker.Points.Clear();
            legendBox.Children.Clear();
            foreach (ChartPrimitive primitive in primitiveList)
            {
                if (primitive.ShowInLegend)
                {
                    legendBox.Children.Insert(0, new ColorLabel(primitive.Label, primitive.Color));
                }
                if (primitive.HitTest)
                {
                    closestPointPicker.Points.AddRange(primitive.Points);
                }
            }
            InvalidateMeasure();
            SetChartTransform(clippedPlotCanvas.ActualWidth, clippedPlotCanvas.ActualHeight);
            clippedPlotCanvas.Children.Clear();
            foreach (ChartPrimitive primitive in primitiveList)
            {
                primitive.GeometryTransform = shapeTransform;
                clippedPlotCanvas.Children.Add(primitive.GetUIElement());
            }
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Resize the plot by changing the transform and drawing the grid lines
        /// </summary>
        protected void ResizePlot()
        {
            SetChartTransform(clippedPlotCanvas.ActualWidth, clippedPlotCanvas.ActualHeight);
            // Don't need to re-render the plot lines, just change the transform

            // Still need to redraw the grid lines
            InvalidateMeasure();
            // Grid lines are now added in the measure override to stop flickering
        }

        /// <summary>
        /// Set the chart transform
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected void SetChartTransform(double width, double height)
        {
            // Note This is required even if we are using plotAreaOverride. 
            // Calling this method causes some necessary invalidation of the contents that redraws the graphs later.
            // Otherwise, when using plotAreaOverride, the graphs are not redrawn.
            Rect plotArea = ChartUtilities.GetPlotRectangle(primitiveList, 0.1f);

            if (plotAreaOverride.HasValue)
                plotArea = plotAreaOverride.Value;


            // Enforce uniform scaling
            if (UniformScaling)
            {
                // TODO Improve this method
                plotArea.Width = plotArea.Height = 
                    Math.Max(plotArea.Width, plotArea.Height);
            }

            scaledMinPoint = plotArea.Location;
            scaledMinPoint.Offset(-plotArea.Width*panZoomCalculator.Pan.X, plotArea.Height*panZoomCalculator.Pan.Y);
            scaledMinPoint.Offset(0.5*plotArea.Width*(1 - 1/panZoomCalculator.Zoom.X),
                                  0.5*plotArea.Height*(1 - 1/panZoomCalculator.Zoom.Y));

            scaledMaxPoint = scaledMinPoint;
            scaledMaxPoint.Offset(plotArea.Width/panZoomCalculator.Zoom.X, plotArea.Height/panZoomCalculator.Zoom.Y);

            var plotScale = new Point();
            plotScale.X = (width/plotArea.Width)*panZoomCalculator.Zoom.X;
            plotScale.Y = (height/plotArea.Height)*panZoomCalculator.Zoom.Y;

            Matrix shapeMatrix = Matrix.Identity;
            shapeMatrix.Translate(-scaledMinPoint.X, -scaledMinPoint.Y);
            shapeMatrix.Scale(plotScale.X, plotScale.Y);
            shapeTransform.Matrix = shapeMatrix;

            TextCanvasInfo.ScaledMaxPoint = scaledMaxPoint;
            TextCanvasInfo.ScaledMinPoint = scaledMinPoint;
        }

        /// <summary>
        /// Add grid lines here
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size constraint)
        {
            DrawGridlinesAndLabels(new Size(textCanvas.ActualWidth, textCanvas.ActualHeight), scaledMinPoint,
                                   scaledMaxPoint);
            return base.MeasureOverride(constraint);
        }

        /// <summary>
        /// Draw all the gridlines and labels for the gridlines
        /// </summary>
        /// <param name="size"></param>
        /// <param name="minXY"></param>
        /// <param name="maxXY"></param>
        protected void DrawGridlinesAndLabels(Size size, Point minXY, Point maxXY)
        {
            // Clear the text canvas
            textCanvas.Children.Clear();

            // Create brush for writing text
            Brush axisBrush = new SolidColorBrush(gridLineColor);
            Brush axisScaleBrush = new SolidColorBrush(gridLineLabelColor);

            // Need to pick appropriate scale increment.
            // Go for a 2Exx, 5Exx, or 1Exx type scale
            double scaleX = 0.0;
            double scaleY = 0.0;

            // Work out all the limits

            if (maxXY.X != minXY.X)
                scaleX = size.Width/(maxXY.X - minXY.X);
            if (maxXY.Y != minXY.Y)
                scaleY = size.Height/(maxXY.Y - minXY.Y);
            double optimalSpacingX = optimalGridLineSpacing.X/scaleX;

            double spacingX = ChartUtilities.ClosestValueInListTimesBaseToInteger(optimalSpacingX,
                                                                                  new double[] {1, 3, 6}, 12.0);

            if (spacingX < 2.0)
                spacingX = ChartUtilities.Closest_1_2_5_Pow10(optimalSpacingX);

            double optimalSpacingY = optimalGridLineSpacing.Y/scaleY;
            double spacingY = ChartUtilities.Closest_1_2_5_Pow10(optimalSpacingY);

            var startXmult = (int) Math.Ceiling(minXY.X/spacingX);
            var endXmult = (int) Math.Floor(maxXY.X/spacingX);
            var startYmult = (int) Math.Ceiling(minXY.Y/spacingY);
            var endYmult = (int) Math.Floor(maxXY.Y/spacingY);

            double maxXLabelHeight = 0;

            var pathFigure = new PathFigure();

            // Draw all the vertical gridlines

            for (int lineNo = startXmult; lineNo <= endXmult; ++lineNo)
            {
                double xValue = lineNo*spacingX;
                double xPos = (xValue - minXY.X)*scaleX;

                var startPoint = new Point(xPos, size.Height);
                var endPoint = new Point(xPos, 0);

                pathFigure.Segments.Add(new LineSegment(startPoint, false));
                pathFigure.Segments.Add(new LineSegment(endPoint, true));

                var text = new TextBlock();
                text.Text = xValue.ToString();
                text.Foreground = axisScaleBrush;
                text.Measure(size);

                text.SetValue(Canvas.LeftProperty, xPos - text.DesiredSize.Width*.5);
                text.SetValue(Canvas.TopProperty, size.Height + 1);
                textCanvas.Children.Add(text);
                maxXLabelHeight = Math.Max(maxXLabelHeight, text.DesiredSize.Height);
            }

            xGridlineLabels.Height = maxXLabelHeight + 2;

            // Set string format for vertical text
            double maxYLabelHeight = 0;

            // Draw all the horizontal gridlines

            for (int lineNo = startYmult; lineNo <= endYmult; ++lineNo)
            {
                double yValue = lineNo*spacingY;
                double yPos = flipYAxis
                                  ?
                                      (yValue - maxXY.Y)*scaleY + size.Height
                                  :
                                      (-yValue + minXY.Y)*scaleY + size.Height;

                var startPoint = new Point(0, yPos);
                var endPoint = new Point(size.Width, yPos);

                pathFigure.Segments.Add(new LineSegment(startPoint, false));
                pathFigure.Segments.Add(new LineSegment(endPoint, true));

                var text = new TextBlock();
                text.Text = yValue.ToString();
                text.LayoutTransform = new RotateTransform(-90);
                text.Measure(size);

                text.SetValue(Canvas.LeftProperty, -text.DesiredSize.Width - 1);
                text.SetValue(Canvas.TopProperty, yPos - text.DesiredSize.Height*.5);
                textCanvas.Children.Add(text);
                maxYLabelHeight = Math.Max(maxYLabelHeight, text.DesiredSize.Width);
            }
            yGridLineLabels.Height = maxYLabelHeight + 2;

            var path = new Path();
            path.Stroke = axisBrush;
            var pathGeometry = new PathGeometry(new[] {pathFigure});

            pathGeometry.Transform = (Transform) textCanvas.RenderTransform.Inverse;
            path.Data = pathGeometry;

            textCanvas.Children.Add(path);
        }

        #endregion Protected Methods

        #region Properties

        public bool UniformScaling { get; set; }

        /// <summary>
        /// Gets/Sets the title of the chart
        /// </summary>
        public string Title
        {
            get { return titleBox.Text; }
            set { titleBox.Text = value; }
        }

        /// <summary>
        /// Gets/Sets the X axis label
        /// </summary>
        public string XAxisLabel
        {
            get { return xAxisLabel.Text; }
            set { xAxisLabel.Text = value; }
        }

        /// <summary>
        /// Gets/Sets the Y axis label
        /// </summary>
        public string YAxisLabel
        {
            get { return yAxisLabel.Text; }
            set { yAxisLabel.Text = value; }
        }

        /// <summary>
        /// Gets/Sets the subnotes to be printed at the bottom of the chart
        /// </summary>
        public IEnumerable SubNotes
        {
            get { return subNotes.ItemsSource; }
            set { subNotes.ItemsSource = value; }
        }

        /// <summary>
        /// Gets the collection of chart primitives
        /// </summary>
        public List<ChartPrimitive> Primitives
        {
            get { return primitiveList; }
        }

        /// <summary>
        /// Gets/Sets whether to flip the Y axis or not
        /// </summary>
        public bool FlipYAxis
        {
            get { return flipYAxis; }
            set
            {
                if (flipYAxis != value)
                {
                    flipYAxis = value;

                    clippedPlotCanvas.RenderTransform = flipYAxis
                                                            ?
                                                                new ScaleTransform(1, 1)
                                                            :
                                                                new ScaleTransform(1, -1);

                    panZoomCalculator.FlipYAxis = value;
                    adorner.FlipYAxis = value;
                }
            }
        }

        #endregion Properties

        #region Misc Event Handlers

        /// <summary>
        /// Overrides the measured plot area
        /// </summary>
        public Rect? PlotAreaOverride
        {
            get { return plotAreaOverride; }
            set
            {
                if (value != plotAreaOverride)
                {
                    plotAreaOverride = value;
                    RedrawPlotLines();
                }
            }
        }

        /// <summary>
        /// Handles when the closest point to the mouse cursor changes. Hides
        /// or shows the closest point, and changes the mouse cursor accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closestPointPicker_ClosestPointChanged(object sender, ClosestPointArgs e)
        {
            adorner.Locked = closestPointPicker.Locked;
            adorner.LockPoint = closestPointPicker.ClosestPoint;
        }

        /// <summary>
        /// Handles when the pan or zoom changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panZoomCalculator_PanZoomChanged(object sender, PanZoomArgs e)
        {
            ResizePlot();
			adornerMarkers.InvalidateVisual();
        }

        #endregion Misc Event Handlers

        #region clippedPlotCanvas Event Handlers

        /// <summary>
        /// Adds an adorner when the plot canvas is visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clippedPlotCanvas_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible && adornerLayer == null)
            {
                adornerLayer = AdornerLayer.GetAdornerLayer(clippedPlotCanvas);
				adornerLayer.Add(adornerMarkers);
                adornerLayer.Add(adorner);
                adorner.Visibility = IsMouseOver ? Visibility.Visible : Visibility.Hidden;
            }
            else if (adornerLayer != null)
            {
				adornerLayer.Remove(adornerMarkers);
                adornerLayer.Remove(adorner);
                adornerLayer = null;
            }
        }

        /// <summary>
        /// Handles when the plot size changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clippedPlotCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            chartClip.Clear();
            chartClip.AddGeometry(
                new RectangleGeometry(new Rect(0, 0, clippedPlotCanvas.ActualWidth, clippedPlotCanvas.ActualHeight)));
            panZoomCalculator.Window = new Rect(0, 0, clippedPlotCanvas.ActualWidth, clippedPlotCanvas.ActualHeight);
            ResizePlot();
        }

        /// <summary>
        /// Handles when the plot loses focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clippedPlotCanvas_LostMouseCapture(object sender, MouseEventArgs e)
        {
            panZoomCalculator.StopPanning();
            panZoomCalculator.StopZooming();
            Cursor = Cursors.None;
        }

        /// <summary>
        /// Handles when the mouse moves over the plot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clippedPlotCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(clippedPlotCanvas);

            if (!panZoomCalculator.IsPanning && !panZoomCalculator.IsZooming)
            {
                adorner.MousePoint = mousePos;
                closestPointPicker.MouseMoved(mousePos, shapeTransform.Inverse);
            }
            else
            {
                panZoomCalculator.MouseMoved(clippedPlotCanvas.RenderTransform.Inverse.Transform(mousePos));
            }
        }

        /// <summary>
        /// Handles when the user clicks on the plot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clippedPlotCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
            {
                if (((UIElement) sender).CaptureMouse())
                {
                    Cursor = Cursors.ScrollAll;
                    adorner.Visibility = Visibility.Hidden;
                    panZoomCalculator.StartPan(
                        clippedPlotCanvas.RenderTransform.Inverse.Transform(e.GetPosition(clippedPlotCanvas)));
                }
            }
            else
            {
                Mouse.Capture(null);
                panZoomCalculator.Reset();
            }
        }

        /// <summary>
        /// Handles when the user releases the mouse button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clippedPlotCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            panZoomCalculator.StopPanning();
            if (!panZoomCalculator.IsZooming)
            {
                Mouse.Capture(null);
                Cursor = Cursors.None;
                if (IsMouseOver)
                {
                    adorner.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        ///  Handles when the user clicks on the plot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clippedPlotCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
            {
                if (((UIElement) sender).CaptureMouse())
                {
                    Cursor = Cursors.ScrollAll;
                    adorner.Visibility = Visibility.Visible;
                    panZoomCalculator.StartZoom(
                        clippedPlotCanvas.RenderTransform.Inverse.Transform(e.GetPosition(clippedPlotCanvas)));
                }
            }
        }

        /// <summary>
        /// Handles when the user releases the mouse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clippedPlotCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            panZoomCalculator.StopZooming();
            if (!panZoomCalculator.IsPanning)
            {
                Mouse.Capture(null);
                Cursor = Cursors.None;
            }
            else
            {
                adorner.Visibility = Visibility.Hidden;
                Cursor = Cursors.ScrollAll;
            }
        }

        /// <summary>
        /// Handles when the mouse leaves the plot. Removes the nearest point indicator.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clippedPlotCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            adorner.Locked = false;
            adorner.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Handles when the mouse enters the plot. Puts back the nearest point indicator.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clippedPlotCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            adorner.Visibility = Visibility.Visible;
            adorner.Locked = closestPointPicker.Locked;
        }

        #endregion clippedPlotCanvas Event Handlers

        // ********************************************************************
        // Private Fields
        // ********************************************************************

        // ********************************************************************
        // Public Methods
        // ********************************************************************

        // ********************************************************************
        // Protected Methods
        // ********************************************************************

        // ********************************************************************
        // Properties
        // ********************************************************************

        // ********************************************************************
        // Misc Event Handlers
        // ********************************************************************

        // ********************************************************************
        // clippedPlotCanvas Event Handlers
        // ********************************************************************

        public IList<ChartMarkerSet> MarkerSets
        {
            get
            {
                return markerList;
            }
        }

        /// <summary>
        /// Translates a point from the chart dimension to the default drawing canvas pixel dimension.
        /// The default canvas is the text canvas.
        /// </summary>
        /// <param name="valuePoint"></param>
        /// <returns></returns>
        public Point ToPixel(Point valuePoint)
        {
            return ChartHelper.ToPixel(TextCanvasInfo, valuePoint);
        }


        public double ToPixelX(double valueX)
        {
            return ToPixel(new Point(valueX, 0)).X;
        }

        public double ToPixelY(double valueY)
        {
            return ToPixel(new Point(valueY, 0)).Y;
        }

        /// <summary>
        /// Holds information about the text canvas
        /// </summary>
        public CanvasTransformInfo TextCanvasInfo
        {
            get;
            set;
        }

        public void Clear()
        {
//            m_Data = null;
            MarkerSets.Clear();
//            Intervals.Clear();
        }

        public Point GetMinPoint()
        {
            var minPoint = new Point(double.MaxValue, double.MaxValue);
            foreach (var p in Primitives)
            {
                minPoint.X = Math.Min(minPoint.X, p.MinPoint.X);
                minPoint.Y = Math.Min(minPoint.Y, p.MinPoint.Y);
            }

            foreach (ChartMarkerSet marker in MarkerSets)
            {
                foreach (ChartPoint point in marker.Points)
                {
                    minPoint.X = Math.Min(minPoint.X, point.Point.X);
                    minPoint.Y = Math.Min(minPoint.Y, point.Point.Y);
                }   
            }

            return minPoint;
        }

        public Point GetMaxPoint()
        {
            var maxPoint = new Point(double.MinValue, double.MinValue);
            foreach (var p in Primitives)
            {
                maxPoint.X = Math.Max(maxPoint.X, p.MaxPoint.X);
                maxPoint.Y = Math.Max(maxPoint.Y, p.MaxPoint.Y);
            }

            foreach (ChartMarkerSet marker in MarkerSets)
            {
                foreach (ChartPoint point in marker.Points)
                {
                    maxPoint.X = Math.Max(maxPoint.X, point.Point.X);
                    maxPoint.Y = Math.Max(maxPoint.Y, point.Point.Y);
                }
            }

            return maxPoint;
        }
    }
}