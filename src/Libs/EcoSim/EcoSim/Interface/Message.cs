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
    public class Message
    {
        private Color color;
        private String message;     // the message contents
        private float runTime;      // how long the message has been displayed
        private float duration;     // how long the message should be displayed
        private float fadeTime;     // the time at which the message starts to fade out
        private Boolean expired;    // if the message has lasted its duration

        private float b;    // y-intercept of alpha change
        private float m;    // slope of alpha change

        /// <summary>
        /// Message's alpha value determined by how long it has been displayed
        /// </summary>
        public byte Alpha
        {
            get
            {
                if (runTime > fadeTime)
                {
                    if (runTime >= duration)
                        return 0;
                    return (byte)(255 * (m * runTime + b));
                }
                else
                    return 255;
            }
        }
        public Boolean Expired { get { return expired; } }
        public String MessageString { get { return message; } }
        public Color Color { get { return new Color(color.R, color.G, color.B, Alpha); } }

        public Message(String message, float duration, float solidDurationPercent, Color color)
        {
            this.message = message;
            this.duration = duration;
            this.color = color;

            float delayPeriod = (1 - solidDurationPercent) * duration;
            fadeTime = duration - delayPeriod;
            b = duration / delayPeriod;
            m = -1 / delayPeriod;
        }

        public void Update(GameTime gameTime)
        {
            if (!expired)
            {
                runTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (runTime >= duration)
                    expired = true;
            }
        }
    }
}
