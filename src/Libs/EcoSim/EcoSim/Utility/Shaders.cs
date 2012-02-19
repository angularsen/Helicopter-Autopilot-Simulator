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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Sim
{
    /// <summary>
    /// Shared shader effects
    /// </summary>
    public class Shaders
    {
        public static BasicEffect Primitive;        // Microsoft's standard XNA Shader
        public static Effect PointSprite;
        public static Effect Common;                // dummy effect to link shared parameters
        public static Effect SMEffect;              // shadowmap effect

        public static void LoadContent(GraphicsDevice g, ContentManager cm)
        {
            Primitive = new BasicEffect(g, null);
            PointSprite = cm.Load<Effect>(@"shaders\PointSprite");
            Common = cm.Load<Effect>(@"shaders\common");
            SMEffect = cm.Load<Effect>(@"shaders\Shadowmap");

            Primitive.VertexColorEnabled = true;
        }
    }
}
