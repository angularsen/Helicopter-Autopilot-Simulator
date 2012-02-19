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

namespace Sim.Pathfinding
{
    public enum NodeState
    {
        Open,       // expanded
        Closed,     // has been traversed
        Invalid,    // cannot be traversed
        Valid,      // not searched, but valid
        OnPath,     // part of the final path
    }

    public class Node : IComparable<Node>
    {
        public NodeState State = NodeState.Valid;               // each node is initially accessible
        public List<Node> AdjacentNodes = new List<Node>();     // nodes reachable through this node
        public Vector3 Position;                                // world position of the node

        public Node Parent;         // node this node is reached through in a solved world state
        public float G = 0;         // movement cost to this node from the path that includes its parent
        public float H = 0;         // estimate of distance between this node and the goal
        public float F = 0;         // total cost (G + H)

        public Node(Vector3 position)
        {
            this.Position = position;
        }

        public int CompareTo(Node other) { return this.F.CompareTo(other.F); }
    }
}
