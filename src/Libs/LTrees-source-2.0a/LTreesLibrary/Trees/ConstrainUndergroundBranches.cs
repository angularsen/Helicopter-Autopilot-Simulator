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
    /// Removes branches too close to the ground. Branches that point upwards are never affected by this constraint.
    /// </summary>
    public class ConstrainUndergroundBranches : TreeContraints
    {
        private float limit = 256.0f;

        public ConstrainUndergroundBranches()
        {
        }

        public ConstrainUndergroundBranches(float limit)
        {
            this.limit = limit;
        }

        /// <summary>
        /// Minimum allowed Y-coordinate for branches pointing downwards.
        /// </summary>
        public float Limit
        {
            get { return limit; }
            set { limit = value; }
        }

        #region TreeContraints Members

        public bool ConstrainForward(TreeCrayon crayon, ref float distance, ref float radiusEndScale)
        {
            Matrix m = crayon.GetTransform();
            if (m.Up.Y < 0.0f && m.Translation.Y + m.Up.Y*distance < limit)
                return false; // distance *= 100; // Use distance * 100 to visualize which branches will be cut.
            return true;
        }

        #endregion
    }
}