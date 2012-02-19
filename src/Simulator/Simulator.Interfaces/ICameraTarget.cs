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

namespace Simulator.Interfaces
{
    public interface ICameraTarget
    {
        Matrix Rotation { get; }
        Vector3 Position { get; }
        Vector3 CameraUp { get; }
        Vector3 CameraForward { get; }
    }
}