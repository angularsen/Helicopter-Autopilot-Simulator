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
using System.Xml;

#endregion

namespace LTreesLibrary.Pipeline
{
    public class TreeProfileContent
    {
        public XmlDocument GeneratorXML { get; set; }
        public String TrunkTexture { get; set; }
        public String LeafTexture { get; set; }
        public String TrunkEffect { get; set; }
        public String LeafEffect { get; set; }
    }
}