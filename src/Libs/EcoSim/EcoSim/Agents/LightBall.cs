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
using Sim.Utility;

namespace Sim
{
    class LightBall : ModelEntity, ITargetable
    {
        public event EventHandler Updated;

        private String name;
        private String info;
        private PointLight light;
        private int randomOffset;

        private LinkedList<Vector3> waypoints;
        private LinkedListNode<Vector3> currentWaypoint;
        private float distanceToTravel;
        private Vector3 velocity;
        private float speed = 1;

        public String Name { get { return name; } }
        public String Info { get { return info; } }

        public LightBall(PointLight light)
            : base(Models.Sphere, light.Position)
        {
            name = "Light Ball";
            info = String.Format("- Color: {0}", light.Color);

            this.light = light;
            diffuseColor = light.Color;
            ambientColor = light.Color;
            specularColor = light.Color;
            scale = 0.8f;
            alpha = 0.5f;
            randomOffset = new Random().Next(1000);
        }

        public virtual void Update(float worldTime, Sim.Environment.Terrain t)
        {
            float floorPos = t.CalculateHeight(Position.X, Position.Z) + 20;

            if (currentWaypoint != null)
            {
                // the agent has a waypoint in the queue
                if (distanceToTravel <= 0)
                {
                    // the agent has reached or just passed its destination, so it uses the next waypoint
                    currentWaypoint = currentWaypoint.Next;
                    SetMovement();
                }
                else
                {
                    distanceToTravel -= speed;
                }
            }

            position = position + velocity;
            position.Y = (float)Math.Sin(worldTime * 0.006f + randomOffset)*4 + floorPos + 5;
            light.MoveTo(Position);

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
                info = String.Format("- Color: {0}", light.Color);
            }
        }

        public void MoveTo(Vector3 position)
        {

        }

        public void SetWaypoints(LinkedList<Vector3> waypoints)
        {
            if (waypoints != null)
            {
                this.waypoints = waypoints;
                currentWaypoint = waypoints.First;
                SetMovement();
            }
        }

        private void SetMovement()
        {
            if (currentWaypoint == null)
                velocity = Vector3.Zero;
            else
            {
                Vector2 horizPos = new Vector2(position.X, position.Z);
                Vector2 horizWP = new Vector2(currentWaypoint.Value.X, currentWaypoint.Value.Z);
                Vector2 toMove = Vector2.Normalize(horizWP - horizPos) * speed;

                distanceToTravel = Vector2.Distance(horizPos, horizWP);
                velocity = new Vector3(toMove.X, 0, toMove.Y);
            }
        }

        public void ChangeLightRadius(float amount)
        {
            light.ChangeRadius(amount);
        }

        public override void Draw(Sim.Interface.Camera cam, Sim.Environment.World world, GraphicsDevice g)
        {
            base.Draw(cam, world, g);
            if (waypoints != null)
            {

            }
        }
    }
}
