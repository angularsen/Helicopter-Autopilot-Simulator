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
    public class ProgressBar : Control
    {
        private Panel barPanel;     // unfilled portion of the bar
        private Panel fillPanel;    // filled portion of the bar
        private Label label;        // progress label

        private float progress;

        public ProgressBar(String name, Rectangle area)
            : base(name, area)
        {
            barPanel = new Panel(String.Empty, area);
            barPanel.Texture = UserInterface.TexBar;
            barPanel.Color = Color.White;

            fillPanel = new Panel(String.Empty, new Rectangle(area.X, area.Y, 0, area.Height));
            fillPanel.Texture = UserInterface.TexBlank;
            fillPanel.Color = new Color(173, 216, 230, 100);

            label = new Label(name, area, Label.Fit.AlignCenter);
        }

        public void SetProgress(float percent)
        {
            progress = MathHelper.Clamp(percent, 0, 1);
            Rectangle newArea = fillPanel.Area;
            newArea.Width = (int)(progress * area.Width);
            fillPanel.Area = newArea;
        }

        public void AddProgress(float percent)
        {
            progress = MathHelper.Clamp(progress + percent, 0, 1);
            Rectangle newArea = fillPanel.Area;
            newArea.Width = (int)(progress * area.Width);
            fillPanel.Area = newArea;
        }

        public void SetText(String text)
        {
            label.SetText(text, Label.Fit.AlignCenter);
        }

        public override void Draw(SpriteBatch sb)
        {
            barPanel.Draw(sb);
            fillPanel.Draw(sb);
            label.Draw(sb);
        }

        #region Event Logic

        public override void OnTransparencyChange(float totalTransparency)
        {
            barPanel.OnTransparencyChange(totalTransparency);
            fillPanel.OnTransparencyChange(totalTransparency);
            label.OnTransparencyChange(totalTransparency);
        }

        protected override void OnAreaChange() { }

        protected override void OnMouseLeftPress(UIMouse mouse) { }

        protected override void OnMouseLeftRelease(UIMouse mouse) { }

        protected override void OnMouseRightPress(UIMouse mouse) { }

        protected override void OnMouseRightRelease(UIMouse mouse) { }

        #endregion
    }
}
