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
using System.Threading;

namespace Sim.Interface
{
    public class SetupScreen : Screen
    {
        private Sim.Settings.Build bs = Sim.Settings.Build.Default;
        private ControlGroup controls;

        public SetupScreen(ScreenManager manager)
            : base(manager)
        {
            Initialize();
            
        }

        public override void Initialize()
        {
            Viewport vp = manager.UI.GraphicsDevice.Viewport;

            controls = new ControlGroup();

            int pad = (int)(0.05f * vp.Width);

            SetupComponents(vp, pad);
        }

        public override void ConnectControls()
        {
            controls.Connect(manager.UI);
        }

        public override void DisconnectControls()
        {
            controls.Disconnect(manager.UI);
        }

        private void SetupTerrainComponents(Viewport vp, Rectangle settingsPanelArea)
        {
            int sliderHeight = (int)(0.05f * vp.Height);

            ControlMat mat = new ControlMat(settingsPanelArea, 7, 1, 10);

            Slider scaleX = new Slider("Length Scale", mat.Spaces[0], 0.1f, 10.0f);
            scaleX.SetValue(bs.ScaleX);
            scaleX.Use += delegate(float v) { bs.ScaleX = v; };
            controls.Add(scaleX);

            Slider scaleY = new Slider("Height Scale", mat.Spaces[1], 0.1f, 10.0f);
            scaleY.SetValue(bs.ScaleY);
            scaleY.Use += delegate(float v) { bs.ScaleY = v; };
            controls.Add(scaleY);

            Slider scaleZ = new Slider("Width Scale", mat.Spaces[2], 0.1f, 10.0f);
            scaleZ.SetValue(bs.ScaleZ);
            scaleZ.Use += delegate(float v) { bs.ScaleZ = v; };
            controls.Add(scaleZ);

            Slider vegetationDensity = new Slider("Vegetation", mat.Spaces[3], 0.0f, 1.0f);
            vegetationDensity.SetValue(bs.VegDensity);
            vegetationDensity.Use += delegate(float v) { bs.VegDensity = v; };
            controls.Add(vegetationDensity);

            Slider treeDensity = new Slider("Trees", mat.Spaces[4], 0.0f, 1.0f);
            treeDensity.SetValue(bs.TreeDensity);
            treeDensity.Use += delegate(float v) { bs.TreeDensity = v; };
            controls.Add(treeDensity);

            Slider smoothing = new Slider("Smoothing", mat.Spaces[5], 0, 50);
            smoothing.SetValue(bs.Smoothing);
            smoothing.Use += delegate(float v) { bs.Smoothing = (int)v; };
            controls.Add(smoothing);

            Spinner<Climate> cSpin = new Spinner<Climate>("Climate", mat.Spaces[6]);
            cSpin.Use += delegate() { bs.Climate = cSpin.GetSelected().ToString(); };
            foreach (Climate climate in Enum.GetValues(typeof(Climate)))
                cSpin.Add(climate);
            cSpin.Select((Climate)Enum.Parse(typeof(Climate), bs.Climate));
            controls.Add(cSpin);

            int y = (int)(vp.Height * 0.05f);
            controls.Add(new Label("Setup", new Rectangle(0, y / 2, vp.Width, y * 2), Label.Fit.AlignCenter));
        }

        private void SetupComponents(Viewport vp, int pad)
        {
            int bHeight = (int)(0.05f * vp.Height);
            int bWidth = (int)(0.125f * vp.Width);

            Panel hmPanelBG = new Panel("HM Img BG", new Rectangle(pad, 2 * pad, vp.Width / 3, vp.Width / 3),
                new Color(0,0,0,128));
            controls.Add(hmPanelBG);
            
            Panel hmPanel = new Panel("Heightmap Img", new Rectangle(hmPanelBG.Area.X + 5, hmPanelBG.Area.Y + 5, 
                hmPanelBG.Area.Width - 10, hmPanelBG.Area.Height - 10), Color.White);
            hmPanel.Texture = Textures.Heightmaps[bs.HMIndex];
            controls.Add(hmPanel);

            // area where settings components are displayed
            Rectangle settingsPanelArea = new Rectangle(pad + hmPanel.Area.Right, 2 * pad,
                vp.Width - hmPanel.Area.Width - pad * 3, hmPanelBG.Area.Height);
            Panel settingsPanel = new Panel("Settings", settingsPanelArea, new Color(0, 0, 0, 128));
            controls.Add(settingsPanel);

            // setup the settings components for each tab
            SetupTerrainComponents(vp, settingsPanelArea);

            // button used to return to menu
            Button cancelButton = new Button("Cancel", new Rectangle(pad, vp.Height - bHeight - pad, bWidth,
                bHeight), delegate() { manager.ChangeScreen(new MenuScreen(manager)); });
            controls.Add(cancelButton);

            // button used to build the world
            Button buildButton = new Button("Build", new Rectangle(vp.Width - bWidth - pad, cancelButton.Area.Y,
                bWidth, bHeight), BuildWorld);
            controls.Add(buildButton);

            // button used to display previous heightmap
            Button prevButton = new Button("<", new Rectangle(pad, hmPanel.Area.Bottom + pad / 2,
                bWidth, bHeight), delegate() { hmPanel.Texture = Textures.Heightmaps[bs.HMIndex = bs.HMIndex == 
                    0 ? Textures.Heightmaps.Length - 1 : --bs.HMIndex]; });
            controls.Add(prevButton);

            // button used to display next heightmap
            Button nextButton = new Button(">", new Rectangle(hmPanel.Area.Right - bWidth, prevButton.Area.Y,
                bWidth, bHeight), delegate() { hmPanel.Texture = Textures.Heightmaps[bs.HMIndex = bs.HMIndex == 
                    Textures.Heightmaps.Length - 1 ? 0 : ++bs.HMIndex]; });
            controls.Add(nextButton); 
        }

        private void BuildWorld()
        {
            Sim.Settings.Build.Default.Save();

            BuildData data = new BuildData();
            data.Heightmap = Textures.Heightmaps[bs.HMIndex];
            data.TerrainScale = new Vector3(bs.ScaleX, bs.ScaleY, bs.ScaleZ);
            data.Climate = (Climate)Enum.Parse(typeof(Climate), bs.Climate);
            data.TreeDensity = bs.TreeDensity;
            data.VegetationDensity = bs.VegDensity;
            data.Smoothing = bs.Smoothing;

            WorldBuilder builder = new WorldBuilder(manager.UI.Sim, data);
            Thread worldBuilderThread = new Thread(new ThreadStart(builder.Build));
            worldBuilderThread.Start();
            manager.ChangeScreen(new LoadScreen(manager, builder, false));
        }

        protected override void OnTransparencyChange() { controls.OnTransparencyChange(transparency); }

        public override void Update(GameTime time) { }

        public override void Draw(SpriteBatch sb)
        {
            controls.Draw(sb);
        }
    }
}
