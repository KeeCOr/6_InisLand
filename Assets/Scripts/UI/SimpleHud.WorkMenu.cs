using System.Collections.Generic;
using UnityEngine;

namespace IL6
{
    public sealed partial class SimpleHud
    {
        private bool _workMenuOpen;
        private float _workMenuPreviousScale=1f;
        private Vector2 _workMenuScroll;
        private List<ContextAction> _workMenuActions;

        public bool OpenNearbyWorkMenu()
        {
            if (ControlsBlocked || _pendingBuildKind!=null || (Player!=null && Player.Actions!=null && Player.Actions.IsBusy)) return false;
            var actions=NearbyActions();
            if (actions.Count==0) return false;
            _workMenuActions=actions;_workMenuScroll=Vector2.zero;
            _workMenuPreviousScale=Time.timeScale;
            _resourceDetailsOpen=false;_workMenuOpen=true;
            Time.timeScale=0f;InputReader.HudBlocked=true;
            return true;
        }

        private void CloseNearbyWorkMenu()
        {
            if (!_workMenuOpen) return;
            _workMenuOpen=false;_workMenuActions=null;
            bool anotherPause=_paused || _researchOpen || _buildPaused || _tutorialPaused || _recruitCutPaused
                || (Player!=null && Player.IsDead) || (Progression!=null && Progression.LevelUpPending);
            Time.timeScale=anotherPause?0f:(_workMenuPreviousScale>0f?_workMenuPreviousScale:1f);
            InputReader.HudBlocked=ControlsBlocked;
        }

        public bool TryUseWorkMenuAction(int index)
        {
            if (!_workMenuOpen || _workMenuActions==null || index<0 || index>=_workMenuActions.Count
                || Player==null || Player.IsDead) return false;
            var action=_workMenuActions[index];
            if (!action.Enabled || action.Callback==null) return false;
            CloseNearbyWorkMenu();
            action.Callback();
            return true;
        }

        private void DrawNearbyWorkMenu()
        {
            if (Event.current.type==EventType.KeyDown && Event.current.keyCode==KeyCode.Escape)
            { CloseNearbyWorkMenu();Event.current.Use();return; }
            UiTheme.Rect(new Rect(0,0,MobileHudLayout.Width,MobileHudLayout.Height),new Color(.02f,.04f,.05f,.74f));
            var panel=HudLayout.CenterModal(460,500);
            UiTheme.Panel(panel);UiTheme.TitleBar(panel,"주변 작업 · 선택 후 실행",_title);
            var viewport=new Rect(panel.x+16,panel.y+58,panel.width-32,panel.height-132);
            int count=_workMenuActions!=null?_workMenuActions.Count:0;
            _workMenuScroll=GUI.BeginScrollView(viewport,_workMenuScroll,new Rect(0,0,viewport.width-20,count*56));
            int chosen=-1;
            var rowStyle=new GUIStyle(_smallBtn) { wordWrap=true,fontSize=13 };
            for (int i=0;i<count;i++)
            {
                var action=_workMenuActions[i];
                if (UiTheme.Button(new Rect(0,i*56,viewport.width-24,48),action.Label,rowStyle,action.Enabled)) chosen=i;
            }
            GUI.EndScrollView();
            if (chosen>=0) { TryUseWorkMenuAction(chosen);return; }
            if (UiTheme.Button(new Rect(panel.x+16,panel.yMax-60,panel.width-32,44),"닫기 · ESC",_btn)) CloseNearbyWorkMenu();
        }
    }
}
