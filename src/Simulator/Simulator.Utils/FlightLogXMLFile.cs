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
using Control.Common;
using Microsoft.Xna.Framework;
using State.Model;
using System.Globalization;

#endregion

namespace Simulator.Utils
{
    public static class FlightLogXMLFile
    {
        private static readonly CultureInfo Localization = new CultureInfo("en-us");

        public static void Write(string filepath, IEnumerable<HelicopterLogSnapshot> logs, IEnumerable<Waypoint> waypoints)
        {
            try
            {
                if (File.Exists(filepath))
                    File.Delete(filepath);

                XmlWriter w = XmlWriter.Create(filepath);
                if (w == null) return;

                w.WriteStartDocument();
                w.WriteStartElement("TrajectoryLog");

                foreach (Waypoint waypoint in waypoints)
                {
                    w.WriteStartElement("Waypoint");

                    WriteVector3(w, "Position", waypoint.Position);
                    w.WriteElementString("WaypointType", waypoint.Type.ToString());
                    w.WriteElementString("Radius", waypoint.Radius.ToString(Localization));

                    w.WriteEndElement();
                }

                foreach (HelicopterLogSnapshot plot in logs)
                {
                    w.WriteStartElement("Sample");

                    w.WriteStartElement("Seconds");
                    w.WriteValue(plot.Time.TotalSeconds.ToString(Localization));
                    w.WriteEndElement();

                    WriteVector3(w, "True", plot.True.Position);
                    WriteVector3(w, "Estimated", plot.Estimated.Position);
                    WriteVector3(w, "BlindEstimated", plot.BlindEstimated.Position);

                    if (plot.Observed.Position != Vector3.Zero)
                        WriteVector3(w, "Observed", plot.Observed.Position);

                    WriteFRU(w, "Accelerometer", plot.Accelerometer);

                    w.WriteStartElement("GroundAltitude");
                    w.WriteValue(plot.GroundAltitude.ToString(Localization));
                    w.WriteEndElement();

                    w.WriteEndElement();
                }
                w.WriteEndElement();
                w.WriteEndDocument();
                w.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


       public static FlightLog Read(string filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                    return null;


                string xmlText = ParseHelper.GetResourceText(filepath);
                var reader = new StringReader(xmlText);

                var doc = new XmlDocument();
                doc.Load(reader);
                XmlNode nav = doc.SelectSingleNode("/TrajectoryLog");


                var plots = new List<HelicopterLogSnapshot>();

                XmlNodeList sampleNodes = nav.SelectNodes("Sample");
                foreach (XmlNode sampleNode in sampleNodes)
                    plots.Add(ParseSample(sampleNode));

                var waypoints = new List<Waypoint>();
                XmlNodeList waypointNodes = nav.SelectNodes("Waypoint");
                foreach (XmlNode waypointNode in waypointNodes)
                    waypoints.Add(ParseWaypoint(waypointNode));

                reader.Close();

                return new FlightLog()
                                 {
                                     Plots = plots,
                                     Waypoints = waypoints
                                 };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private static Waypoint ParseWaypoint(XmlNode waypointNode)
        {
            var result = new Waypoint(
                XMLHelper.ParseVector3(waypointNode.SelectSingleNode("Position")),
                0,
                (WaypointType)Enum.Parse(typeof(WaypointType), waypointNode.SelectSingleNode("WaypointType").InnerText, true),
                float.Parse(waypointNode.SelectSingleNode("Radius").InnerText));

            return result;
        }

        private static HelicopterLogSnapshot ParseSample(XmlNode sampleNode)
        {
            var result = new HelicopterLogSnapshot
                       {
                           Estimated = ParsePhysicalHeliState(sampleNode, "Estimated"),
                           BlindEstimated = ParsePhysicalHeliState(sampleNode, "BlindEstimated"),
                           True = ParsePhysicalHeliState(sampleNode, "True"),
                           Time = TimeSpan.FromSeconds(double.Parse(sampleNode.SelectSingleNode("Seconds").InnerText)),
                           Accelerometer = ParseFRU(sampleNode.SelectSingleNode("Accelerometer")),
                           GroundAltitude = float.Parse(sampleNode.SelectSingleNode("GroundAltitude").InnerText),
                       };

            var observedNode = sampleNode.SelectSingleNode("Observed");
            if (observedNode != null)
                result.Observed = ParsePhysicalHeliState(sampleNode, "Observed");

            return result;

        }

        private static PhysicalHeliState ParsePhysicalHeliState(XmlNode sampleNode, string sampleType)
        {
            XmlNode childNode = sampleNode.SelectSingleNode(sampleType);
            return new PhysicalHeliState
                             {
                                 Position = XMLHelper.ParseVector3(childNode)
                             };
        }

        private static void WriteVector3(XmlWriter w, string nodeName, Vector3 v)
        {
            w.WriteStartElement(nodeName);
            w.WriteAttributeString("X", v.X.ToString(Localization));
            w.WriteAttributeString("Y", v.Y.ToString(Localization));
            w.WriteAttributeString("Z", v.Z.ToString(Localization));
            w.WriteEndElement();
        }

        private static void WriteFRU(XmlWriter w, string nodeName, ForwardRightUp value)
        {
            w.WriteStartElement(nodeName);
            w.WriteAttributeString("Forward", value.Forward.ToString(Localization));
            w.WriteAttributeString("Right", value.Right.ToString(Localization));
            w.WriteAttributeString("Up", value.Up.ToString(Localization));
            w.WriteEndElement();
        }

        private static ForwardRightUp ParseFRU(XmlNode node)
        {
            return new ForwardRightUp(
                float.Parse(node.Attributes["Forward"].InnerText, Localization),
                float.Parse(node.Attributes["Right"].InnerText, Localization),
                float.Parse(node.Attributes["Up"].InnerText, Localization));
        }

    }
}