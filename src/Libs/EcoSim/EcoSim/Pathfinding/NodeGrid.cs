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

namespace Sim.Pathfinding
{
    public class NodeGrid
    {
        private Node[] nodes;
        private int width;
        private int length;
        private int spacing = 10;

        public Rectangle GridArea;

        public Node[] Nodes { get { return nodes; } }
        public int Width { get { return width; } }
        public int Height { get { return length; } }
        public int Spacing { get { return spacing; } }

        public NodeGrid(Sim.Environment.Terrain terrain)
        {
            this.width = (int)terrain.Width / spacing;
            this.length = (int)terrain.Length / spacing;

            GridArea = new Rectangle(0, 0, width * spacing, length * spacing);

            BuildGrid(terrain);
        }

        /// <summary>
        /// Creates and connects all nodes in the grid
        /// </summary>
        private void BuildGrid(Sim.Environment.Terrain terrain)
        {
            // create the nodes at their proper locations
            nodes = new Node[width * length];
            for (int z = 0; z < length; z++) 
            {
                for (int x = 0; x < width; x++) 
                {
                    int i = x + z * width;
                    float y = terrain.CalculateHeight(x * spacing, z * spacing);
                    nodes[i] = new Node(new Vector3(x * spacing, y, z * spacing));

                    if (y < 5) // underwater
                        nodes[i].State = NodeState.Invalid;
                    else if (GMath.AngleOfIncline(terrain.CalculateSurfaceNormal(x * spacing, z * spacing)) < 50)
                        nodes[i].State = NodeState.Invalid;
                }
            }

            // connect adjacent nodes
            for (int z = 0; z < length; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    Node n = nodes[x + z * width];

                    // adjacent nodes exist in all 8 directions if the current node is part of the
                    // interior of the grid; if the node is on an edge, certain spots are empty
                    bool addN = z > 0;
                    bool addS = z < length - 1;
                    bool addW = x > 0;
                    bool addE = x < width - 1;

                    if (addN)
                    {
                        n.AdjacentNodes.Add(nodes[x + (z - 1) * width]);                    // N
                        if (addE) n.AdjacentNodes.Add(nodes[(x + 1) + (z - 1) * width]);    // NE
                        if (addW) n.AdjacentNodes.Add(nodes[(x - 1) + (z - 1) * width]);    // NW
                    }

                    if (addS)
                    {
                        n.AdjacentNodes.Add(nodes[x + (z + 1) * width]);                    // S
                        if (addE) n.AdjacentNodes.Add(nodes[(x + 1) + (z + 1) * width]);    // SE
                        if (addW) n.AdjacentNodes.Add(nodes[(x - 1) + (z + 1) * width]);    // SW
                    }

                    if (addE) n.AdjacentNodes.Add(nodes[(x + 1) + z * width]);              // E
                    if (addW) n.AdjacentNodes.Add(nodes[(x - 1) + z * width]);              // W
                }
            }
        }

        public void ResetStates()
        {
            for (int i = 0; i < nodes.Length; i++)
                nodes[i].State = nodes[i].State == NodeState.Invalid ? NodeState.Invalid : NodeState.Valid;
        }

        public LinkedList<Vector3> Search(Vector3 start, Vector3 goal)
        {
            Node n = GetNodeAt(start.X, start.Z);
            Node g = GetNodeAt(goal.X, goal.Z);
            if (n == null || g == null || g.State == NodeState.Invalid)
                return null;

            ResetStates();
            return AStar.Search(n, g, spacing, spacing * 1.41f, width * length);
        }

        public Node GetNodeAt(float x, float z)
        {
            int i = (int)x / spacing + (int)z / spacing * width;
            return i < 0 || i > nodes.Length ? null : nodes[i];
        }

        public Node GetNodeAt(Point p)
        {
            return GetNodeAt(p.X, p.Y);
        }

        public void Draw(GraphicsDevice g)
        {
            VertexPositionColor[] points = new VertexPositionColor[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                Color c;
                
                switch (nodes[i].State)
                {
                    case NodeState.Closed: c = Color.LightBlue; break;
                    case NodeState.Invalid: c = Color.Red; break;
                    case NodeState.OnPath: c = Color.Yellow; break;
                    case NodeState.Open: c = Color.LightGreen; break;
                    case NodeState.Valid: c = Color.Green; break;
                    default: c = Color.LimeGreen; break;
                }
                points[i] = new VertexPositionColor(nodes[i].Position, c);
            }

            g.RenderState.PointSize = 3;
            Sim.Utility.Draw.PointList(g, points);
            g.RenderState.PointSize = 1;
        }
    }
}
