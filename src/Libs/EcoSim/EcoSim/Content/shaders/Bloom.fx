//---------------------------------------------------------------------------//
// Name		: Bloom.fx
// Desc		: Bloom post-processing effect
// Author	: Justin Stoecker. Copyright (C) 2008-2009.
// Other	: Based off of Microsoft Bloom Sample
//			: http://creators.xna.com/en-US/sample/bloom
//---------------------------------------------------------------------------//

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

#define GAUSSIAN_SAMPLES 15

sampler sInput : register(s0);	// sampler for supplied texture from sprite batch
sampler sScene : register(s1);	// sampler for scene texture

float2 vOffsets[GAUSSIAN_SAMPLES];	// offset for texture coordinates when blurring
float fWeights[GAUSSIAN_SAMPLES];	// amount of weight given to an offset sample
float fBloomThreshold = 0.6f;
float fBloomIntensity = 1.15f;

//------------------------------ Pixel Shader -------------------------------//

float4 PS_LuminosityExtract(float2 vTexCoords : TEXCOORD0) : COLOR0
{
	float4 vPixelColor = tex2D(sInput, vTexCoords);		
	return saturate((vPixelColor - fBloomThreshold) / (1 - fBloomThreshold));
}

float4 PS_Blur(float2 vTexCoords : TEXCOORD0) : COLOR0
{
	float4 vPixelColor = 0;

	for (int i=0; i<GAUSSIAN_SAMPLES; i++)
		vPixelColor += tex2D(sInput, vTexCoords + vOffsets[i]) * fWeights[i];
	
	return vPixelColor;
}

float4 PS_Composite(float2 vTexCoords : TEXCOORD0) : COLOR0
{
	float4 vExtractColor = tex2D(sInput, vTexCoords) * fBloomIntensity;
	float4 vSceneColor = tex2D(sScene, vTexCoords);
	
	vSceneColor *= (1 - saturate(vExtractColor));
	
	return vSceneColor + vExtractColor;
}

//------------------------------- Techniques --------------------------------//

technique LuminosityExtract
{
	pass p0
	{
		PixelShader = compile ps_2_0 PS_LuminosityExtract();
	}
}

technique Blur
{
	pass p0
	{
		PixelShader = compile ps_2_0 PS_Blur();
	}
}

technique Composite
{
	pass p0
	{
		PixelShader = compile ps_2_0 PS_Composite();
	}
}
