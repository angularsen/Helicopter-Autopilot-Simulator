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
    /// Heightmap particle deposition settings.
    /// </summary>
    public class HeightmapParticleDepositionSettings
    {
        /// <summary>
        /// Minimum particles per jump.
        /// </summary>
        private readonly int _minParticlesPerJump;

        /// <summary>
        /// Maximum particles per jump.
        /// </summary>
        private int _maxParticlesPerJump;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public HeightmapParticleDepositionSettings()
        {
            Caldera = 0.0f;

            Jumps = 0;
            PeakWalk = 0;

            _minParticlesPerJump = 0;
            _maxParticlesPerJump = 0;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="caldera"></param>
        /// <param name="jumps"></param>
        /// <param name="peakWalk"></param>
        /// <param name="minParticlesPerJump"></param>
        /// <param name="maxParticlesPerJump"></param>
        public HeightmapParticleDepositionSettings(float caldera, int jumps, int peakWalk, int minParticlesPerJump,
                                                   int maxParticlesPerJump)
        {
            Caldera = caldera;

            Jumps = jumps;
            PeakWalk = peakWalk;

            _minParticlesPerJump = minParticlesPerJump;
            _maxParticlesPerJump = maxParticlesPerJump;
        }

        /// <summary>
        /// Get or set the caldera height.
        /// </summary>
        public float Caldera { get; set; }

        /// <summary>
        /// Get or set the number of jumps.
        /// </summary>
        public int Jumps { get; set; }

        /// <summary>
        /// Get or set the peak walk value.
        /// </summary>
        public int PeakWalk { get; set; }

        /// <summary>
        /// Get or set the minimum particles per jump count.
        /// </summary>
        public int MinParticlesPerJump
        {
            get { return _minParticlesPerJump; }
            set { _maxParticlesPerJump = value; }
        }

        /// <summary>
        /// Get or set the maximum particles per jump count.
        /// </summary>
        public int MaxParticlesPerJump
        {
            get { return _maxParticlesPerJump; }
            set { _maxParticlesPerJump = value; }
        }
    }
}

/*======================================================================================================================

									NIN - Nerdy Inverse Network - http://nerdy-inverse.com

======================================================================================================================*/