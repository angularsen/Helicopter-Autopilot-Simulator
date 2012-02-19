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
    /// Heightmap mid point settings.
    /// </summary>
    public class HeightmapMidPointSettings
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public HeightmapMidPointSettings()
        {
            Rough = 0.0f;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rough"></param>
        public HeightmapMidPointSettings(float rough)
        {
            Rough = rough;
        }

        /// <summary>
        /// Get or set the minimum delta value.
        /// </summary>
        public float Rough { get; set; }
    }
}

/*======================================================================================================================

									NIN - Nerdy Inverse Network - http://nerdy-inverse.com

======================================================================================================================*/