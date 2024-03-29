#region Copyright

// A�DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright � 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// Modifies the creation of branches on a tree, to make sure it meets certain requirements, such as
    /// no branches sticking underground.
    /// </summary>
    public interface TreeContraints
    {
        /// <summary>
        /// Called whenever a branch is about to be created by the TreeCrayon.
        /// This may alter the crayon prior the creation of the branch, or it may prohibit the creation
        /// of the branch by returning false.
        /// </summary>
        /// <param name="crayon">The crayon about to create a branch.</param>
        /// <param name="distance">The distance of the branch to be created. May be altered.</param>
        /// <param name="radiusEndScale">Radius end scale of the branch to be created. May be altered.</param>
        /// <returns>True if the branch may be created, false it if should be cancelled.</returns>
        bool ConstrainForward(TreeCrayon crayon, ref float distance, ref float radiusEndScale);
    }
}