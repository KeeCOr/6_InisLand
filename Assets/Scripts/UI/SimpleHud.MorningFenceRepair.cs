using UnityEngine;

namespace IL6
{
    public sealed partial class SimpleHud
    {
        private bool _morningFencePending,_morningFenceOpen,_morningFenceArmed;
        private int _lastFenceRepairDay=-1;
        private float _morningFencePreviousScale=1;
        private MorningFenceRepair _morningFencePlan;
        public bool MorningFenceRepairOpen=>_morningFenceOpen;
        public int MorningFenceRepairCost=>_morningFencePlan!=null?_morningFencePlan.WoodCost:0;

        private void QueueMorningFenceRepair(int day)
        {
            if(day<=_lastFenceRepairDay)return;
            _lastFenceRepairDay=day;_morningFencePending=true;
        }
        private void UpdateMorningFenceRepair()
        {
            if(_morningFenceOpen)
            {
                if(!_morningFenceArmed && !Input.GetKey(KeyCode.Return) && !Input.GetKey(KeyCode.Space)
                    && !Input.GetMouseButton(0) && Input.touchCount==0)_morningFenceArmed=true;
                return;
            }
            if(!_morningFencePending || ControlsBlocked || Player==null || Player.IsDead || GameSession.Instance==null)return;
            if(Gather!=null && Gather.IsActive){Gather.Cancel();return;}
            if(Player.Actions!=null && Player.Actions.IsBusy)return;
            _morningFencePending=false;_morningFencePlan=MorningFenceRepair.Capture();
            if(_morningFencePlan.WoodCost==0){_morningFencePlan=null;return;}
            _morningFencePreviousScale=Time.timeScale;_morningFenceOpen=true;_morningFenceArmed=false;
            _resourceDetailsOpen=false;_showCompanions=false;
            Time.timeScale=0;InputReader.HudBlocked=true;
        }
        public bool ConfirmMorningFenceRepair()
        {
            if(!_morningFenceOpen || Player==null || Player.IsDead || GameSession.Instance==null || _morningFencePlan==null)return false;
            if(!_morningFencePlan.TryApply(GameSession.Instance.Resources))return false;
            DeclineMorningFenceRepair();GameSession.Instance.SaveNow();Sfx.Build();return true;
        }
        public void DeclineMorningFenceRepair()
        {
            _morningFencePending=false;
            if(!_morningFenceOpen)return;
            _morningFenceOpen=false;_morningFencePlan=null;
            bool other=_paused || _researchOpen || _workMenuOpen || _buildPaused || _tutorialPaused || _recruitCutPaused
                || (Player!=null && Player.IsDead) || (Progression!=null && Progression.LevelUpPending);
            Time.timeScale=other?0:Mathf.Max(.0001f,_morningFencePreviousScale);
            InputReader.HudBlocked=ControlsBlocked;
        }
        private void DrawMorningFenceRepair()
        {
            var store=GameSession.Instance!=null?GameSession.Instance.Resources:null;
            int cost=MorningFenceRepairCost,wood=store!=null?store.Get(ResourceKind.Wood):0;
            bool canPay=cost>0 && wood>=cost;
            if(Event.current.type==EventType.KeyDown)
            {
                if(Event.current.keyCode==KeyCode.Escape){DeclineMorningFenceRepair();Event.current.Use();return;}
                if(Event.current.keyCode==KeyCode.Return && _morningFenceArmed && canPay)
                {ConfirmMorningFenceRepair();Event.current.Use();return;}
            }
            UiTheme.Rect(new Rect(0,0,MobileHudLayout.Width,MobileHudLayout.Height),new Color(.02f,.04f,.05f,.72f));
            var panel=HudLayout.CenterModal(460,310);UiTheme.Panel(panel);UiTheme.TitleBar(panel,"아침 · 울타리 보수",_title);
            var centered=new GUIStyle(_label){fontSize=17,alignment=TextAnchor.MiddleCenter,wordWrap=true};
            HudText.Label(new Rect(panel.x+24,panel.y+60,panel.width-48,40),"울타리를 모두 보수할까요?",centered);
            HudText.Label(new Rect(panel.x+24,panel.y+105,panel.width-48,30),$"손상 {_morningFencePlan.DamagedCount}칸 · 파괴 {_morningFencePlan.BrokenCount}칸",centered);
            DrawHudIcon(new Rect(panel.center.x-92,panel.y+151,32,32),ResourceIconKey(ResourceKind.Wood));
            GUI.Label(new Rect(panel.center.x-92,panel.y+151,32,32),new GUIContent("","목재 보유량 / 보수 비용"),GUIStyle.none);
            HudText.Label(new Rect(panel.center.x-52,panel.y+150,154,34),$"{wood} / {cost}",centered);
            HudText.Label(new Rect(panel.x+24,panel.y+193,panel.width-48,28),canPay?"한 칸당 목재 1개 · 파괴된 곳도 복구":cost==0?"보수할 울타리가 없습니다":"목재가 부족합니다",new GUIStyle(_labelSubtle){alignment=TextAnchor.MiddleCenter});
            float bw=(panel.width-60)/2;
            if(UiTheme.Button(new Rect(panel.x+20,panel.yMax-66,bw,44),Application.isMobilePlatform?"다음에":"다음에 · ESC",_btn))DeclineMorningFenceRepair();
            if(UiTheme.Button(new Rect(panel.x+40+bw,panel.yMax-66,bw,44),Application.isMobilePlatform?"보수하기":"보수하기 · Enter",_btn,canPay && _morningFenceArmed))ConfirmMorningFenceRepair();
        }
    }
}
