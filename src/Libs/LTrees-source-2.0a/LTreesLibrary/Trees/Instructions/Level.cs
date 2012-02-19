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
    public class Level : TreeCrayonInstruction
    {
        private int deltaLevel;

        public Level(int deltaLevel)
        {
            this.deltaLevel = deltaLevel;
        }

        public int DeltaLevel
        {
            get { return deltaLevel; }
            set { deltaLevel = value; }
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            crayon.Level = crayon.Level + deltaLevel;
        }

        #endregion
    }
}