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
    public delegate void SliderHandler(float value);

    class Slider : Control
    {
        private Boolean dragging;
        public event SliderHandler Use;

        // labels
        private Label nameLabel;
        private Label valueLabel;

        // bar
        private Rectangle bar;
        private Color barColor = Color.Gray;

        // knob
        private Rectangle knob;
        private Color knobColor = new Color(130,145,177);

        // value
        private float minValue;
        private float maxValue;
        private float currentValue;
        private String valueString = "null";

        public float Value { get { return currentValue; } }
        
        public Slider(String name, Rectangle area, float minValue, float maxValue)
            :base (name, area)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;

            Setup();
        }

        private void Setup()
        {
            Rectangle labelArea = new Rectangle(area.X, area.Y, (int)(0.3f * area.Width), area.Height);
            Rectangle barArea = new Rectangle(labelArea.Right, area.Y, (int)(0.5f * area.Width), area.Height);
            Rectangle valueArea = new Rectangle(barArea.Right, area.Y, area.Width - labelArea.Width - 
                barArea.Width, area.Height);

            nameLabel = new Label(name, labelArea, Label.Fit.AlignLeft);
            nameLabel.TextColor = Color.LightGray;
            valueLabel = new Label(valueString, valueArea, Label.Fit.AlignCenter);
            valueLabel.TextColor = Color.LightGray;

            // calculate the bar's positioning
            int barHeight = 4;
            bar = new Rectangle(barArea.X, barArea.Y + (barArea.Height - barHeight) / 2, barArea.Width, barHeight);

            // calculate the knob's area
            int knobHeight = (int)(area.Height * 0.8f);
            knob = new Rectangle(bar.Left, bar.Y + (bar.Height - knobHeight) / 2, knobHeight, knobHeight);
            CenterKnob(0);
        }

        private void CenterKnob(int xPosition)
        {
            // try centering the knob at xPosition, but keep it on the bar
            float offset = knob.Width / 2.0f;
            float newKnobX = MathHelper.Clamp(xPosition - offset, bar.Left - offset, bar.Right - offset);
            knob = new Rectangle((int)newKnobX, knob.Y, knob.Width, knob.Height);

            // update the current value of the slider
            currentValue = (newKnobX + offset - bar.Left) / bar.Width * (maxValue - minValue) + minValue;
            valueString = String.Format("{0:F}", currentValue);

            if (Use != null)
                Use(currentValue);
        }

        public void SetValue(float value)
        {
            float percent = MathHelper.Clamp((value - minValue) / (maxValue - minValue), 0, 1);
            knob.X = (int)(bar.Left + percent * bar.Width - knob.Width / 2.0f);
            currentValue = value;
            valueString = String.Format("{0:F}", currentValue);
            if (Use != null)
                Use(value);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (active)
            {
                Color textColor = dragging ? Color.White : Color.LightGray;
                nameLabel.TextColor = textColor;
                valueLabel.TextColor = textColor;
            }

            sb.Draw(UserInterface.TexBar, bar, barColor);
            sb.Draw(UserInterface.TexKnob, knob, knobColor);
            nameLabel.Draw(sb);
            valueLabel.Text = valueString;
            
            valueLabel.Draw(sb);
        }

        #region Event Logic

        public override void Unregister(UIMouse mouse)
        {
            base.Unregister(mouse);
            dragging = false;
        }

        public override void OnTransparencyChange(float totalTransparency)
        {
            byte a = (byte)(255 * totalTransparency * transparency);
            barColor = new Color(barColor.R, barColor.G, barColor.B, a);
            knobColor = new Color(knobColor.R, knobColor.G, knobColor.B, a);

            nameLabel.OnTransparencyChange(totalTransparency);
            valueLabel.OnTransparencyChange(totalTransparency);
        }

        protected override void OnAreaChange() { }

        protected override void OnMouseLeftPress(UIMouse mouse)
        {
            if (knob.Contains(mouse.ScreenPositionP))
                dragging = true;
        }

        protected override void OnMouseLeftRelease(UIMouse mouse)
        {
            if (dragging)
                dragging = false;
        }

        protected override void OnMouseRightPress(UIMouse mouse) { }

        protected override void OnMouseRightRelease(UIMouse mouse) { }

        protected override void OnMouseMove(UIMouse mouse)
        {
            base.OnMouseMove(mouse);

            if (dragging)
                CenterKnob(mouse.ScreenPositionP.X);
        }

        #endregion
    }
}
