using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class CausticPass : WaterPass
    {
        CausticPassCore _pass;

        public void Initialize()  
        {
            IsInitialized = true;

            _pass                         =  new CausticPassCore(WaterInstance);
            _pass.OnRenderToCausticTarget += OnRenderToCausticTarget;
            _pass.OnRenderToCameraTarget  += OnRenderToCameraTarget;

            _camEvent = CameraEvent.BeforeForwardAlpha;
            _cmd = new CommandBuffer() { name = _pass.PassName };
        }

        public override void Execute(Camera cam)
        {
            if (!IsInitialized) Initialize();
            _cmd.Clear();
            _pass.Execute(cam, _cmd);
            cam.AddCommandBuffer(_camEvent, _cmd);
        }

        private void OnRenderToCausticTarget(CommandBuffer cmd, RTHandle rt)
        {
            CoreUtils.SetRenderTarget(cmd, rt, ClearFlag.Color, Color.black);
        }

        private void OnRenderToCameraTarget(CommandBuffer cmd)
        {
            CoreUtils.SetRenderTarget(cmd, BuiltinRenderTextureType.CameraTarget);
        }

        public override void Release()
        {
            if (_pass != null)
            {
                _pass.OnRenderToCausticTarget -= OnRenderToCausticTarget;
                _pass.OnRenderToCameraTarget  -= OnRenderToCameraTarget;

                _pass.Release();
            }

            IsInitialized = false;
        }
    }
}