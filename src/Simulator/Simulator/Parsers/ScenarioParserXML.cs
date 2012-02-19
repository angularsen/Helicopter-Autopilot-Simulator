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
using System.Xml;
using Anj.Helpers.XNA;
using Control;
using Control.Common;
using Microsoft.Xna.Framework;
using Simulator.Cameras;
using Simulator.Scenarios;
using Simulator.Utils;
using System.Diagnostics;
using Simulator.Testing;

#endregion

namespace Simulator.Parsers
{
    public struct TerrainInfo
    {
        public int Width;
        public float MinHeight;
        public float MaxHeight;
    }

    public static class ScenarioParserXML
    {
        public static string GetPreSelectedScenarioName(string xmlFilepath)
        {
            string xmlText = ParseHelper.GetResourceText(xmlFilepath);
            var reader = new StringReader(xmlText);

            var doc = new XmlDocument();
            doc.Load(reader);
            XmlNode nav = doc.SelectSingleNode("/root");
            XmlNode preSelectedScenarioNode = nav.SelectSingleNode("PreSelectedScenario");
            return preSelectedScenarioNode.InnerText;
        }


        public static IList<Scenario> GetScenarios(string xmlFilepath)
        {
            var result = new List<Scenario>();

            string xmlText = ParseHelper.GetResourceText(xmlFilepath);
            var reader = new StringReader(xmlText);

            var doc = new XmlDocument();
            doc.Load(reader);
            XmlNode nav = doc.SelectSingleNode("/root");
            XmlNodeList scenarioNodes = nav.SelectNodes("Scenario");
            foreach (XmlNode scenarioNode in scenarioNodes)
                result.Add(ParseScenario(scenarioNode));

            reader.Close();

            return result;
        }

        private static Scenario ParseScenario(XmlNode scenarioNode)
        {
            var scenario = new Scenario();
            scenario.Name = scenarioNode.GetAttribute("Name");
            scenario.BackgroundMusic = ParseBackgroundMusic(scenarioNode.SelectSingleNode("BackgroundMusic"));
            scenario.SceneElements = ParseSceneElements(scenarioNode.SelectSingleNode("Scene"));
            scenario.CameraType = ParseCameraType(scenarioNode.SelectSingleNode("CameraType"));
            scenario.Timeout = ParseTimeout(scenarioNode.SelectSingleNode("TimeoutSeconds"));

            XmlNode terrainInfoNode = scenarioNode.SelectSingleNode("Scene").SelectSingleNode("Terrain");
            if (terrainInfoNode != null)
                scenario.TerrainInfo = ParseTerrainInfo(terrainInfoNode);
            

            scenario.HelicopterScenarios = ParseHelicopterScenario(scenarioNode);
            return scenario;
        }

        private static TimeSpan ParseTimeout(XmlNode timeoutSecondsNode)
        {
            if (timeoutSecondsNode == null)
                return TimeSpan.Zero;

            int timeoutSeconds = int.Parse(timeoutSecondsNode.InnerText);
            return TimeSpan.FromSeconds(timeoutSeconds);
        }

        private static TerrainInfo ParseTerrainInfo(XmlNode terrainInfoNode)
        {
            var result = new TerrainInfo();

            try { result.Width = int.Parse(terrainInfoNode.GetAttribute("Width")); }
            catch { result.Width = 256; }

            try { result.MinHeight = int.Parse(terrainInfoNode.GetAttribute("MinHeight")); }
            catch { result.MinHeight = 0; }

            try { result.MaxHeight = int.Parse(terrainInfoNode.GetAttribute("MaxHeight")); }
            catch { result.MaxHeight = 20; }

            return result;
        }

        /// <summary>
        /// Returns a list of names of the scene elements.
        /// </summary>
        /// <param name="sceneNode"></param>
        /// <returns></returns>
        private static IEnumerable<string> ParseSceneElements(XmlNode sceneNode)
        {
            var result = new List<string>();
            foreach (XmlNode elementNode in sceneNode.ChildNodes)
            {
                string elementName = elementNode.Name;
                result.Add(elementName);
            }

            return result;
        }

        /// <summary>
        /// Returns the resource path for the background music sound effect file.
        /// Returns null if music tag was missing or not recognized.
        /// </summary>
        /// <param name="backgroundMusicNode"></param>
        /// <returns></returns>
        private static string ParseBackgroundMusic(XmlNode backgroundMusicNode)
        {
            if (backgroundMusicNode == null) 
            {
                Console.WriteLine(@"Could not find background music.");
                return null;
            }

            string music = backgroundMusicNode.InnerText;
            if (music == "Airwolf")
                return "Audio/Airwolf Theme";

            return null;
        }

        private static Task ParseTask(XmlNode helicopterNode)
        {
            try
            {
                var task = new Task();

                XmlNode taskNode = helicopterNode.SelectSingleNode("Task");

                var heightAboveGroundNode = taskNode.SelectSingleNode("HoldHeightAboveGround");
                task.HoldHeightAboveGround = (heightAboveGroundNode != null) ? int.Parse(heightAboveGroundNode.InnerText) : -1;

                XmlNode loopNode = taskNode.SelectSingleNode("Loop");
                task.Loop = (loopNode != null) ? bool.Parse(loopNode.InnerText) : false;

                XmlNode defaultRadiusNode = taskNode.SelectSingleNode("DefaultWaypointRadius");
                float defaultWaypointRadius = (defaultRadiusNode != null)
                                           ? float.Parse(defaultRadiusNode.InnerText)
                                           : DefaultAutopilotConfiguration.WaypointRadius;

                XmlNodeList waypointNodes = taskNode.SelectNodes("Waypoint");
                foreach (XmlNode waypointNode in waypointNodes)
                {
                    XmlNode radiusNode = waypointNode.SelectSingleNode("Radius");

                    float radius = (radiusNode != null) 
                        ? float.Parse(radiusNode.InnerText)
                        : defaultWaypointRadius;

                    Waypoint wp = ParseWaypoint(waypointNode, radius);
                    if (wp != null)
                        task.AddWaypoint(wp);
                }

                return task;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"Error parsing scenario XML. " + e);
                return null;
            }
        }

        private static Waypoint ParseWaypoint(XmlNode waypointNode, float waypointRadius)
        {
            // TODO Implement heading angle (also in XML file!)
            Vector3 position = XMLHelper.ParseVector3(waypointNode.SelectSingleNode("Position"));
            
            float heading = 0f;
            var headingNode = waypointNode.SelectSingleNode("HeadingAngle");
            if (headingNode != null)
                heading = float.Parse(headingNode.InnerText);

            var type = (WaypointType) Enum.Parse(typeof (WaypointType), waypointNode.SelectSingleNode("Type").InnerText, true);
            var result = new Waypoint(position, heading, type, waypointRadius);

            var secondsToWaitNode = waypointNode.SelectSingleNode("SecondsToWait");
            if (secondsToWaitNode != null)
                result.SecondsToWait = float.Parse(secondsToWaitNode.InnerText);

            return result;
        }

        private static IList<HelicopterScenario> ParseHelicopterScenario(XmlNode scenarioNode)
        {
            var result = new List<HelicopterScenario>();

            XmlNodeList helicopterNodes = scenarioNode.SelectNodes("Helicopter");
            foreach (XmlNode heliNode in helicopterNodes)
            {
                var heliScenario = new HelicopterScenario();
                heliScenario.Task = ParseTask(heliNode);

                XmlNode playerControlledNode = heliNode.SelectSingleNode("PlayerControlled");
                heliScenario.PlayerControlled = (playerControlledNode != null) ? bool.Parse(playerControlledNode.InnerText) : false;

                XmlNode playEngineSoundNode = heliNode.SelectSingleNode("EngineSound");
                heliScenario.EngineSound = (playEngineSoundNode != null) ? bool.Parse(playEngineSoundNode.InnerText) : false;

                XmlNode startPosNode = heliNode.SelectSingleNode("StartPosition");
                heliScenario.StartPosition = XMLHelper.ParseVector3(startPosNode);

                var assistedAutopilotNode = heliNode.SelectSingleNode("AssistedAutopilot");
                heliScenario.AssistedAutopilot = (assistedAutopilotNode != null)
                                                     ? bool.Parse(assistedAutopilotNode.InnerText)
                                                     : false;


                result.Add(heliScenario);
            }

            return result;
        }

        private static CameraType ParseCameraType(XmlNode cameraTypeNode)
        
        {
            if (cameraTypeNode == null)
            {
                Debug.WriteLine("Using chase camera as default.");
                return CameraType.Chase;
            }

            string cameraType = cameraTypeNode.InnerText;

            if (cameraType == "Cockpit")
                return CameraType.Cockpit;

            if (cameraType == "Chase")
                return CameraType.Chase;

            if (cameraType == "Fixed")
                return CameraType.Fixed;

            if (cameraType == "Free")
                return CameraType.Free;

            throw new ArgumentException("Unknown camera type " + cameraType);
            
        }

        
    }

    public struct SimulationSettings
    {
        public bool SwapStereo;
        public RenderModes RenderMode;
    }
}