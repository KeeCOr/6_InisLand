using UnityEngine;

namespace IL6
{
    /// <summary>Music volume and permanent attribution are available whenever the game is paused.</summary>
    public sealed class MusicCredits : MonoBehaviour
    {
        private void OnGUI()
        {
            if (Time.timeScale > 0f) return;
            var previousMatrix = GUI.matrix;
            var previousColor = GUI.color;
            GUI.matrix = Matrix4x4.identity;
            GUI.color = Color.white;
            float width = Mathf.Min(550f, Screen.width - 24f);
            var panel = new Rect(12f, Screen.height - 150f, width, 138f);
            GUI.Box(panel, "");
            GUI.Label(new Rect(panel.x + 12, panel.y + 6, 180, 22), "BGM");
            float volume = GUI.HorizontalSlider(new Rect(panel.x + 60, panel.y + 14, width - 130, 18), Music.Volume, 0, 1);
            if (Mathf.Abs(volume - Music.Volume) > .005f) Music.Volume = volume;
            GUI.Label(new Rect(panel.xMax - 58, panel.y + 6, 52, 22), $"{Music.Volume * 100:F0}%");
            var style = new GUIStyle(GUI.skin.label) { fontSize = 12, wordWrap = true };
            GUI.Label(new Rect(panel.x + 12, panel.y + 32, width - 24, 98),
                "Music: Childhood, Horizons, Reverie, Moonlight\n" +
                "by Scott Buckley | www.scottbuckley.com.au\n" +
                "CC BY 4.0 | https://creativecommons.org/licenses/by/4.0/\n" +
                "Edited: excerpts, loudness normalization, loop crossfades.", style);
            GUI.matrix = previousMatrix;
            GUI.color = previousColor;
        }
    }
}
