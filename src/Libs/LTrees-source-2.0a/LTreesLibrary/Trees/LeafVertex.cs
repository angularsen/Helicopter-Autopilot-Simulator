#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

#region Using

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// Vertex used in leaf clouds.
    /// </summary>
    /// <remarks>
    /// A leaf consists of four vertices. Each vertex will have the same position, but different offsets.
    /// The vertex shader must use the offset to adjust the position of the output vertex to make the leaf face the camera.
    /// </remark>
    public struct LeafVertex
    {
        /// <summary>
        /// Number of bytes used by a vertex, which is 60 bytes.
        /// </summary>
        public const int SizeInBytes = sizeof (float)*(3 + 2 + 2 + 4 + 3) + sizeof (short)*2;

        public static readonly VertexElement[] VertexElements = {
                                                                    new VertexElement(0, 0, VertexElementFormat.Vector3,
                                                                                      VertexElementMethod.Default,
                                                                                      VertexElementUsage.Position, 0),
                                                                    new VertexElement(0, 12, VertexElementFormat.Vector2,
                                                                                      VertexElementMethod.Default,
                                                                                      VertexElementUsage.
                                                                                          TextureCoordinate, 0),
                                                                    new VertexElement(0, 20, VertexElementFormat.Vector2,
                                                                                      VertexElementMethod.Default,
                                                                                      VertexElementUsage.
                                                                                          TextureCoordinate, 1),
                                                                    new VertexElement(0, 28, VertexElementFormat.Vector4,
                                                                                      VertexElementMethod.Default,
                                                                                      VertexElementUsage.Color, 0),
                                                                    new VertexElement(0, 44, VertexElementFormat.Short2,
                                                                                      VertexElementMethod.Default,
                                                                                      VertexElementUsage.
                                                                                          TextureCoordinate, 2),
                                                                    new VertexElement(0, 48, VertexElementFormat.Vector3,
                                                                                      VertexElementMethod.Default,
                                                                                      VertexElementUsage.Normal, 0),
                                                                };

        /// <summary>
        /// Index of the bone controlling this leaf.
        /// </summary>
        public IdStruct BoneIndex;

        /// <summary>
        /// Normal vector to use in lighting calculations.
        /// This is the orientation of the branch where the leaf was spawned.
        /// </summary>
        public Vector3 BranchNormal;

        /// <summary>
        /// Color tint of the leaf.
        /// </summary>
        public Vector4 Color;

        /// <summary>
        /// Adjusts the vertex position in view space coordinates.
        /// </summary>
        public Vector2 Offset;

        /// <summary>
        /// Center of the leaf. 
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Texture coordinate.
        /// </summary>
        public Vector2 TextureCoordinate;

        public LeafVertex(Vector3 position, Vector2 textureCoordinate, Vector2 offset, Vector4 color, int bone,
                          Vector3 normal)
        {
            Position = position;
            TextureCoordinate = textureCoordinate;
            Offset = offset;
            Color = color;
            BoneIndex.Id = (short) bone;
            BoneIndex.Unused = 0;
            BranchNormal = normal;
        }

        #region Nested type: IdStruct

        public struct IdStruct
        {
            public short Id;
            public short Unused;
        }

        #endregion
    }
}