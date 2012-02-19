/* 
 * Environment Simulator
 * Copyright (C) 2008-2009 Justin Stoecker
 * 
 * This program is free software; you can redistribute it and/or modify it under the terms of the 
 * GNU General Public License as published by the Free Software Foundation; either version 2 of 
 * the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sim.Utility
{
    static class Draw
    {
        public static void PointList(GraphicsDevice g, VertexPositionColor[] points)
        {
            BasicEffect e = Shaders.Primitive;

            e.Begin();
            e.CurrentTechnique.Passes[0].Begin();
            g.VertexDeclaration = new VertexDeclaration(g, VertexPositionColor.VertexElements);
            g.DrawUserPrimitives(PrimitiveType.PointList, points, 0, points.Length);
            e.CurrentTechnique.Passes[0].End();
            e.End();
        }

        public static void LineList(GraphicsDevice g, VertexPositionColor[] lines)
        {
            BasicEffect e = Shaders.Primitive;

            e.Begin();
            e.CurrentTechnique.Passes[0].Begin();
            g.VertexDeclaration = new VertexDeclaration(g, VertexPositionColor.VertexElements);
            g.DrawUserPrimitives(PrimitiveType.LineList, lines, 0, lines.Length / 2);
            e.CurrentTechnique.Passes[0].End();
            e.End();
        }

        public static void SurfaceNormal(GraphicsDevice g, Vector3 position, Sim.Environment.Terrain t)
        {
            BasicEffect e = Shaders.Primitive;

            g.VertexDeclaration = new VertexDeclaration(g, VertexPositionColor.VertexElements);
            VertexPositionColor[] nVerts = new VertexPositionColor[] { 
                new VertexPositionColor(position, Color.Red),
                new VertexPositionColor(position + t.CalculateSurfaceNormal(position.X,position.Z)*20,Color.Yellow)};
            
            e.Begin();
            e.CurrentTechnique.Passes[0].Begin();
            g.DrawUserPrimitives(PrimitiveType.LineList, nVerts, 0, 1);
            e.CurrentTechnique.Passes[0].End();
            e.End();
        }

        public static void BoundingBox(GraphicsDevice g, BoundingBox b)
        {
            BasicEffect e = Shaders.Primitive;

            VertexPositionColor[] vertices = new VertexPositionColor[8];
            Vector3[] corners = b.GetCorners();
            for (int i = 0; i < 8; i++)
            {
                vertices[i].Position = corners[i];
                vertices[i].Color = Color.Red;
            }

            short[] indices = new short[] { 0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7 };

            e.Begin();
            e.CurrentTechnique.Passes[0].Begin();
            g.VertexDeclaration = new VertexDeclaration(g, VertexPositionColor.VertexElements);
            g.DrawUserIndexedPrimitives(PrimitiveType.LineList, vertices, 0, 8, indices, 0, 12);
            e.CurrentTechnique.Passes[0].End();
            e.End();
        }

        public static void Axes(GraphicsDevice g, int scale)
        {
            BasicEffect e = Shaders.Primitive;

            VertexPositionColor[] axes = new VertexPositionColor[] {
                new VertexPositionColor(Vector3.Zero, Color.Red),
                new VertexPositionColor(Vector3.UnitX * scale, Color.Red),
                new VertexPositionColor(Vector3.Zero, Color.Green),
                new VertexPositionColor(Vector3.UnitY * scale, Color.Green),
                new VertexPositionColor(Vector3.Zero, Color.Blue),
                new VertexPositionColor(Vector3.UnitZ * scale, Color.Blue)
            };

            e.Begin();
            e.CurrentTechnique.Passes[0].Begin();
            g.VertexDeclaration = new VertexDeclaration(g, VertexPositionColor.VertexElements);
            g.DrawUserPrimitives(PrimitiveType.LineList, axes, 0, 3);
            e.CurrentTechnique.Passes[0].End();
            e.End();
        }
    }
}
