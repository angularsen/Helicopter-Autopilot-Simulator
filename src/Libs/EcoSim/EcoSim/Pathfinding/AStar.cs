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

// Heuristic options
#define HEURISTIC_MANHATTAN
//#define HEURISTIC_EUCLIDEAN
//#define HEURISTIC_DIAG_SHORTCUT

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sim.Pathfinding
{
    public class AStar
    {

        /// <summary>
        /// A* search algorithm
        /// </summary>
        /// <returns>The sequential list of coordinates along the path</returns>
        public static LinkedList<Vector3> Search(Node start, Node goal, float d, float d2, int heapSize)
        {
            bool pathFound = false;
            NodeHeap open = new NodeHeap(heapSize);

            open.Insert(start);
            while (open.Filled > 0)
            {
                Node current = open.Extract();

                if (current == goal)
                {
                    pathFound = true;
                    break;
                }

                current.State = NodeState.Closed;

                // expand nodes adjacent to the current node
                foreach (Node adjacent in current.AdjacentNodes)
                {
                    // ignore nodes marked invalid or part of the closed set, as they can't be traversed
                    if (adjacent.State == NodeState.Invalid || adjacent.State == NodeState.Closed)
                        continue;

                    if (adjacent.State == NodeState.Open)
                    {
                        // if a node is adjacent but already on the open list, it can be checked to see if
                        // the path through the current node is more efficient
                        float g = CalculateMovementCost(current, adjacent, d, d2);

                        if (g < adjacent.G)
                        {
                            // the cost is less, so the path through the current node is better
                            adjacent.G = g;
                            adjacent.F = g + adjacent.H;
                            adjacent.Parent = current;

                            // have to relocate this node in the heap
                            open.HeapifyNode(adjacent);
                        }
                    }
                    else
                    {
                        // the adjacent node is not part of the open set, and it's a valid choice, so add it
                        adjacent.Parent = current;
                        adjacent.G = CalculateMovementCost(current, adjacent, d, d2);
                        adjacent.H = CalculateHeuristic(adjacent, goal);
                        adjacent.F = adjacent.G + adjacent.H;
                        adjacent.State = NodeState.Open;
                        open.Insert(adjacent);
                    }
                }
            }

            // the set of points to travel is found by starting with the goal
            // and moving backwards until the start is hit
            if (pathFound)
            {
                LinkedList<Vector3> path = new LinkedList<Vector3>();
                while (goal != start.Parent)
                {
                    goal.State = NodeState.OnPath;
                    path.AddFirst(goal.Position);
                    goal = goal.Parent;
                }

                return path;
            }
            return null;
        }

        private static float CalculateMovementCost(Node current, Node adjacent, float d, float d2)
        {
            // if coordinates are both different, the G cost should use the diagonal cost d2 = sqrt(2) * d;
            // otherwise, it should use the straight move cost d
            return adjacent.Position.X != current.Position.X && adjacent.Position.Z != current.Position.Z ?
                d2 + current.G : d + current.G;
        }

        private static float CalculateHeuristic(Node n, Node g)
        {
#if HEURISTIC_MANHATTAN
            return (Math.Abs(g.Position.X - n.Position.X) + Math.Abs(g.Position.Z - n.Position.Z));
#endif

#if HEURISTIC_EUCLIDEAN
            return Vector3.Distance(n.Position, g.Position);
#endif

#if HEURISTIC_DIAG_SHORTCUT
            float distZ = Math.Abs(n.Position.Z - g.Position.Z);
            float distX = Math.Abs(n.Position.X - g.Position.X);
            return distZ > distX ? 1.41f * distX + (distZ - distX) : 1.41f * distZ + (distX - distZ);
#endif
        }
    }
}
