using UnityEngine;
using System.Collections.Generic;

namespace IL6
{
    public sealed partial class SimpleHud
    {
        private bool _showCompanions;
        internal void PrepareHudPreview() { _tutorialDismissed=true; ReleaseStoryPause(); _storyWaitingForGui=false; InputReader.HudBlocked=false; }
        private void HandleSecondaryTouch()
        {
            if(_buildPaused) return;
            for (int i=1; i<Input.touchCount; i++)
            {
                var touch=Input.GetTouch(i);
                if(touch.phase!=TouchPhase.Ended) continue;
                var p=MobileHudLayout.GuiPoint(touch.position);
                if(MobileButtonTheme.Contains(MenuButtonRect,p)) { ToggleMenu(); continue; }
                if(!ControlsBlocked && MobileHudLayout.ResourceTouchBounds.Contains(p)) { _resourceDetailsOpen=!_resourceDetailsOpen; continue; }
                if(!ControlsBlocked && MobileButtonTheme.Contains(MobileHudLayout.CompanionButton,p)) { _showCompanions=!_showCompanions; continue; }
                if(ControlsBlocked && !BuildUiAvailable) continue;
                for(int slot=0;slot<5;slot++)
                {
                    if(slot==4 && !_buildPaused && _pendingBuildKind==null) continue;
                    if(MobileButtonTheme.Contains(MobileHudLayout.ActionButton(slot),p)) { InvokeHudAction(slot); break; }
                }
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
                case 3:UseUnifiedAction();break;
                case 4:if(_pendingBuildKind!=null) _pendingBuildKind=null;else UseUnifiedAction();break;
            }
        }
        private bool ControlsBlocked => _villageReturnActive || _morningFenceOpen || _workMenuOpen || _researchOpen || _buildPaused || _paused || _buildCatalogOpen || _tutorialPaused || Time.timeScale <= 0f
            || (Player != null && Player.IsDead)
            || (Progression != null && Progression.LevelUpPending)
            || TutorialDialogueBlocking
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

        public void UseUnifiedAction()
        {
            if (ControlsBlocked || _pendingBuildKind != null || (_hudMode == HudMode.Build && !_buildRepairMode)) return;
            if (_buildRepairMode) { if(Attacker!=null) Attacker.TryManualAction();return; }
            if (Attacker != null)
            {
                Attacker.TryManualAction();
                return; // A blocked action never falls through into another action or spends resources.
            }
            UsePrimaryAction();
        }

        public bool TryGetContextAction(out System.Action callback, out string label)
        {
            callback=null;label=null;
            if(ControlsBlocked || _pendingBuildKind!=null || (_hudMode==HudMode.Build && !_buildRepairMode)
                || (Player!=null && Player.Actions!=null && Player.Actions.IsBusy)) return false;
            foreach(var action in NearbyActions())
            {
                if(!action.Enabled || action.Callback==null) continue;
                callback=action.Callback;label=action.Label;
                return true;
            }
            return false;
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
            if (Gather != null && !Gather.IsActive) Gather.TryStartNearby(Attacker != null ? Attacker.ManualTreeInteractRadius : 2.2f);
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
            if (_buildPaused) return BuildPlacementControlsRect.Contains(point) || BuildPlacementFeedbackRect.Contains(point);
            if (_resourceDetailsOpen && ResourceDetailsRect.Contains(point)) return true;
            if (MobileHudLayout.Status.Contains(point) || MobileHudLayout.Resources.Contains(point) || MobileHudLayout.Experience.Contains(point)
                || (!_resourceDetailsOpen && GoalTrackerBounds.Contains(point)) || MobileHudLayout.Actions.Contains(point)
                || MobileHudLayout.Movement.Contains(point) || MobileHudLayout.Menu.Contains(point) || MobileHudLayout.Clock.Contains(point)) return true;
            if (MobileHudLayout.CompanionButton.Contains(point)) return true;
            if(SunsetWarningVisible && SunsetWarningRect.Contains(point))return true;
            if (!_resourceDetailsOpen && _showCompanions && MobileHudLayout.CompanionSummary(true).Contains(point)) return true;
            if (!_resourceDetailsOpen && _hudMode!=HudMode.Build && HudLayout.WorkLauncher().Contains(point) && NearbyActions().Count>0) return true;
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
            string attackLabel=Attacker!=null?Attacker.ManualActionLabel:"공격";
            string[] labels={"근접","활","건설",CompactActionLabel(attackLabel),actionName};
            string[] keys=Application.isMobilePlatform ? new[]{"","","","",""} : new[]{"1","2","3","Space","E"};
            HudGlyph[] icons={HudGlyph.Sword,HudGlyph.Bow,HudGlyph.Hammer,HudGlyph.Sword,HudGlyph.Hand};
            if(attackLabel!="공격") icons[3]=attackLabel=="벌목" || attackLabel=="채굴" || attackLabel=="수리" ? HudGlyph.Hammer : HudGlyph.Hand;
            for(int i=0;i<5;i++)
            {
                if(i==4 && !_buildPaused && _pendingBuildKind==null) continue; // One main action button for desktop and touch.
                bool active=i==0 && _hudMode==HudMode.Melee || i==1 && _hudMode==HudMode.Ranged || i==2 && _hudMode==HudMode.Build;
                bool enabled=i==3 ? Attacker!=null && (_hudMode!=HudMode.Build || _buildRepairMode) : true;
                if (_buildPaused) enabled = BuildUiAvailable && (i == 0 || i == 1 || i == 2 || i == 4);
                Color tint=i==3 || i==4 ? MobileButtonTheme.Jade : i==2 ? MobileButtonTheme.Gold
                    : i==1 ? new Color(.18f,.40f,.29f) : new Color(.31f,.36f,.44f);
                if(MobileButtonTheme.Button(MobileHudLayout.ActionButton(i),labels[i],keys[i],icons[i],tint,enabled,active)) InvokeHudAction(i);
            }
            Vector2 axis=Vector2.zero;
            if(Player!=null) { var input=Player.GetComponent<InputReader>();if(input!=null) axis=input.MoveAxis; }
            MobileButtonTheme.Pad(MobileHudLayout.Movement,axis);
            HudText.Label(new Rect(MobileHudLayout.Movement.x,MobileHudLayout.Movement.yMax-20,MobileHudLayout.Movement.width,20),Application.isMobilePlatform?"이동":"이동 · WASD",new GUIStyle(_label){fontSize=12,alignment=TextAnchor.MiddleCenter});
            var companion=MobileHudLayout.CompanionButton;
            if(MobileButtonTheme.Button(companion,RecruitableNpc.CurrentCompanionCount().ToString(),Application.isMobilePlatform?"":"G",HudGlyph.People,MobileButtonTheme.Gold,true,_showCompanions)) _showCompanions=!_showCompanions;
            GUI.enabled=oldEnabled;
        }

        private static string CompactActionLabel(string label)
        {
            if(string.IsNullOrEmpty(label)) return "공격";
            if(label.StartsWith("불 보충")) return "불 보충";
            if(label.StartsWith("시설 강화")) return "시설 강화";
            int end=label.IndexOf(' ');
            return end>0 ? label.Substring(0,end) : label;
        }
    }
}
