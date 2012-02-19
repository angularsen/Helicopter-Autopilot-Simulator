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
using Simulator.Utils;

#endregion

namespace Simulator.Parsers
{
    public static class PIDParserXML
    {
        public static IList<PIDSetup> GetPIDSetups(string xmlFilepath)
        {
            var result = new List<PIDSetup>();

            string xmlText = ParseHelper.GetResourceText(xmlFilepath);
            var reader = new StringReader(xmlText);

            var doc = new XmlDocument();
            doc.Load(reader);
            XmlNode nav = doc.SelectSingleNode("/root");
            XmlNodeList pidSetupNodes = nav.SelectNodes("PIDSetup");
            foreach (XmlNode pidSetupNode in pidSetupNodes)
                result.Add(ParsePIDSetup(pidSetupNode));

            reader.Close();

            return result;
        }

        private static PIDSetup ParsePIDSetup(XmlNode pidSetupNode)
        {
            var result = new PIDSetup();
            result.Name = pidSetupNode.GetAttribute("Name");
            result.PitchAngle = ParsePID(pidSetupNode.SelectSingleNode("PID[@Name='PitchAngle']"));
            result.RollAngle = ParsePID(pidSetupNode.SelectSingleNode("PID[@Name='RollAngle']"));
            result.YawAngle = ParsePID(pidSetupNode.SelectSingleNode("PID[@Name='YawAngle']"));
            result.Throttle = ParsePID(pidSetupNode.SelectSingleNode("PID[@Name='Throttle']"));
            result.ForwardsAccel = ParsePID(pidSetupNode.SelectSingleNode("PID[@Name='Velocity']"));
            result.RightwardsAccel = ParsePID(pidSetupNode.SelectSingleNode("PID[@Name='Velocity']"));

            return result;
        }


        private static PID ParsePID(XmlNode node)
        {
            if (node == null) throw new ArgumentNullException("node");

            float[] pidValues = XMLHelper.ParseAttributesFloat(node, "P", "I", "D");
            if (pidValues == null || pidValues.Length != 3)
                throw new Exception("Unexpected error parsing XML attributes for PID.");

            return new PID
                       {
                           Name = node.GetAttribute("Name"),
                           P = Convert.ToSingle(pidValues[0]),
                           I = Convert.ToSingle(pidValues[1]),
                           D = Convert.ToSingle(pidValues[2]),
                       };
        }
    }
}