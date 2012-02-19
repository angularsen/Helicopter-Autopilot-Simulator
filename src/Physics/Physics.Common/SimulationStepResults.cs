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
using State.Model;

#endregion

namespace Physics.Common
{
    public struct SimulationStepResults
    {
        ///// <summary>
        ///// The number of substeps are always greater than or equal to 2 to hold at least the start and end state of step.
        ///// Note that the last substep of the previous step and the first substep of the current step are equal.
        ///// </summary>
//        public readonly List<SimulationStepResults> Substeps;

        public readonly TimeSpan Duration;
        public readonly TimeSpan EndTime;
        public readonly TimeSpan StartTime;

//        public TimeSpan SubstepDuration;
        public TimestepResult Result;
        public TimestepStartingCondition StartingCondition;

        public SimulationStepResults(TimestepStartingCondition startingCondition, TimestepResult result,
                                     TimeSpan startTime, TimeSpan endTime)
        {
            Duration = endTime - startTime;
            StartTime = startTime;
            EndTime = endTime;
//            Substeps = new List<SimulationStepResults>();

            StartingCondition = startingCondition;
            Result = result;
        }

        public SimulationStepResults(PhysicalHeliState startState, TimeSpan startTime)
        {
            StartingCondition = new TimestepStartingCondition(startState.Position, startState.Velocity,
                                                              startState.Acceleration, startState.Orientation, startTime);
            Result = new TimestepResult(startState.Position, startState.Velocity, startState.Orientation, startTime);
            StartTime = startTime;
            EndTime = startTime;
            Duration = TimeSpan.Zero;
        }

//        public SimulationStepResults(TimeSpan stepDuration, TimeSpan substepDuration, IList<SimulationStepResults> substepResults)
//        {
//            if (substepResults == null || substepResults.Count < 2)
//                throw new ArgumentException("Can't be null or hold less than 2 elements!", "substepResults");
//
//            Duration = stepDuration;
//            SubstepDuration = substepDuration;
//            Substeps = new List<SimulationStepResults>(substepResults);
//        }

//        public SimulationStepResults(PhysicalHeliState startPhysicalState, TimeSpan startTime) 
//        {
//            Duration = TimeSpan.Zero;
//            SubstepDuration = TimeSpan.Zero;
//
//            var substep = new SubstepResults(startPhysicalState.Position, startPhysicalState.Velocity, startPhysicalState.Acceleration,
//                                             startPhysicalState.Orientation, startTime);
//
        // Add initial state as both start and end state of timestep
//            Substeps = new List<SimulationStepResults> { /*substep, */substep };
//        }
    }

    public struct TimestepResult
    {
        public TimeSpan EndTime;
        public Quaternion Orientation;
        public Vector3 Position;
        public Vector3 Velocity;

        public TimestepResult(Vector3 position, Vector3 velocity, Quaternion orientation, TimeSpan endTime)
        {
            Position = position;
            Velocity = velocity;
            Orientation = orientation;
            EndTime = endTime;
        }
    }

    public struct TimestepStartingCondition
    {
        public Vector3 Acceleration;
        public Quaternion Orientation;
        public Vector3 Position;
        public TimeSpan StartTime;
        public Vector3 Velocity;

        public TimestepStartingCondition(TimestepResult result, Vector3 acceleration)
            : this(result.Position, result.Velocity, acceleration, result.Orientation, result.EndTime)
        {
        }

        public TimestepStartingCondition(Vector3 position, Vector3 velocity, Vector3 acceleration,
                                         Quaternion orientation, TimeSpan startTime)
        {
            Acceleration = acceleration;
            Position = position;
            Velocity = velocity;
            Orientation = orientation;
            StartTime = startTime;
        }
    }

//    public struct SubstepResults
//    {
//        public Vector3 Acceleration;
//        public Matrix Orientation;
//        public Vector3 Position;
//        public Vector3 Velocity;
//        public TimeSpan EndTime;
//
//        public SubstepResults(Vector3 position, Vector3 velocity, Vector3 acceleration, Matrix orientation, TimeSpan endTime)
//        {
//            Acceleration = acceleration;
//            Position = position;
//            Velocity = velocity;
//            Orientation = orientation;
//            EndTime = endTime;
//        }
//    }
}