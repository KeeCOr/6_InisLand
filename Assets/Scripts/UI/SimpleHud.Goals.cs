using UnityEngine;
namespace IL6
{
    public sealed partial class SimpleHud
    {
        private bool _goalsCollapsed=true;
        private Rect GoalTrackerBounds
        {
            get
            {
                if(!_tutorialDismissed && _storyTutorial!=null && (!_storyTutorial.IsDialogue || _storyTutorial.NpcGuided) && !_storyTutorial.IsComplete)
                {var tutorial=MobileHudLayout.Goals;tutorial.height=160;return tutorial;}
                var manager=SettlementGoalManager.Instance;
                if(manager==null || manager.Goals==null || manager.Goals.Count==0)return Rect.zero;
                int remaining=0;
                foreach(var goal in manager.Goals)if(!goal.Completed)remaining++;
                return QuestTrackerLayout.Panel(MobileHudLayout.Width,_goalsCollapsed,remaining==0,Mathf.Max(0,remaining-1));
            }
        }
        private void DrawSettlementGoals()
        {
            if(DrawStoryObjective())return;
            var manager=SettlementGoalManager.Instance;
            if(manager==null || manager.Goals==null || manager.Goals.Count==0)return;
            var goals=manager.Goals;int completed=0,active=-1;
            for(int i=0;i<goals.Count;i++)if(goals[i].Completed)completed++;else if(active<0)active=i;
            var panel=GoalTrackerBounds;DrawQuestTrackerBacking(panel);
            var heading=new GUIStyle(_label){fontSize=16,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft,normal={textColor=new Color(1f,.94f,.70f)}};
            QuestLabel(QuestTrackerLayout.Header(panel),active>=0?goals[active].Title:"마을 복구 완료",heading);
            QuestLabel(QuestTrackerLayout.Count(panel),$"{completed}/{goals.Count}",new GUIStyle(heading){fontSize=12,alignment=TextAnchor.MiddleRight});
            if(active>=0)
            {
                var toggle=QuestTrackerLayout.Toggle(panel);
                QuestLabel(toggle,_goalsCollapsed?"+":"−",new GUIStyle(heading){fontSize=22,alignment=TextAnchor.MiddleCenter});
                if(GUI.Button(toggle,new GUIContent("",_goalsCollapsed?"퀘스트 상세 펼치기":"퀘스트 상세 접기"),GUIStyle.none))_goalsCollapsed=!_goalsCollapsed;
            }
            // Keep this event's geometry stable until the next repaint after toggling.
            bool collapsed=panel.height<200;
            if(active<0)
            {
                QuestLabel(new Rect(panel.x+16,panel.y+80,panel.width-32,22),"섬을 탐험하며 마을을 키워보세요",new GUIStyle(_labelSubtle){fontSize=13});return;
            }
            var current=goals[active];
            if(!collapsed)QuestLabel(QuestTrackerLayout.Detail(panel),current.Detail,new GUIStyle(_labelSubtle){fontSize=13,wordWrap=true,alignment=TextAnchor.UpperLeft,normal={textColor=Color.white}});
            var rows=QuestTrackerLayout.Rows(current.Progress);
            for(int i=0;i<rows.Length && i<3;i++)
            {
                var row=QuestTrackerLayout.Condition(panel,collapsed,i);
                var color=rows[i].Complete?new Color(.62f,.87f,.74f):Color.white;
                var style=new GUIStyle(_label){fontSize=14,alignment=TextAnchor.MiddleLeft,normal={textColor=color}};
                QuestLabel(new Rect(row.x,row.y,row.width-88,row.height),rows[i].Label,style);
                QuestLabel(new Rect(row.xMax-80,row.y,80,row.height),rows[i].Value,new GUIStyle(style){alignment=TextAnchor.MiddleRight,fontStyle=FontStyle.Bold});
            }
            if(collapsed)return;
            bool hasNext=false;
            for(int i=active+1;i<goals.Count;i++)if(!goals[i].Completed){hasNext=true;break;}
            if(!hasNext)return;
            UiTheme.Rect(new Rect(panel.x+16,panel.y+210,panel.width-32,1),new Color(.72f,.79f,.82f,.22f));
            QuestLabel(new Rect(panel.x+16,panel.y+218,panel.width-32,18),"다음 목표",new GUIStyle(_labelSubtle){fontSize=11});
            int shown=0;
            for(int i=active+1;i<goals.Count && shown<2;i++)
            {
                if(goals[i].Completed)continue;
                QuestLabel(QuestTrackerLayout.Next(panel,shown++),goals[i].Title,new GUIStyle(_labelSubtle){fontSize=13});
            }
        }
        private void DrawQuestTrackerBacking(Rect panel)=>QuestTrackerFrameSkin.Draw(panel);
        private static void QuestLabel(Rect rect,string text,GUIStyle style)
        {
            var shadow=new GUIStyle(style){normal={textColor=new Color(.025f,.035f,.04f,.95f)}};
            HudText.Label(new Rect(rect.x-1,rect.y-1,rect.width,rect.height),text,shadow);
            HudText.Label(new Rect(rect.x+1,rect.y+1,rect.width,rect.height),text,shadow);
            HudText.Label(rect,text,style);
        }
    }
}
