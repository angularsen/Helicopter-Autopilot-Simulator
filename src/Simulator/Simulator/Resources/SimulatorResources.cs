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
using System.IO;
using System.Linq;
using System.Xml;

#if !XBOX
using Anj.XNA.Joysticks;
#endif

using Control;
using Simulator.Parsers;
using Simulator.Scenarios;
using Simulator.Testing;
using Simulator.Utils;

#endregion

namespace Simulator.Resources
{
    public static class SimulatorResources
    {
#if !XBOX
        private static IList<JoystickSetup> _joystickSetups;
#endif

        private static IList<Scenario> _scenarios;
        private static IList<PIDSetup> _pidSetups;
        private static TestConfiguration _testConfiguration;

        /// <summary>
        /// Returns the first scenario matching this name, if any.
        /// Returns null if no scenario is found.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Scenario GetScenario(string name)
        {
            return GetScenarios().FirstOrDefault(scenario => scenario.Name == name);
        }

        public static Scenario GetPreSelectedScenario()
        {
            return GetScenario(ScenarioParserXML.GetPreSelectedScenarioName("Resources/Scenarios.xml"));
        }

        public static IList<Scenario> GetScenarios()
        {
            return _scenarios ?? 
                (_scenarios = ScenarioParserXML.GetScenarios("Resources/Scenarios.xml"));
        }

        public static IList<PIDSetup> GetPIDSetups()
        {
            return _pidSetups ?? 
                (_pidSetups = PIDParserXML.GetPIDSetups("Resources/PIDSetups.xml"));
        }

#if !XBOX
        public static IList<JoystickSetup> GetJoystickSetups()
        {
            return _joystickSetups ??
                   (_joystickSetups = JoystickSetupParserXML.GetJoystickSetups("Resources/JoystickSetups.xml"));
        }
#endif


        public static SimulationSettings GetSimulationSettings()
        {
            var result = new SimulationSettings();

            string xmlText = ParseHelper.GetResourceText(@"Resources/Scenarios.xml");
            var reader = new StringReader(xmlText);

            var doc = new XmlDocument();
            doc.Load(reader);
            XmlNode nav = doc.SelectSingleNode("/root");

            var renderModeNode = nav.SelectSingleNode("RenderMode");
            result.RenderMode = renderModeNode != null
                                    ? (RenderModes)Enum.Parse(typeof(RenderModes), renderModeNode.InnerText, true)
                                    : RenderModes.Normal;

            var swapStereoNode = nav.SelectSingleNode("SwapStereo");

            result.SwapStereo = swapStereoNode != null
                                    ? bool.Parse(swapStereoNode.InnerText)
                                    : false;

            reader.Close();

            return result;
        }

        public static TestConfiguration GetTestConfiguration()
        {
            return GetTestConfiguration("Resources/TestConfiguration.xml");
        }

        public static TestConfiguration GetTestConfiguration(string testConfigurationFilePath)
        {
            return _testConfiguration ??
                   (_testConfiguration = TestConfigurationParserXML.GetConfiguration(testConfigurationFilePath));
        }
    }
}