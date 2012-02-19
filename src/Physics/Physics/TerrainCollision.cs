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
using System.Collections.Generic;
using Jitter;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simulator.Interfaces;

#endregion

namespace Physics
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TerrainCollision : DrawableGameComponent
    {
            public ICameraTarget Helicopter;
        public readonly World World;
//        private Model _boxModel;
//        private bool _leftButtonPressed;
        private RigidBody _terrainBody;
        //        private Model _torusModel;
        private Shape _helicopterShape;

        private enum Models { box, sphere, cylinder, cone, capsule, triangle, num }

        private Model[] models = new Model[(int)Models.num];
        public RigidBody HelicopterBody;
        private float[,] _heightValues;
        private readonly ICameraProvider _cameraProvider;

        public event Action<GameTime> CollidedWithTerrain;


        public TerrainCollision(Game game, ICameraProvider cameraProvider)
            : base(game)
        {
            if (game == null || cameraProvider == null) throw new ArgumentNullException();

            _cameraProvider = cameraProvider;

            World = new World(new CollisionSystemSAP());
        }

        private ICamera Camera { get { return _cameraProvider.Camera; } }


        public void Reset()
        {
            World.Clear();
            BuildScene();
        }

        private static void ResetBody(RigidBody body)
        {
            if (body != null)
            {
                body.AngularVelocity = JVector.Zero;
                body.LinearVelocity = JVector.Zero;
                body.Update();
            }
        }

        public void SetGravity(Vector3 acceleration)
        {
            World.Gravity = Conversion.ToJitterVector(acceleration);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            models[(int)Models.box] = Game.Content.Load<Model>("Models/Primitives/box");
            models[(int)Models.sphere] = Game.Content.Load<Model>("Models/Primitives/sphere");
            models[(int)Models.cylinder] = Game.Content.Load<Model>("Models/Primitives/cylinder");
            models[(int)Models.cone] = Game.Content.Load<Model>("Models/Primitives/cone");
            models[(int)Models.capsule] = Game.Content.Load<Model>("Models/Primitives/capsule");
            models[(int)Models.triangle] = Game.Content.Load<Model>("Models/Primitives/staticmesh");

            //            _boxModel = Game.Content.Load<Model>("Models/box");
            //            _torusModel = Game.Content.Load<Model>("Models/torus");

            // build our basic scene
            BuildScene();

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (HelicopterBody != null &&
                HelicopterBody.CollisionIsland != null &&
                HelicopterBody.CollisionIsland.Arbiter != null)
            {
                foreach (var arbiter in HelicopterBody.CollisionIsland.Arbiter)
                {
                    if (HelicopterBody == arbiter.Body1 || HelicopterBody == arbiter.Body2 &&
                                                           _terrainBody == arbiter.Body1 ||
                        _terrainBody == arbiter.Body2)
                    {
                        if (CollidedWithTerrain != null)
                            CollidedWithTerrain(gameTime);
                    }
                }
            }

            // Note: Commented out while using HeliPhysics to call the World.Step() method
            //            World.Step((float) gameTime.ElapsedGameTime.TotalSeconds, false);

            base.Update(gameTime);
        }

        private void AddShape(Shape shape, JVector position, JVector velocity)
        {
            var body = new RigidBody(shape);
            body.Tag = Color.LightPink;
            body.Position = position;
            body.LinearVelocity = velocity;
            body.AngularVelocity = new JVector(0.5f);
            //            body.Restitution = 0.1f;  note: not implemented?

            // add the bodies to the world.
            World.AddBody(body);
        }




        public override void Draw(GameTime gameTime)
        {
            //            GraphicsDevice.Clear(Color.CornflowerBlue);

            //            SetAllModelEffectParameters();
            //
            // draw all our bodies
            //            foreach (RigidBody body in World.RigidBodies)
            //            {
            //                if (body == _terrainBody) continue;
            //                DrawBody(body);
            //            }

            base.Draw(gameTime);
        }

        private void SetAllModelEffectParameters()
        {
            foreach (Model model in models)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.DiffuseColor = Color.LightGray.ToVector3();
                        effect.Projection = Camera.Projection;
                        effect.View = Camera.View;
                        effect.EnableDefaultLighting();
                    }
                }
            }
        }

        public void SetHeightValues(float[,] heightValues)
        {
            _heightValues = heightValues;
            Reset();
        }

        private void SetHeightmap(float[,] heightValues)
        {
            // Remove any previous heightmaps
            if (_terrainBody != null)
                World.RemoveBody(_terrainBody);

            _heightValues = heightValues;

            if (heightValues != null)
            {
                //            new TerrainShape()
                JOctree terrainOctree = HeightmapToOctree(heightValues);
                var terrainShape = new TriangleMeshShape(terrainOctree);

                _terrainBody = new RigidBody(terrainShape);
                _terrainBody.IsStatic = true;
                _terrainBody.Friction = 0.5f;

                World.AddBody(_terrainBody);
            }

        }

        #region Private methods

        private static JOctree HeightmapToOctree(float[,] heightValues)
        {
            if (heightValues == null) throw new ArgumentNullException("heightValues");

            int rows = heightValues.GetLength(0);
            int cols = heightValues.GetLength(1);

            var vertexPoints = new List<JVector>();
            var vertexIndices = new List<JOctree.TriangleVertexIndices>();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    float height = heightValues[row, col];
                    vertexPoints.Add(new JVector(col, height, row));
                    //                        MyMathHelper.Lerp(col, 0, cols, -128, 128), 
                    //                        height, 
                    //                        MyMathHelper.Lerp(row, 0, rows, -128, 128)));
                }
            }

            int numQuadCols = (cols - 1);
            int numQuadRows = (rows - 1);
            for (int quadRow = 0; quadRow < numQuadRows; quadRow++)
            {
                for (int quadCol = 0; quadCol < numQuadCols; quadCol++)
                {
                    // VI = Vertex Index
                    int topLeftVI = quadRow * cols + quadCol;
                    int topRightVI = topLeftVI + 1;
                    int bottomLeftVI = (quadRow + 1) * cols + quadCol;
                    int bottomRightVI = bottomLeftVI + 1;

                    vertexIndices.Add(new JOctree.TriangleVertexIndices(topLeftVI, topRightVI, bottomRightVI));
                    vertexIndices.Add(new JOctree.TriangleVertexIndices(bottomRightVI, bottomLeftVI, topLeftVI));
                }
            }

            return new JOctree(vertexPoints, vertexIndices);
        }

        /// <summary>
        /// DO NOT USE THIS!
        /// </summary>
        public void TmpBuildScene()
        {
            BuildScene();
        }

        private void BuildScene()
        {
            const float cockpitWidth = 0.15f;
            const float cockpitLength = 0.3f;
            const float cockpitHeight = 0.3f;
//            const float skidLength = 0.15f;
//            const float skidDiameter = 0.005f;

            //            var cockpitBoxShape = new BoxShape(new JVector(cockpitWidth, cockpitHeight, cockpitLength));
            //
            //
            //            var skidCylinderShape = new CylinderShape(skidLength, skidDiameter/2);
            //            var rotX = Matrix.CreateRotationX(MathHelper.ToRadians(90));
            //            JMatrix jRotX = Conversion.ToJitterMatrix(rotX);
            //
            //            var leftSkid = new CompoundShape.TransformedShape(skidCylinderShape, jRotX, new JVector(-cockpitWidth/2 - skidDiameter/2, 0, -skidLength/2));
            //            var rightSkid = new CompoundShape.TransformedShape(skidCylinderShape, jRotX, new JVector(+cockpitWidth/2 + skidDiameter/2, 0, -skidLength/2));
            //            var cockpit = new CompoundShape.TransformedShape(cockpitBoxShape, JMatrix.Identity, new JVector(-cockpitWidth/2, 0, -cockpitLength/2));
            //            _helicopterShape = new CompoundShape(new[]
            //                                                        {
            //                                                            leftSkid, rightSkid, cockpit
            //                                                        });

            // TODO Add skids and rotor bounding boxes
            _helicopterShape = new BoxShape(new JVector(cockpitWidth, cockpitHeight, cockpitLength));

            HelicopterBody = new RigidBody(_helicopterShape);
            World.AddBody(HelicopterBody);
            HelicopterBody.Friction = 30;
            HelicopterBody.Mass = 2;

            World.SetDampingFactors(0.8f, 0.999f);

            SetHeightmap(_heightValues);

            // creating two boxShapes with different sizes
            //            Shape boxShape = new BoxShape(JVector.One);
            //            Shape groundShape = new BoxShape(new JVector(10, 1, 10));

            // create new instances of the rigid body class and pass 
            // the boxShapes to them
            //            var boxBody1 = new RigidBody(boxShape);
            //            var boxBody2 = new RigidBody(boxShape);
            //
            //            boxBody1.Tag = Color.LightPink;
            //            boxBody2.Tag = Color.LightSkyBlue;

            //            var groundBody = new RigidBody(groundShape);
            //            groundBody.Tag = Color.LightGreen;

            // set the position of the box size=(1,1,1)
            // 2 and 5 units above the ground box size=(10,1,10)
            //            boxBody1.Position = new JVector(128, 50, 128);
            //            boxBody2.Position = new JVector(125, 50, 128);

            // make the body static, so it can't be moved
            //            groundBody.IsStatic = true;

            // add the bodies to the world.
            //            _world.AddBody(boxBody1);
            //            _world.AddBody(boxBody2);
            //            _world.AddBody(groundBody);
        }

        private void ExtractData(List<JVector> vertices, List<JOctree.TriangleVertexIndices> indices, Model model)
        {
            var bones = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(bones);
            foreach (ModelMesh mm in model.Meshes)
            {
                Matrix xform = bones[mm.ParentBone.Index];
                foreach (ModelMeshPart mmp in mm.MeshParts)
                {
                    int offset = vertices.Count;
                    var a = new Vector3[mmp.NumVertices];
                    mm.VertexBuffer.GetData(mmp.StreamOffset + mmp.BaseVertex * mmp.VertexStride,
                                            a, 0, mmp.NumVertices, mmp.VertexStride);
                    for (int i = 0; i != a.Length; ++i)
                        Vector3.Transform(ref a[i], ref xform, out a[i]);

                    for (int i = 0; i < a.Length; i++) vertices.Add(new JVector(a[i].X, a[i].Y, a[i].Z));

                    if (mm.IndexBuffer.IndexElementSize != IndexElementSize.SixteenBits)
                        throw new Exception(
                            String.Format("Model uses 32-bit indices, which are not supported."));

                    var s = new short[mmp.PrimitiveCount * 3];
                    mm.IndexBuffer.GetData(mmp.StartIndex * 2, s, 0, mmp.PrimitiveCount * 3);
                    var tvi = new JOctree.TriangleVertexIndices[mmp.PrimitiveCount];
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


        private void DrawBody(RigidBody rb)
        {
            if (rb.Tag is bool && ((bool)rb.Tag) == true) return;

            bool isCompoundShape = (rb.Shape is CompoundShape);

            if (!isCompoundShape)
            {
                DrawShape(rb.Shape, rb.Orientation, rb.Position);
            }
            else
            {
                CompoundShape cShape = rb.Shape as CompoundShape;
                JMatrix orientation = rb.Orientation;
                JVector position = rb.Position;

                foreach (var ts in cShape.Shapes)
                {
                    JVector pos = ts.Position;
                    JMatrix ori = ts.Orientation;

                    JVector.Transform(ref pos, ref orientation, out pos);
                    JVector.Add(ref pos, ref position, out pos);

                    JMatrix.Multiply(ref ori, ref orientation, out ori);

                    DrawShape(ts.Shape, ori, pos);
                }
            }
        }

        private void DrawShape(Shape shape, JMatrix ori, JVector pos)
        {
            Model model = null;
            Matrix scaleMatrix = Matrix.Identity;

            if (shape is TriangleMeshShape)
            {
                model = models[(int)Models.triangle];
            }
            else if (shape is BoxShape)
            {
                model = models[(int)Models.box];
                scaleMatrix = Matrix.CreateScale(Conversion.ToXNAVector((shape as BoxShape).Size));
            }
            else if (shape is SphereShape)
            {
                model = models[(int)Models.sphere];
                scaleMatrix = Matrix.CreateScale((shape as SphereShape).Radius);
            }
            else if (shape is CylinderShape)
            {
                model = models[(int)Models.cylinder];
                CylinderShape cs = shape as CylinderShape;
                scaleMatrix = Matrix.CreateScale(cs.Radius, cs.Height, cs.Radius);
            }
            else if (shape is CapsuleShape)
            {
                model = models[(int)Models.capsule];

            }
            else if (shape is ConeShape)
            {
                ConeShape cs = shape as ConeShape;
                scaleMatrix = Matrix.CreateScale(cs.Radius * 2.0f, cs.Height, cs.Radius * 2.0f);
                model = models[(int)Models.cone];
            }

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index] * scaleMatrix * Conversion.ToXNAMatrix(ori) *
                        Matrix.CreateTranslation(Conversion.ToXNAVector(pos));
                }
                mesh.Draw();
            }
        }

        #endregion
    }
}