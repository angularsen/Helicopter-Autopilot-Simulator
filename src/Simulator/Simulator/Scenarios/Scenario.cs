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
using Simulator.Cameras;
using Simulator.Parsers;

#endregion

namespace Simulator.Scenarios
{
    public class Scenario
    {
        /// <summary>
        /// Resource path to sound effect file.
        /// </summary>
        public string BackgroundMusic;

        public CameraType CameraType;

        public IList<HelicopterScenario> HelicopterScenarios;
        public string Name;

        public IEnumerable<string> SceneElements;
        public TerrainInfo TerrainInfo;
        public TimeSpan Timeout;


        public override string ToString()
        {
            return Name;
        }
    }
}