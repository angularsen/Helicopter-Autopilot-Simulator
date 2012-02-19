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
using Microsoft.Xna.Framework.Graphics;
using Simulator.Interfaces;

#endregion

namespace Simulator.StaticMeshes
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class FlatTexturedGround : SimpleModel
    {

        public FlatTexturedGround(Game game, ICameraProvider cameraProvider)
            : base("Models/FlatTexturedGround", game, cameraProvider)
        {
            
        }

        public override void Draw(GameTime gameTime)
        {
            var oldU = Game.GraphicsDevice.SamplerStates[0].AddressU;
            var oldV = Game.GraphicsDevice.SamplerStates[0].AddressV;

            Game.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Game.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            base.Draw(gameTime);

            Game.GraphicsDevice.SamplerStates[0].AddressU = oldU;
            Game.GraphicsDevice.SamplerStates[0].AddressV = oldV;
        }
    }
}