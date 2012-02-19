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
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

#endregion

namespace Swordfish.WPF.Charts
{
    public class ChartPrimitive
    {
        #region Private Fields

        /// <summary>
        /// Geometry to render
        /// </summary>
        private readonly StreamGeometry geometry;

        /// <summary>
        /// Points that make up the plot
        /// </summary>
        private readonly List<Point> points;

        /// <summary>
        /// The color used to draw the primitive
        /// </summary>
        private Color color = Colors.Red;

        /// <summary>
        /// Flag indicating if the line is dashed or not
        /// </summary>
        private bool dashed;

        /// <summary>
        /// Flag indicating if this primitive is filled or not
        /// </summary>
        private bool filled;

        /// <summary>
        /// Flag indicating whether to hit test the primitive or not
        /// </summary>
        private bool hitTest = true;

        /// <summary>
        /// The label for the primitive
        /// </summary>
        private string label = "Unlabled";

        /// <summary>
        /// The line thickness
        /// </summary>
        private double lineThickness = 1;

        /// <summary>
        /// Flag indicating that the geometry needs to be updated
        /// </summary>
        private bool recalcGeometry = true;

        /// <summary>
        /// Flag indicating if this primitive is shown in the legend
        /// </summary>
        private bool showInLegend = true;


        #endregion Private Fields

        #region Methods

        /// <summary>
        /// Constructor. Initializes class fields.
        /// </summary>
        public ChartPrimitive()
        {
            geometry = new StreamGeometry();
            points = new List<Point>();
        }

        /// <summary>
        /// Copy constructor. Deep copies the ChartPrimitive passed in.
        /// </summary>
        /// <param name="chartPrimitive"></param>
        protected ChartPrimitive(ChartPrimitive chartPrimitive)
            : this()
        {
            if (chartPrimitive == null)
                return;

            color = chartPrimitive.Color;
            label = chartPrimitive.Label;
            filled = chartPrimitive.Filled;
            showInLegend = chartPrimitive.ShowInLegend;
            hitTest = chartPrimitive.HitTest;
            lineThickness = chartPrimitive.LineThickness;
            dashed = chartPrimitive.Dashed;
            Points = chartPrimitive.points;
            CalculateGeometry();
        }

        /// <summary>
        /// Does a deep clone of the current ChartPrimitive
        /// </summary>
        /// <returns></returns>
        public ChartPrimitive Clone()
        {
            return new ChartPrimitive(this);
        }

        /// <summary>
        /// Adds a point to the end
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddPoint(double x, double y)
        {
            AddPoint(new Point(x, y));
        }

        /// <summary>
        /// Adds a point to the end
        /// </summary>
        /// <param name="point"></param>
        public void AddPoint(Point point)
        {
            points.Add(point);
            recalcGeometry = true;
        }

        /// <summary>
        /// Inserts a point
        /// </summary>
        /// <param name="point"></param>
        /// <param name="index"></param>
        public void InsertPoint(Point point, int index)
        {
            points.Insert(index, point);
            recalcGeometry = true;
        }

        /// <summary>
        /// Adds a bezier curve point where it gives a little plateau at the point
        /// </summary>
        /// <param name="point"></param>
        public void AddSmoothHorizontalBar(Point point)
        {
            if (points.Count > 0)
            {
                Point lastPoint = points[points.Count - 1];
                double xDiff = (point.X - lastPoint.X)*.5;

                var controlPoint1 = new Point(lastPoint.X + xDiff, lastPoint.Y);
                var controlPoint2 = new Point(point.X - xDiff, point.Y);
                points.Add(controlPoint1);
                points.Add(controlPoint2);
                points.Add(point);
            }
            else
            {
                AddPoint(point);
            }
        }

        /// <summary>
        /// Does a one off calculation of the geometry to be rendered
        /// </summary>
        protected void CalculateGeometry()
        {
            if (recalcGeometry)
            {
                bool firstPass = true;
                geometry.Clear();
                using (StreamGeometryContext ctx = geometry.Open())
                {
                    foreach (Point point in points)
                    {
                        if (!firstPass)
                        {
                            ctx.LineTo(point, !filled, true);
                        }
                        else
                        {
                            ctx.BeginFigure(point, filled, filled);
                        }
                        firstPass = false;
                    }
                    recalcGeometry = false;
                }
            }
        }

        /// <summary>
        /// Gets the UIElement that can be added to the plot
        /// </summary>
        /// <returns></returns>
        public UIElement GetUIElement()
        {
            var path = new Path();
            path.Data = geometry;
            path.StrokeLineJoin = PenLineJoin.Bevel;

            if (filled)
            {
                path.Stroke = null;
                path.Fill = dashed
                                ? ChartUtilities.CreateHatch50(color, new Size(2, 2))
                                : (Brush) (new SolidColorBrush(color));
            }
            else
            {
                path.Stroke = new SolidColorBrush(color);
                path.StrokeThickness = lineThickness;
                path.Fill = null;
                if (dashed)
                    path.StrokeDashArray = new DoubleCollection(new double[] {2, 2});
            }
            return path;
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets a list of all the points in this primitive
        /// </summary>
        public IList<Point> Points
        {
            set
            {
                if (value != points)
                {
                    points.Clear();
                    if (value != null)
                        points.AddRange(value);
                }
            }
            get { return points; }
        }

        /// <summary>
        /// Gets the minium x,y values in the point collection
        /// </summary>
        public Point MinPoint
        {
            get
            {
                CalculateGeometry();
                if (geometry.Transform.Inverse != null)
                    return geometry.Transform.Inverse.TransformBounds(geometry.Bounds).TopLeft;
                else
                    return geometry.Bounds.TopLeft;
            }
        }

        /// <summary>
        /// Gets the maximum x,y values in the point collection
        /// </summary>
        public Point MaxPoint
        {
            get
            {
                CalculateGeometry();
                if (geometry.Transform.Inverse != null)
                    return geometry.Transform.Inverse.TransformBounds(geometry.Bounds).BottomRight;
                else
                    return geometry.Bounds.BottomRight;
            }
        }

        /// <summary>
        /// Gets/Sets the color of the primitive
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// Gets/Sets the line label
        /// </summary>
        public string Label
        {
            get { return label; }
            set { label = value; }
        }

        /// <summary>
        /// Gets/Sets if the shape is filled or not
        /// </summary>
        public bool Filled
        {
            get { return filled; }
            set { filled = value; }
        }

        /// <summary>
        /// Gets/Sets if the the line is dashed or not
        /// </summary>
        public bool Dashed
        {
            get { return dashed; }
            set { dashed = value; }
        }

        /// <summary>
        /// Gets/Sets if the primitve should be shown in the plot legend.
        /// If this is true the points are tested for when the cursor
        /// is near them so that the cursor can show their value.
        /// </summary>
        public bool ShowInLegend
        {
            get { return showInLegend; }
            set { showInLegend = value; }
        }

        /// <summary>
        /// Gets/Sets if the line is to be hit tested or not
        /// </summary>
        public bool HitTest
        {
            get { return hitTest; }
            set { hitTest = value; }
        }


        /// <summary>
        /// Gets/Sets the line thickness
        /// </summary>
        public double LineThickness
        {
            get { return lineThickness; }
            set { lineThickness = value; }
        }

        /// <summary>
        /// Gets sets the transform for the Geometry
        /// </summary>
        public Transform GeometryTransform
        {
            get { return geometry.Transform; }
            set { geometry.Transform = value; }
        }

        #endregion Properties

        // ********************************************************************
        // Private Fields
        // ********************************************************************

        // ********************************************************************
        // Methods
        // ********************************************************************

        // ********************************************************************
        // Properties
        // ********************************************************************


        /// <summary>
        /// Find value Y from value X in this primitive.
        /// This method assumes the points' X-values are in a non-descending order.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double GetValueY(double x)
        {
            if (Points == null || Points.Count == 0)
                throw new Exception("No points exist in primitive. Can't find value Y from value X.");

            Point previous = Points[0];
            foreach (Point p in Points)
            {
                if (previous.X <= x && p.X >= x)
                    return p.Y;

                previous = p;
            }

            throw new Exception("Could not find any points in this primitive crossing the Y-axis at [X=" + x + "]");
        }
    }
}