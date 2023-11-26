using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class MaskDepthNormalPass : WaterPass
    {
        MaskDepthNormalPassCore _pass;
        RenderTargetIdentifier[] _mrt = new RenderTargetIdentifier[2];

        public void Initialize()
        {
            IsInitialized                   =  true;
            _pass                           =  new MaskDepthNormalPassCore(WaterInstance);
            _pass.OnInitializedRenderTarget += OnInitializedRenderTarget;

            _camEvent = CameraEvent.BeforeForwardAlpha;
            if (_cmd == null) _cmd = new CommandBuffer() {name = _pass.PassName};
        }

        public override void Execute(Camera cam)
        {
            if (!IsInitialized) Initialize();
            _cmd.Clear();
            _pass.Execute(cam, _cmd);
            cam.AddCommandBuffer(_camEvent, _cmd);
        }

        private void OnInitializedRenderTarget(CommandBuffer cmd, RTHandle rt1, RTHandle rt2, RTHandle rt3)
        {
            _mrt[0] = rt1;
            _mrt[1] = rt2;
            CoreUtils.SetRenderTarget(cmd, _mrt, rt3, ClearFlag.All, Color.black);
        }


        public override void Release()
        {
            if (_pass != null)
            {
                _pass.OnInitializedRenderTarget -= OnInitializedRenderTarget;
                _pass.Release();
            }

            IsInitialized = false;
        }
    }
}