#if !XBOX
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
using System.Xml.XPath;
using Anj.Helpers.XNA;
using Anj.XNA.Joysticks;
using Anj.XNA.Joysticks.Wizard;
using Simulator.Utils;

#endregion

namespace Simulator.Parsers
{
    public static class JoystickSetupParserXML
    {
        public static IList<JoystickSetup> GetJoystickSetups(string xmlFilepath)
        {
            var result = new List<JoystickSetup>();

            string xmlText = ParseHelper.GetResourceText(xmlFilepath);
            var reader = new StringReader(xmlText);

            var doc = new XmlDocument();
            doc.Load(reader);
            XmlNode nav = doc.SelectSingleNode("/root");
            XmlNodeList scenariosNodes = nav.SelectNodes("JoystickSetup");
            foreach (XmlNode scenarioNode in scenariosNodes)
                result.Add(ParseJoystickSetup(scenarioNode));

            reader.Close();

            return result;
        }

        private static JoystickSetup ParseJoystickSetup(XmlNode joystickSetupNode)
        {
            var setup = new JoystickSetup();
            setup.Name = joystickSetupNode.GetAttribute("Name");
            XmlNodeList deviceNodes = joystickSetupNode.SelectNodes("JoystickDevice");

            foreach (XmlNode deviceNode in deviceNodes)
                setup.Devices.Add(ParseDevice(deviceNode));

            return setup;
        }

        private static JoystickDevice ParseDevice(XmlNode deviceNode)
        {
            var device = new JoystickDevice();
            device.Name = deviceNode.GetAttribute("Name");
            XmlNodeList axisNodes = deviceNode.SelectNodes("Axis");
            foreach (XmlNode axisNode in axisNodes)
            {
                if (!String.IsNullOrEmpty(axisNode.ToString()))
                    device.Axes.Add(ParseAxis(axisNode));
            }
            return device;
        }

        private static Axis ParseAxis(XmlNode axisNode)
        {
            var axis = new Axis();
            axis.Name = ParseJoystickAxis(axisNode);
            axis.Action = ParseJoystickAxisAction(axisNode.InnerText);
            axis.IsInverted = Boolean.Parse(axisNode.GetAttribute("Inverted"));
            return axis;
        }

        private static JoystickAxis ParseJoystickAxis(XmlNode axisNode)
        {
            return (JoystickAxis)Enum.Parse(typeof(JoystickAxis), axisNode.GetAttribute("Name"));
        }

        private static JoystickAxisAction ParseJoystickAxisAction(string actionString)
        {
            return (JoystickAxisAction) Enum.Parse(typeof (JoystickAxisAction), actionString, true);
        }
    }
}
#endif
