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

#endregion

namespace LTreesLibrary.Trees.Wind
{
    /// <summary>
    /// Animated trees by simulating wind. It is not meant as an accurate animation.
    /// </summary>
    public class TreeWindAnimator
    {
        private WindSource wind;

        public TreeWindAnimator(WindSource wind)
        {
            this.wind = wind;
        }

        public WindSource WindSource
        {
            get { return wind; }
            set { wind = value; }
        }

        public void Animate(TreeSkeleton skeleton, TreeAnimationState state, float seconds)
        {
            var transforms = new Matrix[skeleton.Bones.Count];
            skeleton.CopyAbsoluteBoneTranformsTo(transforms, state.BoneRotations);
            for (int i = 0; i < state.BoneRotations.Length; i++)
            {
                Vector3 dir = Vector3.Transform(Vector3.Up, skeleton.Bones[i].Rotation);
                Vector3 windstr = wind.GetWindStrength(Vector3.Zero);
                Vector3 axis = Vector3.Cross(dir, windstr);
                float strength = axis.Length();
                axis.Normalize();

                // Move the axis from tree space into branch space
                axis = Vector3.TransformNormal(axis, skeleton.Bones[i].InverseReferenceTransform);

                // Normalize strength
                strength = 1.0f - (float) Math.Exp(-0.01f*strength/skeleton.Bones[i].Stiffness);

                Quaternion q = Quaternion.CreateFromAxisAngle(axis, strength*MathHelper.PiOver2);
                state.BoneRotations[i] = skeleton.Bones[i].Rotation*q;
            }
        }

        public void Animate(TreeSkeleton skeleton, TreeAnimationState state, GameTime time)
        {
            float seconds = time.ElapsedGameTime.Milliseconds/1000.0f;

            Animate(skeleton, state, seconds);
        }
    }
}