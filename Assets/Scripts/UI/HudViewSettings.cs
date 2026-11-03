using UnityEngine;

namespace IL6
{
    // UI density and world framing are deliberately independent preferences.
    public static class HudViewSettings
    {
        public static float UiScale
        {
            get => Mathf.Clamp(PlayerPrefs.GetFloat("IL6.UiScale",1f),.8f,1f);
            set => PlayerPrefs.SetFloat("IL6.UiScale",Mathf.Clamp(value,.8f,1f));
        }
        public static float WorldZoom
        {
            get => Mathf.Clamp(PlayerPrefs.GetFloat("IL6.WorldZoom",1f),.75f,1.4f);
            set => PlayerPrefs.SetFloat("IL6.WorldZoom",Mathf.Clamp(value,.75f,1.4f));
        }
        public static float WorldSize(float baseline) => baseline / WorldZoom;
    }
}
