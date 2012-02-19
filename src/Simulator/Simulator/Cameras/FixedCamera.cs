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

namespace Simulator.Cameras
{
    /// <summary>
    /// This camera will have a fixed position and always look at a fixed object.
    /// </summary>
    public class FixedCamera : CameraBase
    {
        public FixedCamera()
        {
        }

        public FixedCamera(Vector3 pos, ICameraTarget lookAtObject, float aspectRatio, float fov, float near, float far)
        {
            LookAtTarget = lookAtObject;
            AspectRatio = aspectRatio;
            FieldOfView = fov;
            NearPlane = near;
            FarPlane = far;
            Position = pos;
        }

        public override void Reset()
        {
            // No need to reset anything
        }

        public override ICamera Clone()
        {
            var r = new FixedCamera
                        {
                            AspectRatio = AspectRatio,
                            FarPlane = FarPlane,
                            FieldOfView = FieldOfView,
                            Frustum = Frustum,
                            LookAt = LookAt,
                            NearPlane = NearPlane,
                            Position = Position,
                            Projection = Projection,
                            LookAtTarget = LookAtTarget,
                            View = View,
                            WoldViewProj = WoldViewProj,
                            World = World
                        };

            return r;
        }


        public override void Update(GameTime time)
        {
            LookAt = LookAtTarget.Position;
            base.Update(time);
        }
    }
}