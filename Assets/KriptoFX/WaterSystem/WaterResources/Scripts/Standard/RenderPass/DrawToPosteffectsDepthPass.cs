using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class DrawToPosteffectsDepthPass: WaterPass
    {
        DrawToPosteffectsDepthPassCore _pass;

        public void Initialize()
        {
            IsInitialized = true;
            _pass                   =  new DrawToPosteffectsDepthPassCore(WaterInstance);
            _pass.OnSetRenderTarget += OnSetRenderTarget;

            _camEvent               =  CameraEvent.BeforeForwardAlpha;
            if (_cmd == null) _cmd = new CommandBuffer() { name = _pass.PassName };
        }

        public override void Execute(Camera cam)
        {
            if (!IsInitialized) Initialize();
            _cmd.Clear();

            var depthID = (cam.actualRenderingPath == RenderingPath.Forward) ? new RenderTargetIdentifier(BuiltinRenderTextureType.Depth) : new RenderTargetIdentifier(BuiltinRenderTextureType.ResolvedDepth);
            _pass.Execute(cam, _cmd, depthID);

            cam.AddCommandBuffer(_camEvent, _cmd);
        }

        private void OnSetRenderTarget(CommandBuffer cmd, Camera cam)
        {
            var depthID = (cam.actualRenderingPath == RenderingPath.Forward) ? new RenderTargetIdentifier(BuiltinRenderTextureType.Depth) : new RenderTargetIdentifier(BuiltinRenderTextureType.ResolvedDepth);
            CoreUtils.SetRenderTarget(cmd, BuiltinRenderTextureType.CameraTarget, depthID);
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
