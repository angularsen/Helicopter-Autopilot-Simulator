using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JitterDemo
{

    /// <summary>
    /// Renders the ground.
    /// Based of an example from msdn:
    /// http://msdn.microsoft.com/en-us/library/bb464051%28XNAGameStudio.10%29.aspx
    /// </summary>
    class QuadDrawer : DrawableGameComponent
    {
        private Texture2D texture;
        private BasicEffect quadEffect;

        private VertexPositionNormalTexture[] Vertices;
        private int[] Indexes;
        private Vector3 origin = Vector3.Zero;
        private Vector3 normal = Vector3.Up;
        private Vector3 up = Vector3.Backward;
        private Vector3 left;
        private Vector3 upperLeft, upperRight, lowerLeft, lowerRight;

        public QuadDrawer(Game game) : base(game)
        {
            Vertices = new VertexPositionNormalTexture[4];
            Indexes = new int[6];

            float width = 240.0f;
            float height = 240.0f;

            // Calculate the quad corners
            left = Vector3.Cross(normal, up);
            Vector3 uppercenter = (up * height / 2) + origin;
            upperLeft = uppercenter + (left * width / 2);
            upperRight = uppercenter - (left * width / 2);
            lowerLeft = upperLeft - (up * height);
            lowerRight = upperRight - (up * height);

            FillVertices();
        }

        private void FillVertices()
        {
            // Fill in texture coordinates to display full texture
            // on quad
            float size = 40.0f;

            Vector2 textureUpperLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureUpperRight = new Vector2(size, 0.0f);
            Vector2 textureLowerLeft = new Vector2(0.0f, size);
            Vector2 textureLowerRight = new Vector2(size, size);

            // Provide a normal for each vertex
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal = normal;
            }

            // Set the position and texture coordinate for each
            // vertex
            Vertices[0].Position = lowerLeft;
            Vertices[0].TextureCoordinate = textureLowerLeft;
            Vertices[1].Position = upperLeft;
            Vertices[1].TextureCoordinate = textureUpperLeft;
            Vertices[2].Position = lowerRight;
            Vertices[2].TextureCoordinate = textureLowerRight;
            Vertices[3].Position = upperRight;
            Vertices[3].TextureCoordinate = textureUpperRight;

            // Set the index buffer for each vertex, using
            // clockwise winding
            Indexes[0] = 0;
            Indexes[1] = 1;
            Indexes[2] = 2;
            Indexes[3] = 2;
            Indexes[4] = 1;
            Indexes[5] = 3;

        }

        VertexDeclaration quadVertexDecl;
        SpriteBatch spriteBatch;

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            texture = this.Game.Content.Load<Texture2D>("checker");
            quadEffect = new BasicEffect(this.GraphicsDevice, null);
            quadEffect.EnableDefaultLighting();

            quadEffect.World = Matrix.Identity;
            quadEffect.TextureEnabled = true;
            quadEffect.Texture = texture;

            quadVertexDecl = new VertexDeclaration(this.GraphicsDevice, VertexPositionNormalTexture.VertexElements);


            base.LoadContent();
        }

        public Matrix View, Projection;

        public override void Draw(GameTime gameTime)
        {
            JitterDemo demo = Game as JitterDemo;

            GraphicsDevice.VertexDeclaration = quadVertexDecl;

            GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap; 

            quadEffect.Begin();

            quadEffect.View = demo.Camera.View;
            quadEffect.Projection = demo.Camera.Projection;

            quadEffect.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f);

            foreach (EffectPass pass in quadEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                GraphicsDevice.DrawUserIndexedPrimitives
                    <VertexPositionNormalTexture>(
                    PrimitiveType.TriangleList,
                    Vertices, 0, 4,
                    Indexes, 0, 2);

                pass.End();
            }
            quadEffect.End();

            base.Draw(gameTime);
        }
    }
}
