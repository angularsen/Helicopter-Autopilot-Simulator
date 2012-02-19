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
    class Flock
    {
        private Vector3 direction;
        public Vector3 Direction { get { return direction; } }

        private IAgentFlockable[] agents;
        private int flockMembers = 0;

        public IAgentFlockable[] Agents { get { return agents; } }

        public Flock(int flockSize)
        {
            agents = new IAgentFlockable[flockSize];
        }

        public bool AddAgent(IAgentFlockable agent)
        {
            // no more room in the flock
            if (flockMembers >= agents.Length)
                return false;

            // agent was added
            agents[flockMembers++] = agent;
            return true;
        }

        public bool RemoveAgent(IAgentFlockable agent)
        {
            // find the index of the agent in the flock
            int agentIndex = -1;
            for (int i = 0; i < flockMembers; i++)
            {
                if (agents[i] == agent) 
                {
                    agentIndex = i;
                    break;
                }
            }

            // the agent is not part of the flock
            if (agentIndex == -1)
                return false;

            // shift the other agents' indices down
            for (int i = agentIndex; i < flockMembers - 1; i++)
                agents[i] = agents[i + 1];

            flockMembers--;
            return true;
        }

        public void Update(float worldTime)
        {
            for (int i = 0; i < agents.Length; i++)
                agents[i].Update(worldTime, agents);
        }
    }
}
