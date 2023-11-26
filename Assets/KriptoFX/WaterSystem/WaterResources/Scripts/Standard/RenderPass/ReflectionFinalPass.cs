using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class ReflectionFinalPass: WaterPass
    {
        ReflectionFinalPassCore _pass;

        public void Initialize()
        {
            IsInitialized = true;
            _pass                           =  new ReflectionFinalPassCore(WaterInstance);
            _pass.OnInitializedRenderTarget += OnInitializedRenderTarget;

            if (_cmd == null) _cmd = new CommandBuffer() { name = _pass.PassName };
            _camEvent                       =  CameraEvent.BeforeForwardAlpha;
        }

        public override void Execute(Camera cam)
        {
            if (!IsInitialized) Initialize();
            _cmd.Clear();
            _pass.Execute(cam, _cmd);
            cam.AddCommandBuffer(_camEvent, _cmd);
        }


        private void OnInitializedRenderTarget(CommandBuffer cmd, Camera cam, RTHandle rt)
        {
            CoreUtils.SetRenderTarget(cmd, rt, ClearFlag.Color, Color.black);
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