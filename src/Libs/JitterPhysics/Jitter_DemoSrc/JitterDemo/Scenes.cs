using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jitter;
using Microsoft.Xna.Framework;
using Jitter.LinearMath;
using Jitter.Dynamics;
using Jitter.Collision.Shapes;
using Jitter.Dynamics.Constraints;
using Microsoft.Xna.Framework.Graphics;
using Jitter.Collision;

namespace JitterDemo
{
    class Scenes
    {
        private const int JengaSize = 30;
        private const int PyramidSize = 20;
        private const int RopeSize = 60;
        private const int WallSize = 12;
        private const int TubeSize = 25;

        private World world = null;

        public Scenes(World world){ this.world = world;}

        public void ResetScenes()
        {
            List<RigidBody> toBeRemoved = new List<RigidBody>();

            foreach (RigidBody body in world.RigidBodies)
            {
                if (body.Tag is Boolean) continue;
                toBeRemoved.Add(body);
            }

            foreach (RigidBody body in toBeRemoved)
            {
                world.RemoveBody(body);
            }

          //  CollisionIsland.ClearStack();

            
        }

        public void Tube()
        {
            Matrix rotMatrix = Matrix.CreateRotationY(MathHelper.Pi / 4.0f);
            Vector3 rotVector = Vector3.Right * 4;

            for (int i = 0; i < TubeSize; i++)
            {
                Matrix rotation = Matrix.Identity;
                if (i % 2 == 0) rotation *= Matrix.CreateRotationY(MathHelper.Pi / 8.0f);

                for (int e = 0; e < 8; e++)
                {
                    rotation = rotation * rotMatrix;
                    Vector3 vector = Vector3.Transform(rotVector, rotation);

                    RigidBody body = new RigidBody(new BoxShape(new JVector(1, 1, 2.5f)));
                    body.Position = Conversion.ToJitterVector(vector) + new JVector(0,i+0.5f,0);
                    body.Orientation = Conversion.ToJitterMatrix(rotation);
   
                    world.AddBody(body);
                }
            }
        }

        public void Rope()
        {
    RigidBody last = null;

    for (int i = 0; i < RopeSize; i++)
    {
        RigidBody body = new RigidBody(new BoxShape(JVector.One));
        body.Position = new JVector(i * 1.5f, 0.5f, 0);

        JVector jpos2 = body.Position;

        body.Position = jpos2;

        world.AddBody(body);
        body.Update();

        if (last != null)
        {
            JVector jpos3 = last.Position;

            JVector dif; JVector.Subtract(ref jpos2, ref jpos3, out dif);
            JVector.Multiply(ref dif, 0.5f, out dif);
            JVector.Subtract(ref jpos2, ref dif, out dif);

            Constraint cons = new PointConstraint(last, body, dif);
            world.AddConstraint(cons);
        }

        last = body;
    }
        }

public void Pyramid()
{

    for (int i = 0; i < PyramidSize; i++)
    {
        for (int e = i; e < PyramidSize; e++)
        {
            RigidBody body = new RigidBody(new BoxShape(new JVector(1, 1, 1f)));
            body.Position = new JVector((e - i * 0.5f), 0.5f + i * 1.0f, 0.0f);
            body.Friction = 0.3f;
            body.Restitution = 0.2f;
            world.AddBody(body);
        }
    }
}

        Random random = new Random();

        private Color RandomColor
        {
            get { return new Color(random.Next(50,255) / 255.0f,
                random.Next(50, 255) / 255.0f,
                random.Next(50, 255) / 255.0f);
            }
        }

public void Wall()
{
    Shape shape = new CylinderShape(0.5f, 0.5f);

    for (int i = 0; i < WallSize; i++)
    {
        for (int e = 0; e < WallSize; e++)
        {
            RigidBody body = new RigidBody(shape);
            body.Position = new JVector((i % 2 == 0) ? e : e + 0.5f, i + 0.5f, 0);
            world.AddBody(body);
        }
    }

}

public void JengaStack()
{
    for (int i = 0; i < JengaSize; i++)
    {
        bool even = (i % 2 == 0);

        for (int e = 0; e < 3; e++)
        {
            JVector size = (even) ? new JVector(1, 1, 3) : new JVector(3, 1, 1);
            RigidBody body = new RigidBody(new BoxShape(size));
            body.Position = new JVector(even ? e : 1.0f, i + 0.5f, even ? 1.0f : e);
            world.AddBody(body);
        }

    }
}
    }
}
