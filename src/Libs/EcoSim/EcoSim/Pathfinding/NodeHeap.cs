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
    /// <summary>
    /// A min binary heap for storing a sorted list of open nodes
    /// </summary>
    class NodeHeap
    {
        private Node[] nodes;
        private int filled = 0;

        public int Filled { get { return filled; } }

        public NodeHeap(int maxNodes)
        {
            nodes = new Node[maxNodes];
        }

        /// <summary>
        /// Adds a new node to the heap, then sorts the heap
        /// </summary>
        public void Insert(Node n)
        {
            if (filled < nodes.Length)
            {
                nodes[filled++] = n;
                HeapifyNode(n, filled - 1);
            }
        }

        /// <summary>
        /// Heapifies a node at a specified index in the array
        /// </summary>
        public void HeapifyNode(Node n, int i)
        {
            while (i > 0 && nodes[i].CompareTo(nodes[(i - 1) / 2]) < 0)
            {
                nodes[i] = nodes[(i - 1) / 2];
                nodes[(i - 1) / 2] = n;
                i = (i - 1) / 2;
            }
        }

        /// <summary>
        /// Heapifies a node at an unknown location in the heap
        /// </summary>
        /// <param name="n"></param>
        public void HeapifyNode(Node n)
        {
            // searching for the node could be improved by maintaining an average F cost across the 
            for (int i = 0; i < nodes.Length; i++)
                if (nodes[i] == n)
                    HeapifyNode(n, i);
        }

        /// <summary>
        /// Removes the Node with the lowest F cost, then sorts the heap
        /// </summary>
        public Node Extract()
        {
            Node top = nodes[0];

            // move the last node to the root
            int iCurrent = 0;
            nodes[iCurrent] = nodes[filled - 1];
            filled--;

            // resort the heap
            while (iCurrent < filled / 2)
            {
                Node current = nodes[iCurrent];

                // calculate the difference between the current node and its children
                // if the total number of nodes is even, the right child of the last
                // branch doesn't exist
                int iLeft = 2 * iCurrent + 1;
                int iRight = 2 * iCurrent + 2;
                float dL = current.F - nodes[iLeft].F;
                float dR = (iCurrent == filled / 2 - 1 && filled % 2 == 0) ? 0 : current.F - nodes[iRight].F;

                if (dL > 0 && dL >= dR)
                {
                    // left child is smallest, equal to the right child, or the only child
                    nodes[iCurrent] = nodes[iLeft];
                    nodes[iLeft] = current;
                    iCurrent = iLeft;
                }
                else if (dR > 0 && dR > dL)
                {
                    // right child is smallest
                    nodes[iCurrent] = nodes[iRight];
                    nodes[iRight] = current;
                    iCurrent = iRight;
                }
                else
                {
                    // neither child is smaller, so end the loop
                    break;
                }
            }

            return top;
        }
    }
}
