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
using Microsoft.Xna.Framework.Graphics;
using Simulator.Interfaces;
using Simulator.Skydome;

#endregion

namespace Simulator.StaticMeshes
{
    public class SimpleModel : DrawableGameComponent, ICameraTarget
    {

            /// <summary>
        /// The FBX importer in XNA does not properly treat the units. When exporting from 3D Studio
        /// the FBX importer simply treats the value as centimeters, no matter what units were used.
        /// </summary>
        private readonly Matrix _fbxImportScale;

        private readonly string _modelName;
        private readonly SunlightParameters _skyParams;
        private Model _model;
        private SunlightEffect _sunlightEffect;
        private readonly ICameraProvider _cameraProvider;

        public SimpleModel(string modelName, Game game, ICameraProvider cameraProvider, SunlightParameters skyParams)
            : base(game)
        {
            _modelName = modelName;
            _cameraProvider = cameraProvider;
            _skyParams = skyParams;

            _fbxImportScale = Matrix.CreateScale(0.01f);

            // TODO Apply transformations to these later if they become needed
            CameraUp = Vector3.Up;
            CameraForward = Vector3.Forward;

            // Initial render transformation
            Position = Vector3.Zero;
            Rotation = Matrix.Identity;
            Scale = Matrix.Identity;

            IsTransparent = false;
            IsTwoSided = false;
            IsSelfLit = false;
        }

        public SimpleModel(string modelName, Game game, ICameraProvider cameraProvider)
            : this(modelName, game, cameraProvider, null)
        {
        }

        #region Properties

        private ICamera Camera { get { return _cameraProvider.Camera; } }

        public Matrix Scale { get; set; }

        public bool IsTransparent { get; set; }

        public bool IsTwoSided { get; set; }

        protected bool IsSelfLit { get; set; }
        public Matrix Rotation { get; set; }

        #endregion

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _sunlightEffect = SunlightEffect.Create(GraphicsDevice, Game.Content);
            _model = Game.Content.Load<Model>(_modelName);

//            var basicEffect = new BasicEffect(GraphicsDevice, null);
            if (_sunlightEffect != null)
            {
                // Replace the object's effects with our provided effect
                foreach (ModelMesh mesh in _model.Meshes)
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        // Reuse existing model texture in sunlight effect
//                        if (part.Effect as BasicEffect != null)
//                            _sunlightEffect.Texture = ((BasicEffect) part.Effect).Texture;
//
                        // Assign sunlight shader
//                        part.Effect = _sunlightEffect;
                    }
            }

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (_sunlightEffect != null)
                _sunlightEffect.Update(Camera, _skyParams);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.AlphaBlendEnable = IsTransparent;
            GraphicsDevice.RenderState.AlphaTestEnable = IsTransparent;

            CullMode oldCullMode = GraphicsDevice.RenderState.CullMode;
            if (IsTwoSided)
                GraphicsDevice.RenderState.CullMode = CullMode.None;


            var transforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    // Apply basic camera transformations
                    var basicEffect = effect as BasicEffect;
                    if (basicEffect != null)
                    {
                        if (IsSelfLit)
                        {
                            basicEffect.LightingEnabled = false;
                            basicEffect.EmissiveColor = new Vector3(1);
                        }

                        basicEffect.View = Camera.View;
                        basicEffect.Projection = Camera.Projection;
                        basicEffect.World = transforms[mesh.ParentBone.Index]*
                                            _fbxImportScale*Scale*
                                            Rotation*
                                            Matrix.CreateTranslation(Position);
                    }
                }
                mesh.Draw();
            }

            // Restore state
            GraphicsDevice.RenderState.CullMode = oldCullMode;

            base.Draw(gameTime);
        }

        #region Implementation of ICameraTarget

        public Vector3 Position { get; set; }
        public Vector3 CameraUp { get; private set; }
        public Vector3 CameraForward { get; private set; }

        #endregion
    }
}