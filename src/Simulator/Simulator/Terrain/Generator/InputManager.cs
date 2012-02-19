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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace NINFocusOnTerrain
{
    /// <summary>
    /// This is the input manager class.
    /// </summary>
    public class InputManager
    {
        /// <summary>
        /// Keep track of the current game pad state.
        /// </summary>
        private GamePadState _currentGamePadState;

        /// <summary>
        /// Keep track of the current keyboard state.
        /// </summary>
        private KeyboardState _currentKeyboardState;

        /// <summary>
        /// Keep track of the current mouse state.
        /// </summary>
        private MouseState _currentMouseState;

        /// <summary>
        /// Keep track of the previous game pads states.
        /// </summary>
        private GamePadState _previousGamePadState;

        /// <summary>
        /// Keep track of the previous keyboard state.
        /// </summary>
        private KeyboardState _previousKeyboardState;

        /// <summary>
        /// Keep track of the previous mouse state.
        /// </summary>
        private MouseState _previousMouseState;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InputManager()
        {
            _currentGamePadState = GamePad.GetState(PlayerIndex.One);
            _previousGamePadState = _currentGamePadState;

            _currentKeyboardState = Keyboard.GetState();
            _previousKeyboardState = _currentKeyboardState;

            _currentMouseState = Mouse.GetState();
            _previousMouseState = _currentMouseState;
        }

        /// <summary>
        /// Current game pad state.
        /// </summary>
        public GamePadState CurrentGamePadState
        {
            get { return _currentGamePadState; }
        }

        /// <summary>
        /// Previous game pad state.
        /// </summary>
        public GamePadState PreviousGamePadState
        {
            get { return _previousGamePadState; }
        }

        /// <summary>
        /// Current keyboard state.
        /// </summary>
        public KeyboardState CurrentKeyboardState
        {
            get { return _currentKeyboardState; }
        }

        /// <summary>
        /// Previous keyboard state.
        /// </summary>
        public KeyboardState PreviousKeyboardState
        {
            get { return _previousKeyboardState; }
        }

        /// <summary>
        /// Update the input devices.
        /// </summary>
        public void Update()
        {
            _previousGamePadState = _currentGamePadState;
            _currentGamePadState = GamePad.GetState(PlayerIndex.One);

            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
        }
    }
}

/*======================================================================================================================

									NIN - Nerdy Inverse Network - http://nerdy-inverse.com

======================================================================================================================*/