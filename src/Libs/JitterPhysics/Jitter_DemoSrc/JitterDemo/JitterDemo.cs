#region Using Statements
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Jitter;
using Jitter.Dynamics;
using Jitter.Collision;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using Jitter.Dynamics.Constraints;
#endregion

namespace JitterDemo
{

    public class JitterDemo : Microsoft.Xna.Framework.Game
    {
        private CodeForm codeForm;
        private QuadDrawer quadDrawer;
        private GraphicsDeviceManager graphics;

        public Camera Camera { private set; get; }

        private enum Models { box,sphere,cylinder,cone,capsule,triangle,num }

        private Model[] models = new Model[(int)Models.num];

        private Display display;
        private DebugDrawer debugDrawer;
        private bool multithread = true;
        
        private World world;

        public JitterDemo()
        {
            this.IsMouseVisible = true;
            graphics = new GraphicsDeviceManager(this);
            
            Content.RootDirectory = "Content";

            this.IsFixedTimeStep = false;
            this.graphics.SynchronizeWithVerticalRetrace = false;

            CollisionSystem collision = new CollisionSystemSAP();
            world = new World(collision); world.AllowDeactivation = false;

            codeForm = new CodeForm(world);

            RigidBody ground = new RigidBody(new BoxShape(new JVector(1000, 10, 1000)));
            ground.Position = new JVector(0, -5, 0); ground.Tag = true;
            ground.IsStatic = true; world.AddBody(ground);

            this.Window.Title = "Jitter Demo (Preview) - Jitter © by Thorben Linneweber";
        }

        protected override void Initialize()
        {
            Camera = new Camera(this);
            Camera.Position = new Vector3(15, 15, 30);
            Camera.Target = Camera.Position + Vector3.Normalize(new Vector3(10, 5, 20));

            debugDrawer = new DebugDrawer(this);
            debugDrawer.UpdateOrder = int.MaxValue / 2;

            display = new Display(this);
            display.DrawOrder = int.MaxValue;
            display.DisplayText[5] = "PRESS ENTER TO SHOW SCENE EDITOR";

            quadDrawer = new QuadDrawer(this);

            this.Components.Add(quadDrawer);
            this.Components.Add(display);
            this.Components.Add(debugDrawer);
            this.Components.Add(Camera);
            base.Initialize();
        }

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

        protected override void LoadContent()
        {
            models[(int)Models.box] = Content.Load<Model>("box");
            models[(int)Models.sphere] = Content.Load<Model>("sphere");
            models[(int)Models.cylinder] = Content.Load<Model>("cylinder");
            models[(int)Models.cone] = Content.Load<Model>("cone");
            models[(int)Models.capsule] = Content.Load<Model>("capsule");
            models[(int)Models.triangle] = Content.Load<Model>("staticmesh");

            List<JVector> vertices = new List<JVector>();
            List<JOctree.TriangleVertexIndices> indices =
                new List<JOctree.TriangleVertexIndices>();

            ExtractData(vertices, indices, models[(int)Models.triangle]);
            JOctree octree = new JOctree(vertices, indices);

            TriangleMeshShape triangleShape = new TriangleMeshShape(octree);
            RigidBody triangleBody = new RigidBody(triangleShape);
            triangleBody.IsStatic = true; triangleBody.Tag = false;
            triangleBody.Position = new JVector(-20, 10, -10);
            world.AddBody(triangleBody);
        }

        private Vector3 RayTo(int x, int y)
        {
            Vector3 nearSource = new Vector3(x, y, 0);
            Vector3 farSource = new Vector3(x, y, 1);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource, Camera.Projection, Camera.View, world);
            Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource, Camera.Projection, Camera.View, world);

            Vector3 direction = farPoint - nearPoint;
            return direction;
        }

        private JVector hitPoint,hitNormal;
        private RigidBody resBody;
        private WorldPointConstraint wp;
        private float hitDistance = 0.0f;
        private int scrollWheel = 0;
        private bool codeFormVisible = false;
        private bool leftClicked, spaceClicked, enterClicked, mClicked;
        private Random random = new Random();

        protected override void Update(GameTime gameTime)
        {
            if (codeFormVisible) return;

            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (keyState.IsKeyDown(Keys.Escape)) this.Exit();

            bool leftHold = (mouseState.LeftButton == ButtonState.Pressed);
            bool spaceHold = (keyState.IsKeyDown(Keys.Space));
            bool enterHold = (keyState.IsKeyDown(Keys.Enter));
            bool mHold = (keyState.IsKeyDown(Keys.M));

            #region Turn multithreading on/off
            if (mHold)
            {
                if (!mClicked) { multithread = !multithread; mClicked = true; }
            }
            #endregion

            #region Object drag & drop

            if (leftHold && !leftClicked)
            {
                JVector ray = Conversion.ToJitterVector(RayTo(mouseState.X, mouseState.Y)); ray.Normalize();
                JVector camp = Conversion.ToJitterVector(Camera.Position);

                float fraction;
                bool result = world.CollisionSystem.Raycast(camp, ray * 100, RaycastCallback, out resBody, out hitNormal, out fraction);

                if (result)
                {
                    hitPoint = camp + fraction * ray * 100;

                    if (wp != null) world.RemoveConstraint(wp);

                    JVector lanchor = hitPoint - resBody.Position;
                    lanchor = JVector.Transform(lanchor, JMatrix.Transpose(resBody.Orientation));

                    wp = new WorldPointConstraint(resBody, lanchor);

                    world.AddConstraint(wp);
                    hitDistance = (Conversion.ToXNAVector(hitPoint) - Camera.Position).Length();
                    scrollWheel = mouseState.ScrollWheelValue;
                    wp.Anchor = hitPoint;
                }

                leftClicked = true;

            }

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                hitDistance += (mouseState.ScrollWheelValue - scrollWheel) * 0.001f;
                scrollWheel = mouseState.ScrollWheelValue;

                if (resBody != null)
                {
                    Vector3 ray = RayTo(mouseState.X, mouseState.Y); ray.Normalize();
                    wp.Anchor = Conversion.ToJitterVector(Camera.Position + ray * hitDistance);
                }
            }
            else
            {
                resBody = null;
                if (wp != null) world.RemoveConstraint(wp);
            }

            #endregion

            #region Show code form

            if (enterHold && !enterClicked && gameTime.TotalGameTime.TotalSeconds > 0.1f)
            {
                display.DisplayText[5] = string.Empty;

                System.Windows.Forms.Form form =
                        (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(this.Window.Handle);

                codeFormVisible = true;
                System.Windows.Forms.DialogResult result = codeForm.ShowDialog(form);
                codeFormVisible = false;

                enterClicked = true;
            }
            #endregion

            #region Spawn random primitive
            if (spaceHold && !spaceClicked)
            {
                int rndn = random.Next(6);

                RigidBody body;

                if (rndn == 0)
                {
                    body = new RigidBody(new ConeShape((float)random.Next(5, 50) / 20.0f, (float)random.Next(10, 20) / 20.0f));
                }
                else if (rndn == 1)
                {
                    body = new RigidBody(new BoxShape(new JVector(
                        (float)random.Next(10, 30) / 20.0f,
                        (float)random.Next(10, 30) / 20.0f, (float)random.Next(10, 30) / 20.0f)));
                }
                else if (rndn == 2)
                {
                    body = new RigidBody(new SphereShape((float)random.Next(30,100) /100.0f));
                }
                else if (rndn == 3)
                {
                    body = new RigidBody(new CylinderShape(1.0f, 0.5f));
                }
                else if (rndn == 4)
                {
                    body = new RigidBody(new CapsuleShape(1.0f, 0.5f));
                }
                else
                {
                    Shape b1 = new BoxShape(new JVector(3, 1, 1));
                    Shape b2 = new BoxShape(new JVector(1, 1, 3));
                    Shape b3 = new CylinderShape(2.0f, 0.5f);

                    CompoundShape.TransformedShape t1 = new CompoundShape.TransformedShape(b1, JMatrix.Identity, JVector.Zero);
                    CompoundShape.TransformedShape t2 = new CompoundShape.TransformedShape(b2, JMatrix.Identity, JVector.Zero);
                    CompoundShape.TransformedShape t3 = new CompoundShape.TransformedShape(b3, JMatrix.Identity, new JVector(0, 0, 0));

                    CompoundShape ms = new CompoundShape(new CompoundShape.TransformedShape[3] { t1, t2, t3 });

                    body = new RigidBody(ms);
                }

                world.AddBody(body);

                body.Position = Conversion.ToJitterVector(Camera.Position);
                body.LinearVelocity = Conversion.ToJitterVector((Camera.Target - Camera.Position) * 40.0f);
                body.Update();

                spaceClicked = true;
            }
            #endregion

            spaceClicked = spaceHold;
            leftClicked = leftHold;
            enterClicked = enterHold;
            mClicked = mHold;

            int contactCount = 0;
            foreach (Arbiter ar in world.ArbiterMap.Values)
                contactCount += ar.ContactList.Count;

            display.DisplayText[0] = "Arbitercount: " + world.ArbiterMap.Values.Count.ToString() + ";" + " Contactcount: " + contactCount.ToString();
            display.DisplayText[2] = "Bodycount: " + world.RigidBodies.Count;
            display.DisplayText[3] = (multithread) ? "Multithreaded" : "Single Threaded";

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (elapsedTime > 1.0f / 100.0f) elapsedTime = 1.0f / 100.0f;
            world.Step(elapsedTime, multithread);

            base.Update(gameTime);
        }

        private bool RaycastCallback(RigidBody body, JVector normal, float fraction)
        {
            if (body.IsStatic) return false;
            else return true;
        }

        #region Drawing Rigid Bodies - Not that fast
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

        private void DrawModel(RigidBody rb)
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

                    JVector.Transform(ref pos,ref orientation,out pos);
                    JVector.Add(ref pos, ref position, out pos);

                    JMatrix.Multiply(ref ori, ref orientation, out ori);

                    DrawShape(ts.Shape, ori, pos);
                }

            }

        }

        public void SetAllModelEffectParameters()
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
        #endregion

        List<JVector> pointList = new List<JVector>();
        List<JVector> lineList = new List<JVector>();

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(63, 66, 73));
          
            SetAllModelEffectParameters();
            
            foreach (RigidBody body in world.RigidBodies) DrawModel(body);

            pointList.Clear(); lineList.Clear();
            foreach (Constraint constr in world.Constraints)
                constr.AddToDebugDrawList(lineList, pointList);

            foreach (JVector v in pointList) debugDrawer.DrawPoint(Conversion.ToXNAVector(v), Color.Red);

            for (int i = 0; i < lineList.Count;i+=2)
            {
                debugDrawer.DrawLine(Conversion.ToXNAVector(lineList[i]), 
                    Conversion.ToXNAVector(lineList[i + 1]), Color.Blue);
            }

            display.DisplayText[1] = "Islandcount: " + world.Islands.Count.ToString();

            base.Draw(gameTime);

            debugDrawer.PointList.Clear();
            debugDrawer.LineList.Clear();
        }

    }
}
