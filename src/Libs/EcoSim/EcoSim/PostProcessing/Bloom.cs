/* 
 * Environment Simulator
 * Copyright (C) 2008-2009 Justin Stoecker
 * 
 * This program is free software; you can redistribute it and/or modify it under the terms of the 
 * GNU General Public License as published by the Free Software Foundation; either version 2 of 
 * the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details.
 */

/*
 * Note: this is based heavily off of the XNA creator's club sample on bloom post-processing:
 * http://creators.xna.com/en-US/sample/bloom
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Sim
{
    struct BlurParameters
    {
        public float[] Weights;
        public Vector2[] Offsets;

        public BlurParameters(float[] weights, Vector2[] offsets)
        {
            this.Weights = weights;
            this.Offsets = offsets;
        }
    };

    public class Bloom
    {
        PostProcessManager ppManager;

        RenderTarget2D rTarget1;
        RenderTarget2D rTarget2;
        Effect e;

        EffectTechnique luminosityExtractTechnique;
        EffectTechnique gaussianBlurTechnique;
        EffectTechnique compositeTechnique;

        float bluriness = 8;
        BlurParameters horizontalBlurParameters;
        BlurParameters verticalBlurParameters;

        public Bloom(PostProcessManager ppManager)
        {
            this.ppManager = ppManager;
        }

        public void LoadContent(GraphicsDevice g, ContentManager cm)
        {
            e = cm.Load<Effect>(@"shaders/bloom");
            luminosityExtractTechnique = e.Techniques["LuminosityExtract"];
            gaussianBlurTechnique = e.Techniques["Blur"];
            compositeTechnique = e.Techniques["Composite"];

            PresentationParameters pp = g.PresentationParameters;
            rTarget1 = new RenderTarget2D(g, pp.BackBufferWidth, pp.BackBufferHeight, 1, g.DisplayMode.Format);
            rTarget2 = new RenderTarget2D(g, pp.BackBufferWidth, pp.BackBufferHeight, 1, g.DisplayMode.Format);

            // calculate the blur parameters
            SetBlur(8);
        }

        public void SetThreshold(float v)
        {
            e.Parameters["fBloomThreshold"].SetValue(v);
        }

        public void SetIntensity(float v)
        {
            e.Parameters["fBloomIntensity"].SetValue(v);
        }

        public void SetBlur(float v)
        {
            bluriness = v;
            float x = 1.0f / ppManager.Screen.Width;
            float y = 1.0f / ppManager.Screen.Height;
            horizontalBlurParameters = CalculateBlurParams(x, 0);
            verticalBlurParameters = CalculateBlurParams(0, y);
        }

        public void Process(GraphicsDevice g, RenderTarget2D sceneTarget)
        {
            // extract brightest parts of the scene
            g.SetRenderTarget(0, rTarget1);
            Texture2D sceneTexture = sceneTarget.GetTexture();
            ppManager.RenderScreenQuad(e, luminosityExtractTechnique, sceneTexture);

            // blur the luminosity extract horizontally
            g.SetRenderTarget(0, rTarget2);
            e.Parameters["fWeights"].SetValue(horizontalBlurParameters.Weights);
            e.Parameters["vOffsets"].SetValue(horizontalBlurParameters.Offsets);
            ppManager.RenderScreenQuad(e, gaussianBlurTechnique, rTarget1.GetTexture());

            // blur the luminosity extract vertically
            g.SetRenderTarget(0, rTarget1);
            e.Parameters["fWeights"].SetValue(verticalBlurParameters.Weights);
            e.Parameters["vOffsets"].SetValue(verticalBlurParameters.Offsets);
            ppManager.RenderScreenQuad(e, gaussianBlurTechnique, rTarget2.GetTexture());
            
            // combine blurred luminosity extract with scene
            g.SetRenderTarget(0, null);
            g.Textures[1] = sceneTexture;
            ppManager.RenderScreenQuad(e, compositeTechnique, rTarget1.GetTexture());
        }

        private BlurParameters CalculateBlurParams(float x, float y)
        {
            int nSamples = e.Parameters["vOffsets"].Elements.Count;
            float[] weights = new float[nSamples];
            Vector2[] offsets = new Vector2[nSamples];

            // the starting pixel
            weights[0] = Gaussian(0);
            offsets[0] = Vector2.Zero;

            // calculate weights / offsets
            float totalWeight = weights[0];
            for (int i = 0; i < nSamples / 2; i++)
            {
                // weight for pixels on both sides
                float weight = Gaussian(i + 1);
                weights[i * 2 + 1] = weight;
                weights[i * 2 + 2] = weight;
                totalWeight += weight * 2;

                // offset for texture coordinates
                Vector2 offset = new Vector2(x, y) * (i * 2 + 1.5f);
                offsets[i * 2 + 1] = offset;
                offsets[i * 2 + 2] = -offset;
            }

            // average the weights by total weight
            for (int i = 0; i < nSamples; i++)
                weights[i] /= totalWeight;

            return new BlurParameters(weights, offsets);
        }

        private float Gaussian(float n)
        {
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * bluriness)) *
                Math.Exp(-(n * n) / (2 * bluriness * bluriness)));
        }
    }
}
