#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

#region Using Statements

#endregion

namespace NINFocusOnTerrain
{
    /// <summary>
    /// Heightmap perlin noise settings.
    /// </summary>
    public class HeightmapPerlinNoiseSettings
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public HeightmapPerlinNoiseSettings()
        {
            Seed = 0;
            Persistence = 0.0f;
            Octaves = 0;
            NoiseSize = 0.0f;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="persistence"></param>
        /// <param name="octaves"></param>
        /// <param name="noiseSize"></param>
        public HeightmapPerlinNoiseSettings(int seed, float persistence, int octaves, float noiseSize)
        {
            Seed = seed;
            Persistence = persistence;
            Octaves = octaves;
            NoiseSize = noiseSize;
        }

        /// <summary>
        /// Get or set the seed value.
        /// </summary>
        public int Seed { get; set; }

        /// <summary>
        /// Get or set the persistence value.
        /// </summary>
        public float Persistence { get; set; }

        /// <summary>
        /// Get or set the octaves count.
        /// </summary>
        public int Octaves { get; set; }

        /// <summary>
        /// Get or set the noise size.
        /// </summary>
        public float NoiseSize { get; set; }
    }
}

/*======================================================================================================================

									NIN - Nerdy Inverse Network - http://nerdy-inverse.com

======================================================================================================================*/