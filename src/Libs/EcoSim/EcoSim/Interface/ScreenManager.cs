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
using Microsoft.Xna.Framework.Input;

namespace Sim.Interface
{
    /// <summary>
    /// Manages a list of screens
    /// </summary>
    public class ScreenManager
    {
        private UserInterface ui;                               // reference to the UI the manager belongs to
        private List<Screen> openScreens = new List<Screen>();  // screens currently being displayed
        private WorldScreen worldScreen;     // the world screen is too complex to repeately recreate 


        public UserInterface UI { get { return ui; } }
        public WorldScreen WorldScreen { get { return worldScreen; } }

        public ScreenManager(UserInterface ui)
        {
            this.ui = ui;
            worldScreen = new WorldScreen(this);
        }

        public void LoadContent(ContentManager cm)
        {
            foreach (Screen s in openScreens)
                s.LoadContent();
            worldScreen.LoadContent();
        }

        public void Update(GameTime time)
        {
            for (int i = 0; i < openScreens.Count; i++)
            {
                Screen screen = openScreens[i];
                if (screen.IsFullyOpen)
                    screen.Update(time);
                else if (screen.IsOpening || screen.IsClosing)
                    screen.Transition(time);
                else
                {
                    openScreens.Remove(screen);
                    i--;
                }
            }
        }

        /// <summary>
        /// Draws open screens
        /// </summary>
        public void Draw(SpriteBatch sb)
        {
            foreach (Screen s in openScreens)
                s.Draw(sb);
        }

        /// <summary>
        /// Opens a new screen and closes the current one
        /// </summary>
        public void ChangeScreen(Screen screenToOpen)
        {
            if (openScreens.Count > 0)
            {
                // last screen is the one on top
                Screen screenToClose = openScreens[openScreens.Count - 1];
                screenToClose.StartClosing();
            }

            screenToOpen.StartOpening();
            openScreens.Add(screenToOpen);
        }
    }
}
