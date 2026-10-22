using UnityEngine;

namespace IL6
{
    public sealed partial class SimpleHud
    {
        private bool _goalsCollapsed;
        private Texture2D _questTrackerPanel;

        private Rect GoalTrackerBounds
        {
            get
            {
                var panel = MobileHudLayout.Goals;
                if (_goalsCollapsed) panel.height = 40;
                else
                {
                    var manager = SettlementGoalManager.Instance;
                    if (manager == null || manager.Goals == null || manager.Goals.Count == 0) return Rect.zero;
                    bool complete = true;
                    foreach (var goal in manager.Goals) if (!goal.Completed) { complete = false; break; }
                    if (complete) panel.height = 100;
                }
                return panel;
            }
        }

        private void DrawSettlementGoals()
        {
            var manager = SettlementGoalManager.Instance;
            if (manager == null || manager.Goals == null || manager.Goals.Count == 0) return;
            var goals = manager.Goals;
            int completed = 0, active = -1;
            for (int i = 0; i < goals.Count; i++)
            {
                if (goals[i].Completed) completed++;
                else if (active < 0) active = i;
            }

            var panel = GoalTrackerBounds;
            DrawQuestTrackerBacking(panel);

            var gold = new Color(1f, .82f, .43f);
            var subtle = new Color(.76f, .81f, .79f);
            var heading = new GUIStyle(_label) { fontSize = 12, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft, normal = { textColor = gold } };
            HudText.Label(new Rect(panel.x+16,panel.y+3,112,32),"초기 목표",heading);
            HudText.Label(new Rect(panel.xMax-104,panel.y+3,48,32),$"{completed}/{goals.Count}",
                new GUIStyle(heading) { alignment = TextAnchor.MiddleRight });
            var toggle = new Rect(panel.xMax-48,panel.y,44,40);
            if (GUI.Button(toggle,new GUIContent(_goalsCollapsed?"+":"−","목표 목록 접기 / 펼치기"),
                new GUIStyle(_smallBtn) { fontSize = 20 })) _goalsCollapsed = !_goalsCollapsed;
            if (_goalsCollapsed) return;

            if (active < 0)
            {
                HudText.Label(new Rect(panel.x+16,panel.y+44,panel.width-32,24),"모든 초기 목표 달성",new GUIStyle(_section) { fontSize = 16 });
                HudText.Label(new Rect(panel.x+16,panel.y+70,panel.width-32,20),"섬을 탐험하며 마을을 키워보세요",new GUIStyle(_labelSubtle) { fontSize = 12 });
                return;
            }

            var current = goals[active];
            HudText.Label(new Rect(panel.x+24,panel.y+39,panel.width-40,28),current.Title,
                new GUIStyle(_section) { fontSize = 17, normal = { textColor = Color.white } });
            var conditions = current.Progress.Split(new[] { "  " },System.StringSplitOptions.RemoveEmptyEntries);
            var conditionStyle = new GUIStyle(_labelSubtle) { fontSize = 13, normal = { textColor = subtle } };
            for (int i = 0; i < conditions.Length && i < 3; i++)
            {
                string condition = conditions[i].Trim();
                int lastSpace = condition.LastIndexOf(' ');
                string label = lastSpace > 0 ? condition.Substring(0,lastSpace) : condition;
                string value = lastSpace > 0 ? condition.Substring(lastSpace+1) : "";
                float y = panel.y+69+i*21;
                HudText.Label(new Rect(panel.x+24,y,panel.width-112,20),label,conditionStyle);
                HudText.Label(new Rect(panel.xMax-90,y,72,20),value,
                    new GUIStyle(conditionStyle) { alignment = TextAnchor.MiddleRight, normal = { textColor = Color.white } });
            }

            UiTheme.Rect(new Rect(panel.x+16,panel.y+138,panel.width-32,1),new Color(.8f,.72f,.49f,.25f));
            int shown = 0;
            for (int i = active+1; i < goals.Count && shown < 2; i++)
            {
                if (goals[i].Completed) continue;
                float y = panel.y+146+shown*27;
                HudText.Label(new Rect(panel.x+17,y,32,22),"다음",new GUIStyle(conditionStyle) { fontSize = 11 });
                HudText.Label(new Rect(panel.x+54,y,panel.width-70,22),goals[i].Title,
                    new GUIStyle(_label) { fontSize = 13, normal = { textColor = subtle } });
                shown++;
            }
        }

        private void DrawQuestTrackerBacking(Rect panel)
        {
            if (_questTrackerPanel == null) _questTrackerPanel = Resources.Load<Texture2D>("UI/Generated20261018/quest_tracker");
            if (_questTrackerPanel != null)
            {
                var old = GUI.color;
                GUI.color = new Color(1f,1f,1f,.86f);
                // Use only the generated panel region; leave its surrounding presentation backdrop out of the HUD.
                GUI.DrawTextureWithTexCoords(panel,_questTrackerPanel,new Rect(30f/1536f,226f/1024f,1477f/1536f,596f/1024f),true);
                GUI.color = old;
            }
            else GeneratedUiSkin.Panel(panel,new Color(.4f,.48f,.47f,.86f));
        }
    }
}
