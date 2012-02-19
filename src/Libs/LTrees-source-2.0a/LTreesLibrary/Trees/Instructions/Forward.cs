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

#endregion

namespace LTreesLibrary.Trees.Instructions
{
    public class Forward : TreeCrayonInstruction
    {
        private float distance;
        private float radiusScale;
        private float variation;

        public Forward(float distance, float variation, float radiusScale)
        {
            this.distance = distance;
            this.variation = variation;
            this.radiusScale = radiusScale;
        }

        public float RadiusScale
        {
            get { return radiusScale; }
            set { radiusScale = value; }
        }

        public float Variation
        {
            get { return variation; }
            set { variation = value; }
        }


        public float Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            crayon.Forward(distance + variation*((float) rnd.NextDouble()*2 - 1), radiusScale);
        }

        #endregion
    }
}