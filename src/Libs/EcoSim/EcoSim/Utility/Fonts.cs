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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Sim
{
    /// <summary>
    /// Collection of spritefonts
    /// </summary>
    class Fonts
    {
        public static SpriteFont Arial;
        public static SpriteFont Tahoma;
        public static SpriteFont Calibri;
        public static SpriteFont Verdana;

        public static void LoadContent(ContentManager cm)
        {
            Arial = cm.Load<SpriteFont>(@"fonts\Arial");
            Tahoma = cm.Load<SpriteFont>(@"fonts\Tahoma");
            Calibri = cm.Load<SpriteFont>(@"fonts\Calibri");
            Verdana = cm.Load<SpriteFont>(@"fonts\Verdana");
        }
    }
}
