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
using Microsoft.Xna.Framework.Content;

namespace Sim.Interface
{
    /// <summary>
    /// Manager for all GUI components
    /// </summary>
    public class UserInterface : DrawableGameComponent
    {
        private SimEngine sim;

        // components
        private UIMouse mouse;                  // handles mouse input
        private UIKeyboard keyboard;            // handles keyboard input
        private MessageLog messageLog;          // handles info messages displayed on screen
        private ScreenManager screenManager;    // manger for all the various screens
        private Camera camera;                  // used to view the world
        private Console console;                // provides access to keyboard commands
        private FPSCounter fpsCounter;          // estimates frames per second
        
        private SpriteBatch sb;
        private Ray mouseRay;

        #region Textures

        public static Texture2D TexBlank;
        public static Texture2D TexBar;
        public static Texture2D TexCursor;
        public static Texture2D TexKnob;
        public static Texture2D TexButtonL;
        public static Texture2D TexButtonM;
        public static Texture2D TexButtonR;

        #endregion

        #region Properties

        public UIMouse Mouse { get { return mouse; } }
        public UIKeyboard Keyboard { get { return keyboard; } }
        public MessageLog MessageLog { get { return messageLog; } }
        public ScreenManager ScreenManager { get { return screenManager; } }
        public Camera Camera { get { return camera; } }
        public Console Console { get { return console; } }

        public SpriteBatch SpriteBatch { get { return sb; } }
        public SimEngine Sim { get { return sim; } }
        public Ray MouseRay { get { return mouseRay; } }

        #endregion

        public UserInterface(SimEngine sim)
            : base(sim)
        {
            this.sim = sim;
            DrawOrder = 3;
            UpdateOrder = 0;
        }

        public override void Initialize()
        {
            mouse = new UIMouse();
            keyboard = new UIKeyboard();
            messageLog = new MessageLog();
            screenManager = new ScreenManager(this);
            camera = new Camera(sim);
            console = new Console(sim);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            sb = new SpriteBatch(Game.GraphicsDevice);

            TexBlank = Game.Content.Load<Texture2D>(@"textures\gui\blank");
            TexBar = Game.Content.Load<Texture2D>(@"textures\gui\bar");
            TexCursor = Game.Content.Load<Texture2D>(@"textures\gui\cursor");
            TexKnob = Game.Content.Load<Texture2D>(@"textures\gui\knob");
            TexButtonL = Game.Content.Load<Texture2D>(@"textures\gui\buttonLeft");
            TexButtonM = Game.Content.Load<Texture2D>(@"textures\gui\buttonMiddle");
            TexButtonR = Game.Content.Load<Texture2D>(@"textures\gui\buttonRight");

            screenManager.LoadContent(Game.Content);
            fpsCounter = new FPSCounter(this.GraphicsDevice.Viewport);
        }

        public override void Update(GameTime gameTime)
        {
            mouse.Update();
            keyboard.Update(gameTime);
            console.Update();
            screenManager.Update(gameTime);
            fpsCounter.Update(gameTime);
            messageLog.Update(gameTime);
            camera.Update(gameTime);

            mouseRay = GMath.GetRayFromScreenPoint(mouse.ScreenPositionV, GraphicsDevice.Viewport,
                camera.Projection, camera.View);
        }

        public void ToggleFPS()
        {
            fpsCounter.Visible = !fpsCounter.Visible;
            //sim.IsFixedTimeStep = !fpsCounter.Visible;
            //sim.Graphics.SynchronizeWithVerticalRetrace = !fpsCounter.Visible;
            //sim.Graphics.ApplyChanges();
        }

        public override void Draw(GameTime gameTime)
        {
            sb.Begin();
            screenManager.Draw(sb);
            messageLog.Draw(sb);
            console.Draw(gameTime, sb);
            fpsCounter.Draw(sb);
            mouse.Draw(sb);
            sb.End();
        }
    }
}
