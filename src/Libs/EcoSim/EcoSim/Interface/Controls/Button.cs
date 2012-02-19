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
using Sim.Settings;

namespace Sim.Interface
{
    public delegate void ButtonHandler();

    class Button : Control
    {
        public event ButtonHandler Use;     // handler for when the button is used

        protected Boolean down;                                     // the button is being pressed down
        protected Color colorUp = Colors.Default.ButtonUp;          // color when up
        protected Color colorDown = Colors.Default.ButtonDown;      // color when pressed
        protected Color colorHover = Colors.Default.ButtonHover;    // color when the mouse is over
        protected Texture2D texL = UserInterface.TexButtonL;
        protected Texture2D texM = UserInterface.TexButtonM;
        protected Texture2D texR = UserInterface.TexButtonR;
        protected Rectangle texLArea;
        protected Rectangle texMArea;
        protected Rectangle texRArea;
        
        protected Label label;  // button name label

        public Button(String name, Rectangle area)
            : base(name, area)
        {
            label = new Label(name, area, Label.Fit.AlignCenter);
            label.TextColor = Color.LightGray;
        }

        public Button(String name, Rectangle area, ButtonHandler onUse)
            : this(name,area)
        {
            Use += onUse;
        }

        protected void OnUse()
        {
            if (Use != null)
                Use();
        }

        public override void Draw(SpriteBatch sb)
        {
            if (hovering)
            {
                Color drawColor = down ? colorDown : colorHover;
                DrawThreeParts(sb, drawColor);
            }
            else
                DrawThreeParts(sb, colorUp);

            if (active) 
                label.TextColor = hovering ? Color.White : Color.LightGray;
            label.Draw(sb);
        }

        protected void DrawThreeParts(SpriteBatch sb, Color color)
        {
            sb.Draw(texL, texLArea, color);
            sb.Draw(texM, texMArea, color);
            sb.Draw(texR, texRArea, color);
        }

        #region Event Logic

        public override void Register(UIMouse mouse)
        {
            base.Register(mouse);
            label.Register(mouse);
        }

        public override void Unregister(UIMouse mouse)
        {
            base.Unregister(mouse);
            label.Unregister(mouse);
        }

        public override void OnTransparencyChange(float totalTransparency)
        {
            float controlTransparency = totalTransparency * transparency;
            byte a = (byte)(255 * controlTransparency);
            colorUp = new Color(colorUp.R, colorUp.G, colorUp.B, a);
            colorDown = new Color(colorDown.R, colorDown.G, colorDown.B, a);
            colorHover = new Color(colorHover.R, colorHover.G, colorHover.B, a);

            label.OnTransparencyChange(controlTransparency);
        }

        protected override void OnAreaChange()
        {
            int lWidth = texL.Width / 2;
            int rWidth = texR.Width / 2;
            if (lWidth + rWidth > area.Width)
            {
                lWidth = area.Width / 2;
                rWidth = area.Width - lWidth;
            }
                
            texLArea = new Rectangle(area.X, area.Y, lWidth, area.Height);
            texRArea = new Rectangle(area.Right - rWidth, area.Y, rWidth, area.Height);
            texMArea = new Rectangle(texLArea.Right, area.Y, area.Width - lWidth - rWidth, area.Height);
        }

        protected override void OnMouseLeftPress(UIMouse mouse)
        {
            if (hovering)
                down = true;
        }

        protected override void OnMouseLeftRelease(UIMouse mouse)
        {
            if (down && hovering)
                OnUse();
            down = false;
        }

        protected override void OnMouseRightPress(UIMouse mouse) { }

        protected override void OnMouseRightRelease(UIMouse mouse) { }

        #endregion
    }
}
