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
using Sim.Environment;

namespace Sim
{
    public abstract class ModelEntity
    {
        protected Model model;
        protected Vector3 position;
        protected float scale = 1;
        protected Matrix rotMat = Matrix.Identity;

        protected Vector3 diffuseColor;
        protected Vector3 ambientColor;
        protected Vector3 specularColor;
        protected float alpha;

        public Vector3 Rotation = Vector3.Zero;

        public Vector3 Position { get { return position; } }

        public ModelEntity(Model model, Vector3 position)
        {
            this.model = model;
            this.position = position;
        }

        public BoundingBox GetBoundingBox()
        {
            BoundingBox b = new BoundingBox();
            float r = model.Meshes[0].BoundingSphere.Radius * scale;
            Vector3 c = model.Meshes[0].BoundingSphere.Center;
            b.Min = new Vector3(c.X - r, c.Y - r, c.Z - r) + position;
            b.Max = new Vector3(c.X + r, c.Y + r, c.Z + r) + position;
            return b;
        }

        public BoundingSphere GetBoundingSphere()
        {
            return new BoundingSphere(this.position, model.Meshes[0].BoundingSphere.Radius * scale);
        }

        public virtual void SetEffectParameters(BasicEffect e, World world)
        {
            e.LightingEnabled = true;
            e.DiffuseColor = diffuseColor;
            e.SpecularColor = specularColor;
            e.SpecularPower = 20;
            e.AmbientLightColor = ambientColor;
            e.PreferPerPixelLighting = false;
            e.DirectionalLight0.Enabled = true;
            e.DirectionalLight0.Direction = -world.Lighting.SunVector;
            e.DirectionalLight0.SpecularColor = Vector3.One * world.Lighting.SunIntensity;
            e.DirectionalLight0.DiffuseColor = world.Lighting.SunColor * world.Lighting.SunIntensity;
            
            e.DirectionalLight1.Enabled = true;
            e.DirectionalLight1.DiffuseColor = world.Lighting.AmbientColor * (1-world.Lighting.SunIntensity) / 10;
            e.DirectionalLight1.Direction = world.Lighting.SunVector;
            e.Alpha = alpha;
        }

        public virtual void Draw(Sim.Interface.Camera cam, World world, GraphicsDevice g)
        {
            SetRenderStates(g);

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                g.Indices = mesh.IndexBuffer;

                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    ModelMeshPart meshPart = mesh.MeshParts[i];

                    g.Vertices[0].SetSource(mesh.VertexBuffer, meshPart.StreamOffset, meshPart.VertexStride);
                    g.VertexDeclaration = meshPart.VertexDeclaration;

                    BasicEffect effect = meshPart.Effect as BasicEffect;
                    
                    

                    SetEffectParameters(effect, world);

                    effect.World =
                        transforms[mesh.ParentBone.Index] *
                        Matrix.CreateScale(scale) *
                        rotMat * 
                        Matrix.CreateTranslation(position);
                    effect.View = cam.View;
                    effect.Projection = cam.Projection;
                }
                mesh.Draw();
            }

            ResetRenderStates(g);
        }

        public virtual void SetRenderStates(GraphicsDevice g) {
            g.RenderState.AlphaBlendEnable = true;

        }

        public virtual void ResetRenderStates(GraphicsDevice g) {
            g.RenderState.AlphaBlendEnable = false;
        }
    }
}
