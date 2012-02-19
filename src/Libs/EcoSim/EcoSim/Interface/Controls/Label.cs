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
    class Label : Control
    {
        private SpriteFont font = Fonts.Arial;
        private Color textColor = Colors.Default.Text;
        private Color shadowColor = Colors.Default.TextShadow;
        private Vector2 position;
        private Vector2 scale;

        private Color bgColor = Colors.Default.PanelBG;
        public bool DrawBackground = false;     // background should be drawn
        private float bgTransparency;           // transparency of background

        public String Text { get { return name; } set { name = value; } }
        public SpriteFont Font { get { return font; } set { font = value; } }
        public Color TextColor { get { return textColor; } set { textColor = value; } }
        public Color ShadowColor { get { return shadowColor; } set { shadowColor = value; } }

        public enum Fit
        {
            AlignLeft,
            AlignCenter,
            AlignRight,
            Stretch
        }

        /// <summary>
        /// Creates a text label to fit within a specified area
        /// </summary>
        public Label(String name, Rectangle area, Fit fit)
            : base(name, area)
        {
            SetText(name, fit);
            bgTransparency = bgColor.A / 255.0f;
        }

        public void SetText(String text, Fit fit)
        {
            this.name = text;

            switch (fit)
            {
                case Fit.AlignLeft:
                    AlignLeft();
                    break;
                case Fit.AlignCenter:
                    CenterLabel();
                    break;
                case Fit.AlignRight:
                    AlignRight();
                    break;
                case Fit.Stretch:
                    StretchLabel();
                    break;
            }
        }

        private void AlignRight()
        {
            Vector2 labelDimensions = font.MeasureString(name);
            scale = Vector2.One;
            position = new Vector2(area.Right - labelDimensions.X, area.Y + (area.Height - labelDimensions.Y) / 2);
        }

        private void AlignLeft()
        {
            Vector2 labelDimensions = font.MeasureString(name);
            scale = Vector2.One;
            position = new Vector2(area.X, area.Y + (area.Height - labelDimensions.Y) / 2);
        }

        private void CenterLabel()
        {
            Vector2 labelDimensions = font.MeasureString(name);
            scale = Vector2.One;
            position = new Vector2(area.X + (area.Width - labelDimensions.X) / 2, 
                area.Y + (area.Height - labelDimensions.Y) / 2);
        }

        private void StretchLabel()
        {
            Vector2 labelDimensions = font.MeasureString(name);
            scale.X = area.Width / labelDimensions.X;
            scale.Y = area.Height / labelDimensions.Y;
            position = new Vector2(area.X, area.Y);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (DrawBackground)
                sb.Draw(UserInterface.TexBlank, area, bgColor);
            sb.DrawString(font, name, position + Vector2.One, shadowColor, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            sb.DrawString(font, name, position, textColor, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        #region Event Logic

        public override void OnTransparencyChange(float totalTransparency) 
        {
            byte a = (byte)(255 * totalTransparency * transparency);
            textColor = new Color(textColor.R, textColor.G, textColor.B, a);
            shadowColor = new Color(shadowColor.R, shadowColor.G, shadowColor.B, a);
            bgColor = new Color(bgColor.R, bgColor.G, bgColor.B, (byte)(a * bgTransparency));
        }

        protected override void OnAreaChange() { }

        protected override void OnMouseLeftPress(UIMouse mouse) { }

        protected override void OnMouseLeftRelease(UIMouse mouse) { }

        protected override void OnMouseRightPress(UIMouse mouse) { }

        protected override void OnMouseRightRelease(UIMouse mouse) { }

        #endregion
    }
}
