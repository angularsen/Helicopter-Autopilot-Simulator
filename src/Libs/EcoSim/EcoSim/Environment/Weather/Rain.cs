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
using Sim.Interface;
using Sim.Graphics;

namespace Sim.Environment
{
    class Rain : WeatherEffect
    {
        private VertexBillboard[] particles;
        private IndexBuffer iBuffer;

        public override void LoadContent(ContentManager cm, GraphicsDevice g)
        {
            texture = cm.Load<Texture2D>(@"textures\sky\rain");
            Effect = cm.Load<Effect>(@"shaders\rain").Clone(g);

            vDec = new VertexDeclaration(g, VertexBillboard.VertexElements);

            Effect.Parameters["t0"].SetValue(texture);
            Effect.Parameters["matWorld"].SetValue(Matrix.Identity);
        }

        public override void Build(Cube c, int numParticles, GraphicsDevice g)
        {
            int numVertices = numParticles * 4;             // each billboard is a quad
            particles = new VertexBillboard[numVertices];

            Random rand = new Random();
            int i = 0;
            while (i < numVertices)
            {
                Vector2 scale = new Vector2(rand.Next(1,4)/10.0f, rand.Next(40,80)/10.0f);
                Vector3 pos = new Vector3(rand.Next((int)c.W), rand.Next((int)c.H), rand.Next((int)c.L));
                particles[i++] = new VertexBillboard(pos, Vector3.Right, Vector3.One, new Vector2(0, 0), scale);
                particles[i++] = new VertexBillboard(pos, Vector3.Right, Vector3.One, new Vector2(1, 0), scale);
                particles[i++] = new VertexBillboard(pos, Vector3.Right, Vector3.One, new Vector2(1, 1), scale);
                particles[i++] = new VertexBillboard(pos, Vector3.Right, Vector3.One, new Vector2(0, 1), scale);
            }

            int[] indices = new int[numParticles * 6];
            for (i = 0; i < numParticles; i++)
            {
                indices[i * 6] = i * 4;
                indices[i * 6 + 1] = i * 4 + 1;
                indices[i * 6 + 2] = i * 4 + 2;
                indices[i * 6 + 3] = i * 4;
                indices[i * 6 + 4] = i * 4 + 2;
                indices[i * 6 + 5] = i * 4 + 3;
            }

            vBuffer = new VertexBuffer(g, VertexBillboard.SizeInBytes * particles.Length, BufferUsage.WriteOnly);
            vBuffer.SetData(particles);
            iBuffer = new IndexBuffer(g, typeof(int), indices.Length, BufferUsage.WriteOnly);
            iBuffer.SetData(indices);

            Effect.Parameters["vOrigin"].SetValue(c.Origin);
            Effect.Parameters["fHeight"].SetValue(c.H);
            Effect.Parameters["fWidth"].SetValue(c.W);
            Effect.Parameters["fLength"].SetValue(c.L);
        }

        public override void Draw(GraphicsDevice g, float worldTime, Camera cam, Vector3 color)
        {
            Effect.Parameters["fTime"].SetValue(worldTime / 750);
            Effect.Parameters["vColor"].SetValue(color);
            Effect.Parameters["vVelocity"].SetValue(new Vector3(Sim.Settings.Graphics.Default.WindX, -200, Sim.Settings.Graphics.Default.WindZ));

            g.VertexDeclaration = vDec;
            g.Indices = iBuffer;
            g.Vertices[0].SetSource(vBuffer, 0, VertexBillboard.SizeInBytes);
            g.RenderState.DepthBufferWriteEnable = false;

            Effect.Begin();
            Effect.CurrentTechnique.Passes[0].Begin();
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, particles.Length, 0, particles.Length / 3);
            Effect.CurrentTechnique.Passes[0].End();
            Effect.End();

            g.RenderState.DepthBufferWriteEnable = true;
        }
    }
}
