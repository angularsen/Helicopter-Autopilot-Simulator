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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Sim.Interface
{
    public delegate void MouseHandler(UIMouse mouse);

    /// <summary>
    /// Handles mouse input
    /// </summary>
    public class UIMouse
    {
        // cursor
        private Color cursorColor = Color.White;

        // events
        public event MouseHandler LPress;       // left button pressed
        public event MouseHandler RPress;       // right button pressed
        public event MouseHandler LRelease;     // left button released
        public event MouseHandler RRelease;     // right button released
        public event MouseHandler Move;         // mouse cursor moved
        public event MouseHandler Drag;         // mouse is dragging

        // states
        private Boolean dragging;           // mouse is dragging
        private MouseState curState;        // state of the mouse during this update
        private MouseState oldState;        // state of the mouse during previous update
        private Vector2 screenPositionV;    // screen location of the mouse cursor (for methods that use vector2s)
        private Point screenPositionP;      // screen location of the mouse cursor (for methods that use points)
        private Point dragPoint;            // screen location where dragging started
        private Vector2 lastMove;           // amount the mouse moved between updates

        // properties
        public MouseState State { get { return curState; } }
        public Vector2 ScreenPositionV { get { return screenPositionV; } }
        public Point ScreenPositionP { get { return screenPositionP; } }
        public Vector2 LastMove { get { return lastMove; } }
        public Boolean Dragging { get { return dragging; } }
        public Point DragPoint { get { return dragPoint; } }

        public void Update()
        {
            // update the mouse state
            oldState = curState;
            curState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            
            // record the mouse's new location
            screenPositionV = new Vector2(curState.X, curState.Y);
            screenPositionP = new Point(curState.X, curState.Y);
            lastMove = new Vector2(curState.X - oldState.X, curState.Y - oldState.Y);

            // check states
            bool lPress = curState.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released;
            bool lRelease = curState.LeftButton == ButtonState.Released && oldState.LeftButton == ButtonState.Pressed;
            bool rPress = curState.RightButton == ButtonState.Pressed && oldState.RightButton == ButtonState.Released;
            bool rRelease = curState.RightButton == ButtonState.Released && oldState.RightButton == ButtonState.Pressed;
            
            bool moved = lastMove != Vector2.Zero;
            if (dragging)
                dragging = !lRelease;
            else
            {
                dragging = moved && curState.LeftButton == ButtonState.Pressed;
                dragPoint = screenPositionP;
            }

            // invoke event handlers
            if (lPress && LPress != null) LPress(this);
            if (lRelease && LRelease != null) LRelease(this);
            if (rPress && RPress != null) RPress(this);
            if (rRelease && RRelease != null) RRelease(this);
            if (moved && Move != null) Move(this);
            if (dragging && Drag != null) Drag(this);
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            sb.Draw(UserInterface.TexCursor, screenPositionV, cursorColor);
        }
    }
}
