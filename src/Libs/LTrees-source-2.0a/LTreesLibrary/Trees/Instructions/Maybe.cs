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
using System.Collections.Generic;

#endregion

namespace LTreesLibrary.Trees.Instructions
{
    /// <summary>
    /// Has a chance of performing a group of instructions; otherwise does nothing.
    /// </summary>
    public class Maybe : TreeCrayonInstruction
    {
        private float chance;
        private List<TreeCrayonInstruction> instructions = new List<TreeCrayonInstruction>();

        public Maybe(float chance)
        {
            this.chance = chance;
        }

        /// <summary>
        /// Probability that the instructions will be executed. Should be between 0.0f and 1.0f.
        /// </summary>
        public float Chance
        {
            get { return chance; }
            set { chance = value; }
        }


        public List<TreeCrayonInstruction> Instructions
        {
            get { return instructions; }
            set { instructions = value; }
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            if (rnd.NextDouble() < chance)
            {
                foreach (TreeCrayonInstruction child in instructions)
                {
                    child.Execute(crayon, rnd);
                }
            }
        }

        #endregion
    }
}