using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace IL6
{
    public sealed class LobbyController : MonoBehaviour
    {
        public string GameSceneName="OnboardingScene";
        public bool IsLoading {get;private set;}
        public int PendingOverwriteSlot {get;private set;}
        private readonly SaveFileV1[] _slots=new SaveFileV1[SaveLoad.SlotCount];
        private Texture2D _background;
        private string _error;
        private void Awake()
        {
            _background=Resources.Load<Texture2D>("UI/StoryIllustrations/village_return");
            RefreshSlots();
        }
        public void RefreshSlots(){for(int i=0;i<_slots.Length;i++)_slots[i]=SaveLoad.LoadSlot(i+1);}
        public bool TryContinue(int slot)
        {
            if(IsLoading || slot<1 || slot>SaveLoad.SlotCount || SaveLoad.LoadSlot(slot)==null)return false;
            StartCoroutine(Launch(slot,false));return true;
        }
        public void RequestNew(int slot)
        {
            if(IsLoading || slot<1 || slot>SaveLoad.SlotCount)return;
            if(SaveLoad.HasSlotFile(slot)){PendingOverwriteSlot=slot;return;}
            StartCoroutine(Launch(slot,true));
        }
        public void CancelOverwrite()=>PendingOverwriteSlot=0;
        public void ConfirmOverwrite()
        {
            if(PendingOverwriteSlot==0 || IsLoading)return;
            int slot=PendingOverwriteSlot;PendingOverwriteSlot=0;StartCoroutine(Launch(slot,true));
        }
        private IEnumerator Launch(int slot,bool fresh)
        {
            IsLoading=true;SaveLoad.SelectSlot(slot);
            if(GameSession.Instance!=null)
            {
                GameSession.Instance.gameObject.SetActive(false);
                Destroy(GameSession.Instance.gameObject);
                yield return null;
            }
            // Keep the old file until the replacement has been written successfully.
            GameSession.IgnoreSaveOnce=fresh;
            var session=new GameObject("GameSession").AddComponent<GameSession>();
            session.enabled=false;
            if(fresh)session.NpcTutorialEnabled=true;
            if(fresh && !SaveLoad.Save(new SaveFileV1{npcTutorialEnabled=true,storyTutorialEnabled=true,resources=session.Resources.Snapshot(),currentDay=1}))
            {_error="저장하지 못했습니다. 저장 공간을 확인해 주세요.";IsLoading=false;RefreshSlots();yield break;}
            Time.timeScale=1;InputReader.HudBlocked=false;session.enabled=true;
            yield return SceneManager.LoadSceneAsync(GameSceneName);
        }
        private GUIStyle Text(int size,Color color,TextAnchor align=TextAnchor.MiddleLeft)=>
            new GUIStyle(GUI.skin.label){font=UiTheme.HudFont,fontSize=size,alignment=align,wordWrap=true,normal={textColor=color}};
        private void OnGUI()
        {
            var matrix=GUI.matrix;GUI.matrix=Matrix4x4.Scale(Vector3.one*MobileHudLayout.Scale);
            float width=MobileHudLayout.Width,height=MobileHudLayout.Height;
            if(_background!=null)GUI.DrawTexture(new Rect(0,0,width,height),_background,ScaleMode.ScaleAndCrop,false);
            UiTheme.Rect(new Rect(0,0,width,height),new Color(.015f,.035f,.05f,.52f));
            HudText.Label(new Rect(0,24,width,76),"이니스랜드",Text(44,UiTheme.TextCream,TextAnchor.MiddleCenter));
            HudText.Label(new Rect(0,100,width,32),"아버지가 돌아올 마을",Text(18,UiTheme.TextGold,TextAnchor.MiddleCenter));
            var panel=LobbyLayout.Panel(width,height);UiTheme.Panel(panel);
            HudText.Label(new Rect(panel.x+28,panel.y+20,panel.width-56,38),"모험을 선택하세요",Text(24,UiTheme.TextCream));
            for(int i=0;i<SaveLoad.SlotCount;i++)
            {
                int slot=i+1;var card=LobbyLayout.Slot(panel,i);var saved=_slots[i];
                UiTheme.Rect(card,new Color(.02f,.055f,.07f,.78f));
                UiTheme.Separator(new Rect(card.x,card.y,card.width,2));
                HudText.Label(new Rect(card.x+20,card.y+18,card.width-40,32),$"저장 슬롯 {slot}",Text(21,UiTheme.TextGold));
                string summary=saved!=null?$"{saved.currentDay}일차 · 마을 복구 중":SaveLoad.HasSlotFile(slot)?"읽을 수 없는 저장":"새로운 모험";
                HudText.Label(new Rect(card.x+20,card.y+70,card.width-40,36),summary,Text(18,UiTheme.TextCream));
                string detail="아직 저장된 모험이 없습니다.";
                if(saved!=null)
                {
                    detail="이 슬롯의 모험을 이어갑니다.";
                    if(DateTimeOffset.TryParse(saved.savedAtUtc,out var time))detail="저장 · "+time.ToLocalTime().ToString("yyyy.MM.dd HH:mm");
                }
                HudText.Label(new Rect(card.x+20,card.y+116,card.width-40,52),detail,Text(14,UiTheme.TextSubtle));
                bool enabled=!IsLoading && PendingOverwriteSlot==0;
                if(UiTheme.Button(LobbyLayout.Continue(card),"이어서 하기",Text(18,UiTheme.TextCream),enabled && saved!=null))TryContinue(slot);
                if(UiTheme.Button(LobbyLayout.New(card),"새로 하기",Text(18,UiTheme.TextCream),enabled,UiButtonRole.Secondary))RequestNew(slot);
            }
            if(IsLoading)HudText.Label(new Rect(0,height-56,width,32),"모험을 준비하고 있습니다…",Text(18,UiTheme.TextCream,TextAnchor.MiddleCenter));
            if(!string.IsNullOrEmpty(_error))HudText.Label(new Rect(panel.x,panel.yMax+8,panel.width,40),_error,Text(16,UiTheme.TextDanger,TextAnchor.MiddleCenter));
            if(PendingOverwriteSlot!=0)
            {
                UiTheme.Rect(new Rect(0,0,width,height),new Color(0,0,0,.72f));
                var modal=new Rect((width-520)/2,(height-242)/2,520,242);UiTheme.Panel(modal);
                HudText.Label(new Rect(modal.x+28,modal.y+24,464,100),$"슬롯 {PendingOverwriteSlot}의 모험을 새로 시작할까요?\n기존 저장은 새 모험으로 바뀝니다.",Text(20,UiTheme.TextCream));
                var button=Text(18,UiTheme.TextCream);
                if(UiTheme.Button(new Rect(modal.x+28,modal.y+154,220,52),"돌아가기",button,true,UiButtonRole.Secondary))CancelOverwrite();
                if(UiTheme.Button(new Rect(modal.x+272,modal.y+154,220,52),"새로 시작",button,true,UiButtonRole.Destructive))ConfirmOverwrite();
            }
            GUI.matrix=matrix;
        }
    }
}
