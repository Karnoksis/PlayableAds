using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public abstract class WaterPass
    {
        public  WaterSystem WaterInstance;
        internal bool IsInitialized;
        protected CommandBuffer _cmd;
        protected CameraEvent _camEvent;

        public void Release(Camera cam)
        {
            if (_cmd != null)
            {
                cam.RemoveCommandBuffer(_camEvent, _cmd);
            }

        }
        public abstract void Execute(Camera cam);
        public abstract void Release();
    }
}