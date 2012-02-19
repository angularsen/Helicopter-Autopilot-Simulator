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
    public class PerfectState : IStateProvider
    {
        private SimulationStepResults _currentState;

        /// <summary>
        ///   The perfect state provider is always ready.
        /// </summary>
        public bool Ready
        {
            get { return true; }
        }

        // TODO Move arguments to constructor, this interface is not common for all implementations

        #region IStateProvider Members

        public void GetState(out PhysicalHeliState estimated, out PhysicalHeliState observed, out PhysicalHeliState blindEstimatedState)
        {
            estimated = new PhysicalHeliState(_currentState.Result.Orientation, _currentState.Result.Position,
                                              _currentState.Result.Velocity,
                                              _currentState.StartingCondition.Acceleration);
            observed = new PhysicalHeliState();
            blindEstimatedState = new PhysicalHeliState();
        }

        public void Update(SimulationStepResults trueState, long elapsedMillis, long totalElapsedMillis,
                           JoystickOutput currentOutput)
        {
            _currentState = trueState;
        }

        #endregion
    }
}