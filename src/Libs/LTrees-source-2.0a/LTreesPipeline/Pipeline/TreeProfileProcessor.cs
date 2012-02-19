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
using System.ComponentModel;
using System.Xml;
using LTreesLibrary.Trees;
using Microsoft.Xna.Framework.Content.Pipeline;

#endregion

namespace LTreesLibrary.Pipeline
{
    [ContentProcessor(DisplayName = "LTree Profile")]
    public class TreeProfileProcessor : ContentProcessor<XmlDocument, TreeProfileContent>
    {
        [DisplayName("Texture Path")]
        [DefaultValue("")]
        [Description("Prefix file's texture asset names.")]
        public String TexturePath { get; set; }

        [DisplayName("Trunk Effect")]
        [DefaultValue("")]
        [Description(
            "Asset name of the effect used to render the branches. If left blank, the reference shader is used.")]
        public String TrunkEffect { get; set; }

        [DisplayName("Leaf Effect")]
        [DefaultValue("")]
        [Description("Asset name of the effect used to render the leaves. If left blank, the reference shader is used.")
        ]
        public String LeafEffect { get; set; }

        private void ErrorInvalidFormat(String message)
        {
            throw new PipelineException("Invalid LTree specification. " + message);
        }

        private String GetChildContent(XmlNode node, String childName)
        {
            XmlNode child = node.SelectSingleNode(childName);
            if (child == null)
                ErrorInvalidFormat("Missing " + childName + " node.");
            return child.InnerText;
        }

        public override TreeProfileContent Process(XmlDocument input, ContentProcessorContext context)
        {
            // Build a tree generator just to validate the XML format
            try
            {
                TreeGenerator.CreateFromXml(input);
            }
            catch (ArgumentException ex)
            {
                ErrorInvalidFormat(ex.Message);
            }

            var content = new TreeProfileContent();

            string path = "";
            if (TexturePath != null && TexturePath != "" && !(TexturePath.EndsWith("/") || TexturePath.EndsWith(@"\")))
                path = TexturePath + "/";
            else if (TexturePath == null)
                path = "";
            else
                path = TexturePath;

            XmlNode treeNode = input.SelectSingleNode("Tree");

            content.GeneratorXML = input;
            content.TrunkTexture = path + GetChildContent(treeNode, "TrunkTexture");
            content.LeafTexture = path + GetChildContent(treeNode, "LeafTexture");
            content.TrunkEffect = TrunkEffect;
            content.LeafEffect = LeafEffect;

            return content;
        }
    }
}