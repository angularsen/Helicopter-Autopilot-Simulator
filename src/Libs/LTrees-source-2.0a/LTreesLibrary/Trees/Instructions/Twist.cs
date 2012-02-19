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

namespace LTreesLibrary.Trees.Instructions
{
    public class Twist : TreeCrayonInstruction
    {
        private float angle;
        private float variation;

        public Twist(float angle, float variation)
        {
            this.angle = MathHelper.ToRadians(angle);
            this.variation = MathHelper.ToRadians(variation);
        }

        public float Variation
        {
            get { return variation; }
            set { variation = value; }
        }


        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            crayon.Twist(Util.Random(rnd, angle, variation));
        }

        #endregion
    }
}