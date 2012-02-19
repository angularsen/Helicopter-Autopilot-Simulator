// Copyright © 2007 by Initial Force AS.  All rights reserved.
//
// Author: Andreas Larsen
 

using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Swordfish.WPF.Charts.Extensions
{
    public class AdornerChartMarkers : Adorner
    {
        private Transform _transform;
        private readonly IList<ChartMarkerSet> _markerSets = new List<ChartMarkerSet>();
        private bool _flipYAxis;
        private readonly UIElement _adornedElement;
        private XYLineChart _parentChart;

        public AdornerChartMarkers(UIElement adornedElement, Transform shapeTransform, IList<ChartMarkerSet> markerSets, XYLineChart parentChart) : base(adornedElement)
        {
            _adornedElement = adornedElement;
            _parentChart = parentChart;
            _markerSets = markerSets;
            _transform = shapeTransform;
        }

        public IList<ChartMarkerSet> MarkerSets
        {
            get { return _markerSets; }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // Introduce clipping
            drawingContext.PushClip(new RectangleGeometry(new Rect(_adornedElement.RenderSize)));

            foreach (ChartMarkerSet set in MarkerSets)
            {
//                ChartPrimitive parent = set.ParentPrimitive;
                var setBrush = new SolidColorBrush(set.Fill);
                var setPen = new Pen(new SolidColorBrush(set.Stroke), set.StrokeWidth);
                
                foreach (ChartPoint p in set.Points)
                {
                    // Snap the marker's Y-position onto the parent primitive
                    var snappedToParentPos = p.Point;
//                    snappedToParentPos.Y = parent.GetValueY(p.Point.X);

                    // Then render it to the adorner layer (which is on top of the plot canvas)
                    Point pixelPos = _parentChart.ToPixel(snappedToParentPos);
                    drawingContext.DrawEllipse(setBrush, setPen, pixelPos, set.Size/2, set.Size/2);

                    const double labelOffsetY = 10;
                    Point labelPixelOffset = new Point(0, +5);
                    if (snappedToParentPos.Y >= 0)
                        labelPixelOffset.Offset(0, labelOffsetY);
                    else
                        labelPixelOffset.Offset(0, -labelOffsetY);

//                    ChartHelper.RenderTextToChartPosition(drawingContext, _parentChart.TextCanvasInfo, p.Label, set.Color, snappedToParentPos, labelPixelOffset, FlipYAxis, true);
                }
            }

            // End clipping
            drawingContext.Pop();
        }

        public bool FlipYAxis
        {
            get { return _flipYAxis; }
            set 
            { 
                _flipYAxis = value; 
                InvalidateVisual();
            }
        }
    }
}
