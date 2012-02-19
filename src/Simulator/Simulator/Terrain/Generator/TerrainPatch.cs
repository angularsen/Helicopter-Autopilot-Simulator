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

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace NINFocusOnTerrain
{
    /// <summary>
    /// This is the terrain patch class.
    /// </summary>
    public class TerrainPatch
    {
        /// <summary>
        /// Depth of the terrain patch.
        /// </summary>
        public readonly int Depth;

        /// <summary>
        /// Width of the terrain patch.
        /// </summary>
        public readonly int Width;

        private readonly Game _game;

        /// <summary>
        /// Index buffer of the terrain patch.
        /// </summary>
        private readonly IndexBuffer _indexBuffer;

        /// <summary>
        /// X offset used when we retrieve the heigth values from the heightmap.
        /// </summary>
        private readonly int _offsetX;

        /// <summary>
        /// Y offset used when we retrieve the height values from the heightmap.
        /// </summary>
        private readonly int _offsetZ;

        /// <summary>
        /// Vertex buffer of the terrain patch.
        /// </summary>
        private readonly VertexBuffer _vertexBuffer;

        /// <summary>
        /// Geometry of the terrain patch.
        /// </summary>
        public VertexPositionNormalTexture[] Geometry;

        /// <summary>
        /// Bounding box used for this patch.
        /// </summary>
        private BoundingBox _boundingBox;

        /// <summary>
        /// Indices of the terrain patch.
        /// </summary>
        private short[] _indices;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TerrainPatch(Game game, Heightmap heightmap, Matrix worldMatrix, int width, int depth, int offsetX,
                            int offsetZ)
        {
            _boundingBox = new BoundingBox();

            _game = game;

            Width = width;
            Depth = depth;

            _offsetX = offsetX;
            _offsetZ = offsetZ;

            _boundingBox.Min.X = offsetX;
            _boundingBox.Min.Z = offsetZ;

            _boundingBox.Max.X = offsetX + width;
            _boundingBox.Max.Z = offsetZ + depth;

            BuildVertexBuffer(heightmap);

            _vertexBuffer = new VertexBuffer(_game.GraphicsDevice,
                                             VertexPositionNormalTexture.SizeInBytes*Geometry.Length,
                                             BufferUsage.WriteOnly);

            _vertexBuffer.SetData(Geometry);

            BuildIndexBuffer();

            _indexBuffer = new IndexBuffer(_game.GraphicsDevice, sizeof (short)*_indices.Length,
                                           BufferUsage.WriteOnly, IndexElementSize.SixteenBits);

            _indexBuffer.SetData(_indices);

            // Apply the world matrix transformation to the bounding box.
            _boundingBox.Min = Vector3.Transform(_boundingBox.Min, worldMatrix);
            _boundingBox.Max = Vector3.Transform(_boundingBox.Max, worldMatrix);
        }

        /// <summary>
        /// Get the bounding box.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get { return _boundingBox; }
        }

        public Matrix World { get; set; }

//        /// <summary>
//        /// Build a terrain patch.
//        /// </summary>
//        /// <param name="game"></param>
//        /// <param name="heightmap"></param>
//        /// <param name="worldMatrix"></param>
//        /// <param name="width"></param>
//        /// <param name="depth"></param>
//        /// <param name="offsetX"></param>
//        /// <param name="offsetZ"></param>
//        public static void BuildPatch(Game game, Heightmap heightmap, Matrix worldMatrix, int width, int depth, int offsetX, int offsetZ)
//        {
//            
//        }

        /// <summary>
        /// Build the vertex buffer as well as the bounding box.
        /// </summary>
        /// <param name="heightmap"></param>
        private void BuildVertexBuffer(Heightmap heightmap)
        {
            int index = 0;

            _boundingBox.Min.Y = float.MaxValue;

            _boundingBox.Max.Y = float.MinValue;

            Geometry = new VertexPositionNormalTexture[Width*Depth];

            for (int z = _offsetZ; z < _offsetZ + Depth; ++z)
            {
                for (int x = _offsetX; x < _offsetX + Width; ++x)
                {
                    // We need to connect the patches by increasing each patch width and depth by 1,
                    // but this means the patches along some of the edges will run out of 
                    // heightmap data, so make sure we cap the index at the edge
                    int cappedX = Math.Min(x, heightmap.Width - 1);
                    int cappedZ = Math.Min(z, heightmap.Depth - 1);

                    float height = heightmap.GetHeightValue(cappedX, cappedZ);

                    if (height < _boundingBox.Min.Y)
                    {
                        _boundingBox.Min.Y = height;
                    }

                    if (height > _boundingBox.Max.Y)
                    {
                        _boundingBox.Max.Y = height;
                    }

                    var position = new Vector3(x, height, z);

                    Vector3 normal;
                    ComputeVertexNormal(heightmap, cappedX, cappedZ, out normal);

                    Geometry[index] = new VertexPositionNormalTexture(position, normal, new Vector2(x, z));

                    ++index;
                }
            }
        }

        /// <summary>
        /// Build the index buffer.
        /// </summary>
        private void BuildIndexBuffer()
        {
            int stripLength = 4 + (Depth - 2)*2;
            int stripCount = Width - 1;

            _indices = new short[stripLength*stripCount];

            int index = 0;

            for (int s = 0; s < stripCount; ++s)
            {
                for (int z = 0; z < Depth; ++z)
                {
                    _indices[index] = (short) (s + Depth*z);

                    ++index;

                    _indices[index] = (short) (s + Depth*z + 1);

                    ++index;
                }
            }
        }

        /// <summary>
        /// Compute vertex normal at the given x,z coordinate.
        /// </summary>
        /// <param name="heightmap"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="normal"></param>
        private static void ComputeVertexNormal(Heightmap heightmap, int x, int z, out Vector3 normal)
        {
            int width = heightmap.Width;
            int depth = heightmap.Depth;

            Vector3 p1;
            Vector3 p2;
            Vector3 avgNormal = Vector3.Zero;

            int avgCount = 0;

            bool spaceAbove = false;
            bool spaceBelow = false;
            bool spaceLeft = false;
            bool spaceRight = false;

            Vector3 tmpNormal;
            Vector3 v1;
            Vector3 v2;

            var center = new Vector3(x, heightmap.GetHeightValue(x, z), z);

            if (x > 0)
            {
                spaceLeft = true;
            }

            if (x < width - 1)
            {
                spaceRight = true;
            }

            if (z > 0)
            {
                spaceAbove = true;
            }

            if (z < depth - 1)
            {
                spaceBelow = true;
            }

            if (spaceAbove && spaceLeft)
            {
                p1 = new Vector3(x - 1, heightmap.GetHeightValue(x - 1, z), z);
                p2 = new Vector3(x - 1, heightmap.GetHeightValue(x - 1, z - 1), z - 1);

                v1 = p1 - center;
                v2 = p2 - p1;

                tmpNormal = Vector3.Cross(v1, v2);
                avgNormal += tmpNormal;

                ++avgCount;
            }

            if (spaceAbove && spaceRight)
            {
                p1 = new Vector3(x, heightmap.GetHeightValue(x, z - 1), z - 1);
                p2 = new Vector3(x + 1, heightmap.GetHeightValue(x + 1, z - 1), z - 1);

                v1 = p1 - center;
                v2 = p2 - p1;

                tmpNormal = Vector3.Cross(v1, v2);
                avgNormal += tmpNormal;

                ++avgCount;
            }

            if (spaceBelow && spaceRight)
            {
                p1 = new Vector3(x + 1, heightmap.GetHeightValue(x + 1, z), z);
                p2 = new Vector3(x + 1, heightmap.GetHeightValue(x + 1, z + 1), z + 1);

                v1 = p1 - center;
                v2 = p2 - p1;

                tmpNormal = Vector3.Cross(v1, v2);
                avgNormal += tmpNormal;

                ++avgCount;
            }

            if (spaceBelow && spaceLeft)
            {
                p1 = new Vector3(x, heightmap.GetHeightValue(x, z + 1), z + 1);
                p2 = new Vector3(x - 1, heightmap.GetHeightValue(x - 1, z + 1), z + 1);

                v1 = p1 - center;
                v2 = p2 - p1;

                tmpNormal = Vector3.Cross(v1, v2);
                avgNormal += tmpNormal;

                ++avgCount;
            }

            normal = avgNormal/avgCount;
        }

        /// <summary>
        /// Draw the terrain patch.
        /// </summary>
        public void Draw()
        {
            int primitivePerStrip = (Depth - 1)*2;
            int stripCount = Width - 1;
            int vertexPerStrip = Depth*2;

            for (int s = 0; s < stripCount; ++s)
            {
                _game.GraphicsDevice.Vertices[0].SetSource(_vertexBuffer, 0,
                                                           VertexPositionNormalTexture.SizeInBytes);

                _game.GraphicsDevice.Indices = _indexBuffer;

                _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0,
                                                           Geometry.Length, vertexPerStrip*s,
                                                           primitivePerStrip);
            }
        }
    }
}

/*======================================================================================================================

									NIN - Nerdy Inverse Network - http://nerdy-inverse.com

======================================================================================================================*/