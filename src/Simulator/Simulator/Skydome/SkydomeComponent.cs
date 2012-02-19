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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Simulator.Interfaces;

#endregion

namespace Simulator.Skydome
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SkydomeComponent : DrawableGameComponent
    {
        private Status _stat;

        #region Properties

        private readonly Game _game;
        private Texture2D _day;
        private Effect _domeEffect;
        private Model _domeModel;

        private float _fPhi;
        private float _fTheta;

        private Texture2D _night;

        private SunlightParameters _parameters;
        private bool _realTime;
        private Texture2D _sunset;
        private readonly ICameraProvider _cameraProvider;

        #endregion

        #region Properties

        private ICamera Camera { get { return _cameraProvider.Camera; } }

        /// <summary>
        /// Gets/Sets Theta value
        /// </summary>
        public float Theta
        {
            get { return _fTheta; }
            set { _fTheta = value; }
        }

        /// <summary>
        /// Gets/Sets Phi value
        /// </summary>
        public float Phi
        {
            get { return _fPhi; }
            set { _fPhi = value; }
        }

        /// <summary>
        /// Gets/Sets actual time computation
        /// </summary>
        public bool RealTime
        {
            get { return _realTime; }
            set { _realTime = value; }
        }

        /// <summary>
        /// Gets/Sets the SkyDome parameters
        /// </summary>
        public SunlightParameters Parameters
        {
            get { return _parameters; }
            private set { _parameters = value; }
        }

        #endregion

        #region Constructor

        public SkydomeComponent(Game game, ICameraProvider cameraProvider) : base(game)
        {
            _cameraProvider = cameraProvider;
            _game = game;

            _realTime = false;
            _parameters = new SunlightParameters();
            _parameters.FogDensity = 0.0058f;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public override void Initialize()
        {
//            _stat = Status.Automatic;
            _stat = Status.Manual;

            base.Initialize();
        }

        #endregion

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Theta = 1.6f; // Set a sunrise position for the skydome

            _domeModel = _game.Content.Load<Model>("Models/skydome");
            _domeEffect = _game.Content.Load<Effect>("Effects/TerrainAndSkydome");

            _day = _game.Content.Load<Texture2D>("Textures/SkyDay");
            _sunset = _game.Content.Load<Texture2D>("Textures/Sunset");
            _night = _game.Content.Load<Texture2D>("Textures/SkyNight");

            _domeEffect.CurrentTechnique = _domeEffect.Techniques["SkyDome"];

            RemapModel(_domeModel, _domeEffect);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Escape))
                Game.Exit();

            switch (_stat)
            {
                case Status.Manual:
                    RealTime = false;
                    if (keyState.IsKeyDown(Keys.Down))
                    {
                        Theta += 0.01f;
                    }
                    if (keyState.IsKeyDown(Keys.Up))
                    {
                        Theta -= 0.01f;
                    }
                    break;
                case Status.Automatic:
                    RealTime = false;
                    Theta -= 0.005f;
                    break;
                case Status.ActualTime:
                    RealTime = true;
                    break;
            }

//            if (keyState.IsKeyDown(Keys.Space) && !_previousKeyState.IsKeyDown(Keys.Space))
//            {
//                _stat++;
//                if ((int) _stat == 3)
//                    _stat = Status.Manual;
//            }


            if (_realTime)
                _fTheta = (float) (gameTime.TotalGameTime.TotalMinutes*(Math.PI)/12.0f/60.0f);

            _parameters.LightDirection = GetDirection();
            _parameters.LightDirection.Normalize();


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            var boneTransforms = new Matrix[_domeModel.Bones.Count];
            _domeModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            Matrix view = Camera.View;
            Matrix projection = Camera.Projection;

            _game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            _game.GraphicsDevice.RenderState.AlphaTestEnable = false;
            _game.GraphicsDevice.RenderState.DepthBufferEnable = true;
            _game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            foreach (ModelMesh mesh in _domeModel.Meshes)
            {
                Matrix world = boneTransforms[mesh.ParentBone.Index]*
                               Matrix.CreateTranslation(Camera.Position.X, Camera.Position.Y - 50.0f,
                                                        Camera.Position.Z);

                Matrix worldIT = Matrix.Invert(world);
                worldIT = Matrix.Transpose(worldIT);

                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["WorldIT"].SetValue(worldIT);
                    effect.Parameters["WorldViewProj"].SetValue(world*view*projection);
                    effect.Parameters["ViewInv"].SetValue(Matrix.Invert(view));
                    effect.Parameters["World"].SetValue(world);

                    effect.Parameters["SkyTextureNight"].SetValue(_night);
                    effect.Parameters["SkyTextureSunset"].SetValue(_sunset);
                    effect.Parameters["SkyTextureDay"].SetValue(_day);

                    effect.Parameters["isSkydome"].SetValue(true);

                    effect.Parameters["LightDirection"].SetValue(_parameters.LightDirection);
                    effect.Parameters["LightColor"].SetValue(_parameters.LightColor);
                    effect.Parameters["LightColorAmbient"].SetValue(_parameters.LightColorAmbient);
                    effect.Parameters["FogColor"].SetValue(_parameters.FogColor);
                    effect.Parameters["fDensity"].SetValue(_parameters.FogDensity);
                    effect.Parameters["SunLightness"].SetValue(_parameters.SunLightness);
                    effect.Parameters["sunRadiusAttenuation"].SetValue(_parameters.SunRadiusAttenuation);
                    effect.Parameters["largeSunLightness"].SetValue(_parameters.LargeSunLightness);
                    effect.Parameters["largeSunRadiusAttenuation"].SetValue(_parameters.LargeSunRadiusAttenuation);
                    effect.Parameters["dayToSunsetSharpness"].SetValue(_parameters.DayToSunsetSharpness);
                    effect.Parameters["hazeTopAltitude"].SetValue(_parameters.HazeTopAltitude);

                    mesh.Draw();
                }
            }
//            _game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
//            _game.GraphicsDevice.RenderState.DepthBufferEnable = true;

            base.Draw(gameTime);
        }

        #region Private Methods

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

        #region Get Light Direction

        private Vector4 GetDirection()
        {
            var y = (float) Math.Cos(_fTheta);
            var x = (float) (Math.Sin(_fTheta)*Math.Cos(_fPhi));
            var z = (float) (Math.Sin(_fTheta)*Math.Sin(_fPhi));
            float w = 1.0f;

            return new Vector4(x, y, z, w);
        }

        #endregion

        #endregion

        #region Nested type: Status

        private enum Status
        {
            Manual = 0,
            Automatic = 1,
            ActualTime = 2
        }

        #endregion
    }
}