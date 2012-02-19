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
using LTreesLibrary.Trees;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simulator.Skydome;

#endregion

namespace Simulator.StaticMeshes.Trees
{
    /// <summary>
    /// Renders a tree mesh and corresponding leaves. Contains a tree Skeleton and the corresponding
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
    public class SunlitSimpleTree : SimpleTree
    {
//        private TreeMesh trunk;
//        private TreeLeafCloud leaves;
//        private TreeAnimationState animationState;
//        private Effect trunkEffect;
//        private Effect leafEffect;
//        private Texture2D trunkTexture;
//        private Texture2D leafTexture;
//        private Matrix[] bindingMatrices;
//        private readonly BasicEffect boneEffect;

        public SunlitSimpleTree(SimpleTree tree)
            : this(tree.GraphicsDevice, tree.Skeleton)
        {
            AnimationState = tree.AnimationState;
            BindingMatrices = tree.BindingMatrices;
            Leaves = tree.Leaves;
            Trunk = tree.Trunk;
            LeafEffect = tree.LeafEffect;
            TrunkEffect = tree.TrunkEffect;
            LeafTexture = tree.LeafTexture;
            TrunkTexture = tree.TrunkTexture;
        }

//        public SunlitSimpleTree(GraphicsDevice device)
//            : base(device)
//        {
//        }

        public SunlitSimpleTree(GraphicsDevice device, TreeSkeleton skeleton)
            : base(device, skeleton)
        {
        }

        /// <summary>
        /// Parameters for sunlight
        /// </summary>
        public SunlightParameters Lighting { get; set; }

        /// <summary>
        /// Draws the trunk using the specified world, view, and projection matrices.
        /// </summary>
        /// <param name="world">World matrix.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        /// <exception cref="InvalidOperationException">If no trunk effect is set.</exception>
        /// <exception cref="InvalidOperationException">If no Skeleton is set.</exception>
        /// <remarks>
        /// This method sets all the appropriate effect parameters.
        /// </remarks>
        public new void DrawTrunk(Matrix world, Matrix view, Matrix projection)
        {
            if (Skeleton == null)
                throw new InvalidOperationException("A Skeleton must be set before trunk can be rendered.");

            if (TrunkEffect == null)
                throw new InvalidOperationException("TrunkEffect must be set before trunk can be rendered.");

            #region New code

            Effect effect = TrunkEffect;

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["WorldIT"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
            effect.Parameters["WorldViewProj"].SetValue(world*view*projection);
            effect.Parameters["ViewInv"].SetValue(Matrix.Invert(view));

            effect.Parameters["Texture"].SetValue(TrunkTexture);

            effect.Parameters["isSkydome"].SetValue(false);

            effect.Parameters["LightDirection"].SetValue(Lighting.LightDirection);
            effect.Parameters["LightColor"].SetValue(Lighting.LightColor);
            effect.Parameters["LightColorAmbient"].SetValue(Lighting.LightColorAmbient);
            effect.Parameters["FogColor"].SetValue(Lighting.FogColor);
            effect.Parameters["fDensity"].SetValue(Lighting.FogDensity);
            effect.Parameters["SunLightness"].SetValue(Lighting.SunLightness);
            effect.Parameters["sunRadiusAttenuation"].SetValue(Lighting.SunRadiusAttenuation);
            effect.Parameters["largeSunLightness"].SetValue(Lighting.LargeSunLightness);
            effect.Parameters["largeSunRadiusAttenuation"].SetValue(Lighting.LargeSunRadiusAttenuation);
            effect.Parameters["dayToSunsetSharpness"].SetValue(Lighting.DayToSunsetSharpness);
            effect.Parameters["hazeTopAltitude"].SetValue(Lighting.HazeTopAltitude);

            #endregion

            #region Original code

//            TrunkEffect.Parameters["World"].SetValue(world);
//            TrunkEffect.Parameters["View"].SetValue(view);
//            TrunkEffect.Parameters["Projection"].SetValue(projection);
//
            Skeleton.CopyBoneBindingMatricesTo(BindingMatrices, AnimationState.BoneRotations);
            TrunkEffect.Parameters["Bones"].SetValue(BindingMatrices);
//
//            TrunkEffect.Parameters["Texture"].SetValue(trunkTexture);

            #endregion

            Trunk.Draw(effect);
        }

        /// <summary>
        /// Draws the leaves using the specified world, view, and projection matrices.
        /// </summary>
        /// <param name="world">World matrix.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        /// <exception cref="InvalidOperationException">If no leaf effect is set.</exception>
        /// <exception cref="InvalidOperationException">If no Skeleton is set.</exception>
        /// <remarks>
        /// This method sets all the appropriate effect parameters.
        /// </remarks>
        public void DrawLeaves(Matrix world, Matrix view, Matrix projection, Vector3 cameraPos)
        {
            if (Skeleton == null)
                throw new InvalidOperationException("A Skeleton must be set before leaves can be rendered.");

            if (LeafEffect == null)
                throw new InvalidOperationException("LeafEffect must be set before leaves can be rendered.");

            LeafEffect.Parameters["WorldView"].SetValue(world*view);
            //leafEffect.Parameters["View"].SetValue(view);
            LeafEffect.Parameters["Projection"].SetValue(projection);
            LeafEffect.Parameters["LeafScale"].SetValue(world.Right.Length());


            Effect effect = LeafEffect;

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["WorldIT"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
            effect.Parameters["WorldViewProj"].SetValue(world*view*projection);
            effect.Parameters["ViewInv"].SetValue(Matrix.Invert(view));

            effect.Parameters["isSkydome"].SetValue(false);

            effect.Parameters["LightDirection"].SetValue(Lighting.LightDirection);
            effect.Parameters["LightColor"].SetValue(Lighting.LightColor);
            effect.Parameters["LightColorAmbient"].SetValue(Lighting.LightColorAmbient);
            effect.Parameters["FogColor"].SetValue(Lighting.FogColor);
            effect.Parameters["fDensity"].SetValue(Lighting.FogDensity);
            effect.Parameters["SunLightness"].SetValue(Lighting.SunLightness);
            effect.Parameters["sunRadiusAttenuation"].SetValue(Lighting.SunRadiusAttenuation);
            effect.Parameters["largeSunLightness"].SetValue(Lighting.LargeSunLightness);
            effect.Parameters["largeSunRadiusAttenuation"].SetValue(Lighting.LargeSunRadiusAttenuation);
            effect.Parameters["dayToSunsetSharpness"].SetValue(Lighting.DayToSunsetSharpness);
            effect.Parameters["hazeTopAltitude"].SetValue(Lighting.HazeTopAltitude);

            if (Skeleton.LeafAxis == null)
            {
                LeafEffect.Parameters["BillboardRight"].SetValue(Vector3.Right);
                LeafEffect.Parameters["BillboardUp"].SetValue(Vector3.Up);
            }
            else
            {
                Vector3 axis = Skeleton.LeafAxis.Value;
                var forward = new Vector3(view.M13, view.M23, view.M33);

                Vector3 right = Vector3.Cross(forward, axis);
                right.Normalize();
                Vector3 up = axis;

                Vector3.TransformNormal(ref right, ref view, out right);
                Vector3.TransformNormal(ref up, ref view, out up);

                LeafEffect.Parameters["BillboardRight"].SetValue(right);
                LeafEffect.Parameters["BillboardUp"].SetValue(up);
            }

            Skeleton.CopyBoneBindingMatricesTo(BindingMatrices, AnimationState.BoneRotations);
            LeafEffect.Parameters["Bones"].SetValue(BindingMatrices);

            // Used for normals of billboard leaves
            Vector3 treePos = Vector3.Transform(Vector3.Zero, BindingMatrices[0]*world);
            effect.Parameters["BillboardNormal"].SetValue(cameraPos - treePos);

            LeafEffect.Parameters["Texture"].SetValue(LeafTexture);

            Leaves.Draw(LeafEffect);
        }
    }
}