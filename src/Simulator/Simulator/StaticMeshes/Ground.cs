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

#endregion

namespace Simulator.StaticMeshes
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Ground : DrawableGameComponent
    {
        private readonly BasicEffect _basicEffect;
        private Model _cube;


        public Ground(Game game, BasicEffect effect)
            : base(game)
        {
            _basicEffect = effect;
        }

        protected override void LoadContent()
        {
            _cube = Game.Content.Load<Model>("chamfercube");

            foreach (ModelMesh mesh in _cube.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    var effect = part.Effect as BasicEffect;
                    Texture2D tmpTex = (effect != null) ? effect.Texture : null;
                    part.Effect = _basicEffect.Clone(GraphicsDevice);
                    ((BasicEffect) part.Effect).Texture = tmpTex;
                }

            base.LoadContent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            Game.GraphicsDevice.RenderState.DepthBufferEnable = true;
            Game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            Game.GraphicsDevice.RenderState.AlphaTestEnable = false;

            var transforms = new Matrix[_cube.Bones.Count];
            _cube.CopyAbsoluteBoneTransformsTo(transforms);

            const int cols = 10, rows = 10;

            for (int z = -rows/2; z < rows/2; z++)
                for (int x = -cols/2; x < cols/2; x++)
                {
                    foreach (ModelMesh mesh in _cube.Meshes)
                    {
                        foreach (BasicEffect effect in mesh.Effects)
                        {
                            effect.EnableDefaultLighting();
                            effect.View = _basicEffect.View;
                            effect.Projection = _basicEffect.Projection;
                            effect.World = transforms[mesh.ParentBone.Index]*
                                           Matrix.CreateScale(0.01f, 0.005f, 0.01f)*
                                           Matrix.CreateTranslation(x/1.0f, -0.5f, z/1.0f);
                        }
                        mesh.Draw();
                    }
                }

            base.Draw(gameTime);
        }
    }
}