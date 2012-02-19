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

#include "Camera.inc"
#include "Lighting.inc"

struct VS_In
{
	float4 vPosition	: POSITION;
};

struct VS_Out
{
	float4 vPosition	: POSITION;
	float4 vPos2D		: TEXCOORD0;
};

VS_Out VS(VS_In input)
{
	VS_Out output;
	
	output.vPosition = mul(input.vPosition, matSunWVP);
	output.vPos2D = output.vPosition;
	
	return output;
}

float4 PS(VS_Out input) : COLOR0
{
	// scale the depth to be in [0,1]
	float4 vDepthColor = 0;
	vDepthColor.r = input.vPos2D.z / input.vPos2D.w;
	return vDepthColor;
}

technique ShadowMap
{
    pass p0
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}
