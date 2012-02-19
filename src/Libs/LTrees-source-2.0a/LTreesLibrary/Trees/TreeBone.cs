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

#endregion

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// A bone used for animating trees. 
    /// </summary>
    /// <remarks>
    /// The reference shader uses hardware skinning to animate trees.
    /// 
    /// A bone will typically control many branches, as it is too expensive to create a bone
    /// for each branch.
    /// 
    /// Bones are created by a <code>Bone</code> instruction in an XML tree specification.
    /// 
    /// TreeBones should not be modified when a tree is animated; instead see <see cref="TreeAnimationState"/>.
    /// </remarks>
    public struct TreeBone
    {
        /// <summary>
        /// Index of the branch where the bone ends. Note that both ancestors and children of this branch
        /// may be controlled by this bone.
        /// </summary>
        public int EndBranchIndex;

        /// <summary>
        /// Inverse absolute transformation in the frame of reference.
        /// </summary>
        public Matrix InverseReferenceTransform;

        /// <summary>
        /// Length of the bone.
        /// </summary>
        public float Length;

        /// <summary>
        /// Index of the bone's parent, or -1 if this is the root bone.
        /// </summary>
        public int ParentIndex;

        /// <summary>
        /// Absolute transformation in the frame of reference.
        /// </summary>
        public Matrix ReferenceTransform;

        /// <summary>
        /// Bone's rotation relative to its parent, in the frame of reference.
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// Resistance to wind. By default, this is determined by the radius of the branch that spawned the bone.
        /// </summary>
        public float Stiffness;

        public TreeBone(Quaternion rotation, int parentIndex, Matrix referenceTransform,
                        Matrix inverseReferenceTransform, float length, float stiffness, int endBranchIndex)
        {
            Rotation = rotation;
            ParentIndex = parentIndex;
            ReferenceTransform = referenceTransform;
            InverseReferenceTransform = inverseReferenceTransform;
            Length = length;
            Stiffness = stiffness;
            EndBranchIndex = endBranchIndex;
        }
    }
}