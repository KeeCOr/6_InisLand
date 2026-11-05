using UnityEngine;

namespace IL6
{
    /// <summary>The adventure menu owns the credits area, avoiding overlays on other paused screens.</summary>
    public sealed class MusicCredits : MonoBehaviour
    {
        public const string Attribution="Music: 눈보라 속 등불 — Dawn / Day / Evening / Night\n" +
                "Original InisLand soundtrack\n" +
                "Instrument sounds: GeneralUser GS by S. Christian Collins\n" +
                "www.schristiancollins.com/generaluser.php";
        public static void Draw(Rect panel,GUIStyle source)
        {
            UiTheme.Rect(panel,new Color(0,0,0,.16f));
            var style=new GUIStyle(source){fontSize=12,wordWrap=true,alignment=TextAnchor.UpperLeft};
            HudText.Label(new Rect(panel.x+10,panel.y+10,panel.width-20,panel.height-20),Attribution,style);
        }
    }
}
