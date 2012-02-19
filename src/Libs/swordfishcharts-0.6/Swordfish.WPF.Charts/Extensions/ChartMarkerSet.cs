// Copyright © 2007 by Initial Force AS.  All rights reserved.
//
// Author: Andreas Larsen


using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Swordfish.WPF.Charts.Extensions
{
    /// <summary>
    /// The ChartMarkerSet is used to hold marker points associated with a parent ChartPrimitive.
    /// Any points added to the ChartMarkerSet will be rendered on top of the parent primitive,
    /// and the Y-position will snap to the primitive's value at X.
    /// </summary>
    public class ChartMarkerSet
    {
        /// <summary>
        /// Geometry to render
        /// </summary>
        private readonly GeometryGroup _geometry;

        /// <summary>
        /// Parent chart primitive to associate markers with.
        /// The markers will snap and be drawn on top of it.
        /// </summary>
        private readonly ChartPrimitive _parentPrimitive;

        /// <summary>
        /// The list of markers.
        /// </summary>
        private readonly IList<ChartPoint> _points;

        /// <summary>
        /// Color for this set of markers, with a default color.
        /// </summary>
        private Color _fill = Colors.Transparent;

        /// <summary>
        /// Color for stroke of markers.
        /// </summary>
        public Color Stroke = Colors.Blue;

        public double StrokeWidth;


        /// <summary>
        /// Initialize a set of markers.
        /// </summary>
        /// <param name="parentPrimitive"></param>
        public ChartMarkerSet(ChartPrimitive parentPrimitive)
        {
            _parentPrimitive = parentPrimitive;
            _geometry = new GeometryGroup();
            _points = new List<ChartPoint>();
        }


        /// <summary>
        /// Add a visual marker to the parent primitive.
        /// </summary>
        /// <param name="p"></param>
        public void AddPoint(ChartPoint p)
        {
            _points.Add(p);
        }

        /// <summary>
        /// Set color for all the markers in this set.
        /// </summary>
        public Color Fill
        {
            get { return _fill; } 
            set { _fill = value; }
        }

        /// <summary>
        /// Size of marker in pixels, typically the average diameter for the variuos marker types.
        /// </summary>
        public double Size { get; set; }

        /// <summary>
        /// The transformation to apply to the geometry when rendering.
        /// </summary>
        public Transform GeometryTransform { get; set; }

        /// <summary>
        /// The set of points associated with the parent primitive.
        /// </summary>
        public IEnumerable<ChartPoint> Points
        {
            get { return _points; }
        }


        /// <summary>
        /// The parent of a marker is typically a graph of type ChartPrimitive that the markers
        /// should be placed onto.
        /// </summary>
        public ChartPrimitive ParentPrimitive
        {
            get { return _parentPrimitive; }
        }

        ///// <summary>
        ///// Return a UIElement that represents the visual markers in this set,
        ///// that can be drawn onto the chart canvas.
        ///// </summary>
        ///// <returns></returns>
        //public UIElement GetUIElement()
        //{
        //    _geometry.Children.Clear();
        //    _geometry.Transform = GeometryTransform;

        //    foreach (ChartPoint p in Points)
        //        _geometry.Children.Add(new EllipseGeometry(p.Point, Size, Size));

        //    var myPath = new Path();
        //    myPath.Stroke = new SolidColorBrush(Stroke);
        //    myPath.StrokeThickness = 1;
        //    myPath.Fill = new SolidColorBrush(Fill);
        //    myPath.Data = _geometry;
        //    return myPath;
        //}
    }
}