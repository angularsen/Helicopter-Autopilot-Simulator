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

namespace LTreesLibrary.Trees.Wind
{
    public interface WindSource
    {
        /// <summary>
        /// Returns the direction and strength of the wind, in a given position in the tree.
        /// </summary>
        /// <param name="position">Position local to the tree.</param>
        /// <returns>Wind strength. 1 is a light wind, 10 is medium, 50 is strong, and 100 is hurricane.</returns>
        Vector3 GetWindStrength(Vector3 position);
    }
}