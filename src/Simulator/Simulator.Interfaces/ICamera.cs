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
    public interface ICamera //: IUpdateable
    {
        Matrix View { get; }
        Matrix Projection { get; }
        BoundingFrustum Frustum { get; }

        ICameraTarget LookAtTarget { get; set; }
        Vector3 Position { get; set; }

        float AspectRatio { get; set; }
        float FieldOfView { get; set; }
        float NearPlane { get; set; }
        float FarPlane { get; set; }
        Vector3 LookAt { get; set; }
        Vector3 Up { get; }

        void DrawHUD(GameTime time);
        void Update(GameTime time);
        void UpdateMatrices();
        void Reset();
        ICamera Clone();
    }
}