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
    public class Backward : TreeCrayonInstruction
    {
        private float distance;
        private float distanceVariation;

        public Backward(float distance, float variation)
        {
            this.distance = distance;
            distanceVariation = variation;
        }

        public float DistanceVariation
        {
            get { return distanceVariation; }
            set { distanceVariation = value; }
        }


        public float Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            crayon.Backward(Util.Random(rnd, distance, distanceVariation));
        }

        #endregion
    }
}