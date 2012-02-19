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

namespace Sim.Graphics
{
    /// <summary>
    /// Custom vertex format structure for generic point sprites that contains position and size information
    /// </summary>
    public struct VertexPointSprite
    {
        public Vector3 Position;
        public float Size;

        public static int SizeInBytes = sizeof(float) * 4;

        public VertexPointSprite(Vector3 position, float size)
        {
            this.Position = position;
            this.Size = size;
        }

        public static VertexElement[] VertexElements = new VertexElement[]
            {
                new VertexElement(0,0, VertexElementFormat.Vector3, VertexElementMethod.Default,
                    VertexElementUsage.Position,0),
                new VertexElement(0,sizeof(float)*3,VertexElementFormat.Single, VertexElementMethod.Default,
                    VertexElementUsage.PointSize,0)
            };
    }

    /// <summary>
    /// Specialized point sprite that has a random number attached to it
    /// </summary>
    public struct VertexPointSpriteParticle
    {
        public Vector3 Position;
        public float Size;
        public Vector2 Rand;

        public static int SizeInBytes = sizeof(float) * 6;

        public VertexPointSpriteParticle(Vector3 position, float size, Vector2 rand)
        {
            this.Position = position;
            this.Size = size;
            this.Rand = rand;
        }

        public static VertexElement[] VertexElements = new VertexElement[]
            {
                new VertexElement(0,0, VertexElementFormat.Vector3, VertexElementMethod.Default,
                    VertexElementUsage.Position,0),
                new VertexElement(0,sizeof(float)*3,VertexElementFormat.Single, VertexElementMethod.Default,
                    VertexElementUsage.PointSize,0),
                new VertexElement(0,sizeof(float)*4,VertexElementFormat.Vector2, VertexElementMethod.Default,
                    VertexElementUsage.TextureCoordinate,0),
            };
    }

    /// <summary>
    /// Vertex used as part of a billboard: stores info for texturing, coloring, and lighting
    /// </summary>
    public struct VertexBillboard
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Color;
        public Vector2 TexCoords;
        public Vector2 Scale;

        public static int SizeInBytes = sizeof(float) * 13;

        public static VertexElement[] VertexElements = new VertexElement[]
            {
                new VertexElement(0,0,VertexElementFormat.Vector3, VertexElementMethod.Default,
                    VertexElementUsage.Position,0),
                new VertexElement(0,sizeof(float)*3,VertexElementFormat.Vector3, VertexElementMethod.Default,
                    VertexElementUsage.Normal,0),
                new VertexElement(0,sizeof(float)*6,VertexElementFormat.Vector3, VertexElementMethod.Default,
                    VertexElementUsage.Color,0),
                new VertexElement(0,sizeof(float)*9, VertexElementFormat.Vector2, VertexElementMethod.Default,
                    VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(0,sizeof(float)*11, VertexElementFormat.Vector2, VertexElementMethod.Default,
                    VertexElementUsage.TextureCoordinate, 1),
            };

        public VertexBillboard(Vector3 position, Vector3 normal, Vector3 color, Vector2 texCoords, Vector2 scale)
        {
            Position = position;
            Normal = normal;
            Color = color;
            TexCoords = texCoords;
            Scale = scale;
        }
    }

    /// <summary>
    /// Vertex that uses multi-texturing with 4 textures
    /// </summary>
    public struct VertexMultitextured
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector4 TextureWeights;

        public static int SizeInBytes = sizeof(float) * 12;

        public static VertexElement[] VertexElements = new VertexElement[]
            {
                new VertexElement(0,0,VertexElementFormat.Vector3,VertexElementMethod.Default,
                    VertexElementUsage.Position,0),
                new VertexElement(0,sizeof(float) * 3,VertexElementFormat.Vector3,VertexElementMethod.Default,
                    VertexElementUsage.Normal,0),
                new VertexElement(0,sizeof(float) * 6,VertexElementFormat.Vector4,VertexElementMethod.Default,
                    VertexElementUsage.TextureCoordinate,0),
                new VertexElement(0,sizeof(float) * 8,VertexElementFormat.Vector4,VertexElementMethod.Default,
                    VertexElementUsage.TextureCoordinate,1)
            };
    }

    /// <summary>
    /// Custom vertex format structure for a multi-textured vertex that includes 4 textures and their weights
    /// </summary>
    public struct VertexTerrain
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinates;
        public Vector4 TextureWeights;
        public Vector3 Tangent;
        public Vector3 Binormal;

        public static int SizeInBytes = sizeof(float) * 18;

        public static VertexElement[] VertexElements = new VertexElement[]
            {
                new VertexElement(0,0,VertexElementFormat.Vector3,VertexElementMethod.Default,
                    VertexElementUsage.Position,0),
                new VertexElement(0,sizeof(float) * 3,VertexElementFormat.Vector3,VertexElementMethod.Default,
                    VertexElementUsage.Normal,0),
                new VertexElement(0,sizeof(float) * 6,VertexElementFormat.Vector4,VertexElementMethod.Default,
                    VertexElementUsage.TextureCoordinate,0),
                new VertexElement(0,sizeof(float) * 8,VertexElementFormat.Vector4,VertexElementMethod.Default,
                    VertexElementUsage.TextureCoordinate,1),
                new VertexElement(0,sizeof(float) * 12, VertexElementFormat.Vector3, VertexElementMethod.Default,
                    VertexElementUsage.Tangent,0),
                new VertexElement(0,sizeof(float) * 15, VertexElementFormat.Vector3, VertexElementMethod.Default,
                    VertexElementUsage.Binormal,0),
            };
    }
}
