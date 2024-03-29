#region Copyright

// A�DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright � 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

#region Using

using System;
using System.Collections.Generic;

#endregion

namespace LTreesLibrary.Trees.Instructions
{
    public class Production
    {
        private List<TreeCrayonInstruction> instructions = new List<TreeCrayonInstruction>();

        public List<TreeCrayonInstruction> Instructions
        {
            get { return instructions; }
            set { instructions = value; }
        }

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            foreach (TreeCrayonInstruction instruction in instructions)
            {
                instruction.Execute(crayon, rnd);
            }
        }
    }
}