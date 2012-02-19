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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Simulator.Interfaces;

#endregion

namespace Simulator.Skydome
{
    public class SunlightEffect : Effect
    {
        private static Effect _effectFromFile;

        private SunlightEffect(GraphicsDevice graphicsDevice, Effect sunlightEffect)
            : base(graphicsDevice, sunlightEffect)
        {
            // Default values
            IsSkyDome = false;
            Texture = null;

            LightColorAmbient = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);
            LightColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            LightDirection = new Vector4(100.0f, 100.0f, 100.0f, 1.0f);

            FogDensity = 1f;
            //            FogColor = new Vector3(1.0f, 1.0f, 1.0f);

            HazeTopAltitude = 100.0f;
            DayToSunsetSharpness = 1.5f;
            LargeSunRadiusAttenuation = 1.0f;
            LargeSunLightness = 0.2f;
            SunRadiusAttenuation = 256.0f;
            SunLightness = 0.2f;

//            ApplyParameters();
        }

        public float Alpha { get; set; }

        public bool IsSkyDome { get; set; }

        public Texture2D Texture { get; set; }

        public Matrix WorldViewProjection
        {
            get { return World*View*Projection; }
        }

        public Matrix World { get; set; }

        public Matrix View { get; set; }

        public Matrix Projection { get; set; }

        public Vector4 LightDirection { get; set; }

        public Vector4 LightColor { get; set; }

        public Vector4 LightColorAmbient { get; set; }

//        public Vector3 FogColor { get; set; }

        public float FogDensity { get; set; }

        public float SunLightness { get; set; }

        public float SunRadiusAttenuation { get; set; }

        public float LargeSunLightness { get; set; }

        public float LargeSunRadiusAttenuation { get; set; }

        public float DayToSunsetSharpness { get; set; }

        public float HazeTopAltitude { get; set; }

        public static SunlightEffect Create(GraphicsDevice graphicsDevice, ContentManager content)
        {
            if (_effectFromFile == null)
            {
                _effectFromFile = content.RootDirectory.Contains("Content")
                                      ? content.Load<Effect>("Effects/Sunlight")
                                      : content.Load<Effect>("Content/Effects/Sunlight");
            }

            return new SunlightEffect(graphicsDevice, _effectFromFile);
        }

        public void Update(ICamera camera, SunlightParameters sunlightParameters)
        {
            View = camera.View;
            Projection = camera.Projection;

            if (sunlightParameters != null)
            {
                LightColor = sunlightParameters.LightColor;
                LightColorAmbient = sunlightParameters.LightColorAmbient;
                LightDirection = sunlightParameters.LightDirection;
                FogDensity = sunlightParameters.FogDensity;

                LightColorAmbient = sunlightParameters.LightColorAmbient;
                LightColor = sunlightParameters.LightColor;
                LightDirection = sunlightParameters.LightDirection;

                HazeTopAltitude = sunlightParameters.HazeTopAltitude;
                DayToSunsetSharpness = sunlightParameters.DayToSunsetSharpness;
                LargeSunRadiusAttenuation = sunlightParameters.LargeSunRadiusAttenuation;
                LargeSunLightness = sunlightParameters.LargeSunLightness;
                SunRadiusAttenuation = sunlightParameters.SunRadiusAttenuation;
                SunLightness = sunlightParameters.SunLightness;
            }

            ApplyParameters();
        }

        public void ApplyParameters()
        {
            Parameters["World"].SetValue(World);
            Parameters["WorldIT"].SetValue(World); // Assumes uniform scaling!
            Parameters["WorldViewProj"].SetValue(WorldViewProjection);
            Parameters["ViewInv"].SetValue(Matrix.Invert(View));

            Parameters["Alpha"].SetValue(Alpha);

            Parameters["isSkydome"].SetValue(IsSkyDome);
            Parameters["DiffuseTexture"].SetValue(Texture);

            Parameters["LightDirection"].SetValue(LightDirection);
            Parameters["LightColor"].SetValue(LightColor);
            Parameters["LightColorAmbient"].SetValue(LightColorAmbient);

//            Parameters["FogColor"].SetValue(FogColor);
            Parameters["fDensity"].SetValue(FogDensity);

            Parameters["SunLightness"].SetValue(SunLightness);
            Parameters["sunRadiusAttenuation"].SetValue(SunRadiusAttenuation);
            Parameters["largeSunLightness"].SetValue(LargeSunLightness);
            Parameters["largeSunRadiusAttenuation"].SetValue(LargeSunRadiusAttenuation);
            Parameters["dayToSunsetSharpness"].SetValue(DayToSunsetSharpness);
            Parameters["hazeTopAltitude"].SetValue(HazeTopAltitude);
        }
    }
}