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
using Control.Common;
using Physics.Common;
using State.Model;

#endregion

namespace Physics
{
    public interface IHeliPhysics
    {
        SimulationStepResults PerformTimestep(PhysicalHeliState prev, JoystickOutput output, TimeSpan stepDuration,
                                              TimeSpan stepEndTime);
    }
}