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

/*
 * Note: these algorithms are based on Craig Reynolds' work "Steering Behavior for Autonomous Characters"
 * more info here: http://www.red3d.com/cwr/steer/
 * 
 * This implementation is my own interpretation of his ideas and not taken or ported from OpenSteer
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sim
{
    public static class Steering
    {
        static Random rand = new Random();

        public struct SteerParameters
        {
            /// <summary>
            /// Scales how quickly the agent can change direction
            /// </summary>
            public float TurningWeight;

            /// <summary>
            /// Angle between wander vector and agent's direction
            /// </summary>
            public float WanderCurrentAngle;

            /// <summary>
            /// Scales the amount of turning
            /// </summary>
            public float WanderWeight;

            public float SeparationRange;
            public float SeparationWeight;
            public float CohesionWeight;
            public float CohesionRange;
            public float AlignmentWeight;
            public float AlignmentRange;
            public Sim.Environment.World World;
            public Vector3 GoalPosition;
            public float GoalWeight;

            public SteerParameters(float sepWeight, float sepRange, float cohWeight, float cohRange, float aliWeight,
                float aliRange, float wanWeight, float turnWeight, Sim.Environment.World world, Vector3 goalPosition, float goalWeight)
            {
                this.WanderCurrentAngle = 0;
                this.World = world;

                this.SeparationWeight = sepWeight;
                this.SeparationRange = sepRange;
                this.CohesionWeight = cohWeight;
                this.CohesionRange = cohRange;
                this.AlignmentWeight = aliWeight;
                this.AlignmentRange = aliRange;
                this.WanderWeight = wanWeight;
                this.TurningWeight = turnWeight;
                this.GoalPosition = goalPosition;
                this.GoalWeight = goalWeight;
            }
        };

        #region Steering Vectors

        /// <summary>
        /// Desire to randomly adjust current direction
        /// </summary>
        public static Vector3 WanderForce(IAgent agent, ref SteerParameters steerParams)
        {
            Vector3 wanderFocus = agent.Direction * 1.4f;

            // the current angle of the wander should be offset slightly
            steerParams.WanderCurrentAngle += (float)(rand.NextDouble() - 0.5f) * 2 * steerParams.TurningWeight;
            Vector3 offset = new Vector3((float)Math.Cos(steerParams.WanderCurrentAngle), 0, (float)Math.Sin(steerParams.WanderCurrentAngle));

            return Vector3.Normalize(wanderFocus + offset) * steerParams.WanderWeight;
        }

        /// <summary>
        /// Desire to maintain distance from other agents represented as a vector
        /// </summary>
        public static Vector3 SeparationForce(IAgent agent, IAgent otherAgent, SteerParameters steerParams)
        {
            return SeparationForce(agent, otherAgent, steerParams, Vector3.Distance(agent.Position, otherAgent.Position));
        }

        /// <summary>
        /// Desire to maintain distance from other agents represented as a vector
        /// </summary>
        public static Vector3 SeparationForce(IAgent agent, IAgent otherAgent, SteerParameters steerParams, float agentSeparation)
        {
            Vector3 separation = (agent.Position - otherAgent.Position) / agentSeparation;
            float magnitude = 1 - agentSeparation / steerParams.SeparationRange;

            return separation * magnitude * steerParams.SeparationWeight;
        }

        /// <summary>
        /// Desire to move toward members of a group represented as a vector
        /// </summary>
        public static Vector3 CohesionForce(IAgent agent, IAgent otherAgent, SteerParameters steerParams)
        {
            Vector3 cohesion = agent.Position == otherAgent.Position ? Vector3.Zero :
                Vector3.Normalize(otherAgent.Position - agent.Position);

            return cohesion * steerParams.CohesionWeight;
        }

        /// <summary>
        /// Desire to align direction with another agent
        /// </summary>
        public static Vector3 AlignmentForce(IAgent otherAgent, SteerParameters steerParams)
        {
            return otherAgent.Direction * steerParams.AlignmentWeight;
        }

        /// <summary>
        /// Desire to move toward a specific point
        /// </summary>
        public static Vector3 GoalForce(IAgent agent, SteerParameters steerParams)
        {
            return (steerParams.GoalPosition - agent.Position) * steerParams.GoalWeight;
        }

        /// <summary>
        /// Keeps agents inside the world and above the surface of the terrain
        /// </summary>
        public static Vector3 WorldBoundsForce(IAgent agent, SteerParameters steerParams)
        {
            Vector3 force = Vector3.Zero;

            float minY = steerParams.World.Terrain.CalculateHeight(agent.Position.X, agent.Position.Z) + 120;
            float maxY = minY * 4;
            float minX = 0;
            float maxX = steerParams.World.Terrain.Width;
            float minZ = 0;
            float maxZ = steerParams.World.Terrain.Length;

            if (agent.Position.X > maxX)
                force += Vector3.UnitX * (maxX - agent.Position.X) / steerParams.SeparationRange;
            else if (agent.Position.X < minX)
                force += Vector3.UnitX * (minX - agent.Position.X) / steerParams.SeparationRange;

            if (agent.Position.Y > maxY)
                force += Vector3.UnitY * (maxY - agent.Position.Y) / steerParams.SeparationRange;
            else if (agent.Position.Y < minY)
                force += Vector3.UnitY * (minY - agent.Position.Y) / steerParams.SeparationRange;

            if (agent.Position.Z > maxZ)
                force += Vector3.UnitZ * (maxZ - agent.Position.Z) / steerParams.SeparationRange;
            else if (agent.Position.Z < minZ)
                force += Vector3.UnitZ * (minZ - agent.Position.Z) / steerParams.SeparationRange;

            return force;
        }

        #endregion
    }
}
