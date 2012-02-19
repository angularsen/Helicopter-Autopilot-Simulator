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
    /// A collection of controls
    /// </summary>
    public class ControlGroup
    {
        private List<Control> controls = new List<Control>();
        private Boolean enabled = true;

        public Boolean Enabled { get { return enabled; } }

        public void Add(Control c)
        {
            controls.Add(c);
        }

        public void Clear()
        {
            controls.Clear();
        }

        /// <summary>
        /// Adjusts the alpha of the controls in the group
        /// </summary>
        public void OnTransparencyChange(float totalTransparency)
        {
            foreach (Control c in controls)
                c.OnTransparencyChange(totalTransparency);
        }

        /// <summary>
        /// Connects the controls to the ui
        /// </summary>
        public void Connect(UserInterface ui)
        {
            foreach (Control c in controls)
                c.Register(ui.Mouse);
        }

        /// <summary>
        /// Disconnects the controls from the ui
        /// </summary>
        public void Disconnect(UserInterface ui)
        {
            foreach (Control c in controls)
                c.Unregister(ui.Mouse);
        }

        public void Enable(UserInterface ui)
        {
            enabled = true;
            Connect(ui);
        }

        public void Disable(UserInterface ui)
        {
            enabled = false;
            Disconnect(ui);
        }

        public void ToggleEnabled(UserInterface ui)
        {
            enabled = !enabled;
            if (enabled)
                Connect(ui);
            else
                Disconnect(ui);
        }

        public void Draw(SpriteBatch sb)
        {
            if (enabled)
            {
                foreach (Control c in controls)
                    c.Draw(sb);
            }
        }
    }
}
