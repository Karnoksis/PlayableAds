using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class ScreenSpaceReflectionPass: WaterPass
    {
        ScreenSpaceReflectionPassCore _pass;

        public void Initialize()
        {
            IsInitialized = true;

            _pass =  new ScreenSpaceReflectionPassCore(WaterInstance);
            _pass.OnSetRenderTarget += OnSetRenderTarget;

            if (_cmd == null) _cmd = new CommandBuffer() { name = _pass.PassName };
            _camEvent               =  CameraEvent.BeforeForwardAlpha;
        }

        public override void Execute(Camera cam)
        {
            if (!IsInitialized) Initialize();
            _cmd.Clear();
            var depthRT = cam.actualRenderingPath == RenderingPath.Forward ? BuiltinRenderTextureType.Depth : BuiltinRenderTextureType.ResolvedDepth;
            _pass.Execute(cam, _cmd, depthRT, WaterInstance.SharedData.CameraOpaqueTexture.GetSafeTexture());
            cam.AddCommandBuffer(_camEvent, _cmd);
        }


        private void OnSetRenderTarget(CommandBuffer cmd, Camera cam, RTHandle rt)
        {
            CoreUtils.SetRenderTarget(cmd, rt, ClearFlag.Color, Color.clear);
        }

        public override void Release()
        {
            if (_pass != null)
            {
                _pass.OnSetRenderTarget -= OnSetRenderTarget;
                _pass.Release();
            }

            IsInitialized = false;
        }
    }
}