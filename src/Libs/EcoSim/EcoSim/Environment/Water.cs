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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Sim.Interface;

namespace Sim.Environment
{
    public class Water
    {
        public bool Visible = true;

        #region Private Fields

        private VertexBuffer vBuffer;
        private IndexBuffer iBuffer;
        private VertexPositionNormalTexture[] vertices;
        private int[] indices;
        private VertexDeclaration declaration;
        private Matrix worldMatrix;

        private float meshWidth = 100;
        private float meshHeight = 100;
        private int meshDivisions = 100;          // mesh is split into 4 * (1 - meshDivisions)^2 vertices
        private float sealevel = 5;             // sealevel
        private SimEngine sim;
        private Effect effect;
        private EffectParameter pTime;
        #endregion

        #region Properties

        public float Sealevel { get { return sealevel; } }

        #endregion

        public Water(SimEngine sim)
        {
            this.sim = sim;
        }

        public void LoadContent(ContentManager cm, Terrain t)
        {
            effect = cm.Load<Effect>(@"shaders\water");

            
            //effect.Parameters["tEnvMap"].SetValue(cm.Load<TextureCube>(@"textures\cubemaps\sea"));
            effect.Parameters["tNormalMap"].SetValue(cm.Load<Texture2D>(@"textures\terrain\wavebumps"));
            effect.Parameters["vTextureScale"].SetValue(35);
            effect.Parameters["tHeightmap"].SetValue(t.Heightmap);
            effect.Parameters["fTerrainWidth"].SetValue(t.Width);
            effect.Parameters["fTerrainLength"].SetValue(t.Length);
            effect.Parameters["matWorld"].SetValue(Matrix.CreateTranslation(0, sealevel, 0));
            effect.Parameters["fShoreBlendCoefficient"].SetValue(10 * t.Scale.Y);
            effect.Parameters["tCloudShadowMap"].SetValue(cm.Load<Texture2D>(@"textures\sky\clouds"));

            pTime = effect.Parameters["fTime"];

            BuildVertices(t);
            BuildIndices();

            declaration = new VertexDeclaration(sim.GraphicsDevice, VertexPositionNormalTexture.VertexElements);
            
        }

        private void BuildVertices(Terrain t)
        {
            meshHeight = t.Length + 5000;
            meshWidth = t.Width + 5000;
            float squareHeight = meshHeight / (float)(meshDivisions + 1);
            float squareWidth = meshWidth / (float)(meshDivisions + 1);

            vertices = new VertexPositionNormalTexture[(meshDivisions + 2) * (meshDivisions + 2)];

            int i = 0;
            for (int y = 0; y < meshDivisions + 2; y++)
            {
                for (int x = 0; x < meshDivisions + 2; x++)
                {
                    float worldX = y * squareHeight - meshHeight / 2 + t.Center.X;
                    float worldZ = -x * squareWidth + meshWidth / 2 + t.Center.Z;
                    vertices[i++] = new VertexPositionNormalTexture(new Vector3(worldX, 0,worldZ),
                        Vector3.Up, new Vector2(worldX / meshWidth, worldZ / meshHeight));
                }
            }

            // save the vertices to the vertex buffer
            vBuffer = new VertexBuffer(sim.GraphicsDevice, vertices.Length * 
                VertexPositionNormalTexture.SizeInBytes, BufferUsage.WriteOnly);
            vBuffer.SetData(vertices);
        }

        private void BuildIndices()
        {
            int squares = (meshDivisions + 1) * (meshDivisions + 1);
            indices = new int[squares * 6]; // 2 triangles = 6 indices per square
            
            int index = 0;
            for (int i = 0; i < squares; i++)
            {
                // get the index for the vertices in the current square
                int topleft = i + i / (meshDivisions + 1);
                int topright = topleft + 1;
                int bottomright = topright + (meshDivisions + 2);
                int bottomleft = bottomright - 1;

                // upper right triangle of square
                indices[index++] = topleft;
                indices[index++] = topright;
                indices[index++] = bottomright;

                // bottom left triangle of square
                indices[index++] = topleft;
                indices[index++] = bottomright;
                indices[index++] = bottomleft;
            }

            // save the indices to the index buffer
            iBuffer = new IndexBuffer(sim.GraphicsDevice, typeof(int),
                indices.Length, BufferUsage.WriteOnly);
            iBuffer.SetData(indices);
        }

        public void Update(float worldTime)
        {
            pTime.SetValue(worldTime / 4000);
        }

        public void Draw()
        {
            if (Visible)
            {
                GraphicsDevice gDevice = sim.GraphicsDevice;

                effect.Parameters["matViewI"].SetValue(Matrix.Invert(sim.UI.Camera.View));

                gDevice.RenderState.AlphaBlendEnable = true;
                gDevice.VertexDeclaration = declaration;
                gDevice.Indices = iBuffer;
                gDevice.Vertices[0].SetSource(vBuffer, 0, VertexPositionNormalTexture.SizeInBytes);

                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();
                gDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);
                effect.CurrentTechnique.Passes[0].End();
                effect.End();

                gDevice.RenderState.AlphaBlendEnable = false;
            }
        }
    }
}
