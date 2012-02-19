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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sim.Graphics;

namespace Sim.Environment
{
    public class Vegetation
    {
        public bool Visible = true;
        private float density = 1.0f;       // percentage of valid vertices that contain vegetation
        private float treeDensity = 0.02f;  // percentage of vegeation as trees

        // drawing distances
        private float grassDrawFadeStartDist;
        private float grassDrawFadeEndDist;
        private float treeDrawFadeStartDist;
        private float treeDrawFadeEndDist;
        private EffectParameter pTexture;

        private bool drawGrass = true;
        private bool drawTrees = true;

        private Effect effect;
        private SimEngine sim;
        private VertexDeclaration declaration;

        // buffers
        private VertexBuffer vBufferGrass;
        private IndexBuffer iBufferGrass;
        private VertexBuffer vBufferTrees;
        private IndexBuffer iBufferTrees;

        // textures & models
        private Texture2D texGrass;
        private Texture2D texTree;
        private Model treeModel;

        public Vegetation(SimEngine sim, BuildData data)
        {
            this.sim = sim;
            treeDensity = data.TreeDensity;
            density = data.VegetationDensity;
            grassDrawFadeEndDist = Sim.Settings.Graphics.Default.GrassDrawDist;
            grassDrawFadeStartDist = 0.8f * grassDrawFadeEndDist;
            treeDrawFadeEndDist = Sim.Settings.Graphics.Default.TreeDrawDist;
            treeDrawFadeStartDist = 0.8f * treeDrawFadeEndDist;
        }

        public void LoadContent(ContentManager cm, Terrain terrain, Climate climate)
        {
            switch (climate)
            {
                case Climate.Tropical:
                    texGrass = cm.Load<Texture2D>(@"textures/vegetation/grassShrub");
                    texTree = cm.Load<Texture2D>(@"textures/vegetation/tree_palm2");
                    break;
                case Climate.Polar:
                    texGrass = cm.Load<Texture2D>(@"textures/vegetation/grassTundra");
                    texTree = cm.Load<Texture2D>(@"textures/vegetation/pine-snowy");
                    break;
                case Climate.Dry:
                    texGrass = cm.Load<Texture2D>(@"textures/vegetation/grassShrub");
                    texTree = cm.Load<Texture2D>(@"textures/vegetation/tree_palm2");
                    break;
                default:
                    break;
            };

            effect = cm.Load<Effect>(@"shaders\vegetation");
            effect.Parameters["tCloudShadowMap"].SetValue(cm.Load<Texture2D>(@"textures\sky\clouds"));
            pTexture = effect.Parameters["t0"];

            Build(terrain, climate);
            declaration = new VertexDeclaration(sim.GraphicsDevice, VertexBillboard.VertexElements);
        }

        private void Build(Terrain t, Climate climate)
        {
            float min = 5;
            float low = t.LowMark * 2;
            float max = t.HighMark / 2;
            float high = (max + low) / 2.0f;

            List<VertexBillboard> treeVertices = new List<VertexBillboard>();
            List<int> treeIndices = new List<int>();

            List<VertexBillboard> grassVertices = new List<VertexBillboard>();
            List<int> grassIndices = new List<int>();

            Random rand = new Random();
            for (int i = 0; i < t.Vertices.Length; i++)
            {
                VertexTerrain v = t.Vertices[i];  // current vertex in the terrain mesh
                float height = v.Position.Y;
                float baseProbability = 0;  // initially, there is no chance of adding a plant

                if (height > min && height < max)   // check if the vertex is within allowed height range
                {
                    if (v.TextureWeights.W >= 0.5f)
                        baseProbability = 0;
                    else if (climate == Climate.Dry)
                    {
                        if (height < low)
                            baseProbability = 0.6f - (height  - max/2) / (max/2 - min);    
                        else if (height > high)
                            baseProbability = 0.2f - (height - high) / (max - high); 
                        else
                            baseProbability = 0.2f; 
                    }
                    else
                    {
                        if (height < low)
                            baseProbability = (height - min) / (low - min);     // fewer plants toward shore
                        else if (height > high)
                            baseProbability = v.TextureWeights.Z - (height - high) / (max - high);   // fewer plants near mountain peaks
                        else
                            baseProbability = v.TextureWeights.Z;    // otherwise, the vertex is perfectly good for a plant
                    }
                }

                baseProbability *= density;     // scale probability by density
                float treeProbability = baseProbability * treeDensity;
                double roll = rand.NextDouble(); // get a random number to check if the plant should be added
                if (baseProbability > roll)
                {
                    if (treeProbability > roll)
                        //trees.Add(new TreeModel(treeModel, v.Position, rand.Next(175,250)/100.0f));
                        AddTree(v, treeVertices, treeIndices, rand, climate);
                    else
                        AddGrass(v, grassVertices, grassIndices, rand, climate);
                }
            }

            // buffer the vegetation
            if (grassVertices.Count > 0)
            {
                vBufferGrass = new VertexBuffer(sim.GraphicsDevice, grassVertices.Count * VertexBillboard.SizeInBytes, BufferUsage.WriteOnly);
                vBufferGrass.SetData(grassVertices.ToArray());
                iBufferGrass = new IndexBuffer(sim.GraphicsDevice, typeof(int), grassIndices.Count, BufferUsage.WriteOnly);
                iBufferGrass.SetData(grassIndices.ToArray());
            }

            if (treeVertices.Count > 0)
            {
                vBufferTrees = new VertexBuffer(sim.GraphicsDevice, treeVertices.Count * VertexBillboard.SizeInBytes, BufferUsage.WriteOnly);
                vBufferTrees.SetData(treeVertices.ToArray());
                iBufferTrees = new IndexBuffer(sim.GraphicsDevice, typeof(int), treeIndices.Count, BufferUsage.WriteOnly);
                iBufferTrees.SetData(treeIndices.ToArray());
            }
        }

        /// <summary>
        /// Adds a tree billboard
        /// </summary>
        private void AddTree(VertexTerrain v, List<VertexBillboard> treeVertices, List<int> treeIndices,
            Random r, Climate climate)
        {
            Vector3 color;
            if (climate == Climate.Polar)
                color = new Vector3(1, r.Next(90, 101) / 100.0f, r.Next(90, 101) / 100.0f);
            else
                color = new Vector3(1, r.Next(90, 101) / 100.0f, r.Next(40, 101) / 100.0f);

            Vector2 scale = new Vector2(r.Next(4, 11), r.Next(50, 100) / 10.0f);

            treeVertices.Add(new VertexBillboard(v.Position, Vector3.Up, color, Vector2.Zero, scale));
            treeVertices.Add(new VertexBillboard(v.Position, Vector3.Up, color, Vector2.UnitX, scale));
            treeVertices.Add(new VertexBillboard(v.Position, Vector3.Up, color, Vector2.One, scale));
            treeVertices.Add(new VertexBillboard(v.Position, Vector3.Up, color, Vector2.UnitY, scale));

            int[] baseIndices = new int[] { 0, 1, 2, 0, 2, 3 };
            int offset = treeIndices.Count / 6 * 4;
            for (int i = 0; i < 6; i++)
                treeIndices.Add(baseIndices[i] + offset);
        }

        /// <summary>
        /// Adds a grass billboard
        /// </summary>
        private void AddGrass(VertexTerrain v, List<VertexBillboard> grassVertices, List<int> grassIndices, 
            Random r, Climate climate)
        {
            float rg = r.Next(50, 101) / 100.0f;
            Vector3 color = Vector3.One;
            if (climate == Climate.Tropical)
                color = new Vector3(rg, rg, r.Next(40, 101) / 100.0f);
            else if (climate == Climate.Polar)
                color = new Vector3(rg, rg, rg);
            else if (climate == Climate.Dry)
                color = new Vector3(rg, rg, r.Next(1, 40) / 100.0f);

            Vector2 scale = new Vector2(r.Next(5, 8), r.Next(5, 20) / 10.0f);

            grassVertices.Add(new VertexBillboard(v.Position, v.Normal, color, Vector2.Zero, scale));
            grassVertices.Add(new VertexBillboard(v.Position, v.Normal, color, Vector2.UnitX, scale));
            grassVertices.Add(new VertexBillboard(v.Position, v.Normal, color, Vector2.One, scale));
            grassVertices.Add(new VertexBillboard(v.Position, v.Normal, color, Vector2.UnitY, scale));

            int[] baseIndices = new int[] { 0, 1, 2, 0, 2, 3 };
            int offset = grassIndices.Count / 6 * 4;
            for (int i = 0; i < 6; i++)
                grassIndices.Add(baseIndices[i] + offset);
        }

        public void Update(float worldTime)
        {
            effect.Parameters["fTime"].SetValue(worldTime / 1000);
        }

        public void Draw()
        {
            if (Visible)
            {
                GraphicsDevice g = sim.GraphicsDevice;

                effect.Parameters["matWorld"].SetValue(Matrix.Identity);

                if (Sim.Settings.Graphics.Default.QualityVegetation)
                    effect.CurrentTechnique = effect.Techniques["Quality"];
                else
                    effect.CurrentTechnique = effect.Techniques["Fast"];

                g.VertexDeclaration = declaration;

                if (drawGrass && vBufferGrass != null)
                {
                    pTexture.SetValue(texGrass);
                    DrawBuffered(g, vBufferGrass, iBufferGrass);
                }

                if (drawTrees && vBufferTrees != null)
                {
                    pTexture.SetValue(texTree);
                    DrawBuffered(g, vBufferTrees, iBufferTrees);
                }
            }
        }

        private void DrawBuffered(GraphicsDevice g, VertexBuffer vBuffer, IndexBuffer iBuffer)
        {
            int numVerts = vBuffer.SizeInBytes / VertexBillboard.SizeInBytes;
            g.Vertices[0].SetSource(vBuffer, 0, VertexBillboard.SizeInBytes);
            g.Indices = iBuffer;

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVerts, 0, numVerts / 2);
                pass.End();
            }
            effect.End();

            g.RenderState.AlphaBlendEnable = false;
            g.RenderState.DepthBufferWriteEnable = true;
            g.RenderState.AlphaTestEnable = false;
        }

        public void SetGrassDrawDistance(float dist)
        {
            grassDrawFadeStartDist = 0.8f * dist;
            grassDrawFadeEndDist = dist;
            if (dist == 0)
                drawGrass = false;
            else
                drawGrass = true;
        }

        public void SetTreeDrawDistance(float dist)
        {
            treeDrawFadeStartDist = 0.8f * dist;
            treeDrawFadeEndDist = dist;
            if (dist == 0)
                drawTrees = false;
            else
                drawTrees = true;
        }
    }
}
