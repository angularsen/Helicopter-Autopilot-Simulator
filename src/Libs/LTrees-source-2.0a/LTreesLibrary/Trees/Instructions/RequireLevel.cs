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
    public enum CompareType
    {
        Less,
        Greater
    }

    public class RequireLevel : TreeCrayonInstruction
    {
        public RequireLevel(int level, CompareType type)
        {
            Instructions = new List<TreeCrayonInstruction>();
            Level = level;
            Type = type;
        }

        public List<TreeCrayonInstruction> Instructions { get; private set; }
        public int Level { get; set; }
        public CompareType Type { get; set; }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            if (crayon.Level >= Level && Type == CompareType.Greater ||
                crayon.Level <= Level && Type == CompareType.Less)
            {
                foreach (TreeCrayonInstruction instruction in Instructions)
                {
                    instruction.Execute(crayon, rnd);
                }
            }
        }

        #endregion
    }
}