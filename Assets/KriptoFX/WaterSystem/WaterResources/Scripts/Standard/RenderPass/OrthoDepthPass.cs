using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class OrthoDepthPass : WaterPass
    {
        OrthoDepthPassCore _pass;
        public void Initialize()
        {
            IsInitialized = true;
            _pass          =  new OrthoDepthPassCore(WaterInstance);
            _pass.OnRender += OnRender;
            _camEvent      =  CameraEvent.AfterForwardOpaque;
        }

        public override void Execute(Camera cam)
        {
            if (!IsInitialized) Initialize();
            _pass.Execute(cam);
        }

       
        private void OnRender(OrthoDepthPassCore.PassData passData, Camera depthCamera)
        {
            var currentShadowDistance = QualitySettings.shadowDistance;
            QualitySettings.shadowDistance = 0;
            var lodBias = QualitySettings.lodBias;

            var terrains      = Terrain.activeTerrains;
            var pixelError    = new float[terrains.Length];
            for (var i = 0; i < terrains.Length; i++)
            {
                pixelError[i]    = terrains[i].heightmapPixelError;
                terrains[i].heightmapPixelError = 1;
            }
            QualitySettings.lodBias         = 10;

            depthCamera.targetTexture = passData.DepthRT;
            depthCamera.Render();
           // Debug.Log("render ortho depth");
            for (var i = 0; i < terrains.Length; i++)
            {
                terrains[i].heightmapPixelError = terrains[i].heightmapPixelError;
            }

            QualitySettings.shadowDistance = currentShadowDistance;
            QualitySettings.maximumLODLevel = 1;
            QualitySettings.lodBias         = lodBias;
        }

        public override void Release()
        {
            if (_pass != null)
            {
                _pass.OnRender -= OnRender;
                _pass.Release();
            }

            IsInitialized = false;
        }
    }
}

