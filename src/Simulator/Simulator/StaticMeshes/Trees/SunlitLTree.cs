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

using LTreesLibrary.Trees;
using LTreesLibrary.Trees.Wind;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Simulator.Interfaces;
using Simulator.Skydome;

#endregion

namespace Simulator.StaticMeshes.Trees
{
    public class SunlitLTree : DrawableGameComponent
    {
        private readonly ICameraProvider _cameraProvider;
        private readonly SunlightParameters _sky;
        private TreeWindAnimator _animator;
        private Vector3 _position;
        private Matrix _scale;
        private SunlitSimpleTree _tree;
        private TreeProfile _treeProfile;
        private WindStrengthSin _wind;
        private Matrix _world;

        public SunlitLTree(Game game, ICameraProvider cameraProvider, SunlightParameters sky, Vector3 position)
            : base(game)
        {
            _cameraProvider = cameraProvider;
            _sky = sky;
            _position = position;
            Content = game.Content;
        }

        protected ContentManager Content { get; private set; }

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                _world = _scale*Matrix.CreateTranslation(value);

                BoundingBox = BoundingBox.CreateFromSphere(
                    _tree.TrunkMesh.BoundingSphere.Transform(_world));
            }
        }

        public BoundingBox BoundingBox { get; private set; }

        #region Overriden

        public override void Initialize()
        {
            _treeProfile = Content.Load<TreeProfile>("Trees/Graywood");
            _tree = new SunlitSimpleTree(_treeProfile.GenerateSimpleTree());
            _tree.Lighting = _sky;

            // Scale tree down to a desired height
            const int desiredHeight = 6;
            float treeModelHeight = _tree.TrunkMesh.BoundingSphere.Radius*2;
            _scale = Matrix.CreateScale(desiredHeight/treeModelHeight);
            Position = _position;

            _wind = new WindStrengthSin();
            _animator = new TreeWindAnimator(_wind);

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            _wind.Update(gameTime);
            _animator.Animate(_tree.Skeleton, _tree.AnimationState, gameTime);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
//            GraphicsDevice.RenderState.DepthBufferEnable = true;
//            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
//            GraphicsDevice.RenderState.AlphaBlendEnable = false;

            var camera = _cameraProvider.Camera;
            _tree.DrawTrunk(_world, camera.View, camera.Projection);
            _tree.DrawLeaves(_world, camera.View, camera.Projection, camera.Position);

            base.Draw(gameTime);
        }

        #endregion
    }
}