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
using Sim.Interface;

namespace Sim.Environment
{
    public class Weather
    {
        private SimEngine sim;
        private GraphicsDevice g;
        private Camera camera;

        public Boolean Active = false;
        public int drawDist = 150;         // maximum distance a particle is visible at

        // weather regions
        WeatherRegion[] regions = new WeatherRegion[4];
        WeatherRegion currentRegion;
        bool cameraRightOfCenter;         // camera is right of the current weather region's center
        bool cameraBelowCenter;         // camera is below the current weather region's center

        public Weather(SimEngine sim)
        {
            this.sim = sim;
            g = sim.GraphicsDevice;
            camera = sim.UI.Camera;
            camera.Moved += new EventHandler(UpdateRegions);
        }

        public void LoadContent()
        {
            // position the regions such that the camera is below and right of the center of regions[0]
            Vector3 offset = sim.UI.Camera.Position - new Vector3(drawDist * 1.5f);
            cameraRightOfCenter = true;
            cameraBelowCenter = true;

            // build the weather region areas
            for (int z = 0; z < 2; z++)
                for (int x = 0; x < 2; x++)
                    regions[z * 2 + x] = new WeatherRegion(new Rectangle((int)offset.X + x * drawDist * 2, (int)offset.Z + z * drawDist * 2, drawDist * 2, 2 * drawDist));

            // connect the weather regions to each other
            regions[0].AdjacentH = regions[1];
            regions[0].AdjacentV = regions[2];
            regions[1].AdjacentH = regions[0];
            regions[1].AdjacentV = regions[3];
            regions[2].AdjacentH = regions[3];
            regions[2].AdjacentV = regions[0];
            regions[3].AdjacentH = regions[2];
            regions[3].AdjacentV = regions[1];

            currentRegion = regions[0];
        }

        public void BuildWeather(Terrain t, Climate climate)
        {
            Type weatherType = climate == Climate.Polar ? typeof(Snow) : typeof(Rain);

            if (regions[0].WeatherEffect == null || regions[0].WeatherEffect.GetType() != weatherType)
            {
                double cubicArea = Math.Pow(drawDist, 3);       // visible weather area
                int numParticles = (int)(cubicArea * 0.02f / drawDist * 150);
                bool wasActive = Active;
                Active = false;

                foreach (WeatherRegion r in regions)
                    r.BuildEffect(weatherType, numParticles, drawDist, sim);
                Active = wasActive;
            }
        }

        private void UpdateRegions(object sender, EventArgs args)
        {
            Vector3 translation = (args as Camera.MoveArgs).translation;
            if (translation.X != 0 || translation.Z != 0)
            {
                CheckRegionChange();
                CheckRegionShift();
            }
        }

        private void CheckRegionChange()
        {
            if (camera.Position.X > currentRegion.Base.Right)
            {
                // the camera crossed the current region's right edge, so it is now left of the
                // horizontally adjacent region's center
                cameraRightOfCenter = false;
                currentRegion = currentRegion.AdjacentH;
            }
            else if (camera.Position.X < currentRegion.Base.X)
            {
                // the camera crossed the current region's left edge, so it is now right of the
                // horizontally adjacent region's center
                cameraRightOfCenter = true;
                currentRegion = currentRegion.AdjacentH;
            }

            if (camera.Position.Z > currentRegion.Base.Bottom)
            {
                // the camera crossed the current region's bottom edge, so it is now above the
                // vertically adjacent region's center
                cameraBelowCenter = false;
                currentRegion = currentRegion.AdjacentV;
            }
            else if (camera.Position.Z < currentRegion.Base.Y)
            {
                // the camera crossed the current region's top edge, so it is now below the
                // vertically adjacent region's center
                cameraBelowCenter = true;
                currentRegion = currentRegion.AdjacentV;
            }
        }

        private void CheckRegionShift()
        {
            // since each region is 2 * drawDist in width/length, regions must be shifted
            // twice this to hop over the current region
            int shift = 4 * drawDist;

            // horizontal shift: move the region to the side of the current and the one next to it
            if (cameraRightOfCenter && camera.Position.X <= currentRegion.Center.X)
            {
                // camera was right of center, but is now left of it
                cameraRightOfCenter = false;
                currentRegion.AdjacentH.Shift(-shift, 0);
                currentRegion.AdjacentH.AdjacentV.Shift(-shift, 0);
            }
            else if (!cameraRightOfCenter && camera.Position.X > currentRegion.Center.X)
            {
                // camera was left of center, but is now right of it
                cameraRightOfCenter = true;
                currentRegion.AdjacentH.Shift(shift, 0);
                currentRegion.AdjacentH.AdjacentV.Shift(shift, 0);
            }

            // vertical shift: move the region above the current and the one next to it
            if (cameraBelowCenter && camera.Position.Z <= currentRegion.Center.Z)
            {
                // camera was below center, and is now above it
                cameraBelowCenter = false;
                currentRegion.AdjacentV.Shift(0, -shift);
                currentRegion.AdjacentV.AdjacentH.Shift(0, -shift);
            }
            else if (!cameraBelowCenter && camera.Position.Z > currentRegion.Center.Z)
            {
                // camera was above center, and is now below it
                cameraBelowCenter = true;
                currentRegion.AdjacentV.Shift(0, shift);
                currentRegion.AdjacentV.AdjacentH.Shift(0, shift);
            }
        }

        public void Draw(float worldTime)
        {
            if (Active)
            {
                for (int i = 0; i < regions.Length; i++)
                    if (camera.Frustum.Intersects(regions[i].BoundingBox))
                        regions[i].WeatherEffect.Draw(g, worldTime, camera, sim.World.Lighting.TotalColor);
            }
        }
    }
}
