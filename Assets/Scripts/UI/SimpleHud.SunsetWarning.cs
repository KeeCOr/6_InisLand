using UnityEngine;
namespace IL6
{
    public sealed partial class SimpleHud
    {
        private readonly SunsetWarningState _sunsetWarning=new();
        private Texture2D _sunsetIcon;
        private bool SunsetWarningVisible=>_sunsetWarning.Visible && !ControlsBlocked && Player!=null
            && GameSession.Instance!=null && GameSession.Instance.Cycle.Phase==Phase.Day;
        private Rect SunsetWarningRect=>new Rect((MobileHudLayout.Width-400)/2f,72,400,70);
        private void UpdateSunsetWarning()
        {
            var cycle=GameSession.Instance!=null?GameSession.Instance.Cycle:null;
            if(_sunsetWarning.Advance(cycle,Time.deltaTime,ControlsBlocked || Player==null))Sfx.SunsetBell();
        }
        private void DrawSunsetClockAccent(Rect clock)
        {
            if(_sunsetWarning.Stage==SunsetWarningStage.None || ControlsBlocked || GameSession.Instance==null
                || GameSession.Instance.Cycle.Phase!=Phase.Day)return;
            float pulse=.55f+.18f*Mathf.Sin(_sunsetWarning.Age*3f);
            GeneratedUiSkin.Pill(new Rect(clock.x-2,clock.y-2,clock.width+4,clock.height+4),new Color(1f,.64f,.25f,pulse));
        }
        private void DrawSunsetWarning()
        {
            if(!SunsetWarningVisible)return;
            float entrance=Mathf.Clamp01(_sunsetWarning.Age/.25f);
            float fade=_sunsetWarning.Stage==SunsetWarningStage.Imminent?1f:Mathf.Clamp01((SunsetWarningState.NoticeDuration-_sunsetWarning.Age)/.5f);
            float alpha=entrance*fade;
            var r=SunsetWarningRect;r.y+=8*(1-entrance);
            GeneratedUiSkin.Panel(r,new Color(1,1,1,alpha));
            var old=GUI.color;GUI.color=new Color(1,1,1,alpha);
            if(_sunsetIcon==null)_sunsetIcon=Resources.Load<Texture2D>("UI/Generated20261103/sunset_warning");
            if(_sunsetIcon!=null)GUI.DrawTexture(new Rect(r.x+12,r.y+11,48,48),_sunsetIcon,ScaleMode.ScaleToFit,true);
            Color amber=new Color(1f,.79f,.43f);
            string title=_sunsetWarning.Stage==SunsetWarningStage.Imminent?"일몰이 임박했습니다":"해가 곧 집니다";
            string hint=_sunsetWarning.Stage==SunsetWarningStage.Imminent?"울타리와 모닥불을 확인하세요":"마을로 돌아가 밤을 준비하세요";
            HudText.Label(new Rect(r.x+72,r.y+11,r.width-148,24),title,new GUIStyle(_label){fontSize=17,fontStyle=FontStyle.Bold,normal={textColor=amber}});
            HudText.Label(new Rect(r.x+72,r.y+39,r.width-148,20),hint,new GUIStyle(_labelSubtle){fontSize=12});
            HudText.Label(new Rect(r.xMax-70,r.y+13,58,44),_sunsetWarning.SecondsRemaining+"초",new GUIStyle(_title){fontSize=25,normal={textColor=amber}});
            GUI.color=old;
        }
    }
}
