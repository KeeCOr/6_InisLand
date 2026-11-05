using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace IL6
{
    public sealed class NewAdventureLoading : MonoBehaviour
    {
        private static readonly string[] Keys={"waiting","storm","ruins","promise"};
        public static readonly string[] Chapters={"떠나던 날","눈폭풍","폭풍이 지나간 뒤","돌아올 곳"};
        public static readonly string[] Story={
            "아버지는 마을 사람들과 함께 떠났다.\n돌아올 테니, 이곳에서 기다려 달라고 했다.",
            "혼자 마을을 지키던 어느 날,\n모든 길을 삼킬 듯한 눈폭풍이 밀려왔다.",
            "폭풍이 지나간 마을은 온통 망가져 있었다.\n집 한 채와 울타리, 작은 불씨만 남았다.",
            "다시 만들 거야.\n아버지가 돌아올 우리 마을을."};
        public int CurrentCard {get;private set;}
        public bool Ready=>_load!=null && _load.progress>=.9f;
        private AsyncOperation _load;
        private bool _enter;
        private readonly Texture2D[] _images=new Texture2D[4];
        public void Advance()
        {
            if(CurrentCard<3){CurrentCard++;return;}
            if(Ready)_enter=true;
        }
        public IEnumerator LoadScene(string scene)
        {
            for(int i=0;i<4;i++)_images[i]=Resources.Load<Texture2D>("UI/StoryIllustrations/"+Keys[i]);
            var session=GameSession.Instance;session.enabled=false;InputReader.HudBlocked=true;
            _load=SceneManager.LoadSceneAsync(scene);_load.allowSceneActivation=false;
            while(!_enter || !Ready)yield return null;
            session.StoryIntroSeen=true;session.StoryVillageRuined=true;session.SaveNow();
            session.enabled=true;InputReader.HudBlocked=false;
            _load.allowSceneActivation=true;yield return _load;
        }
        private void OnGUI()
        {
            var old=GUI.matrix;GUI.matrix=Matrix4x4.Scale(Vector3.one*MobileHudLayout.Scale);
            float w=MobileHudLayout.Width,h=MobileHudLayout.Height;
            UiTheme.Rect(new Rect(0,0,w,h),new Color(.025f,.045f,.065f));
            if(_images[CurrentCard]!=null)GUI.DrawTexture(new Rect(0,0,w,h),_images[CurrentCard],ScaleMode.ScaleToFit,false);
            var panel=new Rect(32,h-242,w-64,210);UiTheme.Panel(panel);
            var title=new GUIStyle(GUI.skin.label){font=UiTheme.HudFont,fontSize=18,normal={textColor=UiTheme.TextGold}};
            var text=new GUIStyle(title){fontSize=23,wordWrap=true,normal={textColor=UiTheme.TextCream}};
            HudText.Label(new Rect(panel.x+24,panel.y+18,panel.width-48,28),Chapters[CurrentCard],title);
            HudText.Label(new Rect(panel.x+24,panel.y+56,panel.width-48,84),Story[CurrentCard],text);
            float progress=_load!=null?Mathf.Clamp01(_load.progress/.9f):0;
            UiTheme.Bar(new Rect(panel.x+24,panel.yMax-38,panel.width-228,8),progress,UiTheme.BarXpFill);
            HudText.Label(new Rect(panel.x+24,panel.yMax-26,panel.width-228,20),Ready?"마을 준비 완료":$"마을을 준비하고 있습니다 · {progress:P0}",new GUIStyle(title){fontSize=12});
            string label=CurrentCard<3?"다음":"마을로";
            if(UiTheme.Button(new Rect(panel.xMax-172,panel.yMax-62,148,48),label,new GUIStyle(title){fontSize=18},CurrentCard<3 || Ready))Advance();
            if(Event.current.type==EventType.KeyDown && Event.current.keyCode==KeyCode.Return){Advance();Event.current.Use();}
            GUI.matrix=old;
        }
    }
}
