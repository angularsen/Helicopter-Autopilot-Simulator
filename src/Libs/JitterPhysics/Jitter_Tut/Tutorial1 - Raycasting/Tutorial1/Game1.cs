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

namespace Tutorial1
{

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Model boxModel;

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
            boxBody1.Position = new JVector(0, 2, 0.0f);
            boxBody2.Position = new JVector(0, 5, 0.2f);
            
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

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // draw all our bodies
            foreach (RigidBody body in world.RigidBodies)
            {
                DrawBoxBody(body);
            }

            base.Draw(gameTime);
        }

        // Helper method to get the 3d ray
        private Vector3 RayTo(int x, int y)
        {
            Vector3 nearSource = new Vector3(x, y, 0);
            Vector3 farSource = new Vector3(x, y, 1);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource, 
                projectionMatrix, viewMatrix, world);
            Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource,
                projectionMatrix, viewMatrix, world);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return direction;
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                // the camera position
                JVector rayStart = new JVector(10, 7, 4);

                // the direction in which we are looking
                JVector rayDirection = Conversion.ToJitterVector(
                    RayTo(mouseState.X, mouseState.Y)) * 100.0f;

                RigidBody hitBody;
                JVector hitNormal;
                float hitFraction;

                // shooot!
                bool result = world.CollisionSystem.Raycast(rayStart, rayDirection, 
                    null, out hitBody, out hitNormal, out hitFraction);

                if (result)
                {
                    // we hit something - activate it (because it could be sleeping)
                    hitBody.IsActive = true;
                    // add a force in the direction we look
                    hitBody.AddForce(rayDirection * 2f);
                }

            }

            // integrate our system one timestep further.
            world.Step((float)1.0f / 100.0f, false);

            base.Update(gameTime);
        }


    }
}
