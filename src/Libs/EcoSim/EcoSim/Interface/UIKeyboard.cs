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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Sim.Interface
{
    /// <summary>
    /// Handles keyboard input
    /// </summary>
    public class UIKeyboard
    {
        private KeyboardState currentState;
        private KeyboardState oldState;
        private bool shiftDown;

        private bool keyHeld;
        private float keyHoldTimer = 0.0f;      
        private float keyHoldWait = 500;        // ms to wait until keyheld is set
        private Keys lastKeyHeld;
        private Keys[] previouslyPressed;
        private Keys[] currentlyPressed;

        public KeyboardState State { get { return currentState; } }
        public bool ShiftDown { get { return ShiftDown; } }

        public void Update(GameTime time)
        {
            oldState = currentState;
            currentState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            shiftDown = currentState.IsKeyDown(Keys.LeftShift) || currentState.IsKeyDown(Keys.RightShift);

            currentlyPressed = currentState.GetPressedKeys();
            previouslyPressed = oldState.GetPressedKeys();

            if (currentlyPressed.Length != previouslyPressed.Length)
            {
                keyHoldTimer = 0.0f;
                keyHeld = false;
                lastKeyHeld = FindLastKeyPressed();
            }

            if (!keyHeld && currentlyPressed.Length > 0)
            {
                keyHoldTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
                if (keyHoldTimer > keyHoldWait)
                {
                    keyHeld = true;
                }
            }
        }

        private Keys FindLastKeyPressed()
        {
            for (int i = 0; i < currentlyPressed.Length; i++)
            {
                Keys current = currentlyPressed[i];
                bool contained = false;

                for (int j = 0; j < previouslyPressed.Length; j++)
                {
                    if (current == previouslyPressed[j])
                    {
                        contained = true;
                        continue;
                    }
                }
                if (!contained)
                    return current;
            }
            return lastKeyHeld;
        }

        /// <summary>
        /// Key was previously up and now down
        /// </summary>
        public Boolean KeyPushed(Keys key)
        {
            return currentState.IsKeyDown(key) && !oldState.IsKeyDown(key);
        }

        /// <summary>
        /// Key is being typed (held or pushed)
        /// </summary>
        public bool KeyTyped(Keys key)
        {
            return (keyHeld && key == lastKeyHeld) || KeyPushed(key);
        }

        /// <summary>
        /// Converts an XNA key to a spritefont valid character
        /// </summary>
        public char? KeyToChar(Keys k)
        {
            byte keyByte = (byte)k;

            if (keyByte > 64 && keyByte < 91)   // a letter
                return shiftDown ? (char)k : char.ToLower((char)k);

            if (keyByte > 47 && keyByte < 58)
            {
                switch ((char)k)
                {
                    case '0': return shiftDown ? ')' : '0';
                    case '1': return shiftDown ? '!' : '1';
                    case '2': return shiftDown ? '@' : '2';
                    case '3': return shiftDown ? '#' : '3';
                    case '4': return shiftDown ? '$' : '4';
                    case '5': return shiftDown ? '%' : '5';
                    case '6': return shiftDown ? '^' : '6';
                    case '7': return shiftDown ? '&' : '7';
                    case '8': return shiftDown ? '*' : '8';
                    case '9': return shiftDown ? '(' : '9';
                }
            }

            switch (k)
            {
                case Keys.Space: return ' ';
                case Keys.Decimal: return '.';
                case Keys.Add: return '+';
                case Keys.Subtract: return '-';
                case Keys.OemSemicolon: return shiftDown ? ':' : ';';
                case Keys.OemQuotes: return shiftDown ? '"' : '\'';
                case Keys.OemPeriod: return shiftDown ? '>' : '.';
                case Keys.OemComma: return shiftDown ? '<' : ',';
                case Keys.OemQuestion: return shiftDown ? '?' : '/';
                case Keys.OemPipe: return shiftDown ? '|' : '\\';
                case Keys.OemPlus: return shiftDown ? '+' : '=';
                case Keys.OemMinus: return shiftDown ? '_' : '-';
                case Keys.OemOpenBrackets: return shiftDown ? '{' : '[';
                case Keys.OemCloseBrackets: return shiftDown ? '}' : ']';
                case Keys.OemTilde: return shiftDown ? '~' : '`';
                default: return null;
            };
        }
    }
}
