#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;

#endregion

namespace Simulator
{
    /// <summary>
    /// This is a wrapper for XNA's keyboard state that generates events
    /// such as KeyPressed and KeyReleased for cleaner code where applicable.
    /// It takes care of the code that remembers if keys were pressed the previous 
    /// game loop iteration.
    /// </summary>
    public class KeyboardEvents : GameComponent
    {
        public event Action<Keys> KeyPressed;
        public event Action<Keys> KeyReleased;

        private readonly Dictionary<Keys, bool> _prevPressedKeys;
        private Keys[] _prevKeys;

        public KeyboardEvents(Game game)
            : base(game)
        {
            _prevPressedKeys = new Dictionary<Keys, bool>();
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            var currentKeys = Keyboard.GetState().GetPressedKeys();
            
            // If no keys are pressed and no keys were pressed, then no changes have occured
            if (currentKeys == null && _prevKeys == null)
                return;

            // If no keys are pressed, but previously there were then release all those.
            if (currentKeys == null)
            {
                if (KeyReleased != null)
                    foreach (var prevKey in _prevKeys)
                        KeyReleased(prevKey);
            }

            // If no keys were pressed, but now there are then all these are pressed.
            else if (_prevKeys == null)
            {
                if (KeyPressed != null)
                    foreach (var currentKey in currentKeys)
                        KeyPressed(currentKey);
            }
            else
            {

                // Identify released keys and fire events
                foreach (var prevKey in _prevPressedKeys.Keys)
                {
                    if (!currentKeys.Contains(prevKey))
                        if (KeyReleased != null) KeyReleased(prevKey);
                }

                // Identify newly pressed keys and fire events
                foreach (var currentKey in (Keys[])currentKeys.Clone())
                {
                    if (!_prevPressedKeys.ContainsKey(currentKey))
                        if (KeyPressed != null) KeyPressed(currentKey);
                }
            }

            _prevKeys = currentKeys;
            
            _prevPressedKeys.Clear();
            if (currentKeys != null)
                foreach (var currentKey in currentKeys)
                    _prevPressedKeys[currentKey] = true;

            base.Update(gameTime);
        }
    }
}