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
using Simulator.Interfaces;

#endregion

namespace Simulator.Common
{
    public class WorldDummy : ICameraTarget
    {
        public WorldDummy(Vector3 pos)
        {
            Position = pos;
            Rotation = Matrix.Identity;
        }

        #region Implementation of IWorldObject

        public Matrix Rotation { get; set; }
        public Vector3 Position { get; set; }

        public Vector3 CameraUp
        {
            get { return Vector3.Up; }
        }

        public Vector3 CameraForward
        {
            get { return Vector3.Forward; }
        }

        #endregion
    }
}