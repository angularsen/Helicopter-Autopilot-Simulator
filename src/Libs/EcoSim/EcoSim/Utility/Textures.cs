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
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Sim.Interface
{
    /// <summary>
    /// Commonly used textures
    /// </summary>
    class Textures
    {
        private const string HM_DIRECTORY = "Content\\Heightmaps";

        public static Texture2D[] Heightmaps;

        public static void LoadHeightmaps(GraphicsDevice gd)
        {
            DirectoryInfo directory = new DirectoryInfo(HM_DIRECTORY);
            FileInfo[] jpegs = directory.GetFiles("*.jpg");
            FileInfo[] bmps = directory.GetFiles("*.bmp");
            FileInfo[] pngs = directory.GetFiles("*.png");
            FileInfo[] images = new FileInfo[jpegs.Length + bmps.Length + pngs.Length];

            jpegs.CopyTo(images, 0);
            bmps.CopyTo(images, jpegs.Length);
            pngs.CopyTo(images, jpegs.Length + bmps.Length);

            Heightmaps = new Texture2D[images.Length];

            for (int i = 0; i < Heightmaps.Length; i++)
            {
                Texture2D heightmap = Texture2D.FromFile(gd, "Content\\Heightmaps\\" + images[i].Name);
                heightmap.Name = images[i].Name;
                Heightmaps[i] = heightmap;
            }
        }
    }
}
