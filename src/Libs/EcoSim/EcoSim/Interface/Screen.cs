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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sim.Interface;

namespace Sim.Interface
{
    /// <summary>
    /// A screen is a grouping of related controls: buttons, sliders, panels, etc.
    /// </summary>
    public abstract class Screen
    {
        protected ScreenManager manager;        // controls the screen's updating and drawing
        protected float transparency = 0.0f;    // master transparency of the screen

        private Boolean closing;                // determines if the screen is fading out
        private Boolean opening;                // determines if the screen is fading in
        private Boolean fullyOpen;              // screen is completely transitioned in
        private float transitionPeriod = 350f;  // how long it takes for the screen to fade in or out (in ms)
        private float transitionTimer = 0f;     // timer for fading screen in and out

        public ScreenManager Manager { get { return manager; } }
        public Boolean IsOpening { get { return opening; } }
        public Boolean IsClosing { get { return closing; } }
        public Boolean IsFullyOpen { get { return fullyOpen; } }

        public Screen(ScreenManager screenManager)
        {
            this.manager = screenManager;
        }

        public virtual void Initialize() { }

        public virtual void LoadContent() { }

        /// <summary>
        /// Updates the status of the screen (and its transparency) while it fades in or out
        /// </summary>
        public void Transition(GameTime time)
        {
            transitionTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
            float percentTransitioned = MathHelper.Clamp(transitionTimer / transitionPeriod, 0.0f, 1.0f);

            // update the screen's overall transparency
            transparency = opening ? percentTransitioned : 1 - percentTransitioned;

            // update the screen's components' transparencies
            OnTransparencyChange();

            // check if the transition is finished
            if (percentTransitioned == 1)
            {
                transitionTimer = 0f;
                if (opening)
                {
                    fullyOpen = true;
                    ConnectControls();
                }
                else
                    fullyOpen = false;

                closing = false;
                opening = false;
            }
        }

        /// <summary>
        /// Tells the screen to start fading in
        /// </summary>
        public void StartOpening()
        {
            closing = false;
            opening = true;
        }

        /// <summary>
        /// Tells the screen to start fading out
        /// </summary>
        public void StartClosing()
        {
            fullyOpen = false;
            closing = true;
            opening = false;
            DisconnectControls();
        }

        /// <summary>
        /// Tells the screen to instantly open (no transition)
        /// </summary>
        public void OpenImmediately()
        {
            fullyOpen = true;
            closing = false;
            opening = false;
            ConnectControls();
        }

        /// <summary>
        /// Tells the screen to instantly close (no transition)
        /// </summary>
        public void CloseImmediately()
        {
            fullyOpen = false;
            closing = false;
            opening = false;
            DisconnectControls();
        }

        /// <summary>
        /// Called when the screen manager transitions the screen so the screen may properly update
        /// its controls' transparencies
        /// </summary>
        protected abstract void OnTransparencyChange();

        /// <summary>
        /// Connects the screen's controls to input devices
        /// </summary>
        public abstract void ConnectControls();

        /// <summary>
        /// Disconnects screen controls from input devices
        /// </summary>
        public abstract void DisconnectControls();

        /// <summary>
        /// Updates the screen's dynamic components
        /// </summary>
        public abstract void Update(GameTime time);

        /// <summary>
        /// Renders the screen
        /// </summary>
        public abstract void Draw(SpriteBatch sb);
    }
}
