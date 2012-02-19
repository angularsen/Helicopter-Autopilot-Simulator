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

using Control;
using Microsoft.Xna.Framework;

#endregion

namespace Simulator.Scenarios
{
    public struct HelicopterScenario
    {
        public bool EngineSound;
        public bool PlayerControlled;
        public Vector3 StartPosition;
        public Task Task;
        public bool AssistedAutopilot;

//        public HelicopterScenario(bool playerControlled, Vector3 startPos, Task task)
//        {
//            PlayerControlled = playerControlled;
//            StartPosition = startPos;
//            Task = task;
//        }
    }
}