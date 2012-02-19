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
using NINFocusOnTerrain;
using Simulator.Interfaces;
using Simulator.Skydome;
using Simulator.Terrain.Generator;

#endregion

namespace Simulator.StaticMeshes.Trees
{
    public class Forest : DrawableGameComponent
    {
        private readonly Game _game;
        private readonly SunlightParameters _sky;
        private readonly TerrainMesh _terrain;
        private readonly List<Vector3> _treePositions;
        private SunlitLTree _tree;
        private readonly ICameraProvider _cameraProvider;

        public Forest(Game game, ICameraProvider cameraProvider, TerrainMesh terrain, SunlightParameters sky)
            : base(game)
        {
            if (game == null || cameraProvider == null || terrain == null)
                throw new ArgumentNullException();

            _game = game;
            _cameraProvider = cameraProvider;
            _terrain = terrain;
            _sky = sky;
            _treePositions = new List<Vector3>();
        }

        private ICamera Camera { get { return _cameraProvider.Camera; } }


        #region Overridden

        public override void Initialize()
        {
            for (int row = 0; row < _terrain.PatchRows; row++)
            {
                for (int col = 0; col < _terrain.PatchColumns; col++)
                {
                    TerrainPatch patch = _terrain.Patches[row*_terrain.PatchColumns + col];
                    PopulatePatch(_treePositions, patch);
                }
            }

//            _treePositions.Add(Vector3.Zero);

            _tree = new SunlitLTree(_game, _cameraProvider, _sky, Vector3.Zero);
            _tree.Initialize();

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            _tree.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Re-use the same tree but with different positions to minimize allocated resources
            foreach (Vector3 pos in _treePositions)
            {
                _tree.Position = pos;

                if (_tree.BoundingBox.Intersects(Camera.Frustum))
                    _tree.Draw(gameTime);
            }

            base.Draw(gameTime);
        }

        #endregion

        #region Private

        private static void PopulatePatch(ICollection<Vector3> trees, TerrainPatch patch)
        {
            int treeDistanceZ = patch.Depth/1;
            int treeDistanceX = patch.Width/1;

            for (int z = 0; z < patch.Depth; z += treeDistanceZ)
            {
                for (int x = 0; x < patch.Width; x += treeDistanceX)
                {
                    int index = z*patch.Width + x;
                    trees.Add(patch.Geometry[index].Position);
                }
            }
        }

        #endregion
    }
}