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

namespace Sim
{

    /// <summary>
    /// Defines a cube area
    /// </summary>
    struct Cube
    {
        public Vector3 Origin;
        public float H;
        public float W;
        public float L;

        public Cube(Vector3 origin, float h, float w, float l)
        {
            Origin = origin;
            H = h;
            W = w;
            L = l;
        }

        public float Volume() { return H * W * L; }
    };

    public struct Triangle
    {
        public Vector3 V0;
        public Vector3 V1;
        public Vector3 V2;

        /// <summary>
        /// Defined clockwise with v0 being the upper-left position
        /// </summary>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }

        public float Width() { return Math.Abs(V1.X - V0.X); }
        public float Length() { return Math.Abs(V2.Z - V0.Z); }
    }

    /// <summary>
    /// Data used for building a new world
    /// </summary>
    public struct BuildData
    {
        public Texture2D Heightmap;
        public Vector3 TerrainScale;
        public Sim.Environment.Climate Climate;
        public float VegetationDensity;
        public float TreeDensity;
        public int Smoothing;

        public BuildData(Texture2D heightmap, Vector3 scale, Sim.Environment.Climate climate,
            float vegetationDensity, float treeDensity, int smoothing)
        {
            this.Heightmap = heightmap;
            this.TerrainScale = scale;
            this.Climate = climate;
            this.VegetationDensity = vegetationDensity;
            this.TreeDensity = treeDensity;
            this.Smoothing = smoothing;
        }
    }
}
