using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Swordfish.WPF.Charts.Extensions
{
    public static class ChartHelper
    {
        ///// <summary>
        ///// Convert a DataStream of values into a ChartData object which can be fed into the charts and displayed.
        ///// The default behaviour is to create a smoothed copy and add along with the original data.
        ///// The original data is colored Gray and the smoothed data Red by default.7
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <param name="streamName"></param>
        ///// <returns></returns>
        //public static ChartData ToChartData(DataVector stream, string streamName)
        //{
        //    if (stream == null)
        //        throw new ArgumentNullException();

        //    if (stream.Count == 0)
        //        throw new ArgumentException("Stream must contain at least one element");

        //    var data = stream;
        //    var smoothedData = Numerics.SmoothBySavitzkyGolay(data, 11);

        //    // We know there is at least one element, so Get(0) is safe!
        //    return new ChartData(
        //        stream.Get(0).Unit,
        //        CreateDataItem(data, streamName, Colors.Gray, 1),
        //        CreateDataItem(smoothedData, streamName, Colors.Red, 1));
        //}

        ///// <summary>
        ///// Loads a set of DataInfo elements into a ChartPrimitive, using the Value and Time.Ticks 
        ///// properties as values for each element.
        ///// </summary>
        ///// <param name="source"></param>
        ///// <param name="name"></param>
        ///// <param name="color"></param>
        ///// <param name="lineWidth"></param>
        ///// <returns></returns>
        //private static ChartPrimitive CreateDataItem(IEnumerable<DataInfo> source, String name, Color color, int lineWidth)
        //{
        //    var cp = new ChartPrimitive
        //    {
        //        Color = color,
        //        Dashed = false,
        //        Filled = false,
        //        Label = name,
        //        LineThickness = lineWidth,
        //        ShowInLegend = false
        //    };

        //    foreach (DataInfo info in source)
        //        cp.AddPoint(info.Time.TotalMilliseconds, info.Value);

        //    return cp;
        //}


        public static void RenderTextToChartPosition(DrawingContext drawingContext, CanvasTransformInfo canvasInfo,
                                                     string text, Color textColor, Point chartPosition,
                                                     Point pixelOffset, bool flipY, bool centreAligned)
        {
            var typeFace = new Typeface("Arial");

            // Render the marker label beneath or above the marker according 
            // to if the Y value is positive or negative
            Point textPos = ToPixel(canvasInfo, chartPosition);
            textPos.Offset(pixelOffset.X, pixelOffset.Y);
            
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeFace,
                10,
                new SolidColorBrush(textColor));

            if (centreAligned)
                formattedText.TextAlignment = TextAlignment.Center;


            // Adjust position and transformation for inverted Y axis.
            // Default behaviour is flipY == false, in case we need to
            // flip the font rendering transform. The drawing canvas
            // has inverted its Y-axis causing the text to render upside-down.
            int transformY = flipY ? 1 : -1;
            textPos.Y *= transformY;

            // Push flip transformation    
            drawingContext.PushTransform(new ScaleTransform(1, transformY));

            // Render text
            drawingContext.DrawText(formattedText, textPos);

            // Pop flip transformation
            drawingContext.Pop();
        }

        /// <summary>
        /// Translates a point from the chart dimension to the drawing canvas pixel dimension
        /// </summary>
        /// <param name="canvasInfo">Information about the canvas and its transformations</param>
        /// <param name="valuePoint">A point in the chart with real plot values</param>
        /// <returns>A point in the drawing canvas with pixel values</returns>
        public static Point ToPixel(CanvasTransformInfo canvasInfo, Point valuePoint)
        {
            Canvas canvas = canvasInfo.Canvas;
            Point scaledMaxPoint = canvasInfo.ScaledMaxPoint;
            Point scaledMinPoint = canvasInfo.ScaledMinPoint;

            double scaleX = 0.0;
            double scaleY = 0.0;

            var canvasSize = new Size(canvas.ActualWidth, canvas.ActualHeight);

            if (scaledMaxPoint.X != scaledMinPoint.X)
                scaleX = canvasSize.Width/(scaledMaxPoint.X - scaledMinPoint.X);

            if (scaledMaxPoint.Y != scaledMinPoint.Y)
                scaleY = canvasSize.Height/(scaledMaxPoint.Y - scaledMinPoint.Y);

            double xPos = (valuePoint.X - scaledMinPoint.X)*scaleX;
            double yPos = (valuePoint.Y - scaledMinPoint.Y)*scaleY;

            return new Point(xPos, yPos);
        }

//        public static void RenderTextToPixelPosition(string text, Point pixelPosition)
//        {
//            
//        }
    }
}