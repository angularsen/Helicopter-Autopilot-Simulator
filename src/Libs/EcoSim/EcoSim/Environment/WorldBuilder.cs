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
using System.Threading;
using Sim.Interface;

namespace Sim.Environment
{
    /// <summary>
    /// Builds all the necessary components of a world and then sends them over to the simulation's world
    /// </summary>
    public class WorldBuilder
    {
        private SimEngine sim;
        private BuildData data;
        private String task = "Loading...";
        private float progress = 0.0f;

        public String Task { get { return task; } }
        public float Progress { get { return progress; } }

        public WorldBuilder(SimEngine sim, BuildData data)
        {
            this.sim = sim;
            this.data = data;
        }

        /// <summary>
        /// Starts building every world component
        /// </summary>
        public void Build()
        {
            sim.World.Paused = true;

            task = "Building Terrain";
            Terrain newTerrain = new Terrain(data, sim);
            newTerrain.LoadContent(sim.Content, data.Climate);
            newTerrain.Build();
            sim.World.LoadTerrain(newTerrain);
            progress = 0.3f;

            task = "Building Water";
            Water newWater = new Water(sim);
            newWater.LoadContent(sim.Content, newTerrain);
            sim.World.Water = newWater;
            progress = 0.35f;

            task = "Building Vegetation";
            Vegetation newVegetation = new Vegetation(sim, data);
            newVegetation.LoadContent(sim.Content, newTerrain, data.Climate);
            sim.World.Vegetation = newVegetation;
            progress = 0.6f;

            task = "Building Pathfinding Nodes";
            LinkedList<Vector3> nodes = new LinkedList<Vector3>();
            
            task = "Building Weather";
            sim.World.Weather.BuildWeather(newTerrain, data.Climate);

            sim.World.Paused = false;
            progress = 1.0f;
        }
    }
}
