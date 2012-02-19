using System;
using Microsoft.Xna.Framework;

namespace Control.Common
{
    public class NavigationMap
    {
        public readonly float[,] HeightValues;
        private readonly int _rows;
        private readonly int _cols;

        public NavigationMap(float[,] heightValues)
        {
            HeightValues = heightValues;
            _rows = HeightValues.GetLength(0);
            _cols = HeightValues.GetLength(1);
        }

        /// <summary>
        /// X - east/west (or columns in heightmap)
        /// Y - north/south (or rows in heightmap)
        /// </summary>
        /// <param name="mapPosition"></param>
        /// <returns></returns>
        public float GetAltitude(Vector2 mapPosition)
        {
            // Vector2.Y is inverted so that positive points towards north. This conflicts with our 
            // heightmap terrain that does not invert in the same way.

            float y = MathHelper.Clamp(-mapPosition.Y, 0, _rows - 1);
            float x = MathHelper.Clamp(mapPosition.X, 0, _cols - 1);

            int y1 = (int) Math.Floor(y);
            int x1 = (int) Math.Floor(x);
            int y2 = (int) Math.Ceiling(y);
            int x2 = (int) Math.Ceiling(x);

            // Bilinear interpolation to find height between heightmap points
            float Q11 = HeightValues[y1, x1];
            float Q21 = HeightValues[y1, x2];
            float Q12 = HeightValues[y2, x1];
            float Q22 = HeightValues[y2, x2];

            // Ref: http://en.wikipedia.org/wiki/Bilinear_interpolation
            float leftWeight = (x2 == x1) ? 0.5f : (x2 - x)/(x2 - x1);
            float rightWeight = 1 - leftWeight;//(x2 == x1) ? 0.5f : (x - x1) / (x2 - x1);
            float bottomWeight = (y2 == y1) ? 0.5f : (y2 - y) / (y2 - y1);
            float topWeight = 1 - bottomWeight;// (y2 == y1) ? 0.5f : (y - y1) / (y2 - y1);

            float R1 = leftWeight*Q11 + rightWeight*Q21;
            float R2 = leftWeight*Q12 + rightWeight*Q22;
            float P = bottomWeight*R1 + topWeight*R2;

            return P;
        }
    }
}