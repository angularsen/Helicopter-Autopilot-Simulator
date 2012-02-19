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

namespace Sim
{
    public class Wildlife
    {
        private SimEngine sim;
        private List<ITargetable> targets = new List<ITargetable>();
        private List<PhysicsBall> pballs = new List<PhysicsBall>();
        private List<LightBall> lightModels = new List<LightBall>();

        private List<Bird> birds = new List<Bird>();
        private Flock birdFlock;

        public Wildlife(SimEngine sim)
        {
            this.sim = sim;

            Random r = new Random();
            int nBirds = 90;
            birdFlock = new Flock(nBirds);
            for (int i = 0; i < nBirds; i++)
            {
                Bird bird = new Bird(new Vector3(r.Next(1000),r.Next(50,200), r.Next(1000)), sim.World);
                birdFlock.AddAgent(bird);
                targets.Add(bird);
            }
        }

        public void AddBall(Vector3 pos)
        {
            Sim.Environment.Lighting lighting = sim.World.Lighting;
            if (lighting.NumberOfLights < 8)
            {
                Sim.Utility.PointLight l = lighting.Lights[lighting.NumberOfLights++];
                l.MoveTo(pos);
                LightBall newLight = new LightBall(l);

                lightModels.Add(newLight);
                targets.Add(newLight);
            }
            else
            {
                PhysicsBall newPBall = new PhysicsBall(pos, lightModels[new Random().Next(8)]);
                pballs.Add(newPBall);
                targets.Add(newPBall);
            }

        }

        public void LoadContent()
        {

        }

        /// <summary>
        /// Finds the closest targetable object that the ray intersects
        /// </summary>
        public ITargetable Select(Ray r)
        {
            float closestDistance = -1;
            ITargetable closestSelection = null;
            foreach (ITargetable t in targets)
            {
                if (t.GetBoundingSphere().Intersects(r) != null)
                {
                    float dist = Vector3.Distance(t.Position, sim.UI.Camera.Position);
                    if (closestSelection == null || dist < closestDistance)
                    {
                        closestSelection = t;
                        closestDistance = dist;
                    }
                }
            }
            return closestSelection;
        }

        public void Update(float worldTime)
        {
            if (sim.World.Terrain != null)
            {
                foreach (LightBall l in lightModels)
                    l.Update(worldTime, sim.World.Terrain);
                foreach (PhysicsBall p in pballs)
                    p.Update(sim.World.Terrain, pballs);

                birdFlock.Update(worldTime);
            }
        }

        public void ApplySteerParameters(Steering.SteerParameters steerParams)
        {
            foreach (Bird b in birdFlock.Agents)
                b.SetSteeringParameters(steerParams);
        }

        public void Draw(float worldTime)
        {
            ITargetable selected = sim.UI.ScreenManager.WorldScreen.Selected;
            foreach (LightBall l in lightModels)
            {
                l.Draw(sim.UI.Camera, sim.World, sim.GraphicsDevice);
            }

            foreach (ModelEntity e in pballs)
            {
                e.Draw(sim.UI.Camera, sim.World, sim.GraphicsDevice);
                if (Sim.Settings.Graphics.Default.ShowBBs)
                    Sim.Utility.Draw.BoundingBox(sim.GraphicsDevice, e.GetBoundingBox());
            }

            foreach (Bird b in birdFlock.Agents)
                b.Draw(sim.UI.Camera, sim.World, sim.GraphicsDevice);

            if (selected is ModelEntity)
            {
                ModelEntity m = selected as ModelEntity;
                Sim.Utility.Draw.BoundingBox(sim.GraphicsDevice, m.GetBoundingBox());
            }
        }
    }
}
