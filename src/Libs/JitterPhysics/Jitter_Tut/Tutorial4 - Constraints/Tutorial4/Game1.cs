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
using Jitter.Dynamics.Constraints;

namespace Tutorial4
{

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Model boxModel;

        // the reference to our physics world.
        World world;
        Matrix viewMatrix, projectionMatrix;

        BasicEffect basicEffect = null;

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

            basicEffect = new BasicEffect(this.GraphicsDevice, null);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            boxModel = Content.Load<Model>("box");
            base.LoadContent();
        }

        PointConstraint pointConstraint;

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
            boxBody1.Position = new JVector(0, 4, 1.0f);
            boxBody2.Position = new JVector(0, 4, -1.0f);

            pointConstraint = new PointConstraint(
                boxBody1, boxBody2, new JVector(0, 4f, 0));

            // add a force to one body - so it's not that boring
            boxBody1.AddForce(JVector.One * 10);

            world.AddConstraint(pointConstraint);

            // make the body static, so it can't be moved
            groundBody.IsStatic = true;

            // add the bodies to the world.
            world.AddBody(boxBody1);
            world.AddBody(boxBody2);
            world.AddBody(groundBody);
        }

        private void DrawBoxBody(RigidBody body)
        {
            // We know that the shape is a boxShape
            BoxShape shape = body.Shape as BoxShape;

            // Create the 4x4 xna matrix, containing the orientation
            // (represented in jitter by a 3x3 matrix) and the position.
            Matrix matrix = Conversion.ToXNAMatrix(body.Orientation);
            matrix.Translation = Conversion.ToXNAVector(body.Position);

            // We have a (1,1,1) box so packing the box size
            // information into the the "scale" part of the xna
            // matrix is a good idea.
            Matrix scaleMatrix = Matrix.CreateScale(shape.Size.X,
                shape.Size.Y, shape.Size.Z);

            // the next lines of code draw the boxModel using the matrix.
            ModelMesh mesh = boxModel.Meshes[0];

            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.DiffuseColor = ((Color)body.Tag).ToVector3();
                effect.EnableDefaultLighting();
                effect.World = scaleMatrix * matrix;

                effect.View = viewMatrix;
                effect.Projection = projectionMatrix;
            }
            mesh.Draw();
        }

        List<JVector> lineList = new List<JVector>();
        List<JVector> pointList = new List<JVector>();

        private void DrawDebugConstraints()
        {
            pointConstraint.AddToDebugDrawList(lineList, pointList);

            basicEffect.Begin();
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
            basicEffect.DiffuseColor = Color.Blue.ToVector3();

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                GraphicsDevice.RenderState.PointSize = 5;
                GraphicsDevice.VertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);

                if (pointList.Count > 0) GraphicsDevice.DrawUserPrimitives<JVector>(PrimitiveType.PointList, pointList.ToArray(), 0, pointList.Count);
                if (lineList.Count > 0) GraphicsDevice.DrawUserPrimitives<JVector>(PrimitiveType.LineList, lineList.ToArray(), 0, lineList.Count / 2);

                pass.End();
            }
            basicEffect.End();

            lineList.Clear();
            pointList.Clear();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawDebugConstraints();

            // draw all our bodies
            foreach (RigidBody body in world.RigidBodies)
            {
                DrawBoxBody(body);
            }

            base.Draw(gameTime);
        }

        protected override void Update(GameTime gameTime)
        {
            // integrate our system one timestep further.
            world.Step((float)1.0f / 100.0f, false);

            base.Update(gameTime);
        }


    }
}
