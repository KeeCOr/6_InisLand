using UnityEngine;
using System.Collections.Generic;

namespace IL6
{
    public sealed partial class SimpleHud
    {
        private bool _showCompanions;
        internal void PrepareHudPreview() { _tutorialDismissed=true; _tutorialPaused=false; InputReader.HudBlocked=false; }
        private void HandleSecondaryTouch()
        {
            for (int i=1; i<Input.touchCount; i++)
            {
                var touch=Input.GetTouch(i);
                if(touch.phase!=TouchPhase.Ended) continue;
                var p=MobileHudLayout.GuiPoint(touch.position);
                if(MobileButtonTheme.Contains(MenuButtonRect,p)) { ToggleMenu(); continue; }
                if(!ControlsBlocked && MobileHudLayout.ResourceTouchBounds.Contains(p)) { _resourceDetailsOpen=!_resourceDetailsOpen; continue; }
                if(!ControlsBlocked && MobileButtonTheme.Contains(MobileHudLayout.CompanionButton,p)) { _showCompanions=!_showCompanions; continue; }
                if(ControlsBlocked && !BuildUiAvailable) continue;
                for(int slot=0;slot<7;slot++)
                    if(MobileButtonTheme.Contains(MobileHudLayout.ActionButton(slot),p)) { InvokeHudAction(slot); break; }
            }
        }

        private Rect MenuButtonRect => MobileHudLayout.Menu;
        private void InvokeHudAction(int slot)
        {
            if (BuildUiAvailable)
            {
                if (slot == 0) SetHudMode(HudMode.Melee);
                else if (slot == 1) SetHudMode(HudMode.Ranged);
                else if (slot == 2) SetHudMode(HudMode.Build);
                else if (slot == 4) CloseBuildCatalog();
                return;
            }
            if(ControlsBlocked) return;
            switch(slot)
            {
                case 0:SetHudMode(HudMode.Melee);break;
                case 1:SetHudMode(HudMode.Ranged);break;
                case 2:SetHudMode(HudMode.Build);break;
                case 3:if(Attacker!=null && _hudMode!=HudMode.Build) Attacker.TryManualAction();break;
                case 4:if(_pendingBuildKind!=null) _pendingBuildKind=null;else UsePrimaryAction();break;
                case 6:if(Attacker!=null) Attacker.AutoAttackEnabled=!Attacker.AutoAttackEnabled;break;
            }
        }
        private bool ControlsBlocked => _researchOpen || _buildPaused || _paused || _buildCatalogOpen || _tutorialPaused || Time.timeScale <= 0f
            || (Player != null && Player.IsDead)
            || (Progression != null && Progression.LevelUpPending)
            || (!_tutorialDismissed && PlayerPrefs.GetInt(TutorialPrefKey, 0) == 0)
            || _recruitCutPaused;

        private List<ContextAction> NearbyActions()
        {
            var actions = new List<ContextAction>();
            var session = GameSession.Instance;
            if (session == null || Player == null) return actions;
            AddRepairAction(actions, session);
            AddRefuelAction(actions, session);
            AddUpgradeAction(actions, session);
            AddResearchAction(actions);
            AddBlacksmithActions(actions, session);
            AddFenceActions(actions, session);
            AddFarmActions(actions);
            AddGatherAction(actions, ResourceKind.Wood, "벌목");
            AddGatherAction(actions, ResourceKind.Stone, "채굴");
            actions.Sort((a,b)=>a.Priority.CompareTo(b.Priority));
            return actions;
        }

        public void UsePrimaryAction()
        {
            if (ControlsBlocked || _pendingBuildKind != null) return;
            if (Gather != null && Gather.IsActive) { Gather.Cancel(); return; }
            if (Player != null && Player.Actions != null && Player.Actions.IsBusy) return;
            foreach (var action in NearbyActions())
            {
                if (!action.Enabled) continue;
                action.Callback?.Invoke();
                return;
            }
            if (Gather != null && !Gather.IsActive) Gather.TryStart();
        }

        private void ToggleMenu()
        {
            if (!_paused && ControlsBlocked) return;
            if (!_paused)
            {
                _preTimeScale = Time.timeScale;
                _paused = true;
                Time.timeScale = 0f;
            }
            else
            {
                _paused = false;
                Time.timeScale = _preTimeScale > 0 ? _preTimeScale : 1;
            }
        }

        private bool IsHudPoint(Vector2 point)
        {
            if (ControlsBlocked && !BuildUiAvailable) return true;
            if (_buildCatalogOpen) return true;
            if (_resourceDetailsOpen && ResourceDetailsRect.Contains(point)) return true;
            if (MobileHudLayout.Status.Contains(point) || MobileHudLayout.Resources.Contains(point)
                || MobileHudLayout.Goals.Contains(point) || MobileHudLayout.Actions.Contains(point)
                || MobileHudLayout.Movement.Contains(point) || MobileHudLayout.Menu.Contains(point)) return true;
            if (MobileHudLayout.CompanionButton.Contains(point)) return true;
            if (MobileHudLayout.CompanionSummary(_showCompanions).Contains(point)) return true;
            if (MobileHudLayout.ResourceTouchBounds.Contains(point)) return true;
            // Construction catalog occupies the strip above the movement/action controls.
            return false;
        }

        private void DrawMobileControls()
        {
            if(MobileButtonTheme.Button(MenuButtonRect,"메뉴","ESC",HudGlyph.Menu,new Color(.24f,.31f,.32f))) ToggleMenu();
            bool oldEnabled=GUI.enabled;
            GUI.enabled=!ControlsBlocked || BuildUiAvailable;
            string actionName="작업";
            var selected=NearbyActions().Find(a=>a.Enabled);
            if(selected.Callback!=null)
            {
                actionName=selected.Label;
                int end=actionName.IndexOf(' ');
                if(end>0 && !actionName.StartsWith("문 ") && !actionName.StartsWith("불 ")) actionName=actionName.Substring(0,end);
                if(actionName.StartsWith("불 보충")) actionName="불 보충";
            }
            if(Gather!=null && Gather.IsActive) actionName="중단";
            if(_pendingBuildKind!=null) actionName="취소";
            else if(_buildPaused) actionName="건설 종료";
            var door=Player!=null ? Player.NearbyDoor : null;
            string[] labels={"근접","활","건설","공격",actionName,door!=null && door.IsOpen ? "출입 가능" : "자동 출입",Attacker!=null && Attacker.AutoAttackEnabled ? "자동 ON":"자동 OFF"};
            string[] keys={"1","2","B/3","Space","E","접근","R"};
            HudGlyph[] icons={HudGlyph.Sword,HudGlyph.Bow,HudGlyph.Hammer,HudGlyph.Sword,door!=null ? HudGlyph.Door:HudGlyph.Hand,HudGlyph.Door,HudGlyph.Auto};
            for(int i=0;i<7;i++)
            {
                bool active=i==0 && _hudMode==HudMode.Melee || i==1 && _hudMode==HudMode.Ranged || i==2 && _hudMode==HudMode.Build || i==6 && Attacker!=null && Attacker.AutoAttackEnabled;
                bool enabled=i==3 ? Attacker!=null && _hudMode!=HudMode.Build : i==5 ? false : i==6 ? Attacker!=null : true;
                if (_buildPaused) enabled = BuildUiAvailable && (i == 0 || i == 1 || i == 2 || i == 4);
                Color tint=i==3 || i==4 ? MobileButtonTheme.Jade : i==2 || i==5 ? MobileButtonTheme.Gold : new Color(.22f,.38f,.38f);
                if(MobileButtonTheme.Button(MobileHudLayout.ActionButton(i),labels[i],keys[i],icons[i],tint,enabled,active)) InvokeHudAction(i);
            }
            Vector2 axis=Vector2.zero;
            if(Player!=null) { var input=Player.GetComponent<InputReader>();if(input!=null) axis=input.MoveAxis; }
            MobileButtonTheme.Pad(MobileHudLayout.Movement,axis);
            HudText.Label(new Rect(MobileHudLayout.Movement.x,MobileHudLayout.Movement.yMax-20,MobileHudLayout.Movement.width,20),"이동 · WASD",new GUIStyle(_label){fontSize=12,alignment=TextAnchor.MiddleCenter});
            var companion=MobileHudLayout.CompanionButton;
            if(MobileButtonTheme.Button(companion,"동료","G",HudGlyph.People,MobileButtonTheme.Gold,true,_showCompanions)) _showCompanions=!_showCompanions;
            GUI.enabled=oldEnabled;
        }
    }
}
