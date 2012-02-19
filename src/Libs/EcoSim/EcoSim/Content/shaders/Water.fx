//--------------------------------------------------------------------------------
// Name		: Water.fx
// Desc		: Rippling reflective water (we don't need refractions).
// Author	: Anirudh S Shastry. Copyright (c) 2004.
//			: Modifications by Justin Stoecker, 2008-2009
//--------------------------------------------------------------------------------

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

float fTime	: Time;

// Custom variables (JS)
float fShoreBlendCoefficient = 35.0f;	// increases the sharpness with which the shore alpha changes
float fTerrainWidth;					// width of the heightmap in 3D  (along the z axis)
float fTerrainLength;					// length of the heightmap in 3D (along the x axis)						

//--------------------------------------------------
// Global variables
//--------------------------------------------------
float4x4 matWorld 			: World;
float4x4 matViewI 			: ViewInverse;

float fBumpHeight = 0.6f;
float2 vTextureScale = { 14.0f, 14.0f };
float2 vBumpSpeed = { -0.0f, 0.05f };
float fFresnelBias = 0.025f;
float fFresnelPower = 2.0f;
float fHDRMultiplier = 3.0f;
float4 vDeepColor = { 0.2f, 0.5f, 0.95f, 1.0f };
float4 vShallowColor = { 0.7f, 0.85f, 0.8f, 1.0f };
float4 vReflectionColor = { 1.0f, 1.0f, 1.0f, 1.0f };
float fReflectionAmount = 0.0f;
float fWaterAmount = 0.5f;
float fWaveAmp = 1.0f;
float fWaveFreq = 0.05f;
texture tNormalMap;
texture tEnvMap;

// we can use the heightmap texture for the water's alpha to give a smooth transition to shallow areas (JS)
texture tHeightmap;

sampler2D s0 = sampler_state {
	Texture = (tNormalMap);
	
	MipFilter 	= Linear;
	MinFilter 	= Linear;
	MagFilter 	= Linear;
	AddressU	= Wrap;
	AddressV	= Wrap;
};

samplerCUBE s1 = sampler_state {
	Texture = (tEnvMap);
	
	MinFilter 	= Linear;
	MagFilter 	= Linear;
	MipFilter 	= Linear;
	AddressU	= Wrap;
	AddressV	= Wrap;
};

// sampler for the heightmap texture (JS)
sampler2D s2 = sampler_state { 
	Texture = (tHeightmap);
	
	MinFilter	= Linear;
	MagFilter	= Linear;
	MipFilter	= Linear;
	AddressU	= Border;
	AddressV	= Border;
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

//--------------------------------------------------
// Vertex shader
//--------------------------------------------------
struct VSOUTPUT {
	float4 vPos				: POSITION;
	float2 vTex				: TEXCOORD0;
	float3 vTanToCube[3]	: TEXCOORD1;
	float2 vBump0			: TEXCOORD4;
	float2 vBump1			: TEXCOORD5;
	float2 vBump2			: TEXCOORD6;
	float3 vView			: TEXCOORD7;
	float3 vPos3D			: TEXCOORD8;
};

// Wave
struct Wave {
	float	fFreq;	// Frequency (2PI / Wavelength)
	float	fAmp;	// Amplitude
	float	fPhase;	// Phase (Speed * 2PI / Wavelength)
	float2	vDir;	// Direction
};

#define NUMWAVES	3
Wave Waves[NUMWAVES] = {
	{ 1.0f, 1.00f, 0.50f, float2( -1.0f, 0.0f ) },
	{ 2.0f, 0.50f, 1.30f, float2( -0.7f, 0.7f ) },
	{ .50f, .50f, 0.250f, float2( 0.2f, 0.1f ) },
};

// Wave functions
float EvaluateWave( Wave w, float2 vPos, float fTime ) {
	return w.fAmp * sin( dot( w.vDir, vPos ) * w.fFreq + fTime * w.fPhase );
}

float EvaluateWaveDifferential( Wave w, float2 vPos, float fTime ) {
	return w.fAmp * w.fFreq * cos( dot( w.vDir, vPos ) * w.fFreq + fTime * w.fPhase );
}

float EvaluateWaveSharp( Wave w, float2 vPos, float fTime, float fK )
{
  return w.fAmp * pow( sin( dot( w.vDir, vPos ) * w.fFreq + fTime * w.fPhase )* 0.5 + 0.5 , fK );
}

float EvaluateWaveSharpDifferential( Wave w, float2 vPos, float fTime, float fK )
{
  return fK * w.fFreq * w.fAmp * pow( sin( dot( w.vDir, vPos ) * w.fFreq + fTime * w.fPhase )* 0.5 + 0.5 , fK - 1 ) * cos( dot( w.vDir, vPos ) * w.fFreq + fTime * w.fPhase );
}

VSOUTPUT VS_Water( float4 inPos : POSITION, float3 inNor : NORMAL, float2 inTex : TEXCOORD0,
				   float3 inTan : TANGENT, float3 inBin : BINORMAL ) {
	VSOUTPUT OUT = (VSOUTPUT)0;
	
	// Generate some waves!
    Waves[0].fFreq 	= fWaveFreq;
    Waves[0].fAmp 	= fWaveAmp;

    Waves[1].fFreq 	= fWaveFreq * 2.0f;
    Waves[1].fAmp 	= fWaveAmp * 0.5f;
    
    Waves[2].fFreq 	= fWaveFreq * 3.0f;
    Waves[2].fAmp 	= fWaveAmp * 1.0f;

	// Sum up the waves
	inPos.y = 0.0f;
	float ddx = 0.0f, ddy = 0.0f;
	
	for( int i = 0; i < NUMWAVES; i++ ) {
    	inPos.y += EvaluateWave( Waves[i], inPos.xz, fTime );
    	float diff = EvaluateWaveDifferential( Waves[i], inPos.xz, fTime);
    	ddx += diff * Waves[i].vDir.x;
    	ddy += diff * Waves[i].vDir.y;
    }

	float4x4 matWVP = mul(matWorld, mul(matView, matProjection));

	// Output the position
	OUT.vPos = mul( inPos, matWVP );
	
	// Save the position for the pixel shader
	OUT.vPos3D = inPos;
	
	// Generate the normal map texture coordinates
	OUT.vTex = inTex * vTextureScale;

	fTime = fmod( fTime, 100.0 );
	OUT.vBump0 = inTex * vTextureScale + fTime * vBumpSpeed;
	OUT.vBump1 = inTex * vTextureScale * 2.0f + fTime * vBumpSpeed * 4.0;
	OUT.vBump2 = inTex * vTextureScale * 4.0f + fTime * vBumpSpeed * 8.0;

	// Compute tangent basis
    float3 vB = float3( 1,  ddx, 0 );
    float3 vT = float3( 0,  ddy, 1 );
    float3 vN = float3( -ddx, 1, -ddy );

	// Compute the tangent space to object space matrix
	float3x3 matTangent = float3x3( fBumpHeight * normalize( vT ),
									fBumpHeight * normalize( vB ),
									normalize( vN ) );
	
	OUT.vTanToCube[0] = mul( matTangent, matWorld[0].xyz );
	OUT.vTanToCube[1] = mul( matTangent, matWorld[1].xyz );
	OUT.vTanToCube[2] = mul( matTangent, matWorld[2].xyz );

	// Compute the world space vector
	float4 vWorldPos = mul( inPos, matWorld );
	OUT.vView = matViewI[3].xyz - vWorldPos;
	
	return OUT;
}

//--------------------------------------------------
// Pixel shader
//--------------------------------------------------
// Shore blending function (Justin Stoecker)
float EvaluateShoreBlend(float3 pos)
{	
	// scale the 3D position of the vertex to its location is relative to the heightmap
	float2 hmPos = float2(pos.x / fTerrainWidth, pos.z / fTerrainLength);
	
	// sample the amount of color at the position
	float colorAmount = tex2D(s2, hmPos).r;
	
	// return the inverse of the amount of color scaled by the blending coefficient
	return 1 - colorAmount * fShoreBlendCoefficient;
}

// Returns true if the position is within the terrain bounds
bool WithinWorldBounds(float3 pos)
{
	return (pos.z > 0 || pos.x < 0 || pos.z < -fTerrainWidth || pos.x > fTerrainLength);
}

float3 Refract( float3 vI, float3 vN, float fRefIndex, out bool fail )
{
	float fIdotN = dot( vI, vN );
	float k = 1 - fRefIndex * fRefIndex * ( 1 - fIdotN * fIdotN );
	fail = k < 0;
	return fRefIndex * vI - ( fRefIndex * fIdotN + sqrt(k) )* vN;
}

float4 PS_Water( VSOUTPUT IN) : COLOR0 {

	// Fetch the normal maps (with signed scaling)
    float4 t0 = tex2D( s0, IN.vBump0 ) * 2.0f - 1.0f;
    float4 t1 = tex2D( s0, IN.vBump1 ) * 2.0f - 1.0f;
    float4 t2 = tex2D( s0, IN.vBump2 ) * 2.0f - 1.0f;

    float3 vN = t0.xyz + t1.xyz + t2.xyz;   
    
	// Compute the tangent to world matrix
    float3x3 matTanToWorld;
    
    matTanToWorld[0] = IN.vTanToCube[0];
    matTanToWorld[1] = IN.vTanToCube[1];
    matTanToWorld[2] = IN.vTanToCube[2];
    
    float3 vWorldNormal = mul( matTanToWorld, vN );
    vWorldNormal = normalize( vWorldNormal );

	// Compute the reflection vector
    IN.vView = normalize( IN.vView );
    float3 vR = reflect( -IN.vView, vWorldNormal );
	
	// Sample the cube map
    float4 vReflect = texCUBE( s1, vR.zyx );    
    vReflect = texCUBE( s1, vR );
    
    // Exaggerate the HDR effect
    vReflect.rgb *= ( 1.0 + vReflect.a * fHDRMultiplier );

	// Compute the Fresnel term
    float fFacing  = 1.0 - max( dot( IN.vView, vWorldNormal ), 0 );
    float fFresnel = fFresnelBias + ( 1.0 - fFresnelBias ) * pow( fFacing, fFresnelPower);

	// Compute the final water color
	vReflectionColor.rgb = vSunColor;
    float4 vWaterColor = lerp( vDeepColor, vShallowColor, fFacing );
    
    // Change alpha using heightmap such that shallower areas are more transparent
    if (fShoreBlendCoefficient > 0 && WithinWorldBounds(IN.vPos3D))
    {
		float blend = EvaluateShoreBlend(IN.vPos3D);
		vWaterColor.a = blend;
		vWaterColor.rgb = lerp(2,vWaterColor.rgb,blend);
	}
	
	// fog
	float dist = distance(IN.vPos3D, vCamPos);
	float blend = 1;
	if (dist > fFogStart)
		blend = (fMaxViewDist - dist) / (fMaxViewDist - fFogStart);
		
	float4 vFinalColor = vWaterColor * fWaterAmount + vReflect * vReflectionColor * fReflectionAmount * fFresnel;

	// apply lighting and sun coloration
	vFinalColor.rgb *= CalculateDirectionalLighting(vWorldNormal.xyz);
	
	// cloud shadows
	vFinalColor.rgb *= saturate(1 - tex2D(sCloudShadowMap, float2(-IN.vPos3D.z, IN.vPos3D.x) / 1500 + fTime / 10).a * 0.55);
	
	// sun specular
	if (vSunVector.y > 0)
	{
		float3 vEye = normalize(IN.vPos3D - vCamPos);
		vFinalColor.rgb += CalculateSpecular(vSunVector, vWorldNormal, vEye, 200) * vSunColor * saturate(2 - overcast) * saturate(lerp(0,1, vSunVector.y / 0.1f));
	}
	
	float3 vLightColors = 0;
	for (int i=0; i<iNumLights; i++)
		vLightColors += CalculatePointLighting(lights[i], IN.vPos3D, float3(0,1,0));
	vFinalColor.rgb *= 1 + vLightColors * 10;
	
	//return lerp(vFogColor * ambientLight, vFinalColor, blend);
	//return lerp(float4(vFogColor,1), vFinalColor, blend);
	return float4(FogBlend(vFinalColor.rgb, IN.vPos3D),blend*vFinalColor.a);
}

//--------------------------------------------------
// Techniques
//--------------------------------------------------
technique techDefault {
	pass p0 {
		VertexShader	= compile vs_3_0 VS_Water();
		PixelShader		= compile ps_3_0 PS_Water();
	}
}
