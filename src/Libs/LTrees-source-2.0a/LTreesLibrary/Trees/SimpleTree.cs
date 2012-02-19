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
    /// Renders a tree mesh and corresponding leaves. Contains a tree skeleton and the corresponding
    /// tree mesh, leaf cloud, animation state, effects, and textures.
    /// </summary>
    /// <remarks>
    /// Because effects should be loaded by the content manager, they must be set manually before the
    /// can be rendered.
    /// 
    /// This is a good place to get started, but to gain more flexibility and performance, you will
    /// eventually need to use the TreeMesh, TreeLeafCloud, TreeAnimationState and TreeSkeleton classes directly.
    /// In a serious application, it is recommended that you write your own application-specific substitute for this class.
    /// </remarks>
    public class SimpleTree
    {
        public Matrix[] BindingMatrices;
        protected BasicEffect BoneEffect;
        public TreeLeafCloud Leaves;
        public TreeMesh Trunk;
        private TreeAnimationState animationState;
        private GraphicsDevice device;
        private Effect leafEffect;
        private Texture2D leafTexture;
        private TreeSkeleton skeleton;
        private Effect trunkEffect;
        private Texture2D trunkTexture;

        public SimpleTree(GraphicsDevice device)
        {
            this.device = device;
            BoneEffect = new BasicEffect(device, new EffectPool());
        }

        public SimpleTree(GraphicsDevice device, TreeSkeleton skeleton)
        {
            this.device = device;
            this.skeleton = skeleton;
            BoneEffect = new BasicEffect(device, new EffectPool());
            UpdateSkeleton();
        }

        public GraphicsDevice GraphicsDevice
        {
            get { return device; }
        }

        /// <summary>
        /// The tree structure displayed.
        /// Setting this will reset the current animation state and result in new meshes being generated.
        /// </summary>
        public TreeSkeleton Skeleton
        {
            get { return skeleton; }
            set
            {
                skeleton = value;
                UpdateSkeleton();
            }
        }

        public TreeMesh TrunkMesh
        {
            get { return Trunk; }
        }

        public TreeLeafCloud LeafCloud
        {
            get { return Leaves; }
        }

        /// <summary>
        /// The current position of all the bones.
        /// Setting this to a new animation state has no performance hit.
        /// </summary>
        public TreeAnimationState AnimationState
        {
            get { return animationState; }
            set { animationState = value; }
        }

        /// <summary>
        /// Effect used to draw the trunk.
        /// </summary>
        public Effect TrunkEffect
        {
            get { return trunkEffect; }
            set { trunkEffect = value; }
        }

        /// <summary>
        /// Effect used to draw the leaves.
        /// </summary>
        public Effect LeafEffect
        {
            get { return leafEffect; }
            set { leafEffect = value; }
        }

        /// <summary>
        /// Texture on the trunk.
        /// </summary>
        public Texture2D TrunkTexture
        {
            get { return trunkTexture; }
            set { trunkTexture = value; }
        }

        /// <summary>
        /// Leaves on the trunk.
        /// </summary>
        public Texture2D LeafTexture
        {
            get { return leafTexture; }
            set { leafTexture = value; }
        }

        private void UpdateSkeleton()
        {
            Trunk = new TreeMesh(device, skeleton);
            Leaves = new TreeLeafCloud(device, skeleton);
            animationState = new TreeAnimationState(skeleton);
            BindingMatrices = new Matrix[skeleton.Bones.Count];
        }

        /// <summary>
        /// Draws the trunk using the specified world, view, and projection matrices.
        /// </summary>
        /// <param name="world">World matrix.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        /// <exception cref="InvalidOperationException">If no trunk effect is set.</exception>
        /// <exception cref="InvalidOperationException">If no skeleton is set.</exception>
        /// <remarks>
        /// This method sets all the appropriate effect parameters.
        /// </remarks>
        public void DrawTrunk(Matrix world, Matrix view, Matrix projection)
        {
            if (skeleton == null)
                throw new InvalidOperationException("A skeleton must be set before trunk can be rendered.");

            if (trunkEffect == null)
                throw new InvalidOperationException("TrunkEffect must be set before trunk can be rendered.");

            trunkEffect.Parameters["World"].SetValue(world);
            trunkEffect.Parameters["View"].SetValue(view);
            trunkEffect.Parameters["Projection"].SetValue(projection);

            skeleton.CopyBoneBindingMatricesTo(BindingMatrices, animationState.BoneRotations);
            trunkEffect.Parameters["Bones"].SetValue(BindingMatrices);

            trunkEffect.Parameters["Texture"].SetValue(trunkTexture);

            Trunk.Draw(trunkEffect);
        }

        /// <summary>
        /// Draws the leaves using the specified world, view, and projection matrices.
        /// </summary>
        /// <param name="world">World matrix.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        /// <exception cref="InvalidOperationException">If no leaf effect is set.</exception>
        /// <exception cref="InvalidOperationException">If no skeleton is set.</exception>
        /// <remarks>
        /// This method sets all the appropriate effect parameters.
        /// </remarks>
        public void DrawLeaves(Matrix world, Matrix view, Matrix projection)
        {
            if (skeleton == null)
                throw new InvalidOperationException("A skeleton must be set before leaves can be rendered.");

            if (leafEffect == null)
                throw new InvalidOperationException("LeafEffect must be set before leaves can be rendered.");

            leafEffect.Parameters["WorldView"].SetValue(world*view);
            leafEffect.Parameters["View"].SetValue(view);
            leafEffect.Parameters["Projection"].SetValue(projection);
            leafEffect.Parameters["LeafScale"].SetValue(world.Right.Length());

            if (skeleton.LeafAxis == null)
            {
                leafEffect.Parameters["BillboardRight"].SetValue(Vector3.Right);
                leafEffect.Parameters["BillboardUp"].SetValue(Vector3.Up);
            }
            else
            {
                Vector3 axis = skeleton.LeafAxis.Value;
                var forward = new Vector3(view.M13, view.M23, view.M33);

                Vector3 right = Vector3.Cross(forward, axis);
                right.Normalize();
                Vector3 up = axis;

                Vector3.TransformNormal(ref right, ref view, out right);
                Vector3.TransformNormal(ref up, ref view, out up);

                leafEffect.Parameters["BillboardRight"].SetValue(right);
                leafEffect.Parameters["BillboardUp"].SetValue(up);
            }

            skeleton.CopyBoneBindingMatricesTo(BindingMatrices, animationState.BoneRotations);
            leafEffect.Parameters["Bones"].SetValue(BindingMatrices);

            leafEffect.Parameters["Texture"].SetValue(leafTexture);

            Leaves.Draw(leafEffect);
        }

        /// <summary>
        /// Draws the tree's bones as lines. Useful for testing and debugging.
        /// </summary>
        /// <param name="world">World matrix.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        public void DrawBonesAsLines(Matrix world, Matrix view, Matrix projection)
        {
            if (skeleton == null)
                throw new InvalidOperationException("A skeleton must be set before bones can be rendered.");

            if (leafEffect == null)
                throw new InvalidOperationException("LeafEffect must be set before leaves can be rendered.");

            if (skeleton.Bones.Count == 0)
                return;

            BoneEffect.World = world;
            BoneEffect.View = view;
            BoneEffect.Projection = projection;

            bool wasDepthBufferOn = device.RenderState.DepthBufferEnable;
            device.RenderState.DepthBufferEnable = false;
            device.RenderState.AlphaTestEnable = false;
            device.RenderState.AlphaBlendEnable = false;

            var transforms = new Matrix[skeleton.Bones.Count];
            skeleton.CopyAbsoluteBoneTranformsTo(transforms, animationState.BoneRotations);

            var vertices = new VertexPositionColor[skeleton.Bones.Count*2];
            for (int i = 0; i < skeleton.Bones.Count; i++)
            {
                vertices[2*i] = new VertexPositionColor(transforms[i].Translation, Color.Red);
                vertices[2*i + 1] =
                    new VertexPositionColor(transforms[i].Translation + transforms[i].Up*skeleton.Bones[i].Length,
                                            Color.Red);
            }

            BoneEffect.Begin();
            foreach (EffectPass pass in BoneEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, skeleton.Bones.Count);
                pass.End();
            }
            BoneEffect.End();

            device.RenderState.DepthBufferEnable = wasDepthBufferOn;
        }
    }
}