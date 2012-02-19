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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NINFocusOnTerrain;
using Simulator.Interfaces;
using Simulator.Skydome;
using Simulator.Terrain.Generator;

#endregion

namespace Simulator.Terrain
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TerrainComponent : DrawableGameComponent
    {
//        public readonly IList<TerrainMesh> TerrainMeshes;
        public TerrainMesh Mesh;
        public Heightmap Heightmap;
        private readonly ContentManager _content;
        private readonly SunlightParameters _sunParams;
        private int _width;
        private float _minHeight;
        private float _maxHeight;
        private Texture2D _day;
        private Texture2D _groundTexture;
        private Texture2D _mudTexture;
        private Texture2D _night;
        private Texture2D _rockTexture;
        private Texture2D _snowTexture;
        private Texture2D _sunset;
        private Effect _terrainEffect;
        private readonly ICameraProvider _cameraProvider;

        #region Contructors

        public TerrainComponent(Game game, ICameraProvider cameraProvider, SunlightParameters sunParams)//, Vector3 position)
            : base(game)
        {
            if (game == null) throw new ArgumentNullException("game");
            if (sunParams == null) throw new ArgumentNullException("sunParams");
            if (cameraProvider == null) throw new ArgumentNullException("cameraProvider");

            _cameraProvider = cameraProvider;
            _sunParams = sunParams;

            _content = new ContentManager(Game.Services);

//            Position = position;  position does not have a function at the moment
//            TerrainMeshes = new List<TerrainMesh>();
        }

        #endregion

//        public Vector3 Position { get; set; }

        private ICamera Camera { get { return _cameraProvider.Camera; } }


        #region Overrides

        protected override void LoadContent()
        {
//            _terrainTexture = _content.Load<Texture2D>("Content/Textures/ground");
            _terrainEffect = _content.Load<Effect>("Content/Effects/TerrainAndSkydome");

            _day = _content.Load<Texture2D>("Content/Textures/SkyDay");
            _sunset = _content.Load<Texture2D>("Content/Textures/Sunset");
            _night = _content.Load<Texture2D>("Content/Textures/SkyNight");


            _groundTexture = _content.Load<Texture2D>("Content/Textures/TerrainGround");
            _mudTexture = _content.Load<Texture2D>("Content/Textures/TerrainMud");
            _rockTexture = _content.Load<Texture2D>("Content/Textures/TerrainRock");
            _snowTexture = _content.Load<Texture2D>("Content/Textures/TerrainSnow");

            base.LoadContent();
        }


        public override void Draw(GameTime gameTime)
        {
            Matrix view = Camera.View;
            Matrix projection = Camera.Projection;

            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.AlphaBlendEnable = false;
            GraphicsDevice.RenderState.AlphaTestEnable = false;

            Effect effect = _terrainEffect;
            effect.Parameters["g_texGround"].SetValue(_groundTexture);
            effect.Parameters["g_texMud"].SetValue(_mudTexture);
            effect.Parameters["g_texRock"].SetValue(_rockTexture);
            effect.Parameters["g_texSnow"].SetValue(_snowTexture);

            effect.Parameters["g_vecHeights"].SetValue(new Vector3(20, 50, 80));

//            effect.Parameters["World"].SetValue(world);
//            effect.Parameters["WorldIT"].SetValue(world);
//            effect.Parameters["WorldViewProj"].SetValue(world * view * projection);
            effect.Parameters["ViewInv"].SetValue(Matrix.Invert(view));

//            effect.Parameters["DiffuseTexture"].SetValue(_terrainTexture);
            effect.Parameters["SkyTextureNight"].SetValue(_night);
            effect.Parameters["SkyTextureSunset"].SetValue(_sunset);
            effect.Parameters["SkyTextureDay"].SetValue(_day);

            effect.Parameters["isSkydome"].SetValue(false);

            effect.Parameters["LightDirection"].SetValue(_sunParams.LightDirection);
            effect.Parameters["LightColor"].SetValue(_sunParams.LightColor);
            effect.Parameters["LightColorAmbient"].SetValue(_sunParams.LightColorAmbient);
            effect.Parameters["FogColor"].SetValue(_sunParams.FogColor);
            effect.Parameters["fDensity"].SetValue(_sunParams.FogDensity);
            effect.Parameters["SunLightness"].SetValue(_sunParams.SunLightness);
            effect.Parameters["sunRadiusAttenuation"].SetValue(_sunParams.SunRadiusAttenuation);
            effect.Parameters["largeSunLightness"].SetValue(_sunParams.LargeSunLightness);
            effect.Parameters["largeSunRadiusAttenuation"].SetValue(_sunParams.LargeSunRadiusAttenuation);
            effect.Parameters["dayToSunsetSharpness"].SetValue(_sunParams.DayToSunsetSharpness);
            effect.Parameters["hazeTopAltitude"].SetValue(_sunParams.HazeTopAltitude);


            Matrix world = Mesh.World;
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["WorldIT"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
            effect.Parameters["WorldViewProj"].SetValue(world*view*projection);
            Mesh.Draw(gameTime, effect);

//            foreach (TerrainMesh mesh in TerrainMeshes)
//            {
//                Matrix world = mesh.World;
//                effect.Parameters["World"].SetValue(world);
//                effect.Parameters["WorldIT"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
//                effect.Parameters["WorldViewProj"].SetValue(world*view*projection);
//                mesh.Draw(gameTime, effect);
//            }

            base.Draw(gameTime);
        }

        #endregion

        public static void RemapModel(Model model, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }
            }
        }

        public void BuildTerrain(int width, float minHeight, float maxHeight)
        {
            _width = width;
            _minHeight = minHeight;
            _maxHeight = maxHeight;

            // When running test scenarios over and over we need to generate the exact same terrains
            const int myRandomTerrainSeed = 1337;
            Heightmap = new Heightmap(myRandomTerrainSeed);
            Heightmap.GenerateFaultHeightmap(_width, _width, _minHeight, _maxHeight,
                                             new HeightmapFaultSettings(16, 256, 1024, 128, 0.5f));

            // TODO Adding world transformation confuses the sunlight/terrain shader.. not sure why
            //                const float scale = 10;
            Matrix world = Matrix.Identity;
            // Matrix.CreateScale(scale) * Matrix.CreateTranslation(scale * Position);

            Mesh = new TerrainMesh(Game, _cameraProvider, Heightmap, world);
            Mesh.Initialize();
//            TerrainMeshes.Add(first);
            return;
        }
    }
}