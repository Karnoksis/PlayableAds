Shader "Hidden/KriptoFX/KWS/VolumetricLighting"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
	}
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6

			#pragma multi_compile _ KWS_USE_DIR_LIGHT

			#pragma multi_compile _ KWS_USE_POINT_LIGHTS
			#pragma multi_compile _ KWS_USE_SHADOW_POINT_LIGHTS
			#pragma multi_compile _ KWS_USE_SPOT_LIGHTS
			#pragma multi_compile _ KWS_USE_SHADOW_SPOT_LIGHTS

			#pragma multi_compile _ USE_CAUSTIC
			#pragma multi_compile _ USE_LOD1 USE_LOD2 USE_LOD3

			#include "../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"
			#include "KWS_Lighting.cginc"
			#include "../Common/CommandPass/KWS_VolumetricLight_Common.cginc"

			half4 RayMarchDirLight(RaymarchData raymarchData, uint rayMarchSteps, bool isUnderwater)
			{
				half4 result = 0;

				#if defined(KWS_USE_DIR_LIGHT) || defined(KWS_USE_DIR_LIGHT_SINGLE) || defined(KWS_USE_DIR_LIGHT_SPLIT) || defined(KWS_USE_DIR_LIGHT_SINGLE_SPLIT)
					float3 currentPos = raymarchData.currentPos;
					
					ShadowLightData light = KWS_DirLightsBuffer[0];

					UNITY_LOOP
					for (uint i = 0; i < rayMarchSteps; ++i)
					{
						half atten = 1;
						UNITY_BRANCH
						if (KWS_UseDirLightShadow == 1)
						{
							atten *= DirLightRealtimeShadow(0, currentPos);
						}
						half scattering = raymarchData.stepSize;
						#if defined(USE_CAUSTIC)
							half underwaterStrength = lerp(saturate((KW_Transparent - 1) / 5), 1, isUnderwater);
							scattering += scattering * RaymarchCaustic(raymarchData.rayStart, currentPos, light.forward) * underwaterStrength;
						#endif

						half3 lightResult = atten * scattering * light.color;
						result.rgb += lightResult;
						currentPos += raymarchData.step;
					}
					float cosAngle = dot(light.forward.xyz, -raymarchData.rayDir);
					result.rgb *= MieScattering(cosAngle);
					if (!isUnderwater) result.a = DirLightRealtimeShadow(0, raymarchData.rayStart);
				#endif
				return result;
			}

			half4 RayMarchAdditionalLights(RaymarchData raymarchData, uint rayMarchSteps)
			{
				half4 result = 0;
				float lightStrengthRelativeToTransparentFix = lerp(1, 15, KW_Transparent / 50.0);

				#if KWS_USE_POINT_LIGHTS
					UNITY_LOOP
					for (uint pointIdx = 0; pointIdx < KWS_PointLightsCount; pointIdx++)
					{
						LightData light = KWS_PointLightsBuffer[pointIdx];

						float3 currentPos = raymarchData.currentPos;
						UNITY_LOOP
						for (uint i = 0; i < KWS_RayMarchSteps; ++i)
						{
							half atten = PointLightAttenuation(pointIdx, currentPos);
							//[branch]if (atten < 0.00001) continue;
							half3 scattering = raymarchData.stepSize * light.color.rgb * lightStrengthRelativeToTransparentFix;
							half3 lightResult = atten * scattering;

							half cosAngle = dot(-raymarchData.rayDir, normalize(currentPos - light.position.xyz));
							lightResult *= MieScattering(cosAngle);

							result.rgb += lightResult;
							currentPos += raymarchData.step;
						}
					}
				#endif

				#if KWS_USE_SHADOW_POINT_LIGHTS
					
					UNITY_LOOP
					for (uint shadowPointIdx = 0; shadowPointIdx < KWS_ShadowPointLightsCount; shadowPointIdx++)
					{
						ShadowLightData light = KWS_ShadowPointLightsBuffer[shadowPointIdx];

						float3 currentPos = raymarchData.currentPos;
						UNITY_LOOP
						for (uint i = 0; i < KWS_RayMarchSteps; ++i)
						{
							#ifdef SHADER_API_VULKAN
								float atten = PointLightAttenuation(shadowPointIdx, currentPos);
							#else
								float atten = PointLightAttenuationShadow(shadowPointIdx, currentPos);
							#endif
						
							//[branch] if (atten < 0.00001) continue;

							float3 scattering = raymarchData.stepSize * light.color.rgb * lightStrengthRelativeToTransparentFix;
							float3 lightResult = atten * scattering;

							float cosAngle = dot(-raymarchData.rayDir, normalize(currentPos - light.position.xyz));
							lightResult *= MieScattering(cosAngle);

							result.rgb += lightResult;
							currentPos += raymarchData.step;
						}
					}
					
				#endif

				#if KWS_USE_SPOT_LIGHTS
					UNITY_LOOP
					for (uint spotIdx = 0; spotIdx < KWS_SpotLightsCount; spotIdx++)
					{
						LightData light = KWS_SpotLightsBuffer[spotIdx];

						float3 currentPos = raymarchData.currentPos;
						UNITY_LOOP
						for (uint i = 0; i < KWS_RayMarchSteps; ++i)
						{
							float atten = SpotLightAttenuation(spotIdx, currentPos);
							//[branch] if (atten < 0.00001) continue;
							float3 scattering = raymarchData.stepSize * light.color.rgb * lightStrengthRelativeToTransparentFix;
							float3 lightResult = atten * scattering;

							float cosAngle = dot(-raymarchData.rayDir, normalize(currentPos - light.position.xyz));
							lightResult *= MieScattering(cosAngle);

							result.rgb += lightResult;
							currentPos += raymarchData.step;
						}
					}
				#endif

				#if KWS_USE_SHADOW_SPOT_LIGHTS

					UNITY_LOOP
					for (uint shadowSpotIdx = 0; shadowSpotIdx < KWS_ShadowSpotLightsCount; shadowSpotIdx++)
					{
						ShadowLightData light = KWS_ShadowSpotLightsBuffer[shadowSpotIdx];

						float3 currentPos = raymarchData.currentPos;
						UNITY_LOOP
						for (uint i = 0; i < KWS_RayMarchSteps; ++i)
						{
							#ifdef SHADER_API_VULKAN
								float atten = SpotLightAttenuation(shadowSpotIdx, currentPos);
							#else
								float atten = SpotLightAttenuationShadow(shadowSpotIdx, currentPos);
							#endif
							//[branch] if (atten < 0.00001) continue;
							float3 scattering = raymarchData.stepSize * light.color.rgb * lightStrengthRelativeToTransparentFix;
							float3 lightResult = atten * scattering;

							float cosAngle = dot(-raymarchData.rayDir, normalize(currentPos - light.position.xyz));
							lightResult *= MieScattering(cosAngle);

							result.rgb += lightResult;
							currentPos += raymarchData.step;
						}
					}
				#endif

				return result;
			}

			inline float4 RayMarch(RaymarchData raymarchData, bool isUnderwater)
			{

				float4 result = 0;
				
				float extinction = 0;
				
				result += RayMarchDirLight(raymarchData, KWS_RayMarchSteps, isUnderwater);
				result += RayMarchAdditionalLights(raymarchData, KWS_RayMarchSteps);

				result.rgb /= KW_Transparent;
				result.rgb *= KWS_VolumeDepthFade;
				result.rgb *= 4;
				result.rgb = max(MIN_THRESHOLD * 2, result.rgb);
				return result;
			}

			half4 frag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				half mask = GetWaterMask(i.uv);
				
				UNITY_BRANCH
				if (EarlyDiscardUnderwaterPixels(mask)) return 0;

				float depthTop = GetWaterDepth(i.uv);
				float depthBot = GetSceneDepth(i.uv);

				//UNITY_BRANCH
				//if (EarlyDiscardDepthOcclusionPixels(depthBot, depthTop, mask)) return 0; //todo blur pass wil have black color leaking in this case. Maybe I need to add the same method for blur pass?

				bool isUnderwater = IsUnderwaterMask(mask);
				RaymarchData raymarchData = InitRaymarchData(i, depthTop, depthBot, isUnderwater);
				half4 finalColor = RayMarch(raymarchData, isUnderwater);

				return finalColor;
			}
			
			ENDCG
		}
	}
}