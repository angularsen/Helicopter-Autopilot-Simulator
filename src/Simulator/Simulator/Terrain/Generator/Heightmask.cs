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

#endregion

namespace NINFocusOnTerrain
{
    /// <summary>
    /// This is the heightmask class.
    /// </summary>
    public class Heightmask : ICloneable
    {
        /// <summary>
        /// Depth of the heightmask.
        /// </summary>
        private int _depth;

        /// <summary>
        /// Mask values of the heightmask.
        /// </summary>
        private float[] _maskValues;

        /// <summary>
        /// Width of the heightmask.
        /// </summary>
        private int _width;

        /// <summary>
        /// Default constructor for a flat heightmask.
        /// </summary>
        public Heightmask()
        {
            _width = 0;
            _depth = 0;

            _maskValues = null;
        }

        /// <summary>
        /// Default constructor for a prebuilt heightmask.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        /// <param name="data"></param>
        public Heightmask(int width, int depth, float[] data)
        {
            _width = width;
            _depth = depth;

            _maskValues = (float[]) data.Clone();
        }

        /// <summary>
        /// Get the width of the heightmask.
        /// </summary>
        public int Width
        {
            get { return _width; }
        }

        /// <summary>
        /// Get the depth of the heightmask.
        /// </summary>
        public int Depth
        {
            get { return _depth; }
        }

        /// <summary>
        /// Mask values of the heightmask.
        /// </summary>
        public float[] MaskValues
        {
            get { return _maskValues; }
        }

        #region ICloneable Members

        /// <summary>
        /// Clone the heightmask.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var heightmask = new Heightmask(_width, _depth, (float[]) _maskValues.Clone());

            return heightmask;
        }

        #endregion

        /// <summary>
        /// Return the mask at the given x,z coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public float GetMaskValue(int x, int z)
        {
            return _maskValues[x + z*_width];
        }

        /// <summary>
        /// Set the mask value at the given x,z coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="value"></param>
        public void SetMaskValue(int x, int z, float value)
        {
            _maskValues[x + z*_width] = value;
        }

        /// <summary>
        /// Save a heightmask to a file.
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

                    for (int i = 0; i < _maskValues.Length; ++i)
                    {
                        writer.Write(_maskValues[i]);
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
        /// Load a heightmask from a file.
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

                _maskValues = new float[_width*_depth];

                for (int i = 0; i < _maskValues.Length; ++i)
                {
                    _maskValues[i] = reader.ReadSingle();
                }

                reader.Close();

                stream.Close();
            }
            catch (Exception)
            {
            }
        }
    }
}

/*======================================================================================================================

									NIN - Nerdy Inverse Network - http://nerdy-inverse.com

======================================================================================================================*/