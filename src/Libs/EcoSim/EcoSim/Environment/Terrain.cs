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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Sim.Interface;
using Sim.Pathfinding;
using Sim.Graphics;

namespace Sim.Environment
{
    public class Terrain
    {
        // options
        public bool Visible = true;
        public bool DrawNodes = false;
        public bool DrawNormals = false;
        private bool drawCursor = false;

        private SimEngine sim;              // simulation the terrain is part of
        private GraphicsDevice g;           // graphics device reference
        private Camera camera;              // camera reference
        private Effect effect;               // shader effect
        private Texture2D texCursor;
        
        private float lowMark;                  // color value for transition between texture weights 1/2 and 2/3
        private float highMark;                 // color value for transition between texture weights 2/3 and 3/4
        private float textureScale = 48;        // scales the texture mapping (higher is lower resolution)
        private Vector3 scale = Vector3.One;    // scaling of the terrain's heightmap to the terrain's actual size
        private Vector4 surfaceReflectivity;    // reflectivity weights for each climate texture
        
        private VertexTerrain[] vertices; // vertices for each triangle in the terrain
        private VertexDeclaration vDecTerrain;  // defines the type of vertices to be sent to GPU
        private VertexBuffer vBuffer;           // vertex buffer for vertices
        private int[] indices;                  // indices of vertices for triangles
        private IndexBuffer iBuffer;            // index buffer for indices
        private NodeGrid nodeGrid;              // pathfinding nodes
        
        private float width;    // width of the scaled terrain
        private float height;   // height of the scaled terrain (highest vertex)
        private float length;   // length of the scaled terrain

        private Texture2D heightmap;            // the heightmap used to generate this terrain
        private float hmRatio;                  // heightmap's height over width
        private int smooths;
        private Vector3 center;

        private EffectTechnique seafloorTechnique;
        private EffectTechnique terrainTechnique;

        #region Seafloor

        private VertexPositionNormalTexture[] floorVertices;
        private int[] floorIndices;
        private VertexDeclaration vDecSeafloor;
       
        #endregion

        public NodeGrid NodeGrid { get { return nodeGrid; } }
        public float LowMark { get { return lowMark; } }
        public float HighMark { get { return highMark; } }
        public VertexTerrain[] Vertices { get { return vertices; } }
        public float Width { get { return width; } }
        public float Length { get { return length; } }
        public float Height { get { return height; } }
        public Vector3 Center { get { return center; } }
        public Vector3 Scale { get { return scale; } }
        public Texture2D Heightmap { get { return heightmap; } }
        public Effect Effect { get { return effect; } }

        public Terrain(BuildData data, SimEngine sim)
        {
            this.sim = sim;
            this.heightmap = data.Heightmap;
            this.scale = data.TerrainScale;
            this.g = sim.GraphicsDevice;
            this.camera = sim.UI.Camera;
            this.smooths = data.Smoothing;

            hmRatio = (float)heightmap.Height / heightmap.Width;
        }

        public void LoadContent(ContentManager cm, Climate climate)
        {
            texCursor = cm.Load<Texture2D>(@"textures\gui\target");
            effect = cm.Load<Effect>(@"shaders\terrain");
            effect.Parameters["matWorld"].SetValue(Matrix.Identity);
            effect.Parameters["tCursor"].SetValue(texCursor);
            effect.Parameters["tCloudShadowMap"].SetValue(cm.Load<Texture2D>(@"textures\sky\clouds"));
            seafloorTechnique = effect.Techniques["Seafloor"];
            terrainTechnique = effect.Techniques["MultiTextured"];

            switch (climate)
            {
                case Climate.Tropical:
                    effect.Parameters["tShore"].SetValue(cm.Load<Texture2D>(@"textures\terrain\sand"));
                    effect.Parameters["tShoreNormals"].SetValue(cm.Load<Texture2D>(@"textures\terrain\sand_normals"));
                    effect.Parameters["tPlains1"].SetValue(cm.Load<Texture2D>(@"textures\terrain\grass"));
                    effect.Parameters["tPlains1Normals"].SetValue(cm.Load<Texture2D>(@"textures\terrain\grass_normals"));
                    effect.Parameters["tPlains2"].SetValue(cm.Load<Texture2D>(@"textures\terrain\grass2"));
                    effect.Parameters["tPlains2Normals"].SetValue(cm.Load<Texture2D>(@"textures\terrain\grass_normals"));
                    effect.Parameters["tRock"].SetValue(cm.Load<Texture2D>(@"textures\terrain\rock"));
                    effect.Parameters["tRockNormals"].SetValue(cm.Load<Texture2D>(@"textures\terrain\rock_normals"));
                    surfaceReflectivity = new Vector4(0, 0, 0, 0.35f);
                    break;
                case Climate.Dry:
                    Texture2D sand = cm.Load<Texture2D>(@"textures\terrain\sand");
                    Texture2D sandNormals = cm.Load<Texture2D>(@"textures\terrain\sand_normals");
                    effect.Parameters["tShore"].SetValue(sand);
                    effect.Parameters["tShoreNormals"].SetValue(sandNormals);
                    effect.Parameters["tPlains1"].SetValue(sand);
                    effect.Parameters["tPlains1Normals"].SetValue(sandNormals);
                    effect.Parameters["tPlains2"].SetValue(cm.Load<Texture2D>(@"textures\terrain\sand2"));
                    effect.Parameters["tPlains2Normals"].SetValue(sandNormals);
                    effect.Parameters["tRock"].SetValue(cm.Load<Texture2D>(@"textures\terrain\rock"));
                    effect.Parameters["tRockNormals"].SetValue(cm.Load<Texture2D>(@"textures\terrain\rock_normals"));
                    surfaceReflectivity = new Vector4(0, 0, 0, 0.35f);
                    break;
                case Climate.Polar:
                    effect.Parameters["tShore"].SetValue(cm.Load<Texture2D>(@"textures\terrain\tundra2"));
                    effect.Parameters["tShoreNormals"].SetValue(cm.Load<Texture2D>(@"textures\terrain\tundra2_normals"));
                    effect.Parameters["tPlains1"].SetValue(cm.Load<Texture2D>(@"textures\terrain\tundra"));
                    effect.Parameters["tPlains1Normals"].SetValue(cm.Load<Texture2D>(@"textures\terrain\tundra_normals"));
                    effect.Parameters["tPlains2"].SetValue(cm.Load<Texture2D>(@"textures\terrain\snow"));
                    effect.Parameters["tPlains2Normals"].SetValue(cm.Load<Texture2D>(@"textures\terrain\snow_normals"));
                    effect.Parameters["tRock"].SetValue(cm.Load<Texture2D>(@"textures\terrain\rocksnow"));
                    effect.Parameters["tRockNormals"].SetValue(cm.Load<Texture2D>(@"textures\terrain\rock_normals"));
                    surfaceReflectivity = new Vector4(0, 0.2f, 0.75f, 0.35f);
                    break;
                case Climate.Continental:
                    break;
                case Climate.Boreal:
                    break;
                default:
                    break;
            }

            effect.Parameters["vSurfaceReflectivity"].SetValue(surfaceReflectivity);
        }

        #region Utility

        public void SetDrawCursor(bool draw)
        {
            drawCursor = draw;
            effect.Parameters["bDrawCursor"].SetValue(drawCursor);
        }

        public void ToggleDetailTexturing()
        {
            effect.Parameters["bDetailEnabled"].SetValue(Sim.Settings.Graphics.Default.TerrainDetail);
        }

        #endregion

        #region Drawing

        public void Draw(float worldTime)
        {
            if (Visible)
            {
                if (drawCursor)
                    DrawTerrainCursor();

                g.RenderState.AlphaBlendEnable = true;

                DrawSeafloor();
                DrawTerrain(worldTime);

                g.RenderState.AlphaBlendEnable = false;
            }

            if (DrawNodes)
                nodeGrid.Draw(g);

            if (DrawNormals)
                DrawVertexNormals();
        }

        private void DrawVertexNormals()
        {
            VertexPositionColor[] lineVerts = new VertexPositionColor[vertices.Length * 6];

            int j = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                lineVerts[j++] = new VertexPositionColor(vertices[i].Position, Color.Yellow);
                lineVerts[j++] = new VertexPositionColor(vertices[i].Normal * 5 + vertices[i].Position, Color.Yellow);
                lineVerts[j++] = new VertexPositionColor(vertices[i].Position, Color.Green);
                lineVerts[j++] = new VertexPositionColor(vertices[i].Tangent * 5 + vertices[i].Position, Color.Green);
                lineVerts[j++] = new VertexPositionColor(vertices[i].Position, Color.Blue);
                lineVerts[j++] = new VertexPositionColor(vertices[i].Binormal * 5 + vertices[i].Position, Color.Blue);
            }
            Utility.Draw.LineList(g, lineVerts);
        }

        private void DrawTerrainCursor()
        {
            Vector3? cursorPos = PickTerrain(sim.UI.MouseRay);
            if (cursorPos.HasValue)
            {
                effect.Parameters["vCursorPos"].SetValue(cursorPos.Value);
                Sim.Utility.Draw.SurfaceNormal(g, cursorPos.Value, this);
            }
        }

        private void DrawSeafloor()
        {
            effect.CurrentTechnique = seafloorTechnique;
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
            g.VertexDeclaration = vDecSeafloor;
            g.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, floorVertices, 0, 4, floorIndices, 0, 2);
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }

        private void DrawTerrain(float worldTime)
        {
            effect.Parameters["fTime"].SetValue(worldTime / 40000);
            effect.CurrentTechnique = terrainTechnique;
            g.VertexDeclaration = vDecTerrain;
            g.Indices = iBuffer;
            g.Vertices[0].SetSource(vBuffer, 0, VertexTerrain.SizeInBytes);
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }

        public void DrawShadowmap()
        {
            Effect e = Shaders.SMEffect;

            g.VertexDeclaration = vDecTerrain;
            g.Indices = iBuffer;
            g.Vertices[0].SetSource(vBuffer, 0, VertexTerrain.SizeInBytes);
            e.Begin();
            e.CurrentTechnique.Passes[0].Begin();
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);
            e.CurrentTechnique.Passes[0].End();
            e.End();
        }

        #endregion

        #region Picking / height interpolation

        /*
         * This picking algorithm is taken from Riemer's XNA Recipes 2.0
         * It's a binary search that steps along the projected ray until a position is found
         * to intersect the terrain mesh within a reasonable distance
         * http://www.riemers.net/
         */

        public Vector3? PickTerrain(Ray r)
        {
            Ray? shorter = LinearSearch(r);
            if (shorter == null)
                return null;

            Vector3? point = BinarySearch(shorter.Value);
            return point ?? null;
        }

        private Ray? LinearSearch(Ray r)
        {
            r.Direction /= 300.0f;
            Vector3 nextPoint = r.Position + r.Direction;
            float height = CalculateHeight(nextPoint.X, nextPoint.Z);
            int step = 0;
            while (height < nextPoint.Y)
            {
                r.Position = nextPoint;
                nextPoint = r.Position + r.Direction;
                height = CalculateHeight(nextPoint.X, nextPoint.Z);
                if (step++ >= 300)
                    return null;
            }
            return r;
        }

        private Vector3? BinarySearch(Ray r)
        {
            float accuracy = 0.01f;
            float heightStart = CalculateHeight(r.Position.X, r.Position.Z);
            float curError = r.Position.Y - heightStart;
            int counter = 0;
            while (curError > accuracy)
            {
                r.Direction /= 2.0f;
                Vector3 nextPoint = r.Position + r.Direction;
                float height = CalculateHeight(nextPoint.X, nextPoint.Z);
                if (nextPoint.Y > height)
                {
                    r.Position = nextPoint;
                    curError = r.Position.Y - height;
                }
                if (counter++ == 1000) return null;
            }
            return r.Position;
        }

        /// <summary>
        /// The (x,z) coordinates are outside the terrain
        /// </summary>
        public bool OutsideMesh(float x, float z)
        {
            return x < 0 || x >= width - scale.X || z < 0 || z >= length - scale.Z;
        }

        /// <summary>
        /// Finds the triangle in the mesh the (x,z) coordinates are within
        /// </summary>
        public Triangle FindTriangle(float x, float z, out bool topTriangle)
        {
            int ulIndex = (int)(x / scale.X) + (int)(z / scale.Z) * heightmap.Width;

            Vector3 ul = vertices[ulIndex].Position;
            Vector3 ur = vertices[ulIndex + 1].Position;
            Vector3 ll = vertices[ulIndex + heightmap.Width].Position;
            Vector3 lr = vertices[ulIndex + heightmap.Width + 1].Position;

            topTriangle = ul.X + hmRatio * (ul.Z - z) > x;

            if (topTriangle)
                return new Triangle(ul, ur, lr);    // point is in top triangle of the square
            return new Triangle(ul, lr, ll);        // point is in the bottom triangle of the square
        }

        public float CalculateHeight(float x, float z)
        {
            if (OutsideMesh(x, z))
                return 0;

            bool topTriangle;
            Triangle t = FindTriangle(x, z, out topTriangle);

            // interpolate the height using the three vertices of the triangle
            float xScale = (x - t.V0.X) / scale.X;
            float zScale = (z - t.V0.Z) / scale.Z;

            if (topTriangle)
                return t.V0.Y + (t.V1.Y - t.V0.Y) * xScale + (t.V2.Y - t.V1.Y) * zScale;
            else
                return t.V0.Y + (t.V1.Y - t.V2.Y) * xScale + (t.V2.Y - t.V0.Y) * zScale;

        }

        public Vector3 CalculateSurfaceNormal(float x, float z)
        {
            if (OutsideMesh(x, z))
                return Vector3.Up;

            bool topTriangle;
            Triangle t = FindTriangle(x, z, out topTriangle);

            return Vector3.Normalize(Vector3.Cross(t.V2 -t.V1, t.V1 - t.V0));
        }

        #endregion

        #region Building

        public void Build()
        {
            BuildVertices();
            BuildIndices();
            BuildNormals();
            CalculateTextureWeights();
            CopyToBuffers();
            nodeGrid = new NodeGrid(this);
        }

        private void BuildVertices()
        {
            // store the color of each pixel of the heightmap into 2D array
            Color[] heightMapColors = new Color[heightmap.Width * heightmap.Height];
            if (!heightmap.Name.Equals(""))
            {
                // have to reload from file as a workaround since the graphics device may be using the heightmap
                Texture2D toLoad = Texture2D.FromFile(g, "Content\\Heightmaps\\" + heightmap.Name);
                toLoad.GetData(heightMapColors);
            }
            else
                heightmap.GetData(heightMapColors);

            // declare the type of vertex the graphics card should expect when drawing these vertices
            vDecTerrain = new VertexDeclaration(sim.GraphicsDevice,
                VertexTerrain.VertexElements);
            vertices = new VertexTerrain[heightmap.Width * heightmap.Height];

            // build each vertex of the terrain mesh
            for (int i = 0; i < heightMapColors.Length; i++)
            {
                // world coordinates of the current vertex
                float x = (i % heightmap.Width) * scale.X;
                float z = (i / heightmap.Width) * scale.Z;
                float y = heightMapColors[i].R / 2.0f * scale.Y;

                vertices[i].Position = new Vector3(x, y, z);
                vertices[i].TextureCoordinates = new Vector2(x / textureScale, z / textureScale);
            }

            ApplySmoothing(smooths);

            // find the highest point after smoothing
            float highest = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (i == 0 || vertices[i].Position.Y > highest)
                    highest = vertices[i].Position.Y;
            }
            
            // these are the scaled dimensions of the terrain (world units)
            width = heightmap.Width * scale.X;
            length = heightmap.Height * scale.Z;
            height = highest;
            center = new Vector3(width / 2, height / 2, length / 2);

            // seafloor
            float floorWidth = width + 5000;
            float floorHeight = length + 5000;
            Vector3 centerXZ = new Vector3(center.X, 0, center.Z);
            floorVertices = new VertexPositionNormalTexture[] {
                new VertexPositionNormalTexture(new Vector3(-floorHeight/2,0,floorWidth/2) + centerXZ,Vector3.Up, Vector2.Zero),
                new VertexPositionNormalTexture(new Vector3(-floorHeight/2,0,-floorWidth/2) + centerXZ, Vector3.Up, Vector2.UnitX),
                new VertexPositionNormalTexture(new Vector3(floorHeight/2,0,floorWidth/2) + centerXZ, Vector3.Up, Vector2.UnitY),
                new VertexPositionNormalTexture(new Vector3(floorHeight/2,0,-floorWidth/2) + centerXZ, Vector3.Up, Vector2.One)};
            floorIndices = new int[] { 0, 1, 2, 1, 3, 2 };
            vDecSeafloor = new VertexDeclaration(g, VertexPositionNormalTexture.VertexElements);
        }

        private void ApplySmoothing(int smooths)
        {
            for (int smooth = 0; smooth < smooths; smooth++)
            {
                for (int x = 0; x < heightmap.Width; x++)
                {
                    for (int y = 0; y < heightmap.Height; y++)
                    {
                        int i = x + y * heightmap.Width;    // current vertex index
                        float sum = vertices[i].Position.Y; // sum of the heights of sampled vertices
                        int sampled = 1;                    // number of adjacent vertices plus the current

                        // indices of the adjacent vertices
                        int nw = i - heightmap.Width - 1;
                        int n = i - heightmap.Width;
                        int ne = i - heightmap.Width + 1;
                        int e = i + 1;
                        int se = i + heightmap.Width + 1;
                        int s = i + heightmap.Width;
                        int sw = i + heightmap.Width - 1;
                        int w = i - 1;

                        // if the current vertex is on an edge, there won't be 8 adjacent vertices
                        bool checkLeft = x != 0;
                        bool checkRight = x != heightmap.Width - 1;
                        bool checkAbove = y != 0;
                        bool checkBelow = y != heightmap.Height - 1;

                        // sum the adjacent vertices
                        if (checkAbove)
                        {
                            sum += vertices[n].Position.Y; sampled++;
                            if (checkLeft) sum += vertices[nw].Position.Y; sampled++;
                            if (checkRight) sum += vertices[ne].Position.Y; sampled++;
                        }
                        if (checkBelow)
                        {
                            sum += vertices[s].Position.Y; sampled++;
                            if (checkLeft) sum += vertices[sw].Position.Y; sampled++;
                            if (checkRight) sum += vertices[se].Position.Y; sampled++;
                        }
                        if (checkRight) sum += vertices[e].Position.Y; sampled++;
                        if (checkLeft) sum += vertices[w].Position.Y; sampled++;

                        // assign the averaged height to the current vertex
                        vertices[i].Position.Y = sum / sampled;
                    }
                }
            }
        }

        private void BuildIndices()
        {
            int maxSquares = (heightmap.Width - 1) * (heightmap.Height - 1); // one square = 2 triangles
            indices = new int[maxSquares * 6]; // 2 triangles = 6 indices per square

            int index = 0;
            for (int i = 0; i < maxSquares; i++)
            {
                int topleft = i % (heightmap.Width - 1) + (i / (heightmap.Width - 1)) * heightmap.Width;
                int topright = topleft + 1;
                int bottomright = topright + heightmap.Width;
                int bottomleft = topleft + heightmap.Width;

                // upper right triangle of square
                indices[index++] = topleft;
                indices[index++] = topright;
                indices[index++] = bottomright;

                // bottom left triangle of square
                indices[index++] = topleft;
                indices[index++] = bottomright;
                indices[index++] = bottomleft;
            }
        }

        private void BuildNormals()
        {
            // initialize each vertex normal to the zero vector
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = Vector3.Zero;

            // add normals from each triangle the vertex is part of
            for (int i = 0; i < indices.Length / 3; i++)
            {
                // indices for the vertices of the current triangle
                int[] triangleIndices = new int[3];
                for (int v = 0; v < 3; v++)
                    triangleIndices[v] = indices[i * 3 + v];

                // calculate the normal & tangent vectors for the surface of the triangle
                Vector3 edge1 = vertices[triangleIndices[0]].Position - vertices[triangleIndices[2]].Position;
                Vector3 edge2 = vertices[triangleIndices[0]].Position - vertices[triangleIndices[1]].Position;
                Vector3 normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

                // assign the normal and tangent vectors to each vertex in the triangle
                for (int v = 0; v < 3; v++)
                {
                    vertices[triangleIndices[v]].Normal += normal;
                    vertices[triangleIndices[v]].Tangent = Vector3.Normalize(edge1);
                    vertices[triangleIndices[v]].Binormal = Vector3.Normalize(Vector3.Cross(edge1, normal));
                }
            }

            // average the normals for each vertex
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();
        }

        private void CopyToBuffers()
        {
            // copy vertices to vertex buffer
            vBuffer = new VertexBuffer(sim.GraphicsDevice, vertices.Length *
                VertexTerrain.SizeInBytes, BufferUsage.WriteOnly);
            vBuffer.SetData(vertices);

            // copy indices to index buffer
            iBuffer = new IndexBuffer(sim.GraphicsDevice, typeof(int),
                indices.Length, BufferUsage.WriteOnly);
            iBuffer.SetData(indices);
        }

        private void CalculateTextureWeights()
        {
            lowMark = 15 * scale.Y;
            highMark = 180 * scale.Y;

            for (int i = 0; i < vertices.Length; i++)
            {
                float height = vertices[i].Position.Y;
                double normalAngle = GMath.AngleOfIncline(vertices[i].Normal);
                Vector4 weights = Vector4.Zero; // start all weights at 0

                if (height < lowMark) // height is in the lower third
                {
                    weights.Y = height / lowMark;   // percent weight 2
                    weights.X = 1 - weights.Y;  // percent weight 1
                }
                else if (height < highMark) // height is in the middle third
                {
                    weights.Z = (height - lowMark) / (highMark - lowMark);  // percent weight 3
                    weights.Y = 1 - weights.Z;                  // percent weight 2
                }
                else // height is in the top third
                {
                    weights.W = (height - highMark) / (255 - highMark) * (float)(90-normalAngle)/90.0f; // percent weight 4
                    weights.Z = 1 - weights.W;                  // percent weight 3
                }

                // surfaces which have steep surfaces should be rocky
                if (normalAngle < 50)
                {
                    float rockyness = MathHelper.Clamp((float)(90 - normalAngle) / 90.0f,0,1);
                    weights *= 1-rockyness;
                    weights.W += rockyness;
                }
                // surfaces which have shallow angles mix in some of the third texture
                if (normalAngle > 75 && vertices[i].Position.Y > lowMark)
                {
                    weights *= 0.4f;
                    weights.Z += 0.6f;
                }

                vertices[i].TextureWeights = weights;
            }
        }

        #endregion
    }
}
