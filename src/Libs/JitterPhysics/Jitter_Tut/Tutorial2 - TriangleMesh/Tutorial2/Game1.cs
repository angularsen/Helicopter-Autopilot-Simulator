using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Jitter;
using Jitter.Collision;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using Microsoft.Xna.Framework.Input;

namespace Tutorial2
{

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Model boxModel;
        Model torusModel;

        // the reference to our physics world.
        World world;
        Matrix viewMatrix, projectionMatrix;

        public Game1()
        {
            this.IsMouseVisible = true;
            graphics = new GraphicsDeviceManager(this);

            // creating a new collision system and adding it to the new world
            CollisionSystem collisionSystem = new CollisionSystemSAP();
            world = new World(collisionSystem);

            // build our basic scene
            BuildScene();

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            viewMatrix = Matrix.CreateLookAt(new Vector3(10, 7, 4),
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0));

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                    GraphicsDevice.Viewport.AspectRatio, 0.01f, 300.0f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            boxModel = Content.Load<Model>("box");
            torusModel = Content.Load<Model>("torus");

            // Get the vertex information out of the model
            List<JVector> positions = new List<JVector>();
            List<JOctree.TriangleVertexIndices> indices = new List<JOctree.TriangleVertexIndices>();
            ExtractData(positions, indices, torusModel);

            // Build an octree of it
            JOctree octree = new JOctree(positions, indices);
            octree.BuildOctree();

            // Pass it to a new instance of the triangleMeshShape
            TriangleMeshShape triangleMeshShape = new TriangleMeshShape(octree);

            // Create a body, using the triangleMeshShape
            RigidBody triangleBody = new RigidBody(triangleMeshShape);
            triangleBody.Tag = Color.LightGray;
            triangleBody.Position = new JVector(0, 3, 0);
            
            // Add the mesh to the world.
            world.AddBody(triangleBody);

            base.LoadContent();
        }

        private void BuildScene()
        {
            // creating two boxShapes with different sizes
            Shape boxShape = new BoxShape(JVector.One);
            Shape groundShape = new BoxShape(new JVector(10, 1, 10));

            // create new instances of the rigid body class and pass 
            // the boxShapes to them
            RigidBody boxBody1 = new RigidBody(boxShape);
            RigidBody boxBody2 = new RigidBody(boxShape);

            boxBody1.Tag = Color.LightPink;
            boxBody2.Tag = Color.LightSkyBlue;

            RigidBody groundBody = new RigidBody(groundShape);

            groundBody.Tag = Color.LightGreen;

            // set the position of the box size=(1,1,1)
            // 2 and 5 units above the ground box size=(10,1,10)
            boxBody1.Position = new JVector(0, 5, 0.0f);
            boxBody2.Position = new JVector(0, 8, 0.2f);
            
            // make the body static, so it can't be moved
            groundBody.IsStatic = true;

            // add the bodies to the world.
            world.AddBody(boxBody1);
            world.AddBody(boxBody2);
            world.AddBody(groundBody);
        }

        private void DrawBody(RigidBody body)
        {
            // Is the body a box or a triangle?
            bool isBox = (body.Shape is BoxShape);

            // Create the 4x4 xna matrix, containing the orientation
            // (represented in jitter by a 3x3 matrix) and the position.
            Matrix matrix = Conversion.ToXNAMatrix(body.Orientation);
            matrix.Translation = Conversion.ToXNAVector(body.Position);

            // We have a (1,1,1) box so packing the box size
            // information into the the "scale" part of the xna
            // matrix is a good idea.
            Matrix scaleMatrix = Matrix.Identity;

            if (isBox)
            {
                BoxShape shape = body.Shape as BoxShape;
                scaleMatrix = Matrix.CreateScale(shape.Size.X, shape.Size.Y, shape.Size.Z);
            }

            // the next lines of code draw the boxModel using the matrix.
            Model model= (isBox) ? boxModel : torusModel;

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.DiffuseColor = ((Color)body.Tag).ToVector3();
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * scaleMatrix * matrix;
                    effect.View = viewMatrix;

                    effect.Projection = projectionMatrix;
                }
                mesh.Draw();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // draw all our bodies
            foreach (RigidBody body in world.RigidBodies)
            {
                DrawBody(body);
            }

            base.Draw(gameTime);
        }

        // Extracts the vertices and vertexindices of a model
        #region  public void ExtractData(List<JVector> vertices, List<TriangleVertexIndices> indices, Model model)
        public void ExtractData(List<JVector> vertices, List<JOctree.TriangleVertexIndices> indices, Model model)
        {
            Matrix[] bones_ = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(bones_);
            foreach (ModelMesh mm in model.Meshes)
            {
                Matrix xform = bones_[mm.ParentBone.Index];
                foreach (ModelMeshPart mmp in mm.MeshParts)
                {
                    int offset = vertices.Count;
                    Vector3[] a = new Vector3[mmp.NumVertices];
                    mm.VertexBuffer.GetData<Vector3>(mmp.StreamOffset + mmp.BaseVertex * mmp.VertexStride,
                        a, 0, mmp.NumVertices, mmp.VertexStride);
                    for (int i = 0; i != a.Length; ++i)
                        Vector3.Transform(ref a[i], ref xform, out a[i]);

                    for (int i = 0; i < a.Length; i++) vertices.Add(new JVector(a[i].X, a[i].Y, a[i].Z));

                    if (mm.IndexBuffer.IndexElementSize != IndexElementSize.SixteenBits)
                        throw new Exception(
                            String.Format("Model uses 32-bit indices, which are not supported."));
                    short[] s = new short[mmp.PrimitiveCount * 3];
                    mm.IndexBuffer.GetData<short>(mmp.StartIndex * 2, s, 0, mmp.PrimitiveCount * 3);
                    JOctree.TriangleVertexIndices[] tvi = new JOctree.TriangleVertexIndices[mmp.PrimitiveCount];
                    for (int i = 0; i != tvi.Length; ++i)
                    {
                        tvi[i].I0 = s[i * 3 + 2] + offset;
                        tvi[i].I1 = s[i * 3 + 1] + offset;
                        tvi[i].I2 = s[i * 3 + 0] + offset;
                    }
                    indices.AddRange(tvi);
                }
            }
        }
        #endregion


        protected override void Update(GameTime gameTime)
        {
            // integrate our system one timestep further.
            world.Step((float)1.0f / 100.0f, false);

            base.Update(gameTime);
        }


    }
}
