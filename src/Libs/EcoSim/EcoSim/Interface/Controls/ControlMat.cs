/* 
 * Environment Simulator
 * Copyright (C) 2008-2009 Justin Stoecker
 * 
 * This program is free software; you can redistribute it and/or modify it under the terms of the 
 * GNU General Public License as published by the Free Software Foundation; either version 2 of 
 * the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sim.Interface
{
    /// <summary>
    /// An collection of spaces that may assigned to individual controls for organization
    /// </summary>
    class ControlMat
    {
        private Rectangle area;                 // total area occupied by the spaces
        private Rectangle[] spaces;             // the individual spaces

        public enum FillOrder
        {
            RowsFirst,
            ColumnsFirst
        }

        public Rectangle[] Spaces { get { return spaces; } }

        public ControlMat(Rectangle area, int rows, int cols, int pad, FillOrder fillOrder)
        {
            this.area = area;
            CalculateSpaces(rows, cols, pad, fillOrder);
        }

        public ControlMat(Rectangle area, int rows, int cols, int pad)
            : this(area,rows,cols,pad,FillOrder.RowsFirst)
        {
        }

        /// <summary>
        /// Calculates the areas for individual buttons which will fill the button area
        /// </summary>
        private void CalculateSpaces(int rows, int cols, int pad, FillOrder fillOrder)
        {
            // initialize space dimensions and arrays
            int spaceWidth = (area.Width - (cols - 1) * pad - 2 * pad) / cols;
            int spaceHeight = (area.Height - (rows - 1) * pad - 2 * pad) / rows;
            spaces = new Rectangle[cols * rows];

            if (fillOrder == FillOrder.RowsFirst)
            {
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        int x = area.X + col * (pad + spaceWidth) + pad;
                        int y = area.Y + row * (pad + spaceHeight) + pad;
                        spaces[row * cols + col] = new Rectangle(x, y, spaceWidth, spaceHeight);
                    }
                }
            }
            else
            {
                for (int col = 0; col < cols; col++)
                {
                    for (int row = 0; row < rows; row++)
                    {
                        int x = area.X + col * (pad + spaceWidth) + pad;
                        int y = area.Y + row * (pad + spaceHeight) + pad;
                        spaces[col * rows + row] = new Rectangle(x, y, spaceWidth, spaceHeight);
                    }
                }
            }
        }

        public void Draw(SpriteBatch sb)
        {
            for (int i = 0; i < spaces.Length; i++)
            {
                sb.Draw(UserInterface.TexBlank, spaces[i], Color.Black);
                sb.DrawString(Fonts.Tahoma, i.ToString(), new Vector2(spaces[i].X, spaces[i].Y),
                    Color.White);
            }
        }
    }
}
