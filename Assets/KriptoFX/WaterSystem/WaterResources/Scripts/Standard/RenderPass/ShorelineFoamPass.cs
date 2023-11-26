using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class ShorelineFoamPass : WaterPass
    {
        ShorelineFoamPassCore _pass;

        public void Initialize()
        {
            IsInitialized           =  true;
            _pass                   =  new ShorelineFoamPassCore(WaterInstance);
            _pass.OnSetRenderTarget += OnSetRenderTarget;

            _camEvent = CameraEvent.BeforeForwardAlpha;
            if (_cmd == null) _cmd = new CommandBuffer() {name = _pass.PassName};
        }

        public override void Execute(Camera cam)
        {
            if (!IsInitialized) Initialize();
            _cmd.Clear();
            var depthRT = cam.actualRenderingPath == RenderingPath.Forward ? BuiltinRenderTextureType.Depth : BuiltinRenderTextureType.ResolvedDepth;
            var colorRT = BuiltinRenderTextureType.CameraTarget;
            _pass.Execute(cam, _cmd, colorRT, depthRT);

            cam.AddCommandBuffer(_camEvent, _cmd);
        }

        private void OnSetRenderTarget(CommandBuffer cmd, Camera cam, RTHandle rt)
        {
            CoreUtils.SetRenderTarget(cmd, rt);
            CoreUtils.ClearRenderTarget(cmd, ClearFlag.Color, Color.clear);
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