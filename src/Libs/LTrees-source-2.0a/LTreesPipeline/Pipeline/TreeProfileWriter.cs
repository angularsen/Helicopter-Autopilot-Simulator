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

using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

#endregion

namespace LTreesLibrary.Pipeline
{
    [ContentTypeWriter]
    public class TreeProfileWriter : ContentTypeWriter<TreeProfileContent>
    {
        protected override void Write(ContentWriter output, TreeProfileContent value)
        {
            var xmlSource = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(xmlSource);
            value.GeneratorXML.Save(writer);

            //output.Write(xmlSource.ToString());
            output.Write(xmlSource.ToString());
            output.Write(value.TrunkTexture);
            output.Write(value.LeafTexture);
            output.Write(value.TrunkEffect == null ? "" : value.TrunkEffect);
            output.Write(value.LeafEffect == null ? "" : value.LeafEffect);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof (TreeProfileReader).AssemblyQualifiedName;
        }
    }
}