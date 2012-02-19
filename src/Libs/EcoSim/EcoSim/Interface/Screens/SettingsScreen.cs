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
    public class SettingsScreen : Screen
    {
        private ControlGroup settings;

        public SettingsScreen(ScreenManager manager)
            : base(manager)
        {
            Initialize();
        }

        public override void Initialize()
        {
            Viewport vp = manager.UI.GraphicsDevice.Viewport;

            settings = new ControlGroup();

            int pad = (int)(0.05f * vp.Width);
            Rectangle pArea = new Rectangle(pad, (int)(0.15f * vp.Height), vp.Width - 2 * pad, (int)(0.35f * vp.Height));

            settings.Add(new Panel("Main Panel", pArea, new Color(0, 0, 0, 100)));
            settings.Add(new Label("Graphics Settings", new Rectangle(pArea.X, pArea.Y, pArea.Width, (int)Fonts.Arial.MeasureString("Graphics Settings").Y),
                Label.Fit.AlignCenter));

            ControlMat mat = new ControlMat(pArea, 4, 3, 5);


            Checkbox drawDetailBox = new Checkbox("Terrain Detail", new Rectangle(0, 0, 50, 50));
            drawDetailBox.SetState(Sim.Settings.Graphics.Default.TerrainDetail);
            drawDetailBox.StateChanged += delegate() { Sim.Settings.Graphics.Default.TerrainDetail = drawDetailBox.State; };
            settings.Add(drawDetailBox);

            int bHeight = (int)(0.05f * vp.Height);
            int bWidth = (int)(0.125f * vp.Width);
            Button backButton = new Button("Back", new Rectangle(pad, vp.Height - bHeight - pad, bWidth,
                bHeight), delegate() { manager.ChangeScreen(new MenuScreen(manager)); });
            settings.Add(backButton);

            Button applyButton = new Button("Apply", new Rectangle(vp.Width - bWidth - pad, backButton.Area.Y,
                bWidth, bHeight), delegate() { });
            settings.Add(applyButton);
        }

        public override void ConnectControls() { settings.Connect(manager.UI); }

        public override void DisconnectControls() { settings.Disconnect(manager.UI); }

        protected override void OnTransparencyChange() { settings.OnTransparencyChange(transparency); }

        public override void Update(GameTime time) { }

        public override void Draw(SpriteBatch sb)
        {
            settings.Draw(sb);
        }
    }
}
