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
    class Snow : WeatherEffect
    {
        private VertexPointSpriteParticle[] particles;

        public override void LoadContent(ContentManager cm, GraphicsDevice g)
        {
            texture = cm.Load<Texture2D>(@"textures\sky\snow");
            Effect = cm.Load<Effect>(@"shaders\snow").Clone(g);
            Effect.Parameters["t0"].SetValue(texture);
            vDec = new VertexDeclaration(g, VertexPointSpriteParticle.VertexElements);
        }

        public override void Build(Cube c, int numParticles, GraphicsDevice g)
        {
            particles = new VertexPointSpriteParticle[numParticles];

            Random rand = new Random();
            for (int i = 0; i < numParticles; i++)
            {
                Vector3 pos = new Vector3(rand.Next((int)c.W), rand.Next((int)c.H), rand.Next((int)c.L));
                Vector2 randoms = new Vector2(rand.Next(1000), rand.Next(1000));
                float pSize = rand.Next(5, 20) / 10.0f;
                particles[i] = new VertexPointSpriteParticle(pos, pSize, randoms);
            }

            vBuffer = new VertexBuffer(g, VertexPointSpriteParticle.SizeInBytes * particles.Length,
                BufferUsage.Points);
            vBuffer.SetData(particles);

            Effect.Parameters["vOrigin"].SetValue(c.Origin);
            Effect.Parameters["fHeight"].SetValue(c.H);
            Effect.Parameters["fWidth"].SetValue(c.W);
            Effect.Parameters["fLength"].SetValue(c.L);
        }

        public override void Draw(GraphicsDevice g, float worldTime, Camera cam, Vector3 color)
        {
            Sim.Settings.Graphics gs = Sim.Settings.Graphics.Default;
            Effect.Parameters["matWorld"].SetValue(WorldMatrix);
            Effect.Parameters["matProjection"].SetValue(cam.Projection);
            Effect.Parameters["matView"].SetValue(cam.View);
            Effect.Parameters["fTime"].SetValue(worldTime / 500);
            Effect.Parameters["fTurbulence"].SetValue(gs.WindTurbulence);
            Effect.Parameters["vVelocity"].SetValue(new Vector3(gs.WindX, -15, gs.WindZ));
            Effect.Parameters["vColor"].SetValue(color);

            g.RenderState.DepthBufferWriteEnable = false;
            g.VertexDeclaration = vDec;
            g.Vertices[0].SetSource(vBuffer, 0, VertexPointSpriteParticle.SizeInBytes);

            Effect.Begin();
            Effect.CurrentTechnique.Passes[0].Begin();
            g.DrawPrimitives(PrimitiveType.PointList, 0, particles.Length);
            Effect.CurrentTechnique.Passes[0].End();
            Effect.End();

            g.RenderState.DepthBufferWriteEnable = true;
        }
    }
}
