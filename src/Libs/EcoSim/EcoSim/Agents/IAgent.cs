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
    public interface IAgent
    {
        Vector3 Position { get; }
        Vector3 Direction { get; set; }
        string CurrentBehavior { get; }
        void Update(float worldTime);     // the agent must be capable of updating its behavior
    }

    public interface IAgentFlockable : IAgent
    {
        void Update(float worldTime, IAgent[] flockAgents);
    }
}
