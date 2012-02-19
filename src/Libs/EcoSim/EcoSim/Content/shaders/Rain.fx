//---------------------------------------------------------------------------//
// Name		: Rain.fx
// Desc		: Rain particle effect using cylindrical billboards
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

#include "camera.inc"
#include "lighting.inc"

float4x4 matWorld;
				
float fDrawDist = 1000;
float fTime;		// time in the world (seconds)
float3 vVelocity;	// velocity of the particle
float3 vColor;

float3 vOrigin;		// min point of the cube area
float fWidth;		// width of the weather region (x-axis)
float fHeight;		// height of the weather region (y-axis)
float fLength;		// length of the weather region (z-axis)

//------------------------------- Texture Samplers --------------------------//

texture t0;
sampler s0 = sampler_state { 
	Texture		= (t0);
	MinFilter	= Linear;
	MagFilter	= Linear;
	MipFilter	= Linear;
	AddressU	= Wrap;
	AddressV	= Wrap;
};

//------------------------------- Vertex Formats ----------------------------//

struct VS_In
{
	float3 vPosition	: POSITION0;
	float3 vNormal		: NORMAL0;
	float3 vColor		: COLOR0;
	float2 vTexCoords	: TEXCOORD0;
	float2 vScale		: TEXCOORD1;
};

struct VS_Out
{
	float4 vPosition	: POSITION;
	float2 vTexCoords	: TEXCOORD0;
	float fAlpha		: TEXCOORD3;
};

//------------------------------ Vertex Shaders -----------------------------//

VS_Out VS(VS_In input)
{
	VS_Out output;
	
	// offset some rain particles' xz direction slightly
	vVelocity.xz /= input.vScale.y / 10;
	
	// move the position of the particle using its velocity and the current time
	float3 vDisplacement = fTime * vVelocity;
	input.vPosition.y = vOrigin.y + (fHeight + (input.vPosition.y + vDisplacement.y) % fHeight) % fHeight;
	
	if (vVelocity.x >= 0)
		input.vPosition.x = vOrigin.x + (input.vPosition.x + vDisplacement.x) % fWidth;
	else
		input.vPosition.x = vOrigin.x + fWidth - (input.vPosition.x - vDisplacement.x) % fWidth;
	if (vVelocity.z >= 0)
		input.vPosition.z = vOrigin.z + (input.vPosition.z + vDisplacement.z) % fLength;
	else
		input.vPosition.z = vOrigin.z + fLength - (input.vPosition.z - vDisplacement.z) % fLength;
	
	// calculate position of this vertex
	float3 vCenter = mul(input.vPosition, matWorld);
	float3 vEye = vCenter - vCamPos;
	float3 vRotAxis = normalize(-vVelocity);
	float3 vSide = normalize(cross(vEye, vRotAxis));	
	
	float3 vFinalPos = vCenter;
	vFinalPos += (input.vTexCoords.x - 0.5f) * vSide * input.vScale.x;
	vFinalPos += (1.5f - input.vTexCoords.y * 1.5f) * vRotAxis * input.vScale.y;	
	
	output.vPosition = mul(float4(vFinalPos, 1), matViewProjection);
	
	output.vTexCoords = input.vTexCoords;
	
	// create the appearance of some sheets of rain by fading particles in certain areas
	float fSheetAlpha = saturate(1.5f + cos(fTime + (input.vPosition.x + input.vPosition.z)/fDrawDist*2));
	
	// fade rain particles that are far away
	//output.fAlpha = 0.2f * saturate((1 - distance(vCamPos, input.vPosition) / fDrawDist)) * fSheetAlpha;
	output.fAlpha = 0.2f * saturate(1 - output.vPosition.z / fDrawDist) * fSheetAlpha;
	
	return output;
}

//------------------------------ Pixel Shader -------------------------------//

float4 PS(VS_Out input) : COLOR0
{
	float4 vPixelColor = tex2D(s0,input.vTexCoords);
	vPixelColor.rgb *= vColor * (1 + input.fAlpha) * 2;
	vPixelColor.a *= input.fAlpha;
	return vPixelColor;
}

//------------------------------- Techniques --------------------------------//

technique Snow
{
	pass p0
	{
		PointSpriteEnable = true;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		VertexShader = compile vs_1_1 VS();
		PixelShader = compile ps_2_0 PS();
	}
}


