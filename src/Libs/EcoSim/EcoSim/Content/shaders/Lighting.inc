//---------------------------------------------------------------------------//
// Name		: Lighting.inc
// Desc		: Shared lighting parameters and methods
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

struct PointLight 
{
    float3 vColor;
    float3 vPosition;
    float fFalloff;
    float fRange;
};

// point lights
shared PointLight lights[8];
shared int iNumLights = 0;

// directional light from the sun
shared float3 vSunColor;
shared float3 vSunVector;
shared float fSunIntensity;
shared float fDepthBias = 0.001f;

// terrain shadow map
shared bool bShadowsEnabled = true;
shared float4x4 matSunWVP;
shared texture tShadowmap;
sampler sShadowmap = sampler_state { 
	Texture		= (tShadowmap);
	MinFilter	= Linear;
	MagFilter	= Linear;
	MipFilter	= Linear;
	AddressU	= Clamp;
	AddressV	= Clamp;
};

shared float overcast = 1;	// overcast skies

// ambient lighting
shared float3 vAmbientColor;
shared float fAmbient;

float CalculateSunDiffuse(float4 vPosLightView, float3 vNormal)
{
	// map coordinates from [-1,1] to [0,1]
	float2 vSMTexCoords;
	vSMTexCoords.x = vPosLightView.x / 2.0f + 0.5f;
	vSMTexCoords.y = -vPosLightView.y / 2.0f + 0.5f;
	
	// sample the shadowmap and calculate the pixel depth
	float fDiffuse = 0;
	float fSMDepth = tex2D(sShadowmap, vSMTexCoords).r;
	float fPixelDepth = vPosLightView.z;

	// compare the depth stored in the shadowmap with the pixel's actual depth
	if (fPixelDepth - fDepthBias <= fSMDepth) {
		// pixel is lit, so apply normal diffuse lighting and cloud shadowing
		fDiffuse =  saturate(dot(vSunVector, vNormal)) / overcast;
	}
       
    return fDiffuse;
}

// Calculates specular highlight using the direction of a light, a surface's normal, and the camera eye vector
float CalculateSpecular(float3 vLightDir, float3 vNormal, float3 vEye, float fPower)
{
	float3 vReflect = -reflect(vLightDir, vNormal);
	return pow(dot(vReflect, vEye), fPower);
}

float3 CalculateDirectionalLighting(float3 vNormal)
{
	float fDiffuse = saturate(dot(vSunVector, vNormal));
	return fAmbient * vAmbientColor + fDiffuse * vSunColor * fSunIntensity / overcast;
}

float3 CalculatePointLighting(PointLight light, float3 vPosition, float3 vNormal)
{
	float3 vLightDir = light.vPosition - vPosition;
	float fDistance = length(vLightDir);
	float fTotalIntensity = pow(saturate((light.fRange - fDistance) / light.fRange), light.fFalloff);
	
	// diffuse
	float fDiffuse = saturate(dot(vNormal, vLightDir));
	float3 vDiffuseColor = fDiffuse * light.vColor;
	
	return vDiffuseColor * fTotalIntensity;
} 