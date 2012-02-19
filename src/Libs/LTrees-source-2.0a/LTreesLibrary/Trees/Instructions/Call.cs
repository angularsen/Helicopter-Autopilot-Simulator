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
    public class Call : TreeCrayonInstruction
    {
        private int deltaLevel;
        private List<Production> productions = new List<Production>();

        public Call(List<Production> productions, int deltaLevel)
        {
            this.productions = productions;
            this.deltaLevel = deltaLevel;
        }

        public int DeltaLevel
        {
            get { return deltaLevel; }
            set { deltaLevel = value; }
        }


        public List<Production> Productions
        {
            get { return productions; }
            set { productions = value; }
        }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            if (productions.Count == 0)
                throw new InvalidOperationException("No productions exist for call.");

            if (crayon.Level + deltaLevel < 0)
                return;

            crayon.Level = crayon.Level + deltaLevel;

            int i = rnd.Next(0, productions.Count);
            productions[i].Execute(crayon, rnd);

            crayon.Level = crayon.Level - deltaLevel;
        }

        #endregion
    }
}