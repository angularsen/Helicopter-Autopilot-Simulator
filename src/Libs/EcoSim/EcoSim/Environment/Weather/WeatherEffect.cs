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
using Microsoft.Xna.Framework.Content;

namespace Sim.Environment
{
    abstract class WeatherEffect
    {
        public Matrix WorldMatrix = Matrix.Identity;
        public Effect Effect;
        protected Texture2D texture;
        protected VertexBuffer vBuffer;
        protected VertexDeclaration vDec;

        public abstract void LoadContent(ContentManager cm, GraphicsDevice g);
        public abstract void Build(Cube c, int numParticles, GraphicsDevice g);
        public abstract void Draw(GraphicsDevice g, float worldTime, Sim.Interface.Camera cam, Vector3 color);
    }
}
