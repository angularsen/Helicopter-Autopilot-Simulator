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

#endregion

namespace Control.Common
{
    [Flags]
    public enum Actions
    {
        None = 0,
        Hover = 1,
        AlignNose = 2,
        Accelerate = 4,
        Decelerate = 8,
    }
}