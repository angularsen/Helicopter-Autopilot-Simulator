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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Sim.Interface;
using Sim.Environment;
using System.Threading;

namespace Sim
{
    public class SimEngine : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private UserInterface ui;
        private World world;
        private PostProcessManager ppManager;

        public UserInterface UI { get { return ui; } }
        public World World { get { return world; } }
        public GraphicsDeviceManager Graphics { get { return graphics; } }
        public PostProcessManager PostProcessManager { get { return ppManager; } }

        public SimEngine()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            int w = Graphics.GraphicsDevice.DisplayMode.Width;
            int h = GraphicsDevice.DisplayMode.Height;
            System.Console.WriteLine("blah: " + w + h);

            SetResolution();

            Shaders.LoadContent(GraphicsDevice, Content);
            Fonts.LoadContent(Content);
            Textures.LoadHeightmaps(GraphicsDevice);
            Models.LoadContent(Content);

            world = new World(this);
            ui = new UserInterface(this);

            Components.Add(ui);
            Components.Add(world);

            ppManager = new PostProcessManager();

            base.Initialize();
        }

        public void SetResolution()
        {
            Graphics.PreferredBackBufferWidth = Sim.Settings.Graphics.Default.ResolutionWidth;
            Graphics.PreferredBackBufferHeight = Sim.Settings.Graphics.Default.ResolutionHeight;
            Graphics.IsFullScreen = Sim.Settings.Graphics.Default.FullScreen;
            Graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            ppManager.LoadContent(GraphicsDevice, Content);

            BuildData data = new BuildData(Texture2D.FromFile(GraphicsDevice, "Content\\Heightmaps\\default.jpg"), 
                new Vector3(3.33f, 2.4f, 3.13f), Climate.Tropical, 0.8f, 0.04f, 2);

            WorldBuilder builder = new WorldBuilder(this, data);
            Thread builderThread = new Thread(new ThreadStart(builder.Build));
            builderThread.Start();

            ui.ScreenManager.ChangeScreen(new LoadScreen(ui.ScreenManager, builder, true));
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        static class Program
        {
            static void Main(string[] args)
            {

                using (SimEngine sim = new SimEngine())
                    sim.Run();
            }
        }
    }
}
