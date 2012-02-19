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
    public class Bone : TreeCrayonInstruction
    {
        public Bone(int delta)
        {
            Delta = delta;
        }

        public int Delta { get; set; }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            crayon.Bone(Delta);
        }

        #endregion
    }
}