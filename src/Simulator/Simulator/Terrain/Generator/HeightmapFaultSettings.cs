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
    /// Heightmap fault settings.
    /// </summary>
    public class HeightmapFaultSettings
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public HeightmapFaultSettings()
        {
            MinimumDelta = 0;
            MaximumDelta = 0;

            Iterations = 0;
            IterationsPerFilter = 0;

            FilterValue = 0.0f;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="minDelta"></param>
        /// <param name="maxDelta"></param>
        /// <param name="iterations"></param>
        /// <param name="iterationsPerFilter"></param>
        /// <param name="filterValue"></param>
        public HeightmapFaultSettings(int minDelta, int maxDelta, int iterations, int iterationsPerFilter,
                                      float filterValue)
        {
            MinimumDelta = minDelta;
            MaximumDelta = maxDelta;

            Iterations = iterations;
            IterationsPerFilter = iterationsPerFilter;

            FilterValue = filterValue;
        }

        /// <summary>
        /// Get or set the minimum delta value.
        /// </summary>
        public int MinimumDelta { get; set; }

        /// <summary>
        /// Get or set the maximum delta value.
        /// </summary>
        public int MaximumDelta { get; set; }

        /// <summary>
        /// Get or set the iterations count.
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// Get or set the iterations count per filter value.
        /// </summary>
        public int IterationsPerFilter { get; set; }

        /// <summary>
        /// Get or set the filter value.
        /// </summary>
        public float FilterValue { get; set; }
    }
}

/*======================================================================================================================

									NIN - Nerdy Inverse Network - http://nerdy-inverse.com

======================================================================================================================*/