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

namespace Sim.Interface
{
    public class MenuScreen : Screen
    {
        private ControlGroup menu;

        public MenuScreen(ScreenManager manager)
            : base(manager)
        {
            Initialize();
        }

        public override void Initialize()
        {
            Viewport vp = manager.UI.GraphicsDevice.Viewport;
            
            menu = new ControlGroup();

            int mW = vp.Width / 4;
            int mH = vp.Height / 3;
            Rectangle mArea = new Rectangle((vp.Width - mW) / 2, (vp.Height - mH) / 2, mW, mH);

            ControlMat mat = new ControlMat(mArea, 5, 1, (int)(0.0125f * vp.Height));

            menu.Add(new Button("New", mat.Spaces[1],
                delegate() { manager.ChangeScreen(new SetupScreen(manager)); }));
            //menu.Add(new Button("Settings", mat.Spaces[1],
            //    delegate() { manager.ChangeScreen(new SettingsScreen(manager)); }));
            menu.Add(new Button("Return", mat.Spaces[2],
                delegate() { manager.ChangeScreen(manager.WorldScreen); }));
            menu.Add(new Button("Exit", mat.Spaces[3],
                delegate() { manager.UI.Game.Exit(); }));
        }

        public override void ConnectControls()
        {
            menu.Connect(manager.UI);
        }

        public override void DisconnectControls()
        {
            menu.Disconnect(manager.UI);
        }

        protected override void OnTransparencyChange()
        {
            menu.OnTransparencyChange(transparency);
        }

        public override void Update(GameTime time)
        {

        }

        public override void Draw(SpriteBatch sb)
        {
            menu.Draw(sb);
        }
    }
}
