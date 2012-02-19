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
using Sim.Utility;

namespace Sim.Environment
{
    public class Lighting
    {
        private World world;
        private PointLight[] lights = new PointLight[8];
        private int numberOfLights = 0;

        // shadow mapping
        private Matrix sunWVP;
        private RenderTarget2D smapTarget;
        public Texture2D smapTexture;
        private DepthStencilBuffer smapStencilBuffer;

        private EffectParameter pSunColor;
        private EffectParameter pSunVector;
        private EffectParameter pSunIntensity;
        private EffectParameter pAmbientColor;
        private EffectParameter pAmbientLight;
        private EffectParameter pNumberOfLights;
        private EffectParameter pFogColor;
        
        private Vector3 sunColor;                       // color of directional lighting from the sun
        private Vector3 sunVector;                      // directed from horizontal plane toward sun
        private Vector3 ambientColor;                   // scattered light color
        private Vector3 totalColor;                     // sum of ambient and sun colors
        private float ambientLight;
        private float sunIntensity;
        private float sunAngle;
        

        #region Properties

        public PointLight[] Lights { get { return lights; } }
        public int NumberOfLights { get { return numberOfLights; } set { numberOfLights = value; } }
        public Vector3 SunColor { get { return sunColor; } }
        public Vector3 SunVector { get { return sunVector; } }
        public Vector3 AmbientColor { get { return ambientColor; } }
        public Vector3 TotalColor { get { return totalColor; } }
        public float AmbientLight { get { return ambientLight; } }
        public float SunIntensity { get { return sunIntensity; } }
        public float SunAngle { get { return sunAngle; } }
        

        #endregion

        public Lighting(World world)
        {
            this.world = world;

            pSunColor = Shaders.Common.Parameters["vSunColor"];
            pSunVector = Shaders.Common.Parameters["vSunVector"];
            pAmbientColor = Shaders.Common.Parameters["vAmbientColor"];
            pAmbientLight = Shaders.Common.Parameters["fAmbient"];
            pNumberOfLights = Shaders.Common.Parameters["iNumLights"];
            pSunIntensity = Shaders.Common.Parameters["fSunIntensity"];
            pFogColor = Shaders.Common.Parameters["vFogColor"];

            //Shaders.Common.Parameters["tCloudShadowMap"].SetValue(world.Game.Content.Load<Texture2D>(@"textures\sky\clouds"));

            Random r = new Random();
            if (lights[0] == null)
            {
                for (int i = 0; i < lights.Length; i++)
                {
                    Vector3 color = new Vector3(r.Next(1000) / 1000.0f, r.Next(1000) / 1000.0f, r.Next(1000) / 1000.0f);
                    lights[i] = new PointLight(Vector3.Zero, color, Shaders.Common.Parameters["lights"].Elements[i]);
                }
            }

            // setup the shadowmap render target
            GraphicsDevice g = world.GraphicsDevice;
            PresentationParameters pp = world.GraphicsDevice.PresentationParameters;
            int shadowWidth = pp.BackBufferWidth;
            int shadowHeight = pp.BackBufferHeight;

            smapTarget = new RenderTarget2D(g, shadowWidth, shadowHeight, 1, SurfaceFormat.Single);
            smapStencilBuffer = new DepthStencilBuffer(g, shadowWidth, shadowHeight, g.DepthStencilBuffer.Format);
        }

        public void Update(float worldTime)
        {
            UpdateSun();

            pSunColor.SetValue(sunColor);
            pSunVector.SetValue(sunVector);
            pAmbientColor.SetValue(ambientColor);
            pAmbientLight.SetValue(ambientLight);
            pNumberOfLights.SetValue(numberOfLights);
            pSunIntensity.SetValue(sunIntensity);
            pFogColor.SetValue(totalColor);
        }

        private void UpdateSun()
        {
            sunAngle = world.WorldTotalTime * world.DayTimeRatio;

            // an angle of zero points east for sunrise (0,0,-1)
            sunVector = new Vector3(0, (float)Math.Sin(sunAngle), -(float)Math.Cos(sunAngle));

            // sun intensity is 0 at night to prevent lighting the underside of objects
            sunIntensity = MathHelper.Clamp(Vector3.Dot(sunVector, Vector3.Up) * 1.5f, 0, 1);
            ambientLight = 0.2f;

            // sun color is a bit more red/orange when lower in the sky
            sunColor = new Vector3(1, 0.5f + sunIntensity / 2, sunIntensity);
            ambientColor = new Vector3(1, 1 - sunIntensity / 10, 1 - sunIntensity / 5);
            totalColor = sunColor * sunIntensity + ambientColor * ambientLight;
            totalColor /= Sim.Settings.Graphics.Default.Overcast;
            
            // update the sun's world-view-projection matrix used for the shadowmap
            Matrix sunWorld = Matrix.Identity;
            Matrix sunView = Matrix.CreateLookAt(world.Terrain.Center + sunVector * 1500, world.Terrain.Center, Vector3.Up);
            Matrix sunProjection = Matrix.CreateOrthographic(world.Terrain.Length * 1.1f, world.Terrain.Length * 1.1f, 0.5f, 3000);
            sunWVP = sunWorld * sunView * sunProjection;
        }

        public void RenderShadowmap()
        {
            DepthStencilBuffer prevStencilBuffer = world.GraphicsDevice.DepthStencilBuffer;
            world.GraphicsDevice.DepthStencilBuffer = smapStencilBuffer;

            Shaders.Common.Parameters["matSunWVP"].SetValue(sunWVP);
            world.GraphicsDevice.SetRenderTarget(0, smapTarget);

            // clear target white so that pixels not in the scene aren't shaded
            world.GraphicsDevice.Clear(ClearOptions.Target, Color.White, 0, 0);

            world.Terrain.DrawShadowmap();

            world.GraphicsDevice.SetRenderTarget(0, null);
            smapTexture = smapTarget.GetTexture();
            Shaders.Common.Parameters["tShadowmap"].SetValue(smapTexture);

            world.GraphicsDevice.DepthStencilBuffer = prevStencilBuffer;
        }
    }
}
