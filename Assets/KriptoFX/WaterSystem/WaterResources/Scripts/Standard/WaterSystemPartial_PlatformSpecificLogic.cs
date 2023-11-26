using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace KWS
{
    public partial class WaterSystem
    {
        ///////////////////////////// platform specific components /////////////////////////////////////////////////
        internal ReflectionPass PlanarReflectionComponent = new PlanarReflection();
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
      


        internal List<ThirdPartyAssetDescription> ThirdPartyFogAssetsDescription = new List<ThirdPartyAssetDescription>()
        { 
            new ThirdPartyAssetDescription(){EditorName = "Native Unity Fog"},
            new ThirdPartyAssetDescription(){EditorName = "Enviro", AssetNameSearchPattern = "Enviro - Sky and Weather", ShaderDefine = "ENVIRO_FOG", ShaderInclude = "EnviroFogCore.cginc"},
            new ThirdPartyAssetDescription(){EditorName = "Enviro 3", AssetNameSearchPattern = "Enviro 3 - Sky and Weather", ShaderDefine = "ENVIRO_3_FOG", ShaderInclude = "FogInclude.cginc"},
            new ThirdPartyAssetDescription(){EditorName = "Azure", AssetNameSearchPattern = "Azure[Sky]", ShaderDefine = "AZURE_FOG", ShaderInclude = "AzureFogCore.cginc"},
            new ThirdPartyAssetDescription(){EditorName = "Weather maker", AssetNameSearchPattern = "WeatherMaker", ShaderDefine = "WEATHER_MAKER", ShaderInclude = "WeatherMakerFogExternalShaderInclude.cginc"},
            new ThirdPartyAssetDescription(){EditorName = "Atmospheric height fog", AssetNameSearchPattern = "Atmospheric Height Fog", ShaderDefine = "ATMOSPHERIC_HEIGHT_FOG", ShaderInclude = "AtmosphericHeightFog.cginc", DrawToDepth = true},
            new ThirdPartyAssetDescription(){EditorName = "Volumetric fog and mist 2", AssetNameSearchPattern = "VolumetricFog", ShaderDefine = "VOLUMETRIC_FOG_AND_MIST", ShaderInclude = "VolumetricFogOverlayVF.cginc", DrawToDepth = true},
            new ThirdPartyAssetDescription(){EditorName = "COZY Weather", AssetNameSearchPattern = "Cozy Weather", ShaderDefine = "COZY_FOG", ShaderInclude = "StylizedFogIncludes.cginc", DrawToDepth = true},
        };

        WaterPassHandler _waterPassHandler;

        void SubscribeBeforeCameraRendering()
        {
            Camera.onPreCull += OnBeforeCameraRendering;
        }

        void UnsubscribeBeforeCameraRendering()
        {
            Camera.onPreCull -= OnBeforeCameraRendering;
        }

        void SubscribeAfterCameraRendering()
        {
            Camera.onPostRender += OnAfterCameraRendering;
        }

        void UnsubscribeAfterCameraRendering()
        {
            Camera.onPostRender -= OnAfterCameraRendering;
        }



        void InitializeWaterPlatformSpecificResources()
        {
            if (_waterPassHandler == null)
            {
                _waterPassHandler = _tempGameObject.AddComponent<WaterPassHandler>();
                _waterPassHandler.Initialize(this);
            }

            isWaterPlatformSpecificResourcesInitialized = true;
        }

        void RenderPlatformSpecificFeatures(Camera cam)
        {
            EnableDepthRenderingIfRequired(cam);
        }

        void ReleasePlatformSpecificResources()
        {
            isWaterPlatformSpecificResourcesInitialized = false;
        }

        public static void EnableDepthRenderingIfRequired(Camera cam)
        {
            if (cam.actualRenderingPath == RenderingPath.Forward && cam.depthTextureMode == DepthTextureMode.None) cam.depthTextureMode = DepthTextureMode.Depth;
        }

        Light _lastSun;
        private Transform _lastSunTransform;
        void SetGlobalPlatformSpecificShaderParams()
        {
            var fogState = 0;
            if (RenderSettings.fog)
            {
                if (RenderSettings.fogMode      == FogMode.Linear) fogState             = 1;
                else if (RenderSettings.fogMode == FogMode.Exponential) fogState        = 2;
                else if (RenderSettings.fogMode == FogMode.ExponentialSquared) fogState = 3;
            }
            Shader.SetGlobalInt(KWS_ShaderConstants.DynamicWaterParams.KWS_FogState, fogState);
            Shader.SetGlobalTexture(KWS_ShaderConstants.ReflectionsID.KWS_SkyTexture, ReflectionProbe.defaultTexture);

            var currentSun = RenderSettings.sun;
            if (currentSun != null)
            {
                if (_lastSun == null || _lastSun != currentSun)
                {
                    _lastSun          = currentSun;
                    _lastSunTransform = currentSun.transform;
                }

                Shader.SetGlobalVector(KWS_ShaderConstants.DynamicWaterParams.KWS_DirLightDireciton, -_lastSunTransform.forward);
                Shader.SetGlobalVector(KWS_ShaderConstants.DynamicWaterParams.KWS_DirLightColor,     _lastSun.color * _lastSun.intensity);
            }

            SphericalHarmonicsL2 sh;
            LightProbes.GetInterpolatedProbe(WaterRelativeWorldPosition, null, out sh);
            var ambient = new Vector3(sh[0, 0] - sh[0, 6], sh[1, 0] - sh[1, 6], sh[2, 0] - sh[2, 6]);
            ambient = Vector3.Max(ambient, Vector3.zero);
            Shader.SetGlobalVector(KWS_ShaderConstants.DynamicWaterParams.KWS_AmbientColor, ambient);
        }
    }

}