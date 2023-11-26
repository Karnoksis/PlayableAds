using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class UnderwaterPass: WaterPass
    {
        UnderwaterPassCore _pass;

        public void Initialize()
        {
            IsInitialized = true;
            _pass                   =  new UnderwaterPassCore(WaterInstance);
            _pass.OnSetRenderTarget += OnSetRenderTarget;

            _camEvent = CameraEvent.AfterForwardAlpha;
            if (_cmd == null) _cmd = new CommandBuffer() { name = _pass.PassName };
        }

        public override void Execute(Camera cam)
        {
            if (!IsInitialized) Initialize();
            _cmd.Clear();
            _pass.Execute(cam, _cmd, BuiltinRenderTextureType.CameraTarget);

            _camEvent = WaterInstance.Settings.UnderwaterQueue switch
            {
                WaterSystem.UnderwaterQueueEnum.BeforeTransparent => CameraEvent.BeforeForwardAlpha,
                WaterSystem.UnderwaterQueueEnum.AfterTransparent  => CameraEvent.AfterForwardAlpha,
                _ => CameraEvent.AfterForwardAlpha
            };

            cam.AddCommandBuffer(_camEvent, _cmd);
        }

        private void OnSetRenderTarget(CommandBuffer cmd, RTHandle rt)
        {
            if(rt == null) CoreUtils.SetRenderTarget(cmd, BuiltinRenderTextureType.CameraTarget);
            else CoreUtils.SetRenderTarget(cmd, rt);
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