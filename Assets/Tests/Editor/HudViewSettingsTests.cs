using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public class HudViewSettingsTests
    {
        private float _ui,_zoom;
        private bool _hadUi,_hadZoom;
        [SetUp] public void Save() { _hadUi=PlayerPrefs.HasKey("IL6.UiScale");_hadZoom=PlayerPrefs.HasKey("IL6.WorldZoom");_ui=HudViewSettings.UiScale;_zoom=HudViewSettings.WorldZoom; }
        [TearDown] public void Restore()
        {
            if(_hadUi) HudViewSettings.UiScale=_ui;else PlayerPrefs.DeleteKey("IL6.UiScale");
            if(_hadZoom) HudViewSettings.WorldZoom=_zoom;else PlayerPrefs.DeleteKey("IL6.WorldZoom");
        }

        [Test] public void UiDensityDoesNotChangeWorldFraming()
        {
            HudViewSettings.WorldZoom=1.25f;
            float size=HudViewSettings.WorldSize(10f);
            HudViewSettings.UiScale=.8f;
            Assert.That(HudViewSettings.WorldSize(10f),Is.EqualTo(size));
            Assert.That(size,Is.EqualTo(8f));
        }
        [Test] public void ZoomDoesNotChangeUiDensity()
        {
            HudViewSettings.UiScale=.9f;
            float scale=MobileHudLayout.Scale;
            HudViewSettings.WorldZoom=1.4f;
            Assert.That(MobileHudLayout.Scale,Is.EqualTo(scale));
        }
        [Test] public void LargestUiFitsBaselineLandscapeAndKeepsControlsApart()
        {
            HudViewSettings.UiScale=1f;
            for(int i=0;i<4;i++)
                for(int j=i+1;j<4;j++)
                    Assert.That(MobileHudLayout.ActionButton(i).Overlaps(MobileHudLayout.ActionButton(j)),Is.False);
            Assert.That(MobileHudLayout.Status.Overlaps(MobileHudLayout.Clock),Is.False);
            Assert.That(MobileHudLayout.Goals.Overlaps(MobileHudLayout.Actions),Is.False);
        }
    }
}
