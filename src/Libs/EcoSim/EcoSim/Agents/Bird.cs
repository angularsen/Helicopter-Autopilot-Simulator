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
    public class Bird : ModelEntity, IAgentFlockable, ITargetable
    {
        public static Steering.SteerParameters DefaultBirdFlockParameters =
            new Steering.SteerParameters(0.3f, 40, 0.2f, 100, 3.0f, 100, 0.55f, 0.2f, null, Vector3.Zero, 0.0f);
        public static Steering.SteerParameters DefaultBirdWanderParameters =
            new Steering.SteerParameters(1.0f, 60, 1.0f, 200, 1.0f, 200, 1.0f, 0.3f, null, Vector3.Zero, 0.0f);

        public enum Behavior
        {
            Flocking,
            Wandering,
            Landing
        };

        private Steering.SteerParameters steerParams;
        private Vector3 direction = Vector3.UnitX;
        private Behavior behavior = Behavior.Flocking;
        private float speed = 1.0f;

        public Vector3 Direction { get { return direction; } set { direction = value; } }
        public string CurrentBehavior { get { return behavior.ToString(); } }

        // ITargetable
        public event EventHandler Updated;
        public string Name { get { return "Birdy"; } }
        public string Info { get { return behavior.ToString(); } }

        private Random r = new Random();

        public Bird(Vector3 position, Sim.Environment.World world)
            : base(Models.Birdy, position)
        {
            steerParams.World = world;
            SetBehavior(Behavior.Flocking);
            
            scale = 0.5f;
            diffuseColor = new Vector3(0.2f, 0.2f, 0.2f);
            ambientColor = new Vector3(0.3f);
            specularColor = Vector3.Zero;
            alpha = 1.0f;
        }

        public void SetSteeringParameters(Steering.SteerParameters steerParams)
        {
            this.steerParams = steerParams;
        }

        public void SetBehavior(Behavior behavior)
        {
            Sim.Environment.World w = steerParams.World;

            this.behavior = behavior;
            switch (behavior)
            {
                case Behavior.Flocking:
                    diffuseColor = Color.Black.ToVector3();
                    steerParams = DefaultBirdFlockParameters;
                    steerParams.World = w;
                    break;
                case Behavior.Wandering:
                    diffuseColor = Color.PaleGreen.ToVector3();
                    steerParams = DefaultBirdWanderParameters;
                    steerParams.World = w;
                    break;
            }
        }

        public override void Draw(Sim.Interface.Camera cam, Sim.Environment.World world, Microsoft.Xna.Framework.Graphics.GraphicsDevice g)
        {
            // rotate the bird
            rotMat = Matrix.Identity;
            Vector3 rotAxis = Vector3.Cross(direction, -Vector3.UnitX);
            if (rotAxis != Vector3.Zero)
            {
                rotAxis.Normalize();
                float angle = (float)Math.Acos(Vector3.Dot(direction, Vector3.UnitX)) + MathHelper.Pi;
                rotMat = Matrix.CreateFromAxisAngle(rotAxis, angle);
            }

            base.Draw(cam, world, g);
        }

        public void Update(float worldTime)
        {
        }

        public void Update(float worldTime, IAgent[] flockMembers)
        {
            switch (behavior)
            {
                case Behavior.Flocking:
                    Behaviors.Flock(this, flockMembers, ref steerParams);
                    break;
                case Behavior.Wandering:
                    Behaviors.Wander(this, flockMembers, ref steerParams);
                    break;
            }

            position += direction * speed * MathHelper.Clamp((float)Math.Cos(worldTime / 1000) + 2,1.0f,1.5f);
            
            if (Updated != null)
                Updated.Invoke(this, null);
        }
    }
}
