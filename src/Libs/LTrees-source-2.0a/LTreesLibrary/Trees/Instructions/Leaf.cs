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
    public class Leaf : TreeCrayonInstruction
    {
        private Vector4 color;
        private Vector4 colorVariation;
        private Vector2 size;
        private Vector2 sizeVariation;

        public Leaf(Vector4 color, Vector4 colorVariation)
        {
            this.color = color;
            this.colorVariation = colorVariation;
            size = new Vector2(128, 128);
        }

        public Leaf()
        {
            color = new Vector4(1, 1, 1, 1);
            colorVariation = Vector4.Zero;
            Size = new Vector2(128, 128);
        }

        public Vector2 SizeVariation
        {
            get { return sizeVariation; }
            set { sizeVariation = value; }
        }


        public Vector2 Size
        {
            get { return size; }
            set { size = value; }
        }


        public Vector4 ColorVariation
        {
            get { return colorVariation; }
            set { colorVariation = value; }
        }

        public Vector4 Color
        {
            get { return color; }
            set { color = value; }
        }

        public float AxisOffset { get; set; }

        #region TreeCrayonInstruction Members

        public void Execute(TreeCrayon crayon, Random rnd)
        {
            if (crayon.Level == 0)
            {
                float rotation = 0.0f;
                if (crayon.Skeleton.LeafAxis == null)
                    rotation = (float) rnd.NextDouble()*MathHelper.TwoPi;
                crayon.Leaf(rotation,
                            size + sizeVariation*(2.0f*(float) rnd.NextDouble() - 1.0f),
                            color + colorVariation*(2.0f*(float) rnd.NextDouble() - 1.0f),
                            AxisOffset);
            }
        }

        #endregion
    }
}