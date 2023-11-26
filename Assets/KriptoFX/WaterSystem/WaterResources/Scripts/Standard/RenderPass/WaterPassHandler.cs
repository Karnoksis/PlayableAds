using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    [ExecuteAlways]
    [AddComponentMenu("")]
    public class WaterPassHandler : MonoBehaviour
    {
        public WaterSystem WaterInstance;

        bool _isCanUpdate;

        readonly OrthoDepthPass _orthoDepthPass = new OrthoDepthPass();
        readonly ShorelineWavesPass _shorelineWavesPass = new ShorelineWavesPass();
        readonly MaskDepthNormalPass _maskDepthNormalPass = new MaskDepthNormalPass();
        readonly CausticPass _causticPass = new CausticPass();
        readonly CopyColorPass _copyColorPass = new CopyColorPass();
        readonly ReflectionFinalPass _reflectionFinalPass = new ReflectionFinalPass();
        readonly ScreenSpaceReflectionPass _ssrPass = new ScreenSpaceReflectionPass();
        readonly VolumetricLightingPass _volumetricLightingPass = new VolumetricLightingPass();
        readonly DrawMeshPass _drawMeshPass = new DrawMeshPass();
        readonly ShorelineFoamPass _shorelineFoamPass = new ShorelineFoamPass();
        readonly ShorelineDrawFoamToScreenPass _shorelineDrawFoamToScreenPass = new ShorelineDrawFoamToScreenPass();
        readonly UnderwaterPass _underwaterPass = new UnderwaterPass();
        readonly DrawToPosteffectsDepthPass _drawToDepthPass = new DrawToPosteffectsDepthPass();

        private List<WaterPass> _waterPasses;

        internal static int UpdatedInstancesPerFrame;


        public void Initialize(WaterSystem waterInstance)
        {
            WaterInstance = waterInstance;

            var cameraSize = KWS_CoreUtils.GetScreenSizeLimited(WaterSystem.IsSinglePassStereoEnabled);
            WaterSystem.RTHandles.SetReferenceSize(cameraSize.x, cameraSize.y);

            if (_waterPasses == null) _waterPasses = new List<WaterPass>()
            {
                _orthoDepthPass, _shorelineWavesPass, _maskDepthNormalPass, _causticPass, _copyColorPass, _reflectionFinalPass,
                _ssrPass, _volumetricLightingPass, _drawMeshPass, _shorelineFoamPass, _shorelineDrawFoamToScreenPass, _underwaterPass, _drawToDepthPass
            };
            foreach (var waterPass in _waterPasses) waterPass.WaterInstance = WaterInstance;
            
            Camera.onPreRender += MyPreCull;
            Camera.onPostRender += MyPostRender;
           
            _isCanUpdate = true;
        }

        void OnDisable()
        {
            Camera.onPostRender -= MyPreCull;
            Camera.onPostRender -= MyPostRender;
            _isCanUpdate = false;

            if (_waterPasses != null)
            {
                foreach (var waterPass in _waterPasses)
                {
                    if (waterPass != null) waterPass.Release();
                }
            }
        }

        void ExecutePass(WaterPass pass, Camera cam)
        {
            pass.Execute(cam);
        }

        public void MyPreCull(Camera cam)
        {
            if (!_isCanUpdate) return;
            if (!WaterInstance.IsWaterVisible || !KWS_CoreUtils.CanRenderWaterForCurrentCamera(WaterInstance, cam)) return;
           
            var cameraSize = KWS_CoreUtils.GetScreenSizeLimited(WaterSystem.IsSinglePassStereoEnabled);
            WaterSystem.RTHandles.SetReferenceSize(cameraSize.x, cameraSize.y);
            
            ExecutePass(_orthoDepthPass, cam); 
            ExecutePass(_shorelineWavesPass, cam);
            if (UpdatedInstancesPerFrame == 0)
            {
                _volumetricLightingPass.CollectLightsToBuffer(cam, WaterSystem.VisibleWaterInstances[0]);
                ExecutePass(_maskDepthNormalPass, cam);
                ExecutePass(_causticPass, cam);
                ExecutePass(_copyColorPass, cam);
            }
            
            ExecutePass(_ssrPass, cam);
            ExecutePass(_reflectionFinalPass, cam);
            ExecutePass(_volumetricLightingPass, cam);
            ExecutePass(_shorelineFoamPass, cam);
            ExecutePass(_drawMeshPass, cam);
            ExecutePass(_shorelineDrawFoamToScreenPass, cam);
            ExecutePass(_underwaterPass, cam);
            ExecutePass(_drawToDepthPass, cam);

            UpdatedInstancesPerFrame++;
            if (UpdatedInstancesPerFrame >= WaterSystem.VisibleWaterInstances.Count) UpdatedInstancesPerFrame = 0;
        }

        public void MyPostRender(Camera cam)
        {
            if (!_isCanUpdate) return;
            if (!KWS_CoreUtils.CanRenderWaterForCurrentCamera(WaterInstance, cam)) return;

            foreach (var waterPass in _waterPasses) waterPass.Release(cam);
        }


    }
}
