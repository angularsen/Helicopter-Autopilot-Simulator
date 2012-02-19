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
    class Checkbox : Control
    {
        public event ButtonHandler StateChanged;
        private Boolean state = false;
        private Boolean down = false;

        private Vector2 labelPos;

        private Texture2D texture = UserInterface.TexBar;
        private Texture2D checkTexture = UserInterface.TexKnob;
        private Color color = Color.White;

        public Boolean State { get { return state; } }

        public Checkbox(String name, Rectangle area)
            : base(name, area)
        {
            labelPos = new Vector2(area.Right, area.Y);
        }

        public void SetState(Boolean state)
        {
            this.state = state;
            if (StateChanged != null)
                StateChanged();
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            sb.Draw(texture, area, color);
            if (state)
                sb.Draw(checkTexture, area, color);
            sb.DrawString(Fonts.Arial, name, labelPos, color);
        }

        #region Event Logic

        public override void OnTransparencyChange(float totalTransparency)
        {
            byte a = (byte)(255 * totalTransparency * transparency);
            color = new Color(color.R, color.G, color.B, a);
        }

        protected override void OnAreaChange() { }

        protected override void OnMouseLeftPress(UIMouse mouse)
        {
            if (hovering)
                down = true;
        }

        protected override void OnMouseLeftRelease(UIMouse mouse)
        {
            if (down && hovering)
                SetState(!state);
            down = false;
        }

        protected override void OnMouseRightPress(UIMouse mouse) { }

        protected override void OnMouseRightRelease(UIMouse mouse) { }

        #endregion
    }
}
