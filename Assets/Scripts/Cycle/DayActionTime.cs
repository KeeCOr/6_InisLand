using UnityEngine;
namespace IL6
{
    public static class DayActionTime
    {
        public static float Delta(Phase phase,PlayerController player,float elapsed)
        {
            if(elapsed<=0)return 0;
            if(phase!=Phase.Day)return elapsed;
            if(player==null || player.IsDead || InputReader.HudBlocked)return 0;
            var action=player.Actions;
            bool active=action!=null && (action.State==PlayerAction.Move || action.State==PlayerAction.Attack || action.State==PlayerAction.Logging || action.State==PlayerAction.Gather);
            var visual=player.GetComponent<CharacterActionVisual>();
            return active || (visual!=null && visual.IsPlaying)?elapsed:0;
        }
    }
}
