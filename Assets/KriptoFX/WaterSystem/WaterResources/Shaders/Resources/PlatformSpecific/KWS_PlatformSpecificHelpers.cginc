//#define ENVIRO_FOG
//#define ENVIRO_3_FOG
//#define AZURE_FOG
//#define WEATHER_MAKER
//#define ATMOSPHERIC_HEIGHT_FOG
//#define VOLUMETRIC_FOG_AND_MIST
//#define COZY_FOG

//ATMOSPHERIC_HEIGHT_FOG also need to change the "Queue" = "Transparent-1"      -> "Queue" = "Transparent+2"
//VOLUMETRIC_FOG_AND_MIST also need to enable "Water->Rendering->DrawToDepth"

#define _FrustumCameraPlanes unity_CameraWorldClipPlanes

#ifdef KWS_COMPUTE
	#undef ENVIRO_3_FOG
#endif

//------------------  unity includes   ----------------------------------------------------------------

#ifndef HLSL_SUPPORT_INCLUDED
	#include "HLSLSupport.cginc"
#endif

#ifndef UNITY_CG_INCLUDED
	#include "UnityCG.cginc"
#endif

#ifndef UNITY_LIGHTING_COMMON_INCLUDED
	#include "UnityLightingCommon.cginc"
#endif
//-------------------------------------------------------------------------------------------------------




//------------------  thid party assets  ----------------------------------------------------------------

#if defined(ENVIRO_FOG)
	#include "Assets/Third-party assets/Enviro - Sky and Weather/Core/Resources/Shaders/Core/EnviroFogCore.cginc"
#endif

#if defined(ENVIRO_3_FOG)
	#include "Assets/Third-party assets/Enviro 3 - Sky and Weather/Resources/Shader/Includes/FogInclude.cginc"
#endif

#if defined(AZURE_FOG)
	#include "Assets/Third-party assets/Azure[Sky] Dynamic Skybox/Shaders/Transparent/AzureFogCore.cginc"
#endif

#if defined(WEATHER_MAKER)
	#include "Assets/WeatherMaker/Prefab/Shaders/WeatherMakerFogExternalShaderInclude.cginc"
#endif

#if defined(ATMOSPHERIC_HEIGHT_FOG)
	#include "Assets/Third-party assets/BOXOPHOBIC/Atmospheric Height Fog/Core/Includes/AtmosphericHeightFog.cginc"
#endif

#if defined(VOLUMETRIC_FOG_AND_MIST)
	#include "Assets/Third-party assets/VolumetricFog/Resources/Shaders/VolumetricFogOverlayVF.cginc"
#endif

#if defined(COZY_FOG)
	#include "Assets/Third-party assets/Distant Lands/Cozy Weather/Contents/Materials/Shaders/Includes/StylizedFogIncludes.cginc"
#endif

//-------------------------------------------------------------------------------------------------------


#ifndef KWS_WATER_VARIABLES
	#include "..\Common\KWS_WaterVariables.cginc"
#endif

DECLARE_TEXTURE(_CameraDepthTexture);
DECLARE_TEXTURE(_CameraOpaqueTexture);

SamplerState sampler_CameraDepthTexture;
float4 _CameraDepthTexture_TexelSize;
float4 _CameraDepthTexture_ST;

SamplerState sampler_CameraOpaqueTexture;
float4 _CameraOpaqueTexture_TexelSize;
float4 _CameraOpaqueTexture_RTHandleScale;

TextureCube KWS_SkyTexture;

inline void OverrideUnityInstanceMatrixes(float3 position, float3 size)
{	
	#ifndef KWS_COMPUTE
		position.y += 0.001;
		unity_ObjectToWorld._11_21_31_41 = float4(size.x, 0,  0, 0);
		unity_ObjectToWorld._12_22_32_42 = float4(0, size.y, 0, 0);
		unity_ObjectToWorld._13_23_33_43 = float4(0, 0, size.z, 0);
		unity_ObjectToWorld._14_24_34_44 = float4(position.xyz, 1);

		unity_WorldToObject = unity_ObjectToWorld;
		unity_WorldToObject._14_24_34 *= -1;
		unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
	#endif
}

inline float4 ObjectToClipPos(float4 vertex)
{
	return UnityObjectToClipPos(vertex);
}

inline float2 GetTriangleUVScaled(uint vertexID)
{
	#if UNITY_UV_STARTS_AT_TOP
		return float2((vertexID << 1) & 2, 1.0 - (vertexID & 2));
	#else
		return float2((vertexID << 1) & 2, vertexID & 2);
	#endif
}


inline float4 GetTriangleVertexPosition(uint vertexID, float z = UNITY_NEAR_CLIP_VALUE)
{
	float2 uv = float2((vertexID << 1) & 2, vertexID & 2);
	return float4(uv * 2.0 - 1.0, z, 1.0);
}

inline float2 GetNormalizedRTHandleUV(float2 screenUV)
{
	return screenUV;
}

inline float3 LocalToWorldPos(float3 localPos)
{
	return mul(UNITY_MATRIX_M, float4(localPos, 1));
}

inline float3 WorldToLocalPos(float3 worldPos)
{
	return mul(unity_WorldToObject, float4(worldPos, 1));
}

inline float3 WorldToLocalPosWithoutTranslation(float3 worldPos)
{
	return mul((float3x3)unity_WorldToObject, worldPos);
}

inline float3 GetCameraRelativePosition(float3 worldPos)
{
	return worldPos;
}

inline float3 GetAbsoluteWorldSpacePos()
{
	return UNITY_MATRIX_I_V._m03_m13_m23;
	//return _WorldSpaceCameraPos.xyz; //cause shader error in 'Hidden/KriptoFX/KWS/VolumetricLighting': Program 'frag', error X8000: D3D11 Internal Compiler Error: Invalid Bytecode:
	//source register relative index temp register component 1 in r7 uninitialized. Opcode #61 (count is 1-based) at line 15 (on vulkan)

}

inline float3 GetWorldSpaceViewDirNorm(float3 worldPos)
{
	return normalize(_WorldSpaceCameraPos.xyz - worldPos.xyz);
}

inline float3 GetWorldSpaceNormal(float3 normal)
{
	return normalize(mul((float3x3)unity_ObjectToWorld, normal)).xyz;
}

inline float GetWorldToCameraDistance(float3 worldPos)
{
	return length(_WorldSpaceCameraPos.xyz - worldPos.xyz);
}


float3 GetWorldSpacePositionFromDepth(float2 uv, float deviceDepth)
{
	float4 positionCS = float4(uv * 2.0 - 1.0, deviceDepth, 1.0);
	#if UNITY_UV_STARTS_AT_TOP
		positionCS.y = -positionCS.y;
	#endif
	
	float4 hpositionWS = mul(KWS_MATRIX_I_VP, positionCS);
	return hpositionWS.xyz / hpositionWS.w;
}

inline float GetSceneDepth(float2 uv)
{
	return SAMPLE_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, uv, 0);
}

inline float4 GetSceneDepthGather(float2 uv)
{
	return SAMPLE_TEXTURE_GATHER(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
}

inline float3 GetAmbientColor()
{
	return KWS_AmbientColor;
}

inline float2 GetSceneColorNormalizedUV(float2 uv)
{
	#if defined(UNITY_STEREO_INSTANCING_ENABLED)
		return clamp(uv, 0.001, 0.999) * _CameraOpaqueTexture_RTHandleScale.xy;
	#else
		return clamp(uv, 0.001, 0.999) * _CameraOpaqueTexture_RTHandleScale.xy;
	#endif
}

inline half3 GetSceneColor(float2 uv)
{
	return SAMPLE_TEXTURE_LOD(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, GetSceneColorNormalizedUV(uv), 0);
}

inline half3 GetSceneColorPoint(float2 uv)
{
	return SAMPLE_TEXTURE_LOD(_CameraOpaqueTexture, sampler_point_clamp, GetSceneColorNormalizedUV(uv), 0);
}


inline half3 GetSceneColorWithDispersion(float2 uv, float dispersionStrength)
{
	half3 refraction;
	refraction.r = GetSceneColor(uv - _CameraOpaqueTexture_TexelSize.xy * dispersionStrength).r;
	refraction.g = GetSceneColor(uv).g;
	refraction.b = GetSceneColor(uv + _CameraOpaqueTexture_TexelSize.xy * dispersionStrength).b;
	return refraction;
}

inline float3 ScreenPosToWorldPos(float2 uv)
{
	float depth = GetSceneDepth(uv);
	float3 posWS = GetWorldSpacePositionFromDepth(uv, depth);
	return posWS;
}

inline float4 WorldPosToScreenPos(float3 pos)
{
	float4 projected = mul(KWS_MATRIX_VP, float4(pos, 1.0f));
	projected.xy = (projected.xy / projected.w) * 0.5f + 0.5f;
	#ifdef UNITY_UV_STARTS_AT_TOP
		projected.y = 1 - projected.y;
	#endif
	return projected;
}


inline float2 WorldPosToScreenPosReprojectedPrevFrame(float3 pos, float2 texelSize)
{
	float4 projected = mul(KWS_PREV_MATRIX_VP, float4(pos, 1.0f));
	float2 uv = (projected.xy / projected.w) * 0.5f + 0.5f;
	#ifdef UNITY_UV_STARTS_AT_TOP
		uv.y = 1 - uv.y;
	#endif
	return uv + texelSize * 0.5;
}

inline float3 GetMainLightDir()
{
	return KWS_DirLightDireciton.xyz;
}

inline float3 GetMainLightColor()
{
	return KWS_DirLightColor.xyz;
}

inline float3 KWS_GetSkyColor(float3 reflDir, float lod)
{
	UNITY_BRANCH
	if(KWS_OverrideSkyColor == 1) return KWS_CustomSkyColor;
	else return KWS_SkyTexture.SampleLevel(sampler_trilinear_clamp, reflDir, lod).xyz;
}


inline void GetInternalFogVariables(float4 pos, float3 viewDir, float surfaceDepthZ, float screenPosZ, out half3 fogColor, out half3 fogOpacity)
{
	if (KWS_FogState > 0)
	{
		float fogFactor = 0 ;
		
		if (KWS_FogState == 1) fogFactor = surfaceDepthZ * unity_FogParams.z + unity_FogParams.w;
		else if (KWS_FogState == 2) fogFactor = exp2(-unity_FogParams.y * surfaceDepthZ);
		else if (KWS_FogState == 3) fogFactor = exp2(-unity_FogParams.x * surfaceDepthZ * unity_FogParams.x * surfaceDepthZ);

		fogOpacity = 1 - saturate(fogFactor);
		fogColor = unity_FogColor;
	}
	else
	{
		fogOpacity = half3(0.0, 0.0, 0.0);
		fogColor = half3(0.0, 0.0, 0.0);
	}
}


inline half3 ComputeInternalFog(half3 sourceColor, half3 fogColor, half3 fogOpacity)
{
	return lerp(sourceColor, lerp(sourceColor, fogColor, fogOpacity), saturate(KWS_FogState));
}

inline half3 ComputeThirdPartyFog(half3 sourceColor, float3 worldPos, float2 screenUV, float screenPosZ)
{
	#if defined(ENVIRO_FOG)
		sourceColor = TransparentFog(half4(sourceColor, 1.0), worldPos.xyz, screenUV, Linear01Depth(screenPosZ));
	#elif defined(ENVIRO_3_FOG)
		sourceColor = ApplyFogAndVolumetricLights(sourceColor, screenUV, worldPos.xyz, Linear01Depth(screenPosZ));
	#elif defined(AZURE_FOG)
		sourceColor = ApplyAzureFog(half4(sourceColor, 1.0), worldPos.xyz).xyz;
	#elif defined(WEATHER_MAKER)
		_DirectionalLightMultiplier = 1;
		_PointSpotLightMultiplier = 1;
		_AmbientLightMultiplier = 1;
		sourceColor = ComputeWeatherMakerFog(half4(sourceColor, 1.0), worldPos, true);
	#elif defined(ATMOSPHERIC_HEIGHT_FOG)
		float4 fogParams = GetAtmosphericHeightFog(worldPos);
		fogParams.a = saturate(fogParams.a * 1.35f); //by some reason max value < 0.75;
		sourceColor = ApplyAtmosphericHeightFog(half4(sourceColor, 1.0), fogParams).xyz;
	#elif defined(VOLUMETRIC_FOG_AND_MIST)
		sourceColor = overlayFog(worldPos, float4(screenUV, screenPosZ, 1), half4(sourceColor, 1.0)).xyz;
	#elif defined(COZY_FOG)
		sourceColor = BlendStylizedFog(worldPos, half4(sourceColor.xyz, 1));
	#endif

	return max(0, sourceColor);
}

inline float GetExposure()
{
	return 1;
}

float GetSurfaceDepth(float screenPosZ)
{
	return UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPosZ);
}

