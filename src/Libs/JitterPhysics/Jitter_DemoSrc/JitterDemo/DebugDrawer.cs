using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JitterDemo
{

    /// <summary>
    /// Draw axis aligned bounding boxes, points and lines.
    /// </summary>
    public class DebugDrawer : DrawableGameComponent
    {
        BasicEffect basicEffect;

        public DebugDrawer(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            basicEffect = new BasicEffect(this.GraphicsDevice, null);
            basicEffect.VertexColorEnabled = true;
        }

        public void DrawLine(Vector3 p0, Vector3 p1, Color color)
        {
            LineList.Add(new VertexPositionColor(p0, color));
            LineList.Add(new VertexPositionColor(p1, color));
        }

        private static void SetElement(ref Vector3 v, int index, float value)
        {
            if (index == 0)
                v.X = value;
            else if (index == 1)
                v.Y = value;
            else if (index == 2)
                v.Z = value;
            else
                throw new ArgumentOutOfRangeException("index");
        }

        private static float GetElement(Vector3 v, int index)
        {
            if (index == 0)
                return v.X;
            if (index == 1)
                return v.Y;
            if (index == 2)
                return v.Z;

            throw new ArgumentOutOfRangeException("index");
        }

        public void DrawAabb(Vector3 from, Vector3 to, Color color)
        {
            Vector3 halfExtents = (to - from) * 0.5f;
            Vector3 center = (to + from) * 0.5f;

            Vector3 edgecoord = new Vector3(1f, 1f, 1f), pa, pb;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    pa = new Vector3(edgecoord.X * halfExtents.X, edgecoord.Y * halfExtents.Y,
                        edgecoord.Z * halfExtents.Z);
                    pa += center;

                    int othercoord = j % 3;
                    SetElement(ref edgecoord, othercoord, GetElement(edgecoord, othercoord) * -1f);
                    pb = new Vector3(edgecoord.X * halfExtents.X, edgecoord.Y * halfExtents.Y,
                        edgecoord.Z * halfExtents.Z);
                    pb += center;

                    DrawLine(pa, pb, color);
                }
                edgecoord = new Vector3(-1f, -1f, -1f);
                if (i < 3)
                    SetElement(ref edgecoord, i, GetElement(edgecoord, i) * -1f);
            }
        }


        public List<VertexPositionColor> PointList = new List<VertexPositionColor>();
        public List<VertexPositionColor> LineList = new List<VertexPositionColor>();

        public void DrawPoint(Vector3 pos, Color color)
        {
            PointList.Add(new VertexPositionColor(pos, color));
        }

        public override void Draw(GameTime gameTime)
        {
            if (LineList.Count + PointList.Count == 0) return;

            JitterDemo demo = Game as JitterDemo;
            
            basicEffect.View = demo.Camera.View;
            basicEffect.Projection = demo.Camera.Projection;

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                GraphicsDevice.RenderState.PointSize = 5;
                GraphicsDevice.VertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);

                if (PointList.Count > 0) 
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.PointList, PointList.ToArray(), 0, PointList.Count);

                if (LineList.Count > 0) 
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, LineList.ToArray(), 0, LineList.Count / 2);

                pass.End();
            }
            basicEffect.End();

            base.Draw(gameTime);
        }

    }
}
