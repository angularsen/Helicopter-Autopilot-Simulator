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

namespace Sim.Environment
{
    class WeatherRegion
    {
        public Rectangle Base;
        public Vector3 Center;
        public WeatherRegion AdjacentH;
        public WeatherRegion AdjacentV;
        public WeatherEffect WeatherEffect;

        private float ceiling = 500;
        private BoundingBox boundingBox;
        public BoundingBox BoundingBox { get { return boundingBox; } }

        public WeatherRegion(Rectangle area)
        {
            this.Base = area;
            this.Center = new Vector3((area.X + area.Right) / 2.0f, 0, (area.Y + area.Bottom) / 2.0f);

            UpdateBoundingBox();
        }

        private void UpdateBoundingBox()
        {
            Vector3 min = new Vector3(Base.X, 0, Base.Y);
            Vector3 max = new Vector3(Base.Right, ceiling, Base.Bottom);
            boundingBox = new BoundingBox(min, max);
        }

        public void Shift(int x, int y)
        {
            Base.X += x;
            Base.Y += y;
            Center = new Vector3((Base.X + Base.Right) / 2.0f, 0, (Base.Y + Base.Bottom) / 2.0f);
            UpdateBoundingBox();

            WeatherEffect.Effect.Parameters["vOrigin"].SetValue(new Vector3(Base.X, 0, Base.Y));
        }

        public void BuildEffect(Type weatherType, int numParticles, float drawDist, SimEngine sim)
        {
            if (weatherType == typeof(Rain))
                WeatherEffect = new Rain();
            else if (weatherType == typeof(Snow))
                WeatherEffect = new Snow();

            WeatherEffect.LoadContent(sim.Content, sim.GraphicsDevice);
            WeatherEffect.Build(new Cube(new Vector3(Base.X,0,Base.Y), ceiling, Base.Width, Base.Height), numParticles, sim.GraphicsDevice);
            WeatherEffect.Effect.Parameters["fDrawDist"].SetValue(drawDist * 0.9f);
        }
    }
}
