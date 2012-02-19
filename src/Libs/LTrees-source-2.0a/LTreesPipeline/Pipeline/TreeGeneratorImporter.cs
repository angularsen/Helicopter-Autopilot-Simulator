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

using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline;

#endregion

namespace LTreesLibrary.Pipeline
{
    /// <summary>
    /// Imports XML tree specifications.
    /// </summary>
    /// <remarks>
    /// It really just imports XML files as an XmlDocument.
    /// </remarks>
    [ContentImporter(".ltree", DisplayName = "LTree Specification", DefaultProcessor = "TreeProfileProcessor")]
    public class TreeGeneratorImporter : ContentImporter<XmlDocument>
    {
        public override XmlDocument Import(string filename, ContentImporterContext context)
        {
            var doc = new XmlDocument();
            doc.Load(filename);
            return doc;
        }
    }
}