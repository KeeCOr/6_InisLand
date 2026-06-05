using UnityEngine;

namespace IL6
{
    public static class HudStyleConfig
    {
        public const float Margin = 14f;
        public const float PanelPadding = 8f;
        public const float PanelGap = 8f;

        public const float TopStatusWidth = 248f;
        public const float TopStatusHeight = 98f;
        public const float ResourcePanelWidth = 210f;
        public const float ResourceCellHeight = 28f;

        public const float BottomVillageWidth = 300f;
        public const float BottomVillageHeight = 142f;

        public const float ContextPanelWidth = 390f;
        public const float ContextButtonHeight = 26f;
        public const int ContextActionLimit = 3;

        public const float BuildHotbarWidth = 520f;
        public const float BuildHotbarHeight = 70f;

        public const float RecruitDialogWidth = 380f;
        public const float RecruitDialogHeight = 110f;

        public const float ModalWidth = 720f;
        public const float ModalHeight = 420f;

        public const float IconSmall = 16f;
        public const float IconMedium = 20f;
        public const float IconLarge = 32f;

        public static float BottomSafeY(float height)
            => Mathf.Max(Margin, Screen.height - height - Margin);

        public static float ClampPanelX(float x, float width)
            => Mathf.Clamp(x, Margin, Mathf.Max(Margin, Screen.width - width - Margin));
    }
}
