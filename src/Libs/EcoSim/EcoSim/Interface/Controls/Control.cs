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
using Sim.Interface;

namespace Sim.Interface
{
    public abstract class Control
    {
        protected Rectangle area;               // area of the screen the control occupies
        protected String name;                  // name of the control
        protected float transparency = 1.0f;    // transparency when fully active
        protected Boolean hovering;             // the mouse is over the control
        protected Boolean active;               // control is connected to the input devices

        protected MouseHandler LPress;      // event handler for when the mouse's left button is pressed
        protected MouseHandler LRelease;    // event handler for when the mouse's left button is released
        protected MouseHandler RPress;      // event handler for when the mouse's right button is pressed
        protected MouseHandler RRelease;    // event handler for when the mouse's right button is released
        protected MouseHandler Move;        // event handler for when the mouse is moved

        public float Transparency { get { return transparency; } }
        public Rectangle Area { get { return area; } set { area = value; OnAreaChange(); } }
        
        public Control(String name, Rectangle area)
        {
            this.name = name;
            Area = area;

            LPress += new MouseHandler(OnMouseLeftPress);
            LRelease += new MouseHandler(OnMouseLeftRelease);
            RPress += new MouseHandler(OnMouseRightPress);
            RRelease += new MouseHandler(OnMouseRightRelease);
            Move += new MouseHandler(OnMouseMove);
        }
        
        /// <summary>
        /// Connects the component's event handlers to the mouse
        /// </summary>
        public virtual void Register(UIMouse mouse)
        {
            mouse.LPress += LPress;
            mouse.LRelease += LRelease;
            mouse.RPress += RPress;
            mouse.RRelease += RRelease;
            mouse.Move += Move;
            active = true;
            OnMouseMove(mouse);
        }

        /// <summary>
        /// Disconnects the component's event handlers from the mouse
        /// </summary>
        public virtual void Unregister(UIMouse mouse)
        {
            mouse.LPress -= LPress;
            mouse.LRelease -= LRelease;
            mouse.RPress -= RPress;
            mouse.RRelease -= RRelease;
            mouse.Move -= Move;
            hovering = false;
            active = false;
        }

        public abstract void Draw(SpriteBatch sb);

        /// <summary>
        /// Screen the control is part of has changed transparency
        /// </summary>
        public abstract void OnTransparencyChange(float totalTransparency);

        /// <summary>
        /// The control's area has been modified
        /// </summary>
        protected abstract void OnAreaChange();

        protected abstract void OnMouseLeftPress(UIMouse mouse);
        protected abstract void OnMouseLeftRelease(UIMouse mouse);
        protected abstract void OnMouseRightPress(UIMouse mouse);
        protected abstract void OnMouseRightRelease(UIMouse mouse);

        protected virtual void OnMouseMove(UIMouse mouse) 
        {
            if (area.Contains(mouse.ScreenPositionP))
                hovering = true;
            else
                hovering = false;
        }
    }
}
