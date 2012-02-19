//---------------------------------------------------------------------------//
// Name		: PointSprite.fx
// Desc		: Used for drawing point sprites
// Author	: Justin Stoecker. Copyright (C) 2008-2009.
// Other	: Parts adapted from Riemer Grootjan's XNA Series 2
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

Texture tTexture;
sampler sTexture = sampler_state { 
	texture = <tTexture>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter = LINEAR; 
	AddressU = clamp; 
	AddressV = clamp;
};

//---------------------------------------------------------------------------//
// Vertex Shaders
//---------------------------------------------------------------------------//
struct VS_Input
{
    float4 vPosition	: POSITION0;
    float4 vColor		: COLOR0;
    float1 fSize		: PSIZE;
};

struct VS_Output
{
    float4 vPosition	: POSITION0;
    float1 fSize		: PSIZE;
};

VS_Output VS(VS_Input input)
{
    VS_Output output = (VS_Output)0;
     
    float4x4 matVP = mul (matView, matProjection);
	float4x4 matWVP = mul (matWorld, matVP); 
    output.vPosition = mul(input.vPosition, matWVP);    
    output.fSize = input.fSize * matProjection._m11 / output.vPosition.w * 200;
    
    return output;  
}

struct VS_Output_Sun
{
	float4 vPosition	: POSITION0;
	float1 fSize		: PSIZE;
	float3 vWPosition	: TEXCOORD0;
};

VS_Output_Sun VS_Sun(VS_Input input)
{
    VS_Output_Sun output = (VS_Output_Sun)0;
     
    float4x4 matVP = mul (matView, matProjection);
	float4x4 matWVP = mul (matWorld, matVP); 
    output.vPosition = mul(input.vPosition, matWVP);    
    output.fSize = input.fSize;
    output.vWPosition = input.vPosition;
    
    return output;  
}
//---------------------------------------------------------------------------//
// Pixel Shaders
//---------------------------------------------------------------------------//
struct PS_Input
{
	float2 vTexCoords	: TEXCOORD0;
};

float4 PS(PS_Input input) : COLOR0
{
    return tex2D(sTexture, input.vTexCoords);
}

float4 PS_Sun(PS_Input psInput, VS_Output_Sun vsOutput) : COLOR0
{
	float4 vColor = tex2D(sTexture, psInput.vTexCoords);
	vColor.a *= saturate(2-overcast);
	vColor.a *= lerp(0,1, vSunVector.y / 0.1f);
	return vColor;
}

//---------------------------------------------------------------------------//
// Techniques
//---------------------------------------------------------------------------//
technique Default
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}

technique Sun
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VS_Sun();
        PixelShader = compile ps_2_0 PS_Sun();
    }
}

