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
using System.IO;
using Microsoft.Xna.Framework;

#endregion

namespace NINFocusOnTerrain
{
    /// <summary>
    /// This is the heightmap class.
    /// </summary>
    /// <remarks>
    /// The fractal generation algorithms are inspired from Jason Shankel works in Game Programming Gems 1.
    /// </remarks>
    public class Heightmap : ICloneable
    {
        /// <summary>
        /// Depth of the heightmap.
        /// </summary>
        private int _depth;

        /// <summary>
        /// Default fault settings.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private HeightmapFaultSettings _faultSettings;

        /// <summary>
        /// Height values of the heightmap.
        /// </summary>
        private float[] _heightValues;

        /// <summary>
        /// Maximum height of the heightmap.
        /// </summary>
        private float _maximumHeight;

        private readonly int _randomSeed;

        /// <summary>
        /// Default mid point settings.
        /// </summary>
        private HeightmapMidPointSettings _midPointSettings;

        /// <summary>
        /// Minimum height of the heightmap.
        /// </summary>
        private float _minimumHeight;

        /// <summary>
        /// Default particle deposition settings.
        /// </summary>
        private HeightmapParticleDepositionSettings _particleDepositionSettings;

        /// <summary>
        /// Default perlin noise settings.
        /// </summary>
        private HeightmapPerlinNoiseSettings _perlinNoiseSettings;

        /// <summary>
        /// Width of the heightmap.
        /// </summary>
        private int _width;

        private RandomHelper _random;

        /// <summary>
        /// Default constructor for a flat heightmap.
        /// </summary>
        public Heightmap(int randomSeed) : this(0,0,0,0,null,randomSeed)
        {
        }

        /// <summary>
        /// Default constructor for a prebuilt heightmap.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="data"></param>
        /// <param name="randomSeed"></param>
        public Heightmap(int width, int depth, float min, float max, float[] data, int randomSeed)
        {
            _width = width;
            _depth = depth;

            _minimumHeight = min;
            _maximumHeight = max;
            _randomSeed = randomSeed;

            if (data != null)
                _heightValues = (float[]) data.Clone();

            _random = new RandomHelper(randomSeed);
        }

        /// <summary>
        /// Get the width of the heightmap.
        /// </summary>
        public int Width
        {
            get { return _width; }
        }

        /// <summary>
        /// Get the depth of the heightmap.
        /// </summary>
        public int Depth
        {
            get { return _depth; }
        }

        /// <summary>
        /// Minimum height of the heightmap.
        /// </summary>
        public float MinimumHeight
        {
            get { return _minimumHeight; }
        }

        /// <summary>
        /// Maximum height of the heightmap.
        /// </summary>
        public float MaximumHeight
        {
            get { return _maximumHeight; }
        }

        /// <summary>
        /// Height values of the heightmap.
        /// </summary>
        public float[] HeightValues
        {
            get { return _heightValues; }
        }

        /// <summary>
        /// Get or set the default fault settings.
        /// </summary>
        public HeightmapFaultSettings FaultSettings
        {
            get { return _faultSettings; }
            set { _faultSettings = value; }
        }

        /// <summary>
        /// Get or set the default mid point settings.
        /// </summary>
        public HeightmapMidPointSettings MidPointSettings
        {
            get { return _midPointSettings; }
            set { _midPointSettings = value; }
        }

        /// <summary>
        /// Get or set the default particle deposition settings.
        /// </summary>
        public HeightmapParticleDepositionSettings ParticleDepositionSettings
        {
            get { return _particleDepositionSettings; }
            set { _particleDepositionSettings = value; }
        }

        /// <summary>
        /// Get or set the default perlin noise settings.
        /// </summary>
        public HeightmapPerlinNoiseSettings PerlinNoiseSettings
        {
            get { return _perlinNoiseSettings; }
            set { _perlinNoiseSettings = value; }
        }

        #region ICloneable Members

        /// <summary>
        /// Clone the heightmap.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var heightmap = new Heightmap(_width, _depth, _minimumHeight, _maximumHeight,
                                          (float[]) _heightValues.Clone(), _randomSeed);

            return heightmap;
        }

        #endregion

        /// <summary>
        /// Return the height at the given x,z coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public float GetHeightValue(int x, int z)
        {
            return _heightValues[x + z*_width];
        }

        /// <summary>
        /// Set the height value at the given x,z coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="value"></param>
        public void SetHeightValue(int x, int z, float value)
        {
            if (value > _maximumHeight)
            {
                value = _maximumHeight;
            }

            if (value < _minimumHeight)
            {
                value = _minimumHeight;
            }

            _heightValues[x + z*_width] = value;
        }

        /// <summary>
        /// Return the percentage of the height based on the minimum and maximum values at the given x,z coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public float GetHeightPercentage(int x, int z)
        {
            return (_heightValues[x + z*_width]/_maximumHeight);
        }

        /// <summary>
        /// Generate a random heightmap.
        /// </summary>
        public void GenerateRandomHeightmap()
        {
            for (int x = 0; x < _width; ++x)
            {
                for (int z = 0; z < _depth; ++z)
                {
                    _heightValues[x + z * _width] = _random.GetFloatInRange(_minimumHeight, _maximumHeight);
                }
            }
        }

        /// <summary>
        /// Generate a random heightmap.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void GenerateRandomHeightmap(int width, int depth, float min, float max)
        {
            _width = width;
            _depth = depth;

            _minimumHeight = min;
            _maximumHeight = max;

            _heightValues = new float[_width*_depth];

            GenerateRandomHeightmap();
        }

        /// <summary>
        /// Generate a random heightmap using fault line deformation and the passed parameters.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="settings"></param>
        public void GenerateFaultHeightmap(int width, int depth, float min, float max, HeightmapFaultSettings settings)
        {
            _width = width;
            _depth = depth;

            _minimumHeight = min;
            _maximumHeight = max;

            _faultSettings = settings;

            _heightValues = new float[_width*_depth];

            GenerateFaultHeightmap();
        }

        /// <summary>
        /// Generate a random heightmap using fault line deformation and the _faultSettings.
        /// </summary>
        public void GenerateFaultHeightmap()
        {
            int x1, z1, dx1, dz1;
            int x2, z2, dx2, dz2;

            int deltaHeight;

            for (int i = 0; i < _faultSettings.Iterations; ++i)
            {
                // Calculate the deltaHeight for this iteration.
                // (linear interpolation from max delta to min delta).
                deltaHeight = _faultSettings.MaximumDelta -
                              ((_faultSettings.MaximumDelta - _faultSettings.MinimumDelta)*i)/_faultSettings.Iterations;

                // Pick two random points on the field for the line.
                // (make sure they aren't identical).
                x1 = _random.Random.Next(_width);
                z1 = _random.Random.Next(_depth);

                do
                {
                    x2 = _random.Random.Next(_width);
                    z2 = _random.Random.Next(_depth);
                } while (x1 == x2 && z1 == z2);

                // dx1, dz1 is a vector in the direction of the line.
                dx1 = x2 - x1;
                dz1 = z2 - z1;

                for (x2 = 0; x2 < _width; ++x2)
                {
                    for (z2 = 0; z2 < _depth; ++z2)
                    {
                        // dx2, dz2 is a vector from x1, z1 to the candidate point.
                        dx2 = x2 - x1;
                        dz2 = z2 - z1;

                        // if y component of the cross product is 'up', then elevate this point.
                        if (dx2*dz1 - dx1*dz2 > 0)
                        {
                            _heightValues[x2 + _width*z2] += deltaHeight;
                        }
                    }
                }

                // Erode the terrain.
                if ((_faultSettings.IterationsPerFilter != 0) && (i%_faultSettings.IterationsPerFilter) == 0)
                {
                    FilterHeightmap(_faultSettings.FilterValue);
                }
            }

            // Normalize heightmap (height field values in the range _minimumHeight - _maximumHeight.
            NormalizeHeightmap();
        }

        /// <summary>
        /// Generate a random heightmap using mid point displacement and passed parameters.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="settings"></param>
        public void GenerateMidPointHeightmap(int width, int depth, float min, float max,
                                              HeightmapMidPointSettings settings)
        {
            _width = width;
            _depth = depth;

            _minimumHeight = min;
            _maximumHeight = max;

            _midPointSettings = settings;

            _heightValues = new float[_width*_depth];

            GenerateMidPointHeightmap();
        }

        /// <summary>
        /// Generate a random heightmap using mid point displacement and default parameters.
        /// </summary>
        public void GenerateMidPointHeightmap()
        {
            int i, ni, mi, pmi;
            int j, nj, mj, pmj;
            int width = _width;
            int depth = _depth;

            float deltaHeight = width*0.5f;

            var r = (float) Math.Pow(2.0f, -1*_midPointSettings.Rough);

            // Since the terrain wraps, all four corners are represented by the value at 0, 0.
            // So seeding the heightfield is very straightforward.
            _heightValues[0] = 1337.0f;

            while (width > 0)
            {
                // Diamond step.
                //
                // We find the values at the center of the rectangles by averaging the values at the
                // corners and adding a random offset.
                //
                // a . . . b
                // .       .
                // .   e   .         e = ( a + b + c + d ) / 4 + random
                // .       .
                // c . . . d
                //
                // a = (i, j)
                // b = (ni, j)
                // c = (i, nj)
                // d = (ni, nj)
                // e = (mi, mj)

                for (i = 0; i < _width; i += width)
                {
                    for (j = 0; j < _depth; j += depth)
                    {
                        ni = (i + width)%_width;
                        nj = (j + depth)%_depth;

                        mi = (i + width/2);
                        mj = (j + depth/2);

                        float a = (_heightValues[i + j*_width] + _heightValues[ni + j*_width] +
                                   _heightValues[i + nj*_width] + _heightValues[ni + nj*_width]);

                        float b = 0.25f + _random.GetFloatInRange(-deltaHeight*0.5f, deltaHeight*0.5f);
                        int index = mi + _width*mj;
                        _heightValues[index] = a*b;
                    }
                }

                // Square step.
                //
                // We find the values on the left and top sides of each rectangle.
                // The right and bottom sides are the left and top sides of the neighboring rectangles.
                // So we don't need to calculate them.
                //
                // Since the heightmap wraps, we are never left hanging. The right side of the last rectangle
                // in a row is the left side of the first rectangle in the row. The bottom side of the last rectangle
                // in a column is the top side of the first rectangle in the column.
                //
                // a . . . b
                // . . . . .
                // . . . . .
                // . . . . .
                // c . . . d
                //
                // . . . . .
                // . . . . .
                // . . e . .
                // . . . . .
                // . . . . .
                //
                // a . f . b
                // . . . . .
                // g . e . h
                // . . . . .
                // c . i . d
                //
                // a . f . b
                // . j l k .                g = ( d + f + a + b ) / 4 + random
                // g . e . h                h = ( a + c + e + f ) / 4 + random
                // . . . . .
                // c . i . d
                //
                // a = (i, j)
                // b = (ni, j)
                // c = (i, nj)
                // d = (mi, pmj)
                // e = (pmi, mj)
                // f = (mi, mj)
                // g = (mi, j)
                // h = (i, mj)

                for (i = 0; i < _width; i += width)
                {
                    for (j = 0; j < _depth; j += depth)
                    {
                        ni = (i + width)%_width;
                        nj = (j + depth)%_depth;

                        mi = (i + width/2);
                        mj = (j + depth/2);

                        pmi = (i - width/2 + _width)%_width;
                        pmj = (j - depth/2 + _depth)%_depth;

                        // Calculate the square value for the top side of the rectangle.
                        _heightValues[mi + j*_width] = (_heightValues[i + j*_width] + _heightValues[ni + j*_width] +
                                                        _heightValues[mi + pmj*_width] + _heightValues[mi + mj*_width])*
                                                       0.25f +
                                                       _random.GetFloatInRange(-deltaHeight*0.5f, deltaHeight*0.5f);

                        // Calculate the square value for the left side of the rectangle.
                        _heightValues[i + mj*_width] = (_heightValues[i + j*_width] + _heightValues[i + nj*_width] +
                                                        _heightValues[pmi + mj*_width] + _heightValues[mi + mj*_width])*
                                                       0.25f +
                                                       _random.GetFloatInRange(-deltaHeight*0.5f, deltaHeight*0.5f);
                    }
                }

                // Set the values for the next iteration.
                width /= 2;
                depth /= 2;
                deltaHeight *= r;
            }

            // Normalize heightmap (height field values in the range _minimumHeight - _maximumHeight.
            NormalizeHeightmap();
        }

        /// <summary>
        /// Generate a random heightmap using particle deposition and the passed parameters.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="settings"></param>
        public void GenerateParticleDepositionHeightmap(int width, int depth, float min, float max,
                                                        HeightmapParticleDepositionSettings settings)
        {
            _width = width;
            _depth = depth;

            _minimumHeight = min;
            _maximumHeight = max;

            _particleDepositionSettings = settings;

            _heightValues = new float[_width*_depth];

            GenerateParticleDepositionHeightmap();
        }

        /// <summary>
        /// Generate a random heightmap using particle deposition and default parameters.
        /// </summary>
        public void GenerateParticleDepositionHeightmap()
        {
            int i, j, m, p, particleCount;
            int x, px, minx, maxx, sx, tx;
            int z, pz, minz, maxz, sz, tz;

            bool done;

            int[] dx = {0, 1, 0, _width - 1, 1, 1, _width - 1, _width - 1};
            int[] dz = {1, 0, _depth - 1, 0, _depth - 1, 1, _depth - 1, 1};

            float ch, ph;

            var calderaMap = new int[_width*_depth];

            // Clear the heightmap.
            for (i = 0; i < _width*_depth; ++i)
            {
                _heightValues[i] = 0.0f;
            }

            // For each jump ..
            for (p = 0; p < _particleDepositionSettings.Jumps; ++p)
            {
                // Pick a random spot.
                x = _random.Random.Next(_width);
                z = _random.Random.Next(_depth);

                // px and pz track where the caldera is formed.
                px = x;
                pz = z;

                // Determine how many particles we are going to drop.
                particleCount = _random.Random.Next(_particleDepositionSettings.MinParticlesPerJump,
                                                         _particleDepositionSettings.MaxParticlesPerJump);

                // Drop particles.
                for (i = 0; i < particleCount; ++i)
                {
                    // If we have to move the drop point, agitate it in a random direction.
                    if ((_particleDepositionSettings.PeakWalk != 0) && ((i%_particleDepositionSettings.PeakWalk) == 0))
                    {
                        m = _random.Random.Next(8);

                        x = (x + dx[m] + _width)%_width;
                        z = (z + dz[m] + _depth)%_depth;
                    }

                    // Drop it.
                    _heightValues[x + z*_width] += 1.0f;

                    // Now agitate it until it settles.
                    sx = x;
                    sz = z;

                    done = false;

                    // While it's not settled
                    while (!done)
                    {
                        // Consider it is.
                        done = true;

                        // Pick a random neighbor and start inspecting.
                        m = _random.Random.Next();

                        for (j = 0; j < 8; ++j)
                        {
                            tx = (sx + dx[(j + m)%8])%_width;
                            tz = (sz + dz[(j + m)%8])%_depth;

                            // If we can move to this neighbor, do it.
                            if (_heightValues[tx + tz*_width] + 1.0f < _heightValues[sx + sz*_width])
                            {
                                _heightValues[tx + tz*_width] += 1.0f;
                                _heightValues[sx + sz*_width] -= 1.0f;

                                sx = tx;
                                sz = tz;

                                done = false;

                                break;
                            }
                        }
                    }

                    // Check to see if the latest point is higher than the caldera point.
                    // If so, move the caldera point here.
                    if (_heightValues[sx + sz*_width] > _heightValues[px + pz*_width])
                    {
                        px = sx;
                        pz = sz;
                    }
                }

                // Now that we are done with the peak, invert the caldera.
                //
                // ch is the caldera cutoff altitude.
                // ph is the height at the caldera start point.
                ph = _heightValues[px + pz*_width];
                ch = ph*(1.0f - _particleDepositionSettings.Caldera);

                // We do a floodfill, so we use an array of integers to mark the visited locations.
                minx = px;
                maxx = px;
                minz = pz;
                maxz = pz;

                // Mark the start location for the caldera.
                calderaMap[px + pz*_width] = 1;

                done = false;

                while (!done)
                {
                    // Assume work is done.
                    done = true;

                    sx = minx;
                    sz = minz;
                    tx = maxx;
                    tz = maxz;

                    // Examine the bounding rectangle looking for unvisited neighbors.
                    for (x = sx; x <= tx; ++x)
                    {
                        for (z = sz; z <= tz; ++z)
                        {
                            px = (x + _width)%_width;
                            pz = (z + _depth)%_depth;

                            // If this cell is marked but unvisited, check it out.
                            if (calderaMap[px + pz*_width] == 1)
                            {
                                // Mark cell as visited.
                                calderaMap[px + pz*_width] = 2;

                                // If this cell should be inverted, invert it and inspect neighbors.
                                // We mark any unmarked and unvisited neighbor.
                                // We don't invert any cells whose height exceeds the initial caldera height.
                                // This prevents small peaks from destroying large ones.
                                if ((_heightValues[px + pz*_width] > ch) && (_heightValues[px + pz*_width] <= ph))
                                {
                                    done = false;

                                    _heightValues[px + pz*_width] = 2*ch - _heightValues[px + pz*_width];

                                    // Left and right neighbors.
                                    px = (px + 1)%_width;

                                    if (calderaMap[px + pz*_width] == 0)
                                    {
                                        if (x + 1 > maxx)
                                        {
                                            maxx = x + 1;
                                        }

                                        calderaMap[px + pz*_width] = 1;
                                    }

                                    px = (px + _width - 2)%_width;

                                    if (calderaMap[px + pz*_width] == 0)
                                    {
                                        if (x - 1 < minx)
                                        {
                                            minx = x - 1;
                                        }

                                        calderaMap[px + pz*_width] = 1;
                                    }

                                    // Top and bottom neighbors.
                                    px = (x + _width)%_width;
                                    pz = (pz + 1)%_depth;

                                    if (calderaMap[px + pz*_width] == 0)
                                    {
                                        if (z + 1 > maxz)
                                        {
                                            maxz = z + 1;
                                        }

                                        calderaMap[px + pz*_width] = 1;
                                    }

                                    pz = (pz + _depth - 2)%_depth;

                                    if (calderaMap[px + pz*_width] == 0)
                                    {
                                        if (z - 1 < minz)
                                        {
                                            minz = z - 1;
                                        }

                                        calderaMap[px + pz*_width] = 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Since calderas increase aliasing, we erode the terrain with a filter value proportional
            // to the prominence of the caldera.
            FilterHeightmap(_particleDepositionSettings.Caldera);

            // Normalize the heightmap.
            NormalizeHeightmap();
        }

        /// <summary>
        /// Generate a random heightmap using perlin noise and the passed parameters.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="settings"></param>
        public void GeneratePerlinNoiseHeightmap(int width, int depth, float min, float max,
                                                 HeightmapPerlinNoiseSettings settings)
        {
            _width = width;
            _depth = depth;

            _minimumHeight = min;
            _maximumHeight = max;

            _perlinNoiseSettings = settings;

            _heightValues = new float[_width*_depth];

            GeneratePerlinNoiseHeightmap();
        }

        /// <summary>
        /// Generate a random heightmap using perlin noise and the default parameters.
        /// </summary>
        public void GeneratePerlinNoiseHeightmap()
        {
            int txi, tzi;

            float freq, amp;

            float xf, tx, fracx;
            float zf, tz, fracz;

            float v1, v2, v3, v4;
            float i1, i2, total;

            // For each height..
            for (int z = 0; z < _depth; ++z)
            {
                for (int x = 0; x < _width; ++x)
                {
                    // Scale x and y to the range of 0.0f, 1.0f.
                    xf = x/(float) _width*_perlinNoiseSettings.NoiseSize;
                    zf = z/(float) _depth*_perlinNoiseSettings.NoiseSize;

                    total = 0.0f;

                    // For each octaves..
                    for (int i = 0; i < _perlinNoiseSettings.Octaves; ++i)
                    {
                        // Calculate frequency and amplitude (different for each octave).
                        freq = (float) Math.Pow(2.0, i);
                        amp = (float) Math.Pow(_perlinNoiseSettings.Persistence, i);

                        // Calculate the x, z noise coodinates.
                        tx = xf*freq;
                        tz = zf*freq;

                        txi = (int) tx;
                        tzi = (int) tz;

                        // Calculate the fractions of x and z.
                        fracx = tx - txi;
                        fracz = tz - tzi;

                        // Get noise per octave for these four points.
                        v1 = RandomHelper.Noise(txi + tzi*57 + _perlinNoiseSettings.Seed);
                        v2 = RandomHelper.Noise(txi + 1 + tzi*57 + _perlinNoiseSettings.Seed);
                        v3 = RandomHelper.Noise(txi + (tzi + 1)*57 + _perlinNoiseSettings.Seed);
                        v4 = RandomHelper.Noise(txi + 1 + (tzi + 1)*57 + _perlinNoiseSettings.Seed);

                        // Smooth noise in the x axis.
                        i1 = MathHelper.SmoothStep(v1, v2, fracx);
                        i2 = MathHelper.SmoothStep(v3, v4, fracx);

                        // Smooth in the z axis.
                        total += MathHelper.SmoothStep(i1, i2, fracz)*amp;
                    }

                    // Save to heightmap.
                    _heightValues[x + z*_width] = total;
                }
            }

            // Normalize the terrain.
            NormalizeHeightmap();
        }

        /// <summary>
        /// Normalize the heightmap.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void NormalizeHeightmap(float min, float max)
        {
            _minimumHeight = min;
            _maximumHeight = max;

            NormalizeHeightmap();
        }

        /// <summary>
        /// Normalize the heightmap.
        /// </summary>
        public void NormalizeHeightmap()
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            // Get the lowest and the highest values.
            for (int x = 0; x < _width; ++x)
            {
                for (int z = 0; z < _depth; ++z)
                {
                    if (_heightValues[x + z*_width] > max)
                    {
                        max = _heightValues[x + z*_width];
                    }

                    if (_heightValues[x + z*_width] < min)
                    {
                        min = _heightValues[x + z*_width];
                    }
                }
            }

            // If the heightmap is flat, we set it to the average between _minimumHeight and _maximumHeight.
            if (max <= min)
            {
                for (int x = 0; x < _width; ++x)
                {
                    for (int z = 0; z < _depth; ++z)
                    {
                        _heightValues[x + z*_width] = (_maximumHeight - _minimumHeight)*0.5f;
                    }
                }

                return;
            }

            // Normalize the value between 0.0 and 1.0 then scale it between _minimumHeight and _maximumHeight.
            float diff = max - min;
            float scale = _maximumHeight - _minimumHeight;

            for (int x = 0; x < _width; ++x)
            {
                for (int z = 0; z < _depth; ++z)
                {
                    _heightValues[x + z*_width] = (_heightValues[x + z*_width] - min)/diff*scale + _minimumHeight;
                }
            }
        }

        /// <summary>
        /// Filter the heightmap using a low pass filter in all four directions..
        /// </summary>
        /// <param name="filterValue"></param>
        public void FilterHeightmap(float filterValue)
        {
            // Erode rows left to right.
            for (int j = 0; j < _depth; ++j)
            {
                for (int i = 1; i < _width; ++i)
                {
                    _heightValues[i + j*_width] = filterValue*_heightValues[i - 1 + j*_width] +
                                                  (1 - filterValue)*_heightValues[i + j*_width];
                }
            }

            // Erode rows right to left.
            for (int j = 0; j < _depth; ++j)
            {
                for (int i = 0; i < _width - 1; ++i)
                {
                    _heightValues[i + j*_width] = filterValue*_heightValues[i + 1 + j*_width] +
                                                  (1 - filterValue)*_heightValues[i + j*_width];
                }
            }

            // Erode columns top to bottom.
            for (int j = 1; j < _depth; ++j)
            {
                for (int i = 0; i < _width; ++i)
                {
                    _heightValues[i + j*_width] = filterValue*_heightValues[i + (j - 1)*_width] +
                                                  (1 - filterValue)*_heightValues[i + j*_width];
                }
            }

            // Erode columns bottom to top.
            for (int j = 0; j < _depth - 1; ++j)
            {
                for (int i = 0; i < _width; ++i)
                {
                    _heightValues[i + j*_width] = filterValue*_heightValues[i + (j + 1)*_width] +
                                                  (1 - filterValue)*_heightValues[i + j*_width];
                }
            }
        }

        /// <summary>
        /// Combine the heightmap with the passed heightmap.
        /// The amount variable is the ratio of the heightmap passed as a parameter.
        /// </summary>
        /// <param name="heightmap"></param>
        /// <param name="amount"></param>
        public void CombineHeightmap(Heightmap heightmap, float amount)
        {
            // Reject the heightmap if it doesn't fit.
            if ((_width != heightmap.Width) ||
                (_depth != heightmap.Depth) ||
                (_minimumHeight != heightmap.MinimumHeight) ||
                (_maximumHeight != heightmap.MaximumHeight))
            {
                return;
            }

            // Combine the heightmaps.
            // H1 = H1 * (1.0f - amount) + H2 * amount
            for (int x = 0; x < _width; ++x)
            {
                for (int z = 0; z < _depth; ++z)
                {
                    _heightValues[x + z*_width] = _heightValues[x + z*_width]*(1.0f - amount) +
                                                  heightmap.GetHeightValue(x, z)*amount;
                }
            }
        }

        /// <summary>
        /// Multiply two heightmaps together.
        /// </summary>
        /// <param name="heightmap"></param>
        public void MultiplyHeightmap(Heightmap heightmap)
        {
            // Reject the heightmap if it doesn't fit.
            if ((_width != heightmap.Width) ||
                (_depth != heightmap.Depth) ||
                (_minimumHeight != heightmap.MinimumHeight) ||
                (_maximumHeight != heightmap.MaximumHeight))
            {
                return;
            }

            // Multiply the heightmaps together.
            for (int x = 0; x < _width; ++x)
            {
                for (int z = 0; z < _depth; ++z)
                {
                    _heightValues[x + z*_width] = _heightValues[x + z*_width]*heightmap.GetHeightValue(x, z);
                }
            }
        }

        /// <summary>
        /// Add two heightmaps together.
        /// </summary>
        /// <param name="heightmap"></param>
        public void AddHeightmap(Heightmap heightmap)
        {
            // Reject the heightmap if it doesn't fit.
            if ((_width != heightmap.Width) ||
                (_depth != heightmap.Depth) ||
                (_minimumHeight != heightmap.MinimumHeight) ||
                (_maximumHeight != heightmap.MaximumHeight))
            {
                return;
            }

            // Add the heightmaps together.
            for (int x = 0; x < _width; ++x)
            {
                for (int z = 0; z < _depth; ++z)
                {
                    _heightValues[x + z*_width] = _heightValues[x + z*_width] + heightmap.GetHeightValue(x, z);
                }
            }
        }

        /// <summary>
        /// Apply a mask on the heightmap.
        /// </summary>
        /// <param name="heightmask"></param>
        public void ApplyMask(Heightmask heightmask)
        {
            // Reject the heightmask if it doesn't fit.
            if ((_width != heightmask.Width) ||
                (_depth != heightmask.Depth))
            {
                return;
            }

            // Apply the heightmask.
            for (int x = 0; x < _width; ++x)
            {
                for (int z = 0; z < _depth; ++z)
                {
                    _heightValues[x + z*_width] = (_heightValues[x + z*_width] - _minimumHeight)*
                                                  heightmask.GetMaskValue(x, z) + _minimumHeight;
                }
            }
        }

        /// <summary>
        /// Save a heightmap to a file.
        /// </summary>
        /// <param name="filename"></param>
        public void SaveToFile(String filename)
        {
            try
            {
                FileStream stream = File.Open(filename, FileMode.OpenOrCreate);

                if (stream != null)
                {
                    var writer = new BinaryWriter(stream);

                    writer.Write(_width);
                    writer.Write(_depth);

                    writer.Write(_minimumHeight);
                    writer.Write(_maximumHeight);

                    for (int i = 0; i < _heightValues.Length; ++i)
                    {
                        writer.Write(_heightValues[i]);
                    }

                    writer.Flush();

                    stream.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Load a heightmap from a file.
        /// </summary>
        /// <param name="filename"></param>
        public void LoadFromFile(String filename)
        {
            try
            {
                FileStream stream = File.Open(filename, FileMode.Open);

                var reader = new BinaryReader(stream);

                _width = reader.ReadInt32();
                _depth = reader.ReadInt32();

                _minimumHeight = reader.ReadSingle();
                _maximumHeight = reader.ReadSingle();

                _heightValues = new float[_width*_depth];

                for (int i = 0; i < _heightValues.Length; ++i)
                {
                    _heightValues[i] = reader.ReadSingle();
                }

                reader.Close();

                stream.Close();
            }
            catch (Exception)
            {
            }
        }

        public void MergeLeft(Heightmap leftTile, int transitionWidth)
        {
            if (leftTile == null) throw new ArgumentNullException("leftTile");
            if (leftTile.Width != Width || leftTile.Depth != Depth)
                throw new ArgumentException("Tiles must have equal widths and depths", "leftTile");

            if (transitionWidth >= Width)
                throw new ArgumentException("Transition width must be less than tile width", "transitionWidth");

            for (int x = 0; x < transitionWidth; x++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    float leftEdgeHeight = leftTile.GetHeightValue(leftTile.Width - 1, z);
                    float originalHeight = GetHeightValue(x, z);
                    float newHeight = MathHelper.Lerp(leftEdgeHeight, originalHeight, (float) x/(transitionWidth - 1));
                    SetHeightValue(x, z, newHeight);
                }
            }
        }
    }
}

/*======================================================================================================================

									NIN - Nerdy Inverse Network - http://nerdy-inverse.com

======================================================================================================================*/