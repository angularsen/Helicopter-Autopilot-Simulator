//---------------------------------------------------------------------------//
// Name		: Vegetation.fx
// Desc		: Used for drawing vegetation billboards
// Author	: Justin Stoecker. Copyright (C) 2008-2009.
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

#include "lighting.inc"
#include "camera.inc"

float4x4 matWorld;

float fTime;

Texture t0;
sampler s0 = sampler_state { 
	texture = <t0> ; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter = LINEAR; 
	AddressU = CLAMP; 
	AddressV = CLAMP;
};

// cloud shadow map
texture tCloudShadowMap;
sampler sCloudShadowMap = sampler_state { 
	Texture		= (tCloudShadowMap);
	MinFilter	= Linear;
	MagFilter	= Linear;
	MipFilter	= Linear;
	AddressU	= Wrap;
	AddressV	= Wrap;
};

//---------------------------------------------------------------------------//
// Vertex Shaders
//---------------------------------------------------------------------------//
struct VS_In
{
	float3 vPos			: POSITION0;
	float3 vNormal		: NORMAL0;
	float3 vColor		: COLOR0;
	float2 vTexCoords	: TEXCOORD0;
	float2 vScale		: TEXCOORD1;
};

struct VS_Out
{
	float4 vPos			: POSITION;
	float2 vTexCoords	: TEXCOORD0;
	float3 vNormal		: TEXCOORD2;
	float3 vColor		: COLOR;
	float fAlpha		: TEXCOORD3;
	float3 vWPos		: TEXCOORD4;
};

VS_Out VS(VS_In input)
{
	VS_Out output;	

    float fWind = (1-input.vTexCoords.y) * sin(fTime + input.vPos.x + input.vPos.y);
    input.vPos.x += fWind;
	
	float3 vCenter = mul(input.vPos, matWorld);
	float3 vEye = vCenter - vCamPos;
	float3 vSide = normalize(cross(vEye, input.vNormal));

	vCenter += (input.vTexCoords.x - 0.5f) * vSide * input.vScale.x;
	vCenter += (1.5f - input.vTexCoords.y * 1.5f) * input.vNormal * input.vScale.y;	
	
	output.fAlpha = 1;	
	output.vPos = mul(float4(vCenter, 1), matViewProjection);
	output.vTexCoords = input.vTexCoords;
	output.vNormal = input.vNormal;
	output.vColor = input.vColor * CalculateDirectionalLighting(input.vNormal);
	output.vWPos = input.vPos;


	float3 vLightColors = 0;
	for (int i=0; i<iNumLights; i++)
		vLightColors += CalculatePointLighting(lights[i], input.vPos, input.vNormal);
	output.vColor.rgb *= 1 + vLightColors * 10;

	return output;
}

//---------------------------------------------------------------------------//
// Pixel Shaders
//---------------------------------------------------------------------------//
float4 PS(VS_Out input) : COLOR0
{ 
	float4 color = tex2D(s0, input.vTexCoords);
	color.rgb *= input.vColor;
	
	
	color.rgb = FogBlend(color.rgb, input.vWPos);
	
	color.rgb *= saturate(1 - tex2D(sCloudShadowMap, float2(-input.vWPos.z, input.vWPos.x) / 1500 + fTime / 40).a * 0.85);
	
	color.a *= input.fAlpha;

	return color;
}

//---------------------------------------------------------------------------//
// Techniques
//---------------------------------------------------------------------------//
technique Fast
{
	pass p0
    {         
		AlphaTestEnable = true;
		AlphaFunc = GreaterEqual;
		AlphaRef = 200;
    	VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 PS();
    }
}

technique Quality
{
	pass p0
    {         
		AlphaTestEnable = true;
		AlphaFunc = GreaterEqual;
		AlphaRef = 200;
    	VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 PS();        
    }
    pass p1
    {
		ZWriteEnable = false;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		AlphaFunc = less;
		VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 PS();
    }
}