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
    public static class Behaviors
    {
        /// <summary>
        /// Combines wandering, cohesion, separation, and alignment steering such that the agent moves
        /// together with other members in the flock
        /// </summary>
        public static void Flock(IAgent agent, IAgent[] flockMembers, ref Steering.SteerParameters steerParams)
        {
            Vector3 wander = Steering.WanderForce(agent, ref steerParams);
            Vector3 separation = Vector3.Zero;
            Vector3 cohesion = Vector3.Zero;
            Vector3 alignment = Vector3.Zero;
            Vector3 worldForce = Steering.WorldBoundsForce(agent, steerParams);
            Vector3 goalForce = Steering.GoalForce(agent, steerParams);

            int alignmentWith = 0;
            int cohesionWith = 0;
            for (int i = 0; i < flockMembers.Length; i++)
            {
                IAgent otherAgent = flockMembers[i];
                if (agent != otherAgent)
                {
                    float separationDistance = Vector3.Distance(agent.Position, otherAgent.Position);
                    if (separationDistance < steerParams.SeparationRange)
                        separation += Steering.SeparationForce(agent, otherAgent, steerParams, separationDistance);
                    else
                    {
                        cohesion += Steering.CohesionForce(agent, otherAgent, steerParams);
                        cohesionWith++;

                        float scale = MathHelper.Clamp(1 - separationDistance / steerParams.AlignmentRange, 0, 1);
                        if (scale > 0)
                        {
                            alignment += Steering.AlignmentForce(otherAgent, steerParams) * scale;
                            alignmentWith++;
                        }
                    }
                }
            }

            if (cohesionWith > 1)
                cohesion /= cohesionWith;
            if (alignmentWith > 1)
                alignment /= alignmentWith;

            agent.Direction = Vector3.Normalize(agent.Direction + wander + separation + cohesion + alignment + worldForce + goalForce);
        }

        /// <summary>
        /// Combines wandering and separation
        /// </summary>
        public static void Wander(IAgent agent, IAgent[] otherAgents, ref Steering.SteerParameters steerParams)
        {
            // add up the steering forces
            Vector3 wander = Steering.WanderForce(agent, ref steerParams);
            Vector3 separation = Vector3.Zero;
            Vector3 bounds = Steering.WorldBoundsForce(agent, steerParams);

            int avoiding = 0;
            for (int i = 0; i < otherAgents.Length; i++)
            {
                IAgent otherAgent = otherAgents[i];

                if (agent != otherAgent)
                {
                    float separationDistance = Vector3.Distance(agent.Position, otherAgent.Position);
                    if (separationDistance < steerParams.SeparationRange)
                    {
                        separation += Steering.SeparationForce(agent, otherAgent, steerParams, separationDistance);
                        avoiding++;
                    }
                }
            }

            agent.Direction = Vector3.Normalize(agent.Direction + wander + separation + bounds);
        }
    }
}
