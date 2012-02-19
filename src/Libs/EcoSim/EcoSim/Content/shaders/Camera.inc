//---------------------------------------------------------------------------//
// Name		: Camera.inc
// Desc		: Shared camera parameters
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

shared float4x4 matView;
shared float4x4 matProjection;
shared float4x4 matViewProjection;

shared float3 vCamPos;

shared float3 vFogColor = float3(0.8f,0.9f,1);
shared float fFogStart = 600;
shared float fMaxViewDist = 2000;

float DistanceBlend(float3 vPosition)
{
	return saturate((distance(vPosition, vCamPos) - fFogStart) / (fMaxViewDist + fFogStart));
}

float3 FogBlend(float3 vColor, float3 vPosition)
{
	return lerp(vColor, vFogColor, DistanceBlend(vPosition));
}


