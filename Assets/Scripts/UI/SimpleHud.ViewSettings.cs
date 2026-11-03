using UnityEngine;

namespace IL6
{
    public sealed partial class SimpleHud
    {
        private Camera _settingsCamera;
        private float _baselineWorldSize;

        private void LateUpdate()
        {
            var camera=Camera.main;
            if(camera==null || !camera.orthographic) return;
            if(_settingsCamera!=camera)
            {
                _settingsCamera=camera;
                _baselineWorldSize=camera.orthographicSize;
            }
            camera.orthographicSize=HudViewSettings.WorldSize(_baselineWorldSize);
        }
    }
}
