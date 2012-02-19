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
    /// Describes a leaf on a tree. Leaves are always placed at the end of a branch.
    /// </summary>
    public struct TreeLeaf
    {
        /// <summary>
        /// Leaf's position offset along the leaf axis. Only used when the leaf axis is non-null on the tree skeleton.
        /// LeafAxis * AxisOffset will be added to the leaf's position on the branch.
        /// </summary>
        public float AxisOffset;

        /// <summary>
        /// Index of the bone controlling this leaf.
        /// </summary>
        public int BoneIndex;

        /// <summary>
        /// Color tint of the leaf.
        /// </summary>
        public Vector4 Color;

        /// <summary>
        /// Index of the branch carrying the leaf.
        /// </summary>
        public int ParentIndex;

        /// <summary>
        /// Rotation of the leaf, in radians.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// Width and height of the leaf.
        /// </summary>
        public Vector2 Size;

        public TreeLeaf(int parentIndex, Vector4 color, float rotation, Vector2 size, int boneIndex, float axisOffset)
        {
            ParentIndex = parentIndex;
            Color = color;
            Rotation = rotation;
            Size = size;
            BoneIndex = boneIndex;
            AxisOffset = axisOffset;
        }
    }
}