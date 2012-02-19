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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Sensors.Model;
using Simulator.Scenarios;
using State.Model;

#endregion

namespace Simulator.Testing
{
    public class ScenarioIntermediaryTestResult
    {
        public float AccEstimatedPositionError;
        public float AccHeightAboveGround;
        public float AccVelocity;
        public float MaxEstimatedPositionError;
        public float MaxHeightAboveGround;
        public float MaxVelocity;
        public float MinEstimatedPositionError;
        public float MinHeightAboveGround;
        public TimeSpan StartTime;
        public int UpdateCount;

        public ScenarioIntermediaryTestResult()
        {
            MinEstimatedPositionError = float.MaxValue;
            MinHeightAboveGround = float.MaxValue;
        }
    }

    public enum TestScenarioEndTrigger
    {
        Crashed,
        TimedOut,
        UserSkipped,
        ReachedDestination
    }

    public struct ScenarioTestResult
    {
        public AutopilotConfiguration Autopilot;
        public float AvgEstimatedPositionError;
        public float AvgHeightAboveGround;
        public float AvgVelocity;
        public TimeSpan Duration;
        public IList<HelicopterLogSnapshot> FlightLog;
        public float MaxEstimatedPositionError;
        public float MaxHeightAboveGround;
        public float MaxVelocity;
        public float MinEstimatedPositionError;
        public float MinHeightAboveGround;
        public Scenario Scenario;
        public SensorSpecifications Sensors;
        public TestScenarioEndTrigger EndTrigger;

        public ScenarioTestResult(ScenarioIntermediaryTestResult i, Scenario scenario, HelicopterBase helicopter,
                                  GameTime gameTime, TestScenarioEndTrigger endTrigger)
        {
            Autopilot = new AutopilotConfiguration
                            {
                                MaxHVelocity = helicopter.Autopilot.MaxHVelocity,
                                PIDSetup = helicopter.Autopilot.PIDSetup.Clone()
                            };

            AvgEstimatedPositionError = i.AccEstimatedPositionError/i.UpdateCount;
            AvgHeightAboveGround = i.AccHeightAboveGround/i.UpdateCount;
            AvgVelocity = i.AccVelocity/i.UpdateCount;
            Duration = gameTime.TotalGameTime - i.StartTime;
            EndTrigger = endTrigger;
            FlightLog = new List<HelicopterLogSnapshot>(helicopter.Log);
            MaxEstimatedPositionError = i.MaxEstimatedPositionError;
            MaxHeightAboveGround = i.MaxHeightAboveGround;
            MaxVelocity = i.MaxVelocity;
            MinEstimatedPositionError = i.MinEstimatedPositionError;
            MinHeightAboveGround = i.MinHeightAboveGround;
            Scenario = scenario;

            Sensors = new SensorSpecifications
                          {
                              AccelerometerStdDev = DefaultAutopilotConfiguration.AccelerometerStdDev,
                              GPSVelocityStdDev = DefaultAutopilotConfiguration.GPSVelocityStdDev,
                              GPSPositionStdDev = DefaultAutopilotConfiguration.GPSPositionStdDev
                          };
        }
    }
}