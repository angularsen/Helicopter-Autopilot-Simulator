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
using Microsoft.Xna.Framework.Content;
using Sim.Interface;
using Sim.Graphics;

namespace Sim.Environment
{
    public class Sky
    {
        public bool Visible = true;

        private SimEngine sim;              // the simulation the sky belongs in
        private Model skyDome;              // the skydome model
        private Texture2D skyTexture;       // the skydome texture
        private Texture2D cloudTexture;


        private Vector3 horizonColor;       // color of the bottom of the skydome


        private Vector3 sunPosition = new Vector3(0, 50, 0);    // position of the sun
        private VertexDeclaration sunVertexDeclaration;         // vertex declaration for the sun
        private Effect sunEffect;                               // point sprite shader for sun
        private Texture2D sunTexture;                           // sun's texture


        public Vector3 HorizonColor { get { return horizonColor; } }


        public Sky(SimEngine sim)
        {
            this.sim = sim;
        }

        public void LoadContent(ContentManager cm)
        {
            sunTexture = cm.Load<Texture2D>(@"textures/sky/sun");
            skyTexture = cm.Load<Texture2D>(@"textures/sky/stars");
            cloudTexture = cm.Load<Texture2D>(@"textures/sky/clouds");
            skyDome = cm.Load<Model>(@"models/skysphere");

            Effect skyEffect = cm.Load<Effect>(@"shaders/sky");
            skyEffect.Parameters["tNight"].SetValue(skyTexture);
            skyEffect.Parameters["tClouds"].SetValue(cloudTexture);
            skyDome.Meshes[0].MeshParts[0].Effect = skyEffect;

            sunEffect = Shaders.PointSprite;
            sunVertexDeclaration = new VertexDeclaration(sim.GraphicsDevice, VertexPointSprite.VertexElements);
        }

        /// <summary>
        /// Updates the sun's lighting direction and the ambient light in the world
        /// </summary>
        public void Update(float worldTime)
        {
            // update the horizon color for the graphics device clear
            float sunHeight = sim.World.Lighting.SunVector.Y;
            if (sunHeight > 0)
                horizonColor = Vector3.Lerp(new Vector3(0.8f, 0.35f, 0),
                    new Vector3(1, 1, 1), sim.World.Lighting.SunIntensity);
            else
                horizonColor = Vector3.Lerp(new Vector3(0.8f, 0.35f, 0),
                    new Vector3(0.2f, 0.2f, 0.2f), MathHelper.Min(-sunHeight * 1.5f, 1));

            // update the position of the sun

        }

        public void Draw(float worldTime)
        {
            if (Visible)
            {
                Sim.Interface.Camera c = sim.UI.Camera;

                // don't write to depth buffer so other objects can be drawn over the skydome
                sim.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

                Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
                skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);

                foreach (ModelMesh mesh in skyDome.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] *
                            Matrix.CreateTranslation(0, -0.15f, 0) * 
                            Matrix.CreateScale(100) *
                            Matrix.CreateTranslation(c.Position);

                        effect.Parameters["fTime"].SetValue(worldTime / 80000);
                        effect.Parameters["matWorld"].SetValue(worldMatrix);
                    }
                    mesh.Draw();
                }

                DrawSun();

                sim.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            }
        }

        private void DrawSun()
        {
            Vector3 camPos = sim.UI.Camera.Position;
            float sunHeight = sim.World.Lighting.SunVector.Y;
            sunPosition = new Vector3(camPos.X, sunHeight * 1500, camPos.Z + sim.World.Lighting.SunVector.Z * 1500);

            GraphicsDevice g = sim.GraphicsDevice;
            VertexPointSprite[] spriteArray = new VertexPointSprite[1];
            spriteArray[0] = new VertexPointSprite(sunPosition, 40);

            sunEffect.CurrentTechnique = sunEffect.Techniques["Sun"];
            sunEffect.Parameters["matWorld"].SetValue(Matrix.Identity);
            sunEffect.Parameters["tTexture"].SetValue(sunTexture);

            g.RenderState.PointSpriteEnable = true;
            g.RenderState.AlphaBlendEnable = true;
            g.RenderState.SourceBlend = Blend.SourceAlpha;
            g.RenderState.DestinationBlend = Blend.DestinationAlpha;

            sunEffect.Begin();
            foreach (EffectPass pass in sunEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                g.VertexDeclaration = sunVertexDeclaration;
                g.DrawUserPrimitives(PrimitiveType.PointList, spriteArray, 0, spriteArray.Length);
                pass.End();
            }
            sunEffect.End();

            g.RenderState.PointSpriteEnable = false;
            g.RenderState.AlphaBlendEnable = false;
            g.RenderState.SourceBlend = Blend.SourceAlpha;
            g.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
        }
    }
}
