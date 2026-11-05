using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public sealed class QuestFramePreviewWindow : EditorWindow
    {
        private GameObject _root;
        private SimpleHud _hud;
        private readonly BindingFlags _flags=BindingFlags.Instance|BindingFlags.NonPublic;
        public static void Open()
        {
            var window=CreateInstance<QuestFramePreviewWindow>();window.titleContent=new GUIContent("InisLand Quest Frame QA");
            window.position=new Rect(40,40,1000,520);window.ShowUtility();
        }
        private void OnEnable()
        {
            _root=new GameObject("Quest frame QA"){hideFlags=HideFlags.HideAndDontSave};
            var manager=_root.AddComponent<SettlementGoalManager>();manager.enabled=false;
            typeof(SettlementGoalManager).GetMethod("Awake",_flags).Invoke(manager,null);
            var views=(List<SettlementGoalManager.GoalView>)typeof(SettlementGoalManager).GetField("_views",_flags).GetValue(manager);
            views.Clear();views.Add(new SettlementGoalManager.GoalView{Title="식량 확보",Detail="농장 2개와 식량 창고를 짓고 식량 35를 모으세요.",Progress="농장 2/2  창고 0/1  식량 18/35"});
            views.Add(new SettlementGoalManager.GoalView{Title="마을 방어"});views.Add(new SettlementGoalManager.GoalView{Title="따뜻한 중심"});
            _hud=_root.AddComponent<SimpleHud>();_hud.enabled=false;
        }
        private void OnGUI()
        {
            if(_hud==null)return;
            EditorGUI.DrawRect(new Rect(0,0,position.width,position.height),new Color(.78f,.84f,.87f));
            var oldFont=GUI.skin.font;GUI.skin.font=UiTheme.HudFont;
            typeof(SimpleHud).GetMethod("EnsureStyles",_flags).Invoke(_hud,null);
            GUI.Label(new Rect(32,16,900,30),"퀘스트 프레임 · 실제 런타임 그리기",new GUIStyle(GUI.skin.label){fontSize=18,normal={textColor=new Color(.12f,.16f,.2f)}});
            var old=GUI.matrix;var origin=QuestTrackerLayout.Panel(MobileHudLayout.Width,true).position;
            for(int i=0;i<3;i++)
            {
                GUI.matrix=old;
                GUI.Label(new Rect(32+i*320,58,280,24),i==0?"기본 보기":i==1?"펼친 보기":"이야기 목표",new GUIStyle(GUI.skin.label){fontSize=14,normal={textColor=Color.black}});
                typeof(SimpleHud).GetField("_tutorialDismissed",_flags).SetValue(_hud,i!=2);
                typeof(SimpleHud).GetField("_goalsCollapsed",_flags).SetValue(_hud,i==0);
                if(i==2)typeof(SimpleHud).GetField("_storyTutorial",_flags).SetValue(_hud,new StoryTutorial((int)StoryTutorialStage.Explore));
                GUI.matrix=Matrix4x4.Translate(new Vector3(32+i*320-origin.x,96-origin.y,0));
                typeof(SimpleHud).GetMethod("DrawSettlementGoals",_flags).Invoke(_hud,null);
            }
            GUI.matrix=old;GUI.skin.font=oldFont;
        }
        private void OnDisable(){if(_root!=null)DestroyImmediate(_root);}
    }
}
