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
    public class ScaleRadius : TreeCrayonInstruction
    {
        private float scale;
        private float variation;

        public ScaleRadius(float scale, float variation)
        {
            this.scale = scale;
            this.variation = variation;
        }

        public float Variation
        {
            get { return variation; }
            set { variation = value; }
        }


        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            crayon.ScaleRadius(Util.Random(rnd, scale, variation));
        }

        #endregion
    }
}