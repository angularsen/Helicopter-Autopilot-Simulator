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

#endregion

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// A static tree mesh composed of simple lines. Each branch is displayed as a line.
    /// Useful for testing and debugging.
    /// </summary>
    public class TreeLineMesh : IDisposable
    {
        private VertexDeclaration declaration;
        private GraphicsDevice device;
        private BasicEffect effect;
        private IndexBuffer ibuffer;
        private int numlines;
        private int numvertices;
        private VertexBuffer vbuffer;

        public TreeLineMesh(GraphicsDevice device, TreeSkeleton skeleton)
        {
            this.device = device;

            Init(skeleton);
        }

        #region IDisposable Members

        public void Dispose()
        {
            vbuffer.Dispose();
            ibuffer.Dispose();
            declaration.Dispose();
            effect.Dispose();
        }

        #endregion

        private void Init(TreeSkeleton skeleton)
        {
            // Get branch transforms
            var transforms = new Matrix[skeleton.Branches.Count];
            skeleton.CopyAbsoluteBranchTransformsTo(transforms);

            // Create the vertices and indices
            numlines = skeleton.Branches.Count;
            numvertices = numlines*2;
            var vertices = new VertexPositionColor[numvertices];
            var indices = new short[numlines*2];

            int vidx = 0;
            int iidx = 0;

            for (int i = 0; i < skeleton.Branches.Count; i++)
            {
                TreeBranch branch = skeleton.Branches[i];

                indices[iidx++] = (short) vidx;
                indices[iidx++] = (short) (vidx + 1);

                vertices[vidx++] = new VertexPositionColor(transforms[i].Translation, Color.White);
                vertices[vidx++] =
                    new VertexPositionColor(Vector3.Transform(new Vector3(0, branch.Length, 0), transforms[i]),
                                            Color.White);
            }

            // Create buffers
            vbuffer = new VertexBuffer(device, numvertices*VertexPositionColor.SizeInBytes, BufferUsage.None);
            vbuffer.SetData<VertexPositionColor>(vertices);
            ibuffer = new IndexBuffer(device, indices.Length*sizeof (short), BufferUsage.None,
                                      IndexElementSize.SixteenBits);
            ibuffer.SetData<short>(indices);

            // Create vertex declaration
            declaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);

            // Create the effect
            effect = new BasicEffect(device, new EffectPool());
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            effect.World = world;
            effect.View = view;
            effect.Projection = projection;

            device.VertexDeclaration = declaration;
            device.Vertices[0].SetSource(vbuffer, 0, VertexPositionColor.SizeInBytes);
            device.Indices = ibuffer;

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, numvertices, 0, numlines);
                pass.End();
            }
            effect.End();
        }
    }
}