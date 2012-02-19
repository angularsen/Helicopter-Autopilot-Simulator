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
using Microsoft.Xna.Framework.Graphics;
using NINFocusOnTerrain;
using Simulator.Interfaces;

#endregion

namespace Simulator.Terrain.Generator
{
    public class TerrainMesh : DrawableGameComponent
    {
        public readonly List<TerrainPatch> Patches;
        private Heightmap _heightmap;
        private VertexDeclaration _vertexDeclaration;
        private readonly ICameraProvider _cameraProvider;


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cameraProvider"></param>
        /// <param name="heightmap"></param>
        /// <param name="world"></param>
        public TerrainMesh(Game game, ICameraProvider cameraProvider, Heightmap heightmap, Matrix world)
            : base(game)
        {
            Patches = new List<TerrainPatch>();
            Heightmap = heightmap;
            _cameraProvider = cameraProvider;
            World = world;
        }

        private ICamera Camera { get { return _cameraProvider.Camera; } }

        public bool IsWireframe { get; set; }

        public Heightmap Heightmap
        {
            get { return _heightmap; }
            set
            {
                if (_heightmap == value) return;

                _heightmap = value;
//                if (value != null) BuildTerrain();
            }
        }

        public int PatchCount { get; private set; }
        public int DrawnPatchCount { get; private set; }
        public int PatchRows { get; private set; }
        public int PatchColumns { get; private set; }

        public BoundingBox BoundingBox { get; set; }

        public Matrix World { get; set; }


        /// <summary>
        /// Load the graphics content and also build the mesh.
        /// </summary>
        protected override void LoadContent()
        {
            BuildTerrain();
        }

        public override void Update(GameTime gameTime)
        {
            // TODO Redundant to LoadContent() ?
            // First time generation
            if (PatchCount == 0) BuildTerrain();

            base.Update(gameTime);
        }


        /// <summary>
        /// Draw the terrain.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="skyDomeEffect"></param>
        /// <param name="world"></param>
        public void Draw(GameTime gameTime, Effect skyDomeEffect)
        {
            DrawnPatchCount = 0;

            GraphicsDevice.RenderState.FillMode = IsWireframe ? FillMode.WireFrame : FillMode.Solid;
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            GraphicsDevice.VertexDeclaration = _vertexDeclaration;

            DrawTerrain(skyDomeEffect);

            base.Draw(gameTime);
        }

        private void DrawTerrain(Effect effect)
        {
            effect.Begin();

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                for (int i = 0; i < Patches.Count; ++i)
                {
                    // Test the patch against frustum.
                    BoundingBox transformedBBox = Transform(Patches[i].BoundingBox, World);
                    if (Camera.Frustum.Contains(Patches[i].BoundingBox) != ContainmentType.Disjoint)
                    {
                        Patches[i].Draw();
                        ++DrawnPatchCount;
                    }
                }
                pass.End();
            }
            effect.End();
        }

        private static BoundingBox Transform(BoundingBox boundingBox, Matrix transform)
        {
            return new BoundingBox(
                Vector3.Transform(boundingBox.Min, transform),
                Vector3.Transform(boundingBox.Max, transform));
        }

        /// <summary>
        /// Build the terrain.
        /// </summary>
        public void BuildTerrain()
        {
            if (Heightmap == null) return;

            int width = Heightmap.Width;
            int depth = Heightmap.Depth;

            // Clear the terrain patches.
            Patches.Clear();

            // Compute the world matrix to place the terrain in the middle of the scene.
//            _world = Matrix.Identity;//Matrix.CreateTranslation(width*-0.5f, 0.0f, depth*-0.5f);

            // Create the terrain patches.
            const int patchWidth = 16;
            const int patchDepth = 16;
            int patchCountX = width/patchWidth;
            int patchCountZ = depth/patchDepth;

            PatchRows = patchCountX;
            PatchColumns = patchCountZ;

            for (int x = 0; x < patchCountX; ++x)
            {
                for (int z = 0; z < patchCountZ; ++z)
                {
                    // It is necessary to use patch width and depths of +1 otherwise there will be
                    // gaps between the patches.. [0,15] and [16,31] have a gap of one unit!
                    var patch = new TerrainPatch(Game, Heightmap, World, patchWidth + 1, patchDepth + 1,
                                                 x*patchWidth, z*patchDepth);
//                        x*(patchWidth - 1), z*(patchDepth - 1));

                    Patches.Add(patch);
                }
            }

            // Find the minimum bounding box that covers all patches
            BoundingBox box = Patches[0].BoundingBox;
            foreach (TerrainPatch patch in Patches)
                box = BoundingBox.CreateMerged(box, patch.BoundingBox);

            BoundingBox = box;
            PatchCount = Patches.Count;

            _vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);
        }

        /// <summary>
        /// Save the terrain to a file.
        /// </summary>
        /// <param name="filename"></param>
        public void SaveToFile(String filename)
        {
            Heightmap.SaveToFile(filename);
        }

        /// <summary>
        /// Load the terrain from a file and build the terrain.
        /// </summary>
        /// <param name="filename"></param>
        public void LoadFromFile(String filename)
        {
            Heightmap.LoadFromFile(filename);

            BuildTerrain();
        }
    }
}