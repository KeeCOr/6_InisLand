using UnityEngine;

namespace IL6
{
    public static class HudLayout
    {
        public static Rect TopLeftStatus()
        {
            return new Rect(
                HudStyleConfig.Margin,
                HudStyleConfig.Margin,
                HudStyleConfig.TopStatusWidth,
                HudStyleConfig.TopStatusHeight);
        }

        public static Rect TopRightResources(int rows)
        {
            float h = HudStyleConfig.PanelPadding * 2f + rows * HudStyleConfig.ResourceCellHeight;
            return new Rect(
                MobileHudLayout.Width - HudStyleConfig.ResourcePanelWidth - HudStyleConfig.Margin,
                HudStyleConfig.Margin,
                HudStyleConfig.ResourcePanelWidth,
                h);
        }

        public static Rect BottomLeftVillage()
        {
            return new Rect(
                HudStyleConfig.Margin,
                HudStyleConfig.BottomSafeY(HudStyleConfig.BottomVillageHeight),
                HudStyleConfig.BottomVillageWidth,
                HudStyleConfig.BottomVillageHeight);
        }

        public static Rect BottomCenterContext(int visibleActions)
        {
            int count = Mathf.Clamp(visibleActions, 1, HudStyleConfig.ContextActionLimit);
            float h = HudStyleConfig.PanelPadding * 2f
                + count * HudStyleConfig.ContextButtonHeight
                + (count - 1) * HudStyleConfig.PanelGap;
            return new Rect(
                MobileHudLayout.Width / 2f - HudStyleConfig.ContextPanelWidth / 2f,
                HudStyleConfig.BottomSafeY(h),
                HudStyleConfig.ContextPanelWidth,
                h);
        }

        public static Rect BuildHotbar()
        {
            return new Rect(
                MobileHudLayout.Width / 2f - HudStyleConfig.BuildHotbarWidth / 2f,
                MobileHudLayout.Height - HudStyleConfig.BuildHotbarHeight - HudStyleConfig.Margin,
                HudStyleConfig.BuildHotbarWidth,
                HudStyleConfig.BuildHotbarHeight);
        }

        public static Rect RecruitDialog()
        {
            return new Rect(
                MobileHudLayout.Width / 2f - 280,
                MobileHudLayout.Height - 400,
                560,
                190);
        }

        public static Rect CatalogCard(int index)
        {
            var modal=CenterModal(760,450);
            return new Rect(modal.x+14+(index%5)*148,modal.y+64+(index/5)*82,140,74);
        }

        public static Rect CenterModal(float width = 0f, float height = 0f)
        {
            float maxW = Mathf.Max(1f, MobileHudLayout.Width - HudStyleConfig.Margin * 2f);
            float maxH = Mathf.Max(1f, MobileHudLayout.Height - HudStyleConfig.Margin * 2f);
            float w = Mathf.Min(width > 0f ? width : HudStyleConfig.ModalWidth, maxW);
            float h = Mathf.Min(height > 0f ? height : HudStyleConfig.ModalHeight, maxH);
            return new Rect(MobileHudLayout.Width / 2f - w / 2f, MobileHudLayout.Height / 2f - h / 2f, w, h);
        }
    }
}
