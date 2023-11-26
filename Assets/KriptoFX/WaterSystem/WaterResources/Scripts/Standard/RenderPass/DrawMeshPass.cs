using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class DrawMeshPass : WaterPass
    {
        DrawMeshPassCore _pass;

        public void Initialize()
        {
            IsInitialized       = true;
            _pass               = new DrawMeshPassCore(WaterInstance);
            _camEvent = CameraEvent.BeforeForwardAlpha;
            if (_cmd == null) _cmd = new CommandBuffer() { name = _pass.PassName };
        }

        public override void Execute(Camera cam)
        {
            if (!IsInitialized) Initialize();
            _cmd.Clear();
            if (WaterSystem.IsSinglePassStereoEnabled) CoreUtils.SetRenderTarget(_cmd, BuiltinRenderTextureType.CurrentActive);
            _pass.Execute(cam, _cmd);

            cam.AddCommandBuffer(_camEvent, _cmd);
        }

        public override void Release()
        {
            if (_pass != null) _pass.Release();
            IsInitialized = false;
        }
    }
}