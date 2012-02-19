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
    /// <summary>
    /// A panel is a textured/colored area
    /// </summary>
    class Panel : Control
    {
        private Texture2D texture = UserInterface.TexBlank;
        private Color color = Sim.Settings.Colors.Default.PanelBG;

        public Texture2D Texture { get { return texture; } set { texture = value; } }
        public Color Color { get { return color; } set { color = value; transparency = color.A/255.0f; } }

        public Panel(String name, Rectangle area, Color color) : base(name, area) 
        {
            Color = color;
        }

        public Panel(String name, Rectangle area) : base(name, area) { }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(texture, area, color);
        }

        #region Event Logic

        public override void OnTransparencyChange(float totalTransparency)
        {
            byte a = (byte)(255 * totalTransparency * transparency);
            color = new Color(color.R, color.G, color.B, a);
        }

        protected override void OnAreaChange() { }

        protected override void OnMouseLeftPress(UIMouse mouse) { }

        protected override void OnMouseLeftRelease(UIMouse mouse) { }

        protected override void OnMouseRightPress(UIMouse mouse) { }

        protected override void OnMouseRightRelease(UIMouse mouse) { }

        #endregion
    }
}
