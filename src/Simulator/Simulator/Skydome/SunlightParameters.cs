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

#endregion

namespace Simulator.Skydome
{
    public class SunlightParameters
    {
        #region Private Properties

        private float dayToSunsetSharpness = 1.5f;
        private float fDensity = 0.0028f;
        private Vector4 fogColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private float hazeTopAltitude = 100.0f;
        private float largeSunLightness = 0.2f;
        private float largeSunRadiusAttenuation = 1.0f;
        private Vector4 lightColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 lightColorAmbient = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);
        private Vector4 lightDirection = new Vector4(100.0f, 100.0f, 100.0f, 1.0f);
        private float sunLightness = 0.2f;
        private float sunRadiusAttenuation = 256.0f;

        #endregion

        public Vector4 LightDirection
        {
            get { return lightDirection; }
            set { lightDirection = value; }
        }

        public Vector4 LightColor
        {
            get { return lightColor; }
            set { lightColor = value; }
        }

        public Vector4 LightColorAmbient
        {
            get { return lightColorAmbient; }
            set { lightColorAmbient = value; }
        }

        public Vector4 FogColor
        {
            get { return fogColor; }
            set { fogColor = value; }
        }

        public float FogDensity
        {
            get { return fDensity; }
            set { fDensity = value; }
        }

        public float SunLightness
        {
            get { return sunLightness; }
            set { sunLightness = value; }
        }

        public float SunRadiusAttenuation
        {
            get { return sunRadiusAttenuation; }
            set { sunRadiusAttenuation = value; }
        }

        public float LargeSunLightness
        {
            get { return largeSunLightness; }
            set { largeSunLightness = value; }
        }

        public float LargeSunRadiusAttenuation
        {
            get { return largeSunRadiusAttenuation; }
            set { largeSunRadiusAttenuation = value; }
        }

        public float DayToSunsetSharpness
        {
            get { return dayToSunsetSharpness; }
            set { dayToSunsetSharpness = value; }
        }

        public float HazeTopAltitude
        {
            get { return hazeTopAltitude; }
            set { hazeTopAltitude = value; }
        }
    }
}