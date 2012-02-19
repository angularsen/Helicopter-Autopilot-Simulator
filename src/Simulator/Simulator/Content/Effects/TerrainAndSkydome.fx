////////////////////////////////////////////////////////////////////
//
// Terrain/skydome shader
//
// Provides lighting, fog and texturing for a skydome model and
// any terrain surrounded by the skydome.
// The fog blends the terrain into the horizon and provides the 
// illusion of daylight, sunset and night.
//
// Requirements:
// TODO
////////////////////////////////////////////////////////////////////

// A vector containing the different heights used to divide the terrain in ground, mud, rock and snow areas.
// Values are stored from the lowest to the highest heights.
uniform extern float3 g_vecHeights;

// Textures used for the terrain.
uniform extern texture g_texGround;
uniform extern texture g_texMud;
uniform extern texture g_texRock;
uniform extern texture g_texSnow;


// Texture samplers.
sampler2D g_smpGround = sampler_state
{
    texture		= <g_texGround>;
    MinFilter	= LINEAR;
    MagFilter	= LINEAR;
    MipFilter	= LINEAR;
    AddressU	= WRAP;
    AddressV	= WRAP;
};

sampler2D g_smpMud = sampler_state 
{
    texture		= <g_texMud>;
    MinFilter	= LINEAR;
    MagFilter	= LINEAR;
    MipFilter	= LINEAR;
    AddressU	= WRAP;
    AddressV	= WRAP;
};

sampler2D g_smpRock = sampler_state 
{
    texture		= <g_texRock>;
    MinFilter	= LINEAR;
    MagFilter	= LINEAR;
    MipFilter	= LINEAR;
    AddressU	= WRAP;
    AddressV	= WRAP;
};

sampler2D g_smpSnow = sampler_state 
{
    texture		= <g_texSnow>;
    MinFilter	= LINEAR;
    MagFilter	= LINEAR;
    MipFilter	= LINEAR;
    AddressU	= WRAP;
    AddressV	= WRAP;
};


//////////////////////////////////////////////////////////////////////////////
//
// 		Simple Atmospheric Scattering
// 		Matthieu Laban - 2007
//
//		Slightly modified (non-skydome lighting) by Alex Urbano - 2008  
//
//////////////////////////////////////////////////////////////////////////////

float4x4 WorldIT;
float4x4 WorldViewProj;
float4x4 World;
float4x4 ViewInv;

struct appdata {
	float3 Position				: POSITION;
	float4 Normal				: NORMAL;
	float2 UV0					: TEXCOORD0;
  	half4 Tangent				: TANGENT0;
  	half4 Binormal				: BINORMAL0;
};

struct vertexOutput {
	float4 HPosition	 		: POSITION;
	float3 WorldLightVec		: TEXCOORD0;
  	float3 WorldNormal	    	: TEXCOORD1;
	float3 WorldEyeDirection	: TEXCOORD2;
  	float4 UV					: TEXCOORD3;
	float4 UV2					: TEXCOORD4;
	//float2 UV3					: TEXCOORD5;
  	half Fog 					: TEXCOORD6;
  	half2 Altitudes 			: TEXCOORD7;
  	float4 vDiffuse				: TEXCOORD5;	
  	
  	//	half3  WorldTangent			: TEXCOORD3;
//	half3  WorldBinorm			: TEXCOORD4;
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

texture DiffuseTexture;

sampler SurfSamplerDiffuse = sampler_state
{
	Texture = <DiffuseTexture>;
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
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




vertexOutput mainVS (appdata IN)
{
	vertexOutput OUT;
	float4 Po = float4(IN.Position.xyz,1);
  	float4 pos = mul(IN.Position, World);
	
	OUT.HPosition = mul( Po, WorldViewProj);
	
	OUT.WorldNormal = mul( IN.Normal, WorldIT).xyz;
	OUT.WorldLightVec = -LightDirection;
	
	float3 Pw = mul( Po, World).xyz;
	OUT.WorldEyeDirection = ViewInv[3].xyz - Pw;
	
	OUT.Altitudes.x = ViewInv[3].y;	

	// Get the height from the input position y component.
	float height = IN.Position.y;
	float diffuse = 1.0f;
	float specular = 0.0f;
	
	// Compute the ratio of each texture for this vertex and multiply it by the lighting.
	
	OUT.vDiffuse.x = diffuse * saturate((g_vecHeights.x - height) / 10.0f);
	OUT.vDiffuse.y = diffuse * saturate((g_vecHeights.y - height) / 10.0f);
	OUT.vDiffuse.z = diffuse * saturate((g_vecHeights.z - height) / 10.0f);
	OUT.vDiffuse.w = diffuse * saturate((height+10 - g_vecHeights.z) / 10.0f);

	
 	float dist = length(OUT.WorldEyeDirection);

	//OUT.Fog = (1.f/exp(pow(dist * fDensity, 2)));
	OUT.Fog = (1.f/exp(pow(dist * fDensity, 2)));
	OUT.Altitudes.y = Pw.y;

	// It is possible to use separate texture coordinates for each texture here
	// but for simplicity we use the same set of coordinates.
	// Packing four sets of UV coordinates in two TEXCOORD registers (can hold four floats each)
	OUT.UV = float4(IN.UV0.x, IN.UV0.y, IN.UV0.x, IN.UV0.y) / 3.0f;
	OUT.UV2 = float4(IN.UV0.x, IN.UV0.y, IN.UV0.x, IN.UV0.y) / 3.0f;

	return OUT;
}

float4 mainPS(vertexOutput IN) : COLOR0
{	
	// Sample the textures
	// Unpack texture coordinates as two sets of UVs are sent per TEXCOORD register
	float4 ground = tex2D(g_smpGround, IN.UV.xy);
	float4 mud = tex2D(g_smpMud, IN.UV.zw);
	float4 rock = tex2D(g_smpRock, IN.UV2.xy);
	float4 snow = tex2D(g_smpSnow, IN.UV2.zw);
	
	// Compute the output color.
	float4 DiffuseColor = ground * IN.vDiffuse.x + mud * IN.vDiffuse.y + rock * IN.vDiffuse.z + snow * IN.vDiffuse.w;
		
	// Calculate light/eye/normal vectors
	float eyeAlt = IN.Altitudes.x;
	float3 eyeVec = normalize(IN.WorldEyeDirection);
	float3 normal = normalize(IN.WorldNormal);
	float3 lightVec = normalize(IN.WorldLightVec);
	
	// Calculate the amount of direct light	
	float NdotL = max( dot( normal, -lightVec), 0);
	
	float4 colorDiffuse  = DiffuseColor * (NdotL * LightColor) + LightColorAmbient * DiffuseColor;
	float4 colorOutput = colorDiffuse;		
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
    

	// Make sun brighter for the skybox...
	// For all else, apply fog on output color
	if (isSkydome)
		colorOutput = fogColor + sunHighlight;
	else
		colorOutput = lerp(fogColor, colorOutput, IN.Fog);
		
	return colorOutput;
}


technique SkyDome 
{
	pass p0
	{
		CullMode = None;
		VertexShader = compile vs_2_0 mainVS();
		PixelShader = compile ps_2_b mainPS();
	}
}
