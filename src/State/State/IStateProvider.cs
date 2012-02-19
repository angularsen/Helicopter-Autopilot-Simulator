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

using Control.Common;
using Physics.Common;
using State.Model;

#endregion

namespace State
{
    public interface IStateProvider
    {
        ///// <summary>
        /////   Returns true only if all sensors are initialized and the state is stable/reliable.
        ///// </summary>
        //bool Ready { get; }

        // TODO Move arguments to constructor, this interface is not common for all implementations
        void GetState(out PhysicalHeliState estimated, out PhysicalHeliState observed, out PhysicalHeliState blindEstimatedState);

        void Update(SimulationStepResults trueState, long elapsedMillis, long totalElapsedMillis,
                    JoystickOutput currentOutput);
    }
}