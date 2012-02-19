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

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Sensors.Model;
using Simulator.Resources;
using Simulator.Testing;
using Simulator.Utils;
using State.Model;

#endregion

namespace Simulator.Parsers
{
    public static class TestConfigurationParserXML
    {
        public static TestConfiguration GetConfiguration(string xmlFilepath)
        {
            var result = new TestConfiguration();

            string xmlText = ParseHelper.GetResourceText(xmlFilepath);
            var reader = new StringReader(xmlText);

            var doc = new XmlDocument();
            doc.Load(reader);
            XmlNode nav = doc.SelectSingleNode("/root/TestConfiguration");

            // Parse configuration
            result.FlyBySensors = bool.Parse(nav.SelectSingleNode("FlyBySensors").InnerText);
            result.UsePerfectSensors = bool.Parse(nav.SelectSingleNode("UsePerfectSensors").InnerText);
            result.UseRangeFinder = bool.Parse(nav.SelectSingleNode("UseRangeFinder").InnerText);
            result.UseGPS = bool.Parse(nav.SelectSingleNode("UseGPS").InnerText);
            result.UseINS = bool.Parse(nav.SelectSingleNode("UseINS").InnerText);

            result.Sensors = ParseSensorSpecifications(nav.SelectSingleNode("SensorSpecifications"));
            result.MaxHVelocities = ParseMaxHVelocities(nav.SelectNodes("MaxHVelocity"));

            List<string> scenarioNames = ParseScenarioNames(nav.SelectNodes("ScenarioName"));

            foreach (string scenarioName in scenarioNames)
                result.TestScenarios.Add(SimulatorResources.GetScenario(scenarioName));

            return result;
        }

        private static SensorSpecifications ParseSensorSpecifications(XmlNode sensorsNode)
        {
            XmlNode accelerometerNode = sensorsNode.SelectSingleNode("Accelerometer");
            XmlNode accelStdDevNode = accelerometerNode.SelectSingleNode("NoiseStdDev");

            XmlNode gpsPosStdDevNode = sensorsNode.SelectSingleNode("GPSPositionAxisStdDev");
            XmlNode gpsVelStdDevNode = sensorsNode.SelectSingleNode("GPSVelocityAxisStdDev");

            var r = new SensorSpecifications
                        {
                            AccelerometerFrequency = float.Parse(accelerometerNode.SelectSingleNode("Frequency").InnerText),

                            AccelerometerStdDev = new ForwardRightUp(
                                float.Parse(accelStdDevNode.SelectSingleNode("Forward").InnerText),
                                float.Parse(accelStdDevNode.SelectSingleNode("Right").InnerText),
                                float.Parse(accelStdDevNode.SelectSingleNode("Up").InnerText)),

                            GPSPositionStdDev = new Vector3(float.Parse( gpsPosStdDevNode.InnerText)),
                            GPSVelocityStdDev = new Vector3(float.Parse( gpsVelStdDevNode.InnerText)),
                            OrientationAngleNoiseStdDev = float.Parse(sensorsNode.SelectSingleNode("OrientationAngleNoiseStdDev").InnerText),
                        };

            return r;
        }

        private static List<float> ParseMaxHVelocities(XmlNodeList velocityNodes)
        {
            var result = new List<float>();
            foreach (XmlNode velocityNode in velocityNodes)
                result.Add(float.Parse( velocityNode.InnerText));

            return result;
        }

        private static List<string> ParseScenarioNames(XmlNodeList scenarioNodes)
        {
            var result = new List<string>();
            foreach (XmlNode scenarioNode in scenarioNodes)
                result.Add(scenarioNode.InnerText);

            return result;
        }
    }
}