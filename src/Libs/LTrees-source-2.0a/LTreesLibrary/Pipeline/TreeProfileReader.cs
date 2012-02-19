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
using LTreesLibrary.Trees;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace LTreesLibrary.Pipeline
{
    public class TreeProfileReader : ContentTypeReader<TreeProfile>
    {
//        public String DefaultTreeShaderAssetName = "LTreeShaders/Trunk";
//        public String DefaultLeafShaderAssetName = "LTreeShaders/Leaves";
        public String DefaultLeafShaderAssetName = "LTreeShaders/LeavesSunlit";
        public String DefaultTreeShaderAssetName = "LTreeShaders/TrunkSunlit";

        protected override TreeProfile Read(ContentReader input, TreeProfile existingInstance)
        {
            // brace yourself for the simple and intuitive way of retrieving the graphics device
            var deviceService =
                input.ContentManager.ServiceProvider.GetService(typeof (IGraphicsDeviceService)) as
                IGraphicsDeviceService;
            GraphicsDevice device = deviceService.GraphicsDevice;

            if (existingInstance == null)
                existingInstance = new TreeProfile(device);

            ContentManager content = input.ContentManager;

            existingInstance.Generator = ReadGenerator(input);
            existingInstance.TrunkTexture = content.Load<Texture2D>(input.ReadString());
            existingInstance.LeafTexture = content.Load<Texture2D>(input.ReadString());

            string trunkEffect = input.ReadString();
            string leafEffect = input.ReadString();

            if (trunkEffect == "")
                trunkEffect = DefaultTreeShaderAssetName;
            if (leafEffect == "")
                leafEffect = DefaultLeafShaderAssetName;

            existingInstance.TrunkEffect = content.Load<Effect>(trunkEffect);
            existingInstance.LeafEffect = content.Load<Effect>(leafEffect);

            var dayTexture = content.Load<Texture2D>("Textures/SkyDay");
            var sunsetTexture = content.Load<Texture2D>("Textures/Sunset");
            var nightTexture = content.Load<Texture2D>("Textures/SkyNight");

            existingInstance.TrunkEffect.Parameters["SkyTextureDay"].SetValue(nightTexture);
            existingInstance.TrunkEffect.Parameters["SkyTextureSunset"].SetValue(sunsetTexture);
            existingInstance.TrunkEffect.Parameters["SkyTextureNight"].SetValue(dayTexture);

            existingInstance.LeafEffect.Parameters["SkyTextureDay"].SetValue(nightTexture);
            existingInstance.LeafEffect.Parameters["SkyTextureSunset"].SetValue(sunsetTexture);
            existingInstance.LeafEffect.Parameters["SkyTextureNight"].SetValue(dayTexture);

            return existingInstance;
        }

        private TreeGenerator ReadGenerator(ContentReader input)
        {
            String source = input.ReadString();
            var xml = new XmlDocument();
            xml.LoadXml(source);
            return TreeGenerator.CreateFromXml(xml);
        }
    }
}