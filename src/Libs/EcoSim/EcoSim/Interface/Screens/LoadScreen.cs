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
using Sim.Environment;

namespace Sim.Interface
{
    public class LoadScreen : Screen
    {
        private WorldBuilder builder;
        private ProgressBar loadBar;
        private Panel background;
        private Label title;
        private bool firstLoad;

        public LoadScreen(ScreenManager manager, WorldBuilder builder, bool firstLoad)
            : base(manager)
        {
            this.builder = builder;
            this.firstLoad = firstLoad;
            Initialize();
        }

        public override void Initialize()
        {
            Viewport vp = manager.UI.GraphicsDevice.Viewport;

            Color bgColor = firstLoad ? Color.Black : new Color(0, 0, 0, 175);
            background = new Panel("Load BG", new Rectangle(0, 0, vp.Width, vp.Height), bgColor);

            int lbW = (int)(vp.Width * 0.8f);
            int lbH = (int)(vp.Height * 0.05f);
            Rectangle lbA = new Rectangle((vp.Width - lbW) / 2, (vp.Height - lbH) / 2, lbW, lbH);
            loadBar = new ProgressBar("Building...", lbA);

            title = new Label("Loading", new Rectangle(0, lbH/2, vp.Width, lbH*2), Label.Fit.AlignCenter);
        }

        protected override void OnTransparencyChange()
        {
            if (!firstLoad || IsClosing)
                background.OnTransparencyChange(transparency);
            loadBar.OnTransparencyChange(transparency);
            title.OnTransparencyChange(transparency);
        }

        public override void ConnectControls() { }

        public override void DisconnectControls() { }

        public override void Update(GameTime time)
        {
            float progress = builder.Progress;
            loadBar.SetProgress(progress);
            loadBar.SetText(builder.Task);

            if (progress == 1.0f)
            {
                if (firstLoad)
                    manager.ChangeScreen(new MenuScreen(manager));
                else
                    manager.ChangeScreen(manager.WorldScreen);
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            background.Draw(sb);
            loadBar.Draw(sb);
            title.Draw(sb);
        }
    }
}
