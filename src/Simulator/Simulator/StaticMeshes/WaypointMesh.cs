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

namespace Simulator.StaticMeshes
{
    public class WaypointMesh : SimpleModel
    {
        public WaypointMesh(Game game, ICameraProvider cameraProvider) 
            : base("Models/waypoint", game, cameraProvider)
        {
            IsTwoSided = true;
            IsTransparent = true;
            IsSelfLit = true;
            Scale = Matrix.CreateScale(0.3f);
        }

        public override void Draw(GameTime gameTime)
        {
            // Render waypoint as three perpendicular billboards
            Rotation = Matrix.Identity;
            base.Draw(gameTime);

            Rotation = Matrix.CreateRotationX(MathHelper.ToRadians(90));
            base.Draw(gameTime);

            Rotation = Matrix.CreateRotationZ(MathHelper.ToRadians(90));
            base.Draw(gameTime);
        }
    }
}