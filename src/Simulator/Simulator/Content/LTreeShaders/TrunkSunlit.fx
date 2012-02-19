/*
 * Copyright (c) 2007-2009 Asger Feldthaus
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

//----------------------------------------------------------------------------
// Shader for Tree trunks
//
// Hardware skinning with support for two directional lights (vertex-based).
//----------------------------------------------------------------------------

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
// 
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.



#define MAXBONES 20

float4x4 WorldIT;
float4x4 WorldViewProj;
float4x4 World;
float4x4 ViewInv;

struct VertexShaderOutput {
    float4 Position	 		    : POSITION;
    float3 WorldLightVec		: TEXCOORD0;
    float3 WorldNormal	    	: TEXCOORD1;
    float3 WorldEyeDirection	: TEXCOORD2;
    half3  WorldTangent			: TEXCOORD3;
    half3  WorldBinorm			: TEXCOORD4;
    float2 UV					: TEXCOORD5;
    half Fog 					: TEXCOORD6;
    half2 Altitudes 			: TEXCOORD7; 
};

struct VertexShaderInput
{
    float4 Position				: POSITION;
    float3 Normal				: NORMAL;
    half4 Tangent				: TANGENT0;
    half4 Binormal				: BINORMAL0;
    float2 UV					: TEXCOORD0;
    int2 BoneIndex              : TEXCOORD1;
};


float4 LightDirection = {100.0f, 100.0f, 100.0f, 1.0f};
float4 LightColor = {1.0f, 1.0f, 1.0f, 1.0f};
float4 LightColorAmbient = {0.0f, 0.0f, 0.0f, 1.0f};

float4 FogColor = {1.0f, 1.0f, 1.0f, 1.0f};

float fDensity ;

bool isSkydome;

float SunLightness = 0.2; 

float sunRadiusAttenuation = 256;

float largeSunLightness = 0.2;
float largeSunRadiusAttenuation = 3;
float dayToSunsetSharpness = 1.5;
float hazeTopAltitude = 20; 

// Should be:  InverseReferenceFrame * AbsoluteBoneTransform
float4x4 Bones[MAXBONES];

texture Texture;

sampler TextureSampler = sampler_state
{
	Texture = (Texture);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

texture SkyTextureNight;

sampler SurfSamplerSkyTextureNight = sampler_state
{
	Texture = <SkyTextureNight>;
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
	AddressU = mirror; 
	AddressV = mirror;
};

texture SkyTextureSunset;

sampler SurfSamplerSkyTextureSunset = sampler_state
{
	Texture = <SkyTextureSunset>;
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
	AddressU = mirror; 
	AddressV = mirror;
};

texture SkyTextureDay;

sampler SurfSamplerSkyTextureDay = sampler_state
{
	Texture = <SkyTextureDay>;
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
	AddressU = mirror; 
	AddressV = mirror;
};


VertexShaderOutput VertexShaderFunction(VertexShaderInput IN)
{
    VertexShaderOutput OUT;
	
    // Sunlight code
    float4 Po = mul(IN.Position, Bones[IN.BoneIndex.x]);
    float3 normal = mul(IN.Normal, Bones[IN.BoneIndex.x]);
    
    float3 Pw = mul( Po, World).xyz;
    
    // Invert but keep Y, not sure why it's needed but fog color was approx. "180 degrees wrong" so the
    // objects did not blend into the skydome background but rather to another skydome color
    float3 eyeDirection = (ViewInv[3].xyz - Pw);   
    eyeDirection.x *= -1;
    eyeDirection.z *= -1;

    float dist = length(eyeDirection);

    OUT.Position = mul( Po, WorldViewProj);
    OUT.WorldNormal = mul(normal, WorldIT).xyz;
    OUT.WorldTangent = mul(IN.Tangent, WorldIT).xyz;
    OUT.WorldBinorm = mul(IN.Binormal, WorldIT).xyz;
    OUT.WorldLightVec = LightDirection;
    OUT.WorldEyeDirection = eyeDirection;
    OUT.Fog = (1.f/exp(pow(dist * fDensity, 2)));
    OUT.Altitudes.x = ViewInv[3].y;	
    OUT.Altitudes.y = Pw.y;
    OUT.UV = IN.UV;

    return OUT;
}

float4 PixelShaderFunction(VertexShaderOutput IN) : COLOR0
{
    // Sunlight code
    float4 colorOutput = float4(0,0,0,1);
    float4 DiffuseColor = tex2D(TextureSampler, IN.UV);
    float4 colorAmbient = DiffuseColor;
		
    //// Calculate light/eye/normal vectors
    float eyeAlt = IN.Altitudes.x;
    float3 eyeVec = normalize(IN.WorldEyeDirection);
    float3 normal = normalize(IN.WorldNormal);
    float3 lightVec = normalize(IN.WorldLightVec);
	
    //// Calculate the amount of direct light	
    float NdotL = max( dot( normal, -lightVec), 0);
	
    float4 colorDiffuse  = DiffuseColor * (NdotL * LightColor);// + LightColorAmbient * DiffuseColor;
    colorOutput += colorDiffuse;		
    colorOutput.a = 1.0f;

    // Calculate sun highlight...	
    float sunHighlight = pow(max(0, dot(lightVec, -eyeVec)), sunRadiusAttenuation) * SunLightness;	
    // Calculate a wider sun highlight 
    float largeSunHighlight = pow(max(0, dot(lightVec, -eyeVec)), largeSunRadiusAttenuation) * largeSunLightness;
	
    // Calculate 2D angle between pixel to eye and sun to eye
    float3 flatLightVec = normalize(float3(lightVec.x, 0, lightVec.z));
    float3 flatEyeVec = normalize(float3(eyeVec.x, 0, eyeVec.z));
    float diff = dot(flatLightVec, -flatEyeVec);	
	
    // Based on camera altitude, the haze will look different and will be lower on the horizon.
    // This is simulated by raising YAngle to a certain power based on the difference between the
    // haze top and camera altitude. 
    // This modification of the angle will show more blue sky above the haze with a sharper separation.
    // Lerp between 0.25 and 1.25
    float val = lerp(0.25, 1.25, min(1, hazeTopAltitude / max(0.0001, eyeAlt)));
    // Apply the power to sharpen the edge between haze and blue sky
    float YAngle = pow(max(0, -eyeVec.y), val);	
	
    // Fetch the 3 colors we need based on YAngle and angle from eye vector to the sun
    float4 fogColorDay = tex2D( SurfSamplerSkyTextureDay, float2( 1 - (diff + 1) * 0.5, 1-YAngle));
    float4 fogColorSunset = tex2D( SurfSamplerSkyTextureSunset, float2( 1 - (diff + 1) * 0.5, 1-YAngle));
    float4 fogColorNight = tex2D( SurfSamplerSkyTextureNight, float2( 1 - (diff + 1) * 0.5, 1-YAngle));
	
    float4 fogColor;
	
    // If the light is above the horizon, then interpolate between day and sunset
    // Otherwise between sunset and night
    if (lightVec.y > 0)
    {
        // Transition is sharpened with dayToSunsetSharpness to make a more realistic cut 
        // between day and sunset instead of a linear transition
        fogColor = lerp(fogColorDay, fogColorSunset, min(1, pow(1 - lightVec.y, dayToSunsetSharpness)));
    }
    else
    {
        // Slightly different scheme for sunset/night.
        fogColor = lerp(fogColorSunset, fogColorNight, min(1, -lightVec.y * 4));
    }
	
    // Add sun highlights
    fogColor += sunHighlight + largeSunHighlight;
    
    // Apply fog on OUT color
    colorOutput = lerp(fogColor, colorOutput, IN.Fog);
	
    // Make sun brighter for the skybox...
    if (isSkydome)
        colorOutput = fogColor + sunHighlight;
		
    return colorOutput;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();

        AlphaBlendEnable = false;
        AlphaTestEnable = false;
        ZEnable = true;
        ZWriteEnable = true;

        CullMode = None;
    }
}
