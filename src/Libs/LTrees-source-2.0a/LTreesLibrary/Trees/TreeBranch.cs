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
    /// Describes one branch segment in a tree.
    /// </summary>
    /// <remarks>
    /// A branch's origin is always somewhere on its parent branch (except for the root branch).
    /// </remarks>
    /// <see cref="TreeSkeleton"/>
    public struct TreeBranch
    {
        /// <summary>
        /// Index of the bone controlling this branch.
        /// </summary>
        public int BoneIndex;

        /// <summary>
        /// Radius at the end of the branch.
        /// </summary>
        public float EndRadius;

        /// <summary>
        /// Length of the branch.
        /// </summary>
        public float Length;

        /// <summary>
        /// Index of the parent branch, or -1 if this is the root branch.
        /// </summary>
        public int ParentIndex;

        /// <summary>
        /// Where on the parent the branch is located. 0.0f is at the start, 1.0f is at the end.
        /// </summary>
        public float ParentPosition;

        /// <summary>
        /// Rotation relative to the branch's parent.
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// Radius at the start of the branch.
        /// </summary>
        public float StartRadius;

        public TreeBranch(Quaternion rotation,
                          float length,
                          float startRadius,
                          float endRadius,
                          int parentIndex,
                          float parentPosition)
        {
            Rotation = rotation;
            Length = length;
            StartRadius = startRadius;
            EndRadius = endRadius;
            ParentIndex = parentIndex;
            ParentPosition = parentPosition;
            BoneIndex = -1;
        }
    }
}