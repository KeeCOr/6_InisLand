using UnityEngine;
namespace IL6
{
    public sealed partial class SimpleHud
    {
        private void DrawGatheringState()
        {
            if (_buildPaused || _paused || Player == null || Player.Actions == null) return;
            var actions = Player.Actions;
            if (actions.State != PlayerAction.Logging && actions.State != PlayerAction.Gather) return;
            var panel = new Rect(MobileHudLayout.Width / 2 - 190, MobileHudLayout.Height - 116, 380, 76);
            UiTheme.Panel(panel);
            string name = actions.State == PlayerAction.Logging ? "벌목" : "채집";
            string phase = actions.Phase == ActionPhase.Start ? "준비" : actions.Phase == ActionPhase.Loop ? "작업 중" : "마무리";
            var target = Gather != null ? Gather.ActiveTarget : null;
            var tree = target != null ? target.GetComponent<TreeHealth>() : null;
            string detail = tree != null ? $"나무 HP {tree.CurrentHp}/{tree.MaxHp}" : "E / 중단 · 이동하면 작업 종료";
            HudText.Label(new Rect(panel.x+14,panel.y+8,352,24),$"{name} · {phase}   {detail}",_section);
            UiTheme.Bar(new Rect(panel.x+14,panel.y+43,352,10),Gather != null ? Gather.Progress : 0,MobileButtonTheme.Gold);
        }
    }
}
