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

namespace Sim
{
    /// <summary>
    /// Estimates frames drawn per second
    /// </summary>
    public class FPSCounter
    {
        private float fps;          // estimated frames (updates) per second
        private float time;         // amount of time passed since last fps calculation
        private float frames;       // number of frames counted since last fps calculation
        private Vector2 position;   // position to draw the FPS count at

        public bool Visible = false;

        public FPSCounter(Viewport vp)
        {
            position = new Vector2(vp.Width - 100, 0);
        }

        public void Update(GameTime gameTime)
        {
            if (Visible)
            {
                time += (float)gameTime.ElapsedRealTime.TotalSeconds;
                frames++;

                if (time >= 1.0f)
                {
                    time -= (int)time;
                    fps = frames;
                    frames = 0;
                }
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (Visible)
            {
                sb.DrawString(Fonts.Tahoma, "FPS: " + fps, position + Vector2.One, Color.Black);
                sb.DrawString(Fonts.Tahoma, "FPS: " + fps, position, Color.White);
            }
        }
    }
}
