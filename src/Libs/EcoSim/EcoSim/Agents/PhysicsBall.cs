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

namespace Sim
{
    class PhysicsBall : ModelEntity, ITargetable
    {
        public event EventHandler Updated;

        public LightBall target;

        protected Vector3 Velocity;
        protected float slowDown = 0.01f;
        protected float maxSpeed = 1;
        protected bool falling;
        protected bool rising;
        protected float radius;

        protected float gravity = 0.06f;
        protected float bounceFriction = 0.4f;
        protected float waitToJumpTimer = 0;
        protected bool waitingToJump = false;

        private String name;
        private String info;

        public String Name { get { return name; } }
        public String Info { get { return info; } }

        public PhysicsBall(Vector3 position, LightBall target) 
            : base(Models.Sphere, position)
        {
            this.target = target;
            scale = 0.8f;
            radius = Models.Sphere.Meshes[0].BoundingSphere.Radius * scale;
            name = "Physics Ball";
            info = "Behavior: Normal";
            diffuseColor = new Vector3(1, 0, 0);
            ambientColor = new Vector3(0.3f);
            specularColor = Vector3.One;
            alpha = 1.0f;
        }

        public void Update(Sim.Environment.Terrain t, List<PhysicsBall> pballs)
        {
            float floorHeight = t.CalculateHeight(position.X, position.Z) + radius;
            bool aboveTerrain = position.Y > floorHeight;

            if (aboveTerrain && !rising && !falling)
            {
                if (position.Y - floorHeight > 0.5f) // angle is great enough to slip
                    falling = true;
                else
                    position.Y = floorHeight;

            }

            if (position.Y < floorHeight)
                position.Y = floorHeight;

            if (falling)
            {
                diffuseColor = new Vector3(1, 1, 0);
                info = "Behavior: Falling";
                waitingToJump = false;
                waitToJumpTimer = 0;
                Velocity.Y -= gravity;
                if (position.Y <= floorHeight)
                {
                    Velocity.Y *= -bounceFriction;   // friction takes away some of the energy
                    if (Velocity.Y > 0.09f)
                    {
                        rising = true;
                        Vector3 bounceV = t.CalculateSurfaceNormal(position.X, position.Z) * bounceFriction;
                        Velocity.X += bounceV.X;
                        Velocity.Z += bounceV.Z;
                    }
                    else
                        Velocity.Y = 0;
                    falling = false;
                }
            }
            else if (rising)
            {
                diffuseColor = new Vector3(1, 1, 0);
                info = "Behavior: Rising";
                waitingToJump = false;
                waitToJumpTimer = 0;
                Velocity.Y -= gravity;
                if (Velocity.Y <= 0)
                {
                    rising = false;
                    falling = true;
                }
            }
            else if (Velocity.X == 0 && Velocity.Z == 0)
            {
                info = "Behavior: Waiting to jump";
                waitingToJump = true;
                diffuseColor = new Vector3(1, 0, 0);
            }

            float xzFriction;
            if (aboveTerrain)
                xzFriction = 0;
            else
                xzFriction = slowDown;

            if (Velocity.X < 0)
                Velocity.X = MathHelper.Clamp(Velocity.X + xzFriction, -maxSpeed, 0);
            else if (Velocity.X > 0)
                Velocity.X = MathHelper.Clamp(Velocity.X - xzFriction, 0, maxSpeed);

            if (Velocity.Z < 0)
                Velocity.Z = MathHelper.Clamp(Velocity.Z + xzFriction, -maxSpeed, 0);
            else if (Velocity.Z > 0)
                Velocity.Z = MathHelper.Clamp(Velocity.Z - xzFriction, 0, maxSpeed);

            // should only update if velocity is nonzero
            position += Velocity;

            Rotation.Z -= Velocity.X / radius;
            Rotation.X -= Velocity.Z / radius;

            if (waitingToJump)
            {
                waitToJumpTimer += 16;
                if (waitToJumpTimer > 1500)
                {
                    Vector3 tV = target.Position - position;
                    tV.Normalize();
                    Velocity += tV * new Random().Next(5);
                    info = "Behavior: Jumping!";
                }
            }

            if (Updated != null)
                Updated(this, EventArgs.Empty);
        }
    }
}
