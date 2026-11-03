using UnityEngine;
namespace IL6
{
    public sealed partial class SimpleHud
    {
        private bool _villageReturnActive;
        private int _lastVillageReturnDay=-1;
        private Texture2D _villageReturnArt;
        private bool VillageReturnVisible=>_villageReturnActive && Player!=null && !Player.IsDead
            && GameSession.Instance!=null && GameSession.Instance.Cycle!=null && GameSession.Instance.Cycle.Phase==Phase.Evening;
        private void UpdateVillageReturn()
        {
            var cycle=GameSession.Instance!=null?GameSession.Instance.Cycle:null;
            if(cycle==null || cycle.Phase!=Phase.Evening){_villageReturnActive=false;return;}
            _villageReturnActive=true;
            if(_lastVillageReturnDay!=cycle.Day)_teleportPending=true;
            if(_teleportPending && VillageReturnPresentation.At(cycle.ElapsedInPhase,cycle.PhaseDurationSec).ReturnDue)
            {
                BringEveryoneToVillage();_teleportPending=false;_lastVillageReturnDay=cycle.Day;
            }
        }
        private void DrawVillageReturnIllustration()
        {
            if(!VillageReturnVisible)return;
            var cycle=GameSession.Instance.Cycle;
            var frame=VillageReturnPresentation.At(cycle.ElapsedInPhase,cycle.PhaseDurationSec);
            DrawVillageReturnFrame(frame);
        }
        private void DrawVillageReturnFrame(VillageReturnFrame frame)
        {
            var screen=new Rect(0,0,MobileHudLayout.Width,MobileHudLayout.Height);
            UiTheme.Rect(screen,new Color(.015f,.025f,.045f,frame.CoverAlpha));
            if(_villageReturnArt==null)_villageReturnArt=Resources.Load<Texture2D>("UI/StoryIllustrations/village_return");
            var old=GUI.color;
            if(_villageReturnArt!=null && frame.IllustrationAlpha>0)
            {
                GUI.color=new Color(1,1,1,frame.IllustrationAlpha);
                GUI.DrawTexture(screen,_villageReturnArt,ScaleMode.ScaleToFit,true);
                GUI.color=old;
                UiTheme.Rect(new Rect(0,screen.height-112,screen.width,112),new Color(.015f,.025f,.04f,.55f*frame.IllustrationAlpha));
                var title=new GUIStyle(_title){fontSize=24,normal={textColor=new Color(1f,.91f,.73f,frame.IllustrationAlpha)}};
                var sub=new GUIStyle(_labelSubtle){fontSize=15,alignment=TextAnchor.MiddleCenter,normal={textColor=new Color(.9f,.94f,.98f,frame.IllustrationAlpha)}};
                HudText.Label(new Rect(20,screen.height-94,screen.width-40,36),"마을로 돌아가는 길",title);
                HudText.Label(new Rect(20,screen.height-51,screen.width-40,26),"밤을 준비합니다",sub);
            }
            if(frame.NoticeAlpha>0)
            {
                var title=new GUIStyle(_title){fontSize=30,normal={textColor=new Color(1f,.9f,.68f,frame.NoticeAlpha)}};
                var sub=new GUIStyle(_labelSubtle){fontSize=16,alignment=TextAnchor.MiddleCenter,normal={textColor=new Color(.9f,.94f,.98f,frame.NoticeAlpha)}};
                HudText.Label(new Rect(20,screen.height/2-32,screen.width-40,48),"해가 졌습니다",title);
                HudText.Label(new Rect(20,screen.height/2+28,screen.width-40,30),"마을로 돌아갑니다",sub);
            }
            GUI.color=old;
        }
    }
}
