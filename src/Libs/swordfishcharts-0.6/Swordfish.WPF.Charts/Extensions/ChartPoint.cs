using System.Windows;

namespace Swordfish.WPF.Charts.Extensions
{
    public class ChartPoint
    {
        public readonly string Label;
        public Point Point;

        public ChartPoint(string label, Point point)
        {
            Label = label;
            Point = point;
        }
    }
}