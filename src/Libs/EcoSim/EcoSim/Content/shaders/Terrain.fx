//---------------------------------------------------------------------------//
// Name		: Terrain.fx
// Desc		: Used for texturing terrain meshes
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

// cursor
float3 vCursorPos;
float fCursorTexWidth = 4;
bool bDrawCursor = false;

// detail blending
bool bDetailEnabled;
bool bBumpEnabled = true;
float fBlendDist =  0.985f;		// depth at which high detail textures are used
float fBlendWidth = 0.015f;		// amount of depth to blend
float fDetailScale = 2;			// high detail texture scale

// material properties
float4 vSurfaceReflectivity;	// reflectivity of the textures (used for specular highlights)

//------------------------------- Texture Samplers --------------------------//

texture tShore;
sampler sShore = sampler_state { 
	Texture		= (tShore);
	MinFilter	= Linear;
	MagFilter	= Linear;
	MipFilter	= Linear;
	AddressU	= Wrap;
	AddressV	= Wrap;
};

texture tShoreNormals;
sampler sShoreNormals = sampler_state { 
	Texture		= (tShoreNormals);
	MinFilter	= Linear;
	MagFilter	= Linear;
	MipFilter	= Linear;
	AddressU	= Wrap;
	AddressV	= Wrap;
};

texture tRock;
sampler sRock = sampler_state { 
	Texture		= (tRock);
	MinFilter	= Linear;
	MagFilter	= Linear;
	MipFilter	= Linear;
	AddressU	= Wrap;
	AddressV	= Wrap;
};

texture tRockNormals;
sampler sRockNormals = sampler_state { 
	Texture		= (tRockNormals);
	MinFilter	= Linear;
	MagFilter	= Linear;
	MipFilter	= Linear;
	AddressU	= Wrap;
	AddressV	= Wrap;
};

texture tPlains1;
sampler sPlains1 = sampler_state { 
	Texture		= (tPlains1);
	MinFilter	= Linear;
	MagFilter	= Linear;
	MipFilter	= Linear;
	AddressU	= Wrap;
	AddressV	= Wrap;
};

texture tPlains1Normals;
sampler sPlains1Normals = sampler_state { 
	Texture		= (tPlains1Normals);
	MinFilter	= Linear;
	MagFilter	= Linear;
	MipFilter	= Linear;
	AddressU	= Wrap;
	AddressV	= Wrap;
};

texture tPlains2;
sampler sPlains2 = sampler_state { 
	Texture		= (tPlains2);
	MinFilter	= Linear;
	MagFilter	= Linear;
	MipFilter	= Linear;
	AddressU	= Wrap;
	AddressV	= Wrap;
};

texture tPlains2Normals;
sampler sPlains2Normals = sampler_state { 
	Texture		= (tPlains2Normals);
	MinFilter	= Linear;
	MagFilter	= Linear;
	MipFilter	= Linear;
	AddressU	= Wrap;
	AddressV	= Wrap;
};

texture tCursor;
sampler sCursor = sampler_state { 
	Texture		= (tCursor);
	MinFilter	= Linear;
	MagFilter	= Linear;
	MipFilter	= Linear;
	AddressU	= Clamp;
	AddressV	= Clamp;
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
//----------------------------- Single-Textured -----------------------------//
//---------------------------------------------------------------------------// 

struct VS_Seafloor_In
{
	float4 vPosition	: POSITION0;
	float4 vColor		: COLOR0;
	float3 vNormal		: NORMAL0;
	float2 vTexCoords	: TEXCOORD0;
};

struct VS_Seafloor_Out
{
	float4 vPosition	: POSITION0;
	float2 vTexCoords	: TEXCOORD0;
	float3 vLightColor	: TEXCOORD1;
	float4 vWPosition	: TEXCOORD2;
};

//------------------------------ Vertex Shader ------------------------------//

VS_Seafloor_Out VS_Seafloor(VS_Seafloor_In input)
{
	VS_Seafloor_Out output = (VS_Seafloor_Out)0;
	
	float4x4 matVP = mul(matView, matProjection);
	float4x4 matWVP = mul(matWorld, matVP);
	
	output.vPosition = mul(input.vPosition, matWVP);
	output.vTexCoords = input.vTexCoords;
	output.vLightColor = CalculateDirectionalLighting(input.vNormal);
	output.vWPosition = input.vPosition;
	
	return output;
}

//------------------------------ Pixel Shader -------------------------------//

float4 PS_Seafloor(VS_Seafloor_Out input) : COLOR0
{
	float4 vPixelColor = tex2D(sShore, input.vTexCoords / 2) / 4;
	vPixelColor.rgb *= input.vLightColor;
	
	float fPixelDistance = distance(input.vWPosition, vCamPos);
	float fAlpha = 1;
	if (fPixelDistance > fFogStart)
		fAlpha = (fMaxViewDist - fPixelDistance) / (fMaxViewDist - fFogStart);
		
	vPixelColor.a = fAlpha;
	
	return vPixelColor;
}

//------------------------------- Technique ---------------------------------//

technique Seafloor
{
	pass p0
	{
		VertexShader = compile vs_1_1 VS_Seafloor();
		PixelShader = compile ps_2_0 PS_Seafloor();
	}
}

//---------------------------------------------------------------------------//
//------------------------------ Multi-Textured -----------------------------//
//---------------------------------------------------------------------------//

struct VS_Terrain_In
{
    float4 vPosition	: POSITION0;
    float3 vNormal		: NORMAL;
    float2 vTexCoords	: TEXCOORD0;
    float4 vTexWeights	: TEXCOORD1;
    float3 vTangent		: TANGENT;
    float3 vBinormal	: BINORMAL;
};

struct VS_Terrain_Out
{
    float4 vPosition	: POSITION0;
    float3 vNormal		: TEXCOORD0;
    float2 vTexCoords	: TEXCOORD1;
    float4 vTexWeights	: TEXCOORD2;
    float3 vWPosition	: TEXCOORD3;
    float  fDepth		: TEXCOORD4;
    float3x3 matTanToWorld	: TEXCOORD5;
    float4 vPosLightView	: TEXCOORD8;		// position transformed to shadowmap coordinates
};

//------------------------------ Vertex Shader ------------------------------//

VS_Terrain_Out VS_Terrain(VS_Terrain_In input)
{
    VS_Terrain_Out output = (VS_Terrain_Out)0;

	float4x4 matVP = mul(matView, matProjection);
	float4x4 matWVP = mul(matWorld, matVP);
    
    output.vPosition = mul(input.vPosition, matWVP);
	
	// transform input position to the shadowmap space
	output.vPosLightView = mul(input.vPosition, matSunWVP);
	output.vPosLightView /= output.vPosLightView.w;
    
    
    output.vNormal = mul(normalize(input.vNormal), matWorld);
    output.vTexCoords = input.vTexCoords;
    output.vTexWeights = input.vTexWeights;
    output.vWPosition = input.vPosition;
    if (bDetailEnabled)
		output.fDepth = output.vPosition.z / output.vPosition.w;
	
	// calculate tangent to world space matrix
	float3x3 matTanToObj;
	matTanToObj[0] = input.vTangent;
	matTanToObj[1] = input.vBinormal;
	matTanToObj[2] = input.vNormal;
	output.matTanToWorld = mul(matTanToObj, matWorld); 
	


    return output;
}

//------------------------------ Pixel Shader -------------------------------//

float3 SampleTextures(float2 vTexCoords, float4 vTexWeights)
{
	float3 vColor = float3(0,0,0);
	if (vTexWeights.x > 0)
		vColor += tex2D(sShore, vTexCoords) * vTexWeights.x;
	if (vTexWeights.y > 0)
		vColor += tex2D(sPlains1, vTexCoords) * vTexWeights.y;
	if (vTexWeights.z > 0)
		vColor += tex2D(sPlains2, vTexCoords) * vTexWeights.z;
    if (vTexWeights.w > 0)
		vColor += tex2D(sRock, vTexCoords) * vTexWeights.w;
    
    return vColor;
}

float3 SampleNormals(float2 vTexCoords, float4 vTexWeights)
{
	float3 vColor = float3(0,0,0);
	if (vTexWeights.x > 0)
		vColor += tex2D(sShoreNormals, vTexCoords) * vTexWeights.x;
	if (vTexWeights.y > 0)
		vColor +=  tex2D(sPlains1Normals, vTexCoords) * vTexWeights.y;
	if (vTexWeights.z > 0)
		vColor +=  tex2D(sPlains2Normals, vTexCoords) * vTexWeights.z;
	if (vTexWeights.w > 0)
		vColor +=  tex2D(sRockNormals, vTexCoords) * vTexWeights.w;
	
	return vColor;
}

float4 PS_Terrain(VS_Terrain_Out input) : COLOR0
{
	float4 vPixelColor;
	
	input.vNormal = normalize(input.vNormal);
	
	// sample normal and diffuse textures
	vPixelColor.rgb = SampleTextures(input.vTexCoords, input.vTexWeights);
	float3 vBump = SampleNormals(input.vTexCoords, input.vTexWeights);
	
	// scale and interpolate textures if detail texturing is enabled
	if (bDetailEnabled)
	{
		float fBlendFactor = clamp((input.fDepth - fBlendDist) / fBlendWidth, 0, 1);
		float3 vNearColor = SampleTextures(input.vTexCoords * fDetailScale, input.vTexWeights);
		vPixelColor.rgb = lerp(vNearColor, vPixelColor.rgb, fBlendFactor);
		
		float3 vBumpDetailed = SampleNormals(input.vTexCoords * fDetailScale, input.vTexWeights);
		vBump = lerp(vBumpDetailed, vBump, fBlendFactor);
	}
	
	// adjust surface normal using the normal map
	float3 vNormalW;
	if (bBumpEnabled) {
		float3 vNormalT = (vBump - 0.5f) * 2.0f;
		vNormalW = normalize(mul(vNormalT, input.matTanToWorld));
	} else {
		vNormalW = input.vNormal;
	}
	
	// calculate diffuse lighting and shadowing
	float fDiffuse;
	if (bShadowsEnabled)
		fDiffuse = CalculateSunDiffuse(input.vPosLightView, vNormalW);
	else
		fDiffuse = saturate(dot(vSunVector, vNormalW));
	
	fDiffuse *= 1 - tex2D(sCloudShadowMap, float2(-input.vWPosition.z, input.vWPosition.x) / 1500 + fTime).a;
	vPixelColor.rgb *= fAmbient * vAmbientColor + fDiffuse * vSunColor * fSunIntensity / overcast;
	
	// sun speculars
	if (vSunVector.y > 0)
	{
		//float3 vEye = normalize(input.vWPosition - vCamPos);
		//float fSpecularScale = 2 * fDiffuse * length(vSurfaceReflectivity * input.vTexWeights) * saturate(2 - overcast) * saturate(lerp(0,1, vSunVector.y / 0.1f));
		//vPixelColor.rgb += CalculateSpecular(-vSunVector, vNormalW, vEye, 300) * vSunColor * fSpecularScale;
	}
	
	// calculate point lighting
	float3 vLightColors = 0;
	for (int i=0; i<iNumLights; i++)
		vLightColors += CalculatePointLighting(lights[i], input.vWPosition, vNormalW);
	vPixelColor.rgb *= 1 + vLightColors * 10;
	
	// draw the cursor if it is enabled
	if (bDrawCursor) {
		float2 vCursorCoords = (vCursorPos.xz-input.vWPosition.xz) / fCursorTexWidth / 2 + 0.5f;
		vPixelColor.rg *= 1+tex2D(sCursor,vCursorCoords).r*5;
	}
	
	float height = input.vWPosition.y;
	if (height < 5)
		vPixelColor.a = lerp(0, 1, height / 5);
	else
		vPixelColor.a = 1;
		
	vPixelColor.rgb = FogBlend(vPixelColor.rgb, input.vWPosition);
	
    return vPixelColor;
}

//------------------------------- Technique ---------------------------------//

technique MultiTextured
{
    pass p0
    {
        VertexShader = compile vs_3_0 VS_Terrain();
        PixelShader = compile ps_3_0 PS_Terrain();
    }
}


