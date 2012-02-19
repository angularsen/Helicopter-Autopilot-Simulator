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

namespace Tutorial3
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
            // creating a box shape, representing the ground
            // and one to create the compound shape
            Shape boxShape = new BoxShape(new JVector(1,1,3));
            Shape groundShape = new BoxShape(new JVector(10, 1, 10));

            // Build the CompoundShape.TransformedShape structure,
            // containing "normal" shapes and position/orientation
            // information.
            CompoundShape.TransformedShape[] transformedShapes = 
                new CompoundShape.TransformedShape[2];

            // Create a rotation matrix (90°)
            JMatrix rotated =
                Conversion.ToJitterMatrix(Matrix.CreateRotationX(MathHelper.PiOver2));

            // the first "sub" shape. A rotatated boxShape.
            transformedShapes[0] = new CompoundShape.TransformedShape(
                boxShape,rotated,JVector.Zero);

            // the second "sub" shape.
            transformedShapes[1] = new CompoundShape.TransformedShape(
                boxShape, JMatrix.Identity, JVector.Zero);

            // Pass the CompoundShape.TransformedShape structure to the compound shape.
            CompoundShape compoundShape = new CompoundShape(transformedShapes);

            RigidBody compoundBody = new RigidBody(compoundShape);

            compoundBody.Position = new JVector(0, 5, 0);

            RigidBody groundBody = new RigidBody(groundShape);
            
            // make the body static, so it can't be moved
            groundBody.IsStatic = true;

            // add the bodies to the world.
            world.AddBody(compoundBody);
            world.AddBody(groundBody);
        }

        private void DrawBoxBody(RigidBody body)
        {
            if (body.Shape is CompoundShape)
            {
                // A bit complicated
                // The shape itself contains shapes with orientation/position
                // Also the body, which owns the shape, has orientation/position

                CompoundShape shape = body.Shape as CompoundShape;

                Matrix matrix = Conversion.ToXNAMatrix(body.Orientation);
                matrix.Translation = Conversion.ToXNAVector(body.Position);

                Matrix matrix1 = Conversion.ToXNAMatrix(shape.Shapes[0].Orientation);
                matrix1.Translation = Conversion.ToXNAVector(shape.Shapes[0].Position);
                matrix1 = Matrix.CreateScale(Conversion.ToXNAVector((shape.Shapes[0].Shape as BoxShape).Size)) * matrix1;

                Matrix matrix2 = Conversion.ToXNAMatrix(shape.Shapes[1].Orientation);
                matrix2.Translation = Conversion.ToXNAVector(shape.Shapes[1].Position);
                matrix2 = Matrix.CreateScale(Conversion.ToXNAVector((shape.Shapes[1].Shape as BoxShape).Size)) * matrix2;

                DrawModel(boxModel, matrix1 * matrix, Color.LightPink);
                DrawModel(boxModel, matrix2 * matrix, Color.LightBlue);
            }
            else
            {
                Matrix matrix = Conversion.ToXNAMatrix(body.Orientation);
                matrix.Translation = Conversion.ToXNAVector(body.Position);
                matrix *= Matrix.CreateScale(Conversion.ToXNAVector((body.Shape as BoxShape).Size));

                DrawModel(boxModel, matrix, Color.LightGreen);
            }
        }

        // Draw helper
        private void DrawModel(Model model,Matrix matrix,Color color)
        {
            ModelMesh mesh = boxModel.Meshes[0];

            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.DiffuseColor = color.ToVector3();
                effect.EnableDefaultLighting();
                effect.World = matrix;

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

        protected override void Update(GameTime gameTime)
        {
            // integrate our system one timestep further.
            world.Step((float)1.0f / 100.0f, false);

            base.Update(gameTime);
        }


    }
}
