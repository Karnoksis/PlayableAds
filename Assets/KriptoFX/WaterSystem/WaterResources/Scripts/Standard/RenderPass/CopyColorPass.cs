using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class CopyColorPass: WaterPass
    {

        RTHandle _colorRT;
        private Material _copyColorMaterial;

        public void Initialize()
        {
            IsInitialized = true;

            _camEvent = CameraEvent.BeforeForwardAlpha;
            if (_cmd == null) _cmd = new CommandBuffer() { name = "Water.CopyColorPass" };
        }

        public override void Execute(Camera cam)
        {
            if (!IsInitialized) Initialize();
            _cmd.Clear();

            if (_copyColorMaterial == null) _copyColorMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.CopyColorShaderName);
            if (_colorRT == null) _colorRT = KWS_CoreUtils.RTHandleAllocVR(Vector2.one, name: "_CameraOpaqueTexture", colorFormat: KWS_CoreUtils.GetGraphicsFormatHDR());

            _cmd.BlitTriangleRTHandle(BuiltinRenderTextureType.CurrentActive, Vector4.one, _colorRT, _copyColorMaterial, ClearFlag.None, Color.clear, 0);
            _cmd.SetGlobalTexture(KWS_ShaderConstants_PlatformSpecific.CopyColorID._CameraOpaqueTexture, _colorRT); //we need to provide color source to compute shaders, so I can't update materials only, I need to update compute shader kernels too
            _cmd.SetGlobalVector(KWS_ShaderConstants_PlatformSpecific.CopyColorID._CameraOpaqueTexture_RTHandleScale, _colorRT.rtHandleProperties.rtHandleScale);
            WaterInstance.SharedData.CameraOpaqueTexture = _colorRT;

            cam.AddCommandBuffer(_camEvent, _cmd);
        }

        public override void Release()
        {
            if (_colorRT != null) _colorRT.Release();
            KW_Extensions.SafeDestroy(_copyColorMaterial);
            //KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);

            IsInitialized = false;
        }
    }
}