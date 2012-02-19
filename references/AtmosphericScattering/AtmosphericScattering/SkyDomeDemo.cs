/*
 * Created by Alex Urbano Álvarez
 * goefuika@gmail.com
 */

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace AtmosphericScattering
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SkyDomeDemo : Microsoft.Xna.Framework.Game
    {
        #region Properties
        GraphicsDeviceManager graphics;
        public ContentManager content;
        private SpriteBatch spriteBatch;
        private SpriteFont Font;
        private Vector2 FontPosition;

        private FreeCamera camera;
        private SkyDome sky;
        private KeyboardState previousKeyState;

        private Model terrain;
        private Texture2D terrainTexture;
        private Effect terrainEffect;

        Texture2D day, sunset, night;
       
        private enum Status
        {
            Manual = 0,
            Automatic = 1,
            ActualTime = 2
        }

        Status stat;
        #endregion

        #region Constructor
        public SkyDomeDemo()
        {
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(Services);
            //Content.RootDirectory = "Content";

        }
        #endregion

        #region Initialize
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            camera = new FreeCamera(new Vector3(0.0f, 0.0f, 0.0f));
            camera.NearPlane = 1.0f;
            camera.FarPlane = 10000f;
            camera.Speed = 500f;
            camera.TurnSpeed = 15;
            camera.Angle = new Vector3(0.0f, 0.0f, 0.0f);

            stat = Status.Automatic;

            base.Initialize();
        }
        #endregion

        #region Load
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            sky = new SkyDome(this, ref camera);
            // Set skydome parameters here

            this.Components.Add(sky);

            terrain = content.Load<Model>("Content/Models/terrain");
            terrainTexture = content.Load<Texture2D>("Content/Models/ground");
            terrainEffect = content.Load<Effect>("Content/Effects/Sky");

            this.day = content.Load<Texture2D>("Content/Textures/SkyDay");
            this.sunset = content.Load<Texture2D>("Content/Textures/Sunset");
            this.night = content.Load<Texture2D>("Content/Textures/SkyNight");

            RemapModel(terrain, terrainEffect);

            Font = content.Load<SpriteFont>("Content/Fuente");
            FontPosition = new Vector2(100, 50);
        }
        #endregion

        #region Unload
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        #endregion

        #region Update
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Escape))
                this.Exit();

            switch (stat)
            {
                case Status.Manual:
                    sky.RealTime = false;
                    if (keyState.IsKeyDown(Keys.Down))
                    {
                        sky.Theta += 0.005f;
                    }
                    if (keyState.IsKeyDown(Keys.Up))
                    {
                        sky.Theta -= 0.005f;
                    }
                    break;
                case Status.Automatic:
                    sky.RealTime = false;
                    sky.Theta -= 0.005f;
                    break;
                case Status.ActualTime:
                    sky.RealTime = true;
                    break;
            }
            
            if (keyState.IsKeyDown(Keys.Space) && !previousKeyState.IsKeyDown(Keys.Space))
            {
                stat++;
                if ((int)stat == 3)
                    stat = Status.Manual;
            }

            camera.Update(this.graphics.GraphicsDevice.Viewport.Width, (float)gameTime.ElapsedGameTime.TotalSeconds);

            previousKeyState = keyState;

            base.Update(gameTime);
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);

            DrawTerrain(gameTime);

            this.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            string output = stat.ToString();
            Vector2 FontOrigin = Font.MeasureString(output) / 2;
            spriteBatch.DrawString(Font, output, FontPosition, Color.LightGreen,
                0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.End();
        }

        private void DrawTerrain(GameTime gameTime)
        {
            Matrix View = camera.View;
            Matrix Projection = camera.Projection;

            foreach (ModelMesh mesh in terrain.Meshes)
            {
                Matrix World = Matrix.CreateScale(4.0f) * Matrix.CreateRotationX((float)Math.PI / 2.0f);
                Matrix WorldIT = Matrix.Invert(World);
                WorldIT = Matrix.Transpose(WorldIT);

                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["WorldIT"].SetValue(WorldIT);
                    effect.Parameters["WorldViewProj"].SetValue(World * View * Projection);
                    effect.Parameters["ViewInv"].SetValue(Matrix.Invert(View));
                    effect.Parameters["World"].SetValue(World);

                    effect.Parameters["DiffuseTexture"].SetValue(terrainTexture);
                    effect.Parameters["SkyTextureNight"].SetValue(night);
                    effect.Parameters["SkyTextureSunset"].SetValue(sunset);
                    effect.Parameters["SkyTextureDay"].SetValue(day);
                    
                    effect.Parameters["isSkydome"].SetValue(false);

                    effect.Parameters["LightDirection"].SetValue(sky.Parameters.LightDirection);
                    effect.Parameters["LightColor"].SetValue(sky.Parameters.LightColor);
                    effect.Parameters["LightColorAmbient"].SetValue(sky.Parameters.LightColorAmbient);
                    effect.Parameters["FogColor"].SetValue(sky.Parameters.FogColor);
                    effect.Parameters["fDensity"].SetValue(0.0003f);
                    effect.Parameters["SunLightness"].SetValue(sky.Parameters.SunLightness);
                    effect.Parameters["sunRadiusAttenuation"].SetValue(sky.Parameters.SunRadiusAttenuation);
                    effect.Parameters["largeSunLightness"].SetValue(sky.Parameters.LargeSunLightness);
                    effect.Parameters["largeSunRadiusAttenuation"].SetValue(sky.Parameters.LargeSunRadiusAttenuation);
                    effect.Parameters["dayToSunsetSharpness"].SetValue(sky.Parameters.DayToSunsetSharpness);
                    effect.Parameters["hazeTopAltitude"].SetValue(sky.Parameters.HazeTopAltitude);

                    mesh.Draw();
                }
            }
        }
        #endregion

        #region Remap Model
        public static void RemapModel(Model model, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }
            }
        }
        #endregion
    }
}
