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
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sim.Utility;
using Sim.Pathfinding;

namespace Sim.Environment
{
    // based on Köppen climate classifications
    public enum Climate
    {
        Dry,
        Tropical,
        Continental,
        Boreal,
        Polar,
    }

    /// <summary>
    /// Hub for the 3D components of the simulation
    /// </summary>
    public class World : DrawableGameComponent
    {
        private SimEngine sim;

        private RenderTarget2D worldRenderTarget;

        private Terrain terrain;
        private Water water;
        private Sky sky;
        private Wildlife wildlife;
        private Weather weather;
        private Vegetation vegetation;
        private Lighting lighting;
        

        public String ClockTime;
        private float worldTime = 30000;
        private const float secondsPerDay = 600;            // number of seconds it takes for a day to complete
        private float dayTimeRatio;                         // world total time -> world day time

        // drawing / options
        public FillMode FillMode = FillMode.Solid;
        public bool Paused = true;
        public bool DrawAxes = false;

        #region Properties

        public float WorldTotalTime { get { return worldTime; } }

        public float DayTimeRatio { get { return dayTimeRatio; } }
        public Sky Sky { get { return sky; } }
        public Weather Weather { get { return weather; } set { weather = value; } }
        public Water Water { get { return water; } set { water = value; } }
        public Terrain Terrain { get { return terrain; } set { terrain = value; } }
        public Vegetation Vegetation { get { return vegetation; } set { vegetation = value; } }
        public Wildlife Wildlife { get { return wildlife; } }
        public Lighting Lighting { get { return lighting; } }

        #endregion

        public World(SimEngine sim)
            : base(sim)
        {
            DrawOrder = 1;
            this.sim = sim;
            dayTimeRatio = MathHelper.TwoPi / (secondsPerDay * 1000);
        }

        protected override void LoadContent()
        {
            sky = new Sky(sim);
            sky.LoadContent(sim.Content);
            weather = new Weather(sim);
            weather.LoadContent();
            wildlife = new Wildlife(sim);
            wildlife.LoadContent();
            lighting = new Lighting(this);

            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            worldRenderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 1, SurfaceFormat.Color); 
        }

        public override void Update(GameTime gameTime)
        {
            if (sim.UI.Keyboard.State.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.OemPlus))
                worldTime += 500;
            else if (sim.UI.Keyboard.State.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.OemMinus))
                worldTime -= 500;

            if (!Paused)
            {
                UpdateWorldTime(gameTime);

                lighting.Update(worldTime);
                sky.Update(worldTime);
                water.Update(worldTime);
                vegetation.Update(worldTime);
                wildlife.Update(worldTime);
            }
        }

        public void LoadTerrain(Terrain t)
        {
            this.terrain = t;
        }

        public void UpdateWorldTime(GameTime gameTime)
        {
            worldTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            float time24HClock = MathHelper.Lerp(6, 12, worldTime * dayTimeRatio / MathHelper.PiOver2);
            float time12HClock = time24HClock % 12;
            int hour = (int)(time12HClock);
            int minute = (int)((time12HClock - hour) * 60);

            if (hour == 0)
                hour = 12;

            if (time24HClock >= 12 && time24HClock < 24)
                ClockTime = String.Format("{0}:{1:00} PM", hour, minute);
            else
                ClockTime = String.Format("{0}:{1:00} AM", hour, minute);
        }

        /// <summary>
        /// Resets render states that the spritebatch may have modified when drawing the GUI
        /// </summary>
        private void ResetRenderStates()
        {
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.AlphaBlendEnable = false;
            GraphicsDevice.RenderState.AlphaTestEnable = false;
            GraphicsDevice.RenderState.FillMode = FillMode;
            GraphicsDevice.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
            GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
        }
 
        public override void Draw(GameTime gameTime)
        {
            ResetRenderStates();

            // render shadowmap first
            if (Sim.Settings.Graphics.Default.ShadowsEnabled)
            {
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
                if (terrain != null)
                    lighting.RenderShadowmap();
            }

            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            if (FillMode == FillMode.Solid && Sim.Settings.Graphics.Default.BloomEnabled)
                GraphicsDevice.SetRenderTarget(0, worldRenderTarget);

            // render the scene as normal
            sky.Draw(worldTime);
            if (terrain!=null)terrain.Draw(worldTime);
            if (water!=null)water.Draw();
            if (vegetation!=null)vegetation.Draw();
            wildlife.Draw(worldTime);
            weather.Draw(worldTime);
            if (DrawAxes) Sim.Utility.Draw.Axes(GraphicsDevice, 1000);

            if (FillMode == FillMode.Solid && Sim.Settings.Graphics.Default.BloomEnabled)
                sim.PostProcessManager.Process(worldRenderTarget);

            GraphicsDevice.RenderState.FillMode = FillMode.Solid;
        }
    }
}
