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
using Microsoft.Xna.Framework;

#endregion

namespace Simulator.Utils
{
    /// <summary>
    /// A helper class that provides a frames-per-second property 
    /// that is updated each game loop cycle.
    /// </summary>
    public class FPS : GameComponent
    {
        public FPS(Game game) : base(game)
        {
        }

        public int Value { get; private set; }

        public override void Update(GameTime gameTime)
        {
            double elapsedSeconds = gameTime.ElapsedRealTime.TotalSeconds;
            Value = (elapsedSeconds > 0)
                        ? Convert.ToInt32(1.0/elapsedSeconds)
                        : 0;

            base.Update(gameTime);
        }
    }
}