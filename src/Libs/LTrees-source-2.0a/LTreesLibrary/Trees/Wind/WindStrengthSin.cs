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

namespace LTreesLibrary.Trees.Wind
{
    public class WindStrengthSin : WindSource
    {
        private int time;

        #region WindSource Members

        public Vector3 GetWindStrength(Vector3 position)
        {
            float seconds = time/1000.0f;
            return 10.0f*Vector3.Right*(float) Math.Sin(seconds*3)
                   + 15.0f*Vector3.Backward*(float) Math.Sin(seconds*5 + 1)
                   + 1.5f*Vector3.Backward*(float) Math.Sin(seconds*11 + 3)
                   + 1.5f*Vector3.Right*(float) Math.Sin(seconds*11 + 3)*(float) Math.Sin(seconds*1 + 3);
        }

        #endregion

        public void Update(GameTime t)
        {
            time += t.ElapsedGameTime.Milliseconds;
        }
    }
}