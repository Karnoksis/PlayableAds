using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class ShorelineDrawFoamToScreenPass : WaterPass
    {
        ShorelineDrawFoamToScreenPassCore _pass;

        public void Initialize()
        {
            IsInitialized           =  true;
            _pass                   =  new ShorelineDrawFoamToScreenPassCore(WaterInstance);
            _pass.OnSetRenderTarget += OnSetRenderTarget;
            _camEvent               =  CameraEvent.BeforeForwardAlpha;

            if (_cmd == null) _cmd = new CommandBuffer() {name = _pass.PassName};
        }

        public override void Execute(Camera cam)
        {
            if (!IsInitialized) Initialize();
            _cmd.Clear();
            _pass.Execute(cam, _cmd, BuiltinRenderTextureType.CameraTarget);
            cam.AddCommandBuffer(_camEvent, _cmd);
        }

        private void OnSetRenderTarget(CommandBuffer cmd, Camera cam, RTHandle rt)
        {
            CoreUtils.SetRenderTarget(cmd, rt);
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