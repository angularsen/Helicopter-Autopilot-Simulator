//---------------------------------------------------------------------------//
// Name		: Sky.fx
// Desc		: Used with a skydome for day/night cycle coloring with stars
// Author	: Justin Stoecker. Copyright (C) 2009.
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

#include "camera.inc"
#include "lighting.inc"

float4x4 matWorld;

float fTime;

// Sky colors
float4 vHorizonColorTwilight = float4(0.4f, 0.16f, 0, 1) * 1.5f;
float4 vHorizonColorDay = float4(1, 1, 1, 1);
float4 vHorizonColorNight = float4(0.2f, 0.2f, 0.2f, 1);
float4 vCeilingColorTwilight = float4(0.17f, 0.15f, 0.15f, 1);
float4 vCeilingColorDay = float4(0.72f, 0.75f, 0.98f, 1);
float4 vCeilingColorNight = float4(0.1f, 0.1f, 0.15f, 1);

struct VS_Input
{
    float4 vPosition	: POSITION0;
    float2 vTexCoords	: TEXCOORD0;
};

struct VS_Output
{
    float4 vPosition	: POSITION0;
    float2 vTexCoords	: TEXCOORD0;
    float4 vWPosition	: TEXCOORD1;
};

//---------------------------------------------------------------------------//
// Texture Samplers
//---------------------------------------------------------------------------//

texture tNight;
sampler sNight = sampler_state {
	texture = <tNight>;
	MagFilter = linear;
	MinFilter = linear;
	MipFilter = linear;
	AddressU  = wrap;
	AddressV  = wrap;
};

texture tClouds;
sampler sClouds = sampler_state {
	texture = <tClouds>;
	MagFilter = linear;
	MinFilter = linear;
	MipFilter = linear;
	AddressU  = wrap;
	AddressV  = wrap;
};

//---------------------------------------------------------------------------//
// Vertex Shader
//---------------------------------------------------------------------------//
VS_Output VS(VS_Input input)
{
    VS_Output output = (VS_Output)0;

	float4x4 matVP = mul(matView, matProjection);
	float4x4 matWVP = mul(matWorld, matVP);
    
    output.vPosition = mul(input.vPosition, matWVP);
    output.vTexCoords = input.vTexCoords;
    output.vWPosition = input.vPosition;

    return output;
}

//---------------------------------------------------------------------------//
// Pixel Shader
//---------------------------------------------------------------------------//
float4 PS(VS_Output input) : COLOR0
{
    float4 vHorizonColor;	// color at the base of the skydome
    float4 vCeilingColor;	// color at the top of the skydome
    
    // interpolate the horizon/ceiling colors based on the time of day
    if (vSunVector.y > 0)
    {
		float amount = min(vSunVector.y * 1.5f, 1);
		vHorizonColor = lerp(vHorizonColorTwilight, vHorizonColorDay, amount);
		vCeilingColor = lerp(vCeilingColorTwilight, vCeilingColorDay, amount);
    }
    else
    {
		float amount = min(-vSunVector.y * 1.5f, 1);
    	vHorizonColor = lerp(vHorizonColorTwilight, vHorizonColorNight, amount);
		vCeilingColor = lerp(vCeilingColorTwilight, vCeilingColorNight, amount);
    }
    
    // interpolate the color of the pixel by its height in the skydome
    float4 vPixelColor = lerp(vHorizonColor, vCeilingColor, saturate(input.vWPosition.y / 0.4f));
    
    // darken sky when overcast
    vPixelColor.rgb /= overcast;
    
    // sunrises/sunsets should glow around the horizon where the sun is rising/setting
    vPixelColor.rgb += float3(1.2f,0.8f,0) * saturate(1 - distance(input.vWPosition, vSunVector)) / 2 * (1-fSunIntensity) / overcast;
    
    // stars should be gradually more visible as they are closer to the ceiling and invisible when
    // closer to the horizon.  stars should also fade in/out during transitions between night/day
    float fHorizonLerp = saturate(lerp(0,1,input.vWPosition.y * 1.5f));
    float fStarVisibility = saturate(max(fHorizonLerp * lerp(0,1,max(-vSunVector.y,0)),0));
    
    // display clouds
    vPixelColor.rgb *= 1-tex2D(sClouds, input.vTexCoords * 4 + fTime * 3).a * fHorizonLerp / 2 * (2-overcast) / 2;
    vPixelColor.rgb *= 1-tex2D(sClouds, input.vTexCoords * 2 + fTime).a * fHorizonLerp  / 2 * (overcast - 1);
    
    // display stars according to visibility, scaling the texture coordinates for smaller stars
    float2 vStarTexCoords = input.vTexCoords * 8;
    vStarTexCoords.y += fTime / 2;
    vPixelColor += tex2D(sNight, vStarTexCoords) * fStarVisibility * 0.6f / (overcast*2);
    

    return vPixelColor;
}

//---------------------------------------------------------------------------//
// Technique
//---------------------------------------------------------------------------//
technique Sky
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VS();
        PixelShader = compile ps_2_0 PS();
    }
}
