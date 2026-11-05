using System.Collections.Generic;
using UnityEngine;
namespace IL6
{
    public sealed partial class SimpleHud
    {
        private const string TutorialPrefKey="il6_story_tutorial_v3";
        private bool _tutorialDismissed;
        private StoryTutorial _storyTutorial;
        private bool _storyReplay;
        private bool _storyWaitingForGui=true;
        private Vector2 _storyLastPosition;
        private int _storyLastWood;
        private readonly HashSet<int> _storyStartingFences=new HashSet<int>();
        private Vector2 _storyTextScroll;
        private readonly Dictionary<string,Texture2D> _storyIllustrations=new Dictionary<string,Texture2D>();
        public StoryTutorial CurrentStoryTutorial => _storyTutorial;
        private bool TutorialDialogueBlocking => !_tutorialDismissed && !_storyWaitingForGui && (_storyTutorial==null || _storyTutorial.IsDialogue) && (_storyTutorial==null || !_storyTutorial.NpcGuided || _guideConversationOpen);

        public void BeginStoryTutorial(bool replay=false)
        {
            _storyReplay=replay;_tutorialDismissed=false;_storyWaitingForGui=false;
            _storyTutorial=new StoryTutorial(replay?0:GameSession.Instance!=null?GameSession.Instance.StoryTutorialStage:0,!replay && GameSession.Instance!=null && GameSession.Instance.NpcTutorialEnabled);
            CaptureStoryBaseline(true);
            EnsureStoryGuide();if(_storyTutorial.IsDialogue && !_storyTutorial.NpcGuided) PauseForStory();
        }
        private void CaptureStoryBaseline(bool initial=false)
        {
            _storyLastPosition=Player!=null?(Vector2)Player.transform.position:Vector2.zero;
            if(!initial) return;
            _storyLastWood=WoodPickup.TotalWoodDelivered;
            _storyStartingFences.Clear();
            foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
                if(b.Kind==BuildingKind.Fence) _storyStartingFences.Add(b.GetInstanceID());
        }
        private void PauseForStory()
        {
            if(_tutorialPaused) return;
            _tutorialPreviousScale=Time.timeScale>0?Time.timeScale:1;_tutorialPaused=true;
            if(Player!=null && Player.TryGetComponent<Rigidbody2D>(out var body)) body.velocity=Vector2.zero;
            Time.timeScale=0;InputReader.HudBlocked=true;
        }
        private void ReleaseStoryPause()
        {
            if(!_tutorialPaused) return;
            _tutorialPaused=false;
            bool blocked=_paused || _buildPaused || _researchOpen || _workMenuOpen || _recruitCutPaused
                || (Player!=null && Player.IsDead) || (Progression!=null && Progression.LevelUpPending);
            Time.timeScale=blocked?0:Mathf.Max(.0001f,_tutorialPreviousScale);
            InputReader.HudBlocked=ControlsBlocked;
        }
        private void PersistStory()
        {
            if(_storyReplay || GameSession.Instance==null) return;
            var s=GameSession.Instance;s.StoryTutorialStage=(int)_storyTutorial.Stage;
            s.StoryTutorialComplete=_storyTutorial.IsComplete;s.SaveNow();
        }
        public void AdvanceStoryDialogue()
        {
            if(_storyTutorial==null || !_storyTutorial.IsDialogue) return;
            if(_storyTutorial.NpcGuided && !_guideConversationOpen)return;
            bool stageChanged=_storyTutorial.AdvanceDialogue();_storyTextScroll=Vector2.zero;
            if(!_storyReplay && _storyTutorial.StormRevealed && GameSession.Instance!=null && !GameSession.Instance.StoryVillageRuined)
            { GameSession.Instance.StoryVillageRuined=true;StoryVillageRuins.Apply(); }
            if(stageChanged)
            {
                CaptureStoryBaseline();PersistStory();
                if(!_storyTutorial.IsDialogue){_guideConversationOpen=false;ReleaseStoryPause();}
                if(_storyTutorial.IsComplete){_tutorialDismissed=true;EnsureStoryGuide();}
            }
        }
        private void DismissTutorial()
        {
            _storyTutorial?.Skip();_tutorialDismissed=true;
            if(!_storyReplay && GameSession.Instance!=null && GameSession.Instance.StoryTutorialEnabled && !GameSession.Instance.StoryVillageRuined)
            {GameSession.Instance.StoryVillageRuined=true;StoryVillageRuins.Apply();}
            ReleaseStoryPause();if(_storyTutorial!=null) PersistStory();
        }
        private void UpdateStoryTutorial()
        {
            if(_tutorialDismissed || Player==null || Player.IsDead) return;
            if(_storyTutorial==null)
            {
                var s=GameSession.Instance;if(s==null) return;
                if(!s.StoryTutorialEnabled || s.StoryTutorialComplete) {_tutorialDismissed=true;return;}
                _storyTutorial=new StoryTutorial(s.StoryTutorialStage,s.NpcTutorialEnabled);CaptureStoryBaseline(true);
                _storyWaitingForGui=_storyTutorial.IsDialogue && !_storyTutorial.NpcGuided;
            }
            EnsureStoryGuide();
            if(_storyWaitingForGui) return;
            if(_paused || _buildPaused || _recruitCutPaused || _researchOpen || _workMenuOpen || (Progression!=null && Progression.LevelUpPending)) return;
            if(_storyTutorial.IsDialogue) {if(!_storyTutorial.NpcGuided || _guideConversationOpen)PauseForStory();return;}
            if(Time.timeScale<=0 || (Player.Actions!=null && Player.Actions.IsBusy)) return;
            Vector2 position=Player.transform.position;float distance=Vector2.Distance(position,_storyLastPosition);_storyLastPosition=position;
            int wood=Mathf.Max(0,WoodPickup.TotalWoodDelivered-_storyLastWood);
            if(_storyTutorial.Stage==StoryTutorialStage.GatherWood) _storyLastWood=WoodPickup.TotalWoodDelivered;
            bool fence=false;
            if(_storyTutorial.Stage==StoryTutorialStage.BuildFence)
                foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
                    if(b.Kind==BuildingKind.Fence && b.CurrentHp>0 && !_storyStartingFences.Contains(b.GetInstanceID())) {fence=true;break;}
            if(_storyTutorial.Observe(distance,wood,fence)) {CaptureStoryBaseline();PersistStory();if(_storyTutorial.NpcGuided)EnsureStoryGuide();else PauseForStory();}
        }
        private void DrawTutorialOverlay()
        {
            if(_tutorialDismissed || _storyTutorial==null || !_storyTutorial.IsDialogue) return;
            if(_storyTutorial.NpcGuided){if(_guideConversationOpen)DrawGuideConversation();return;}
            _storyWaitingForGui=false;PauseForStory();
            var line=_storyTutorial.CurrentLine;float w=MobileHudLayout.Width,h=MobileHudLayout.Height;
            if(!_storyIllustrations.TryGetValue(_storyTutorial.IllustrationPath,out var illustration))
            {
                illustration=Resources.Load<Texture2D>(_storyTutorial.IllustrationPath);
                _storyIllustrations[_storyTutorial.IllustrationPath]=illustration;
            }
            var old=GUI.color;GUI.color=new Color(.025f,.045f,.065f,1);
            GUI.DrawTexture(new Rect(0,0,w,h),Texture2D.whiteTexture);
            GUI.color=Color.white;
            if(illustration!=null) GUI.DrawTexture(new Rect(0,0,w,h),illustration,ScaleMode.ScaleToFit,false);
            float width=Mathf.Min(1120,w-48),left=(w-width)/2,panelHeight=Mathf.Min(238,h*.39f);
            var panel=new Rect(left,h-panelHeight-24,width,panelHeight);
            GUI.color=new Color(.035f,.065f,.08f,.94f);GUI.DrawTexture(panel,Texture2D.whiteTexture);
            GUI.color=new Color(.83f,.72f,.44f,.9f);GUI.DrawTexture(new Rect(panel.x,panel.y,panel.width,2),Texture2D.whiteTexture);
            GUI.color=Color.white;
            float x=panel.x+28,content=panel.width-56;
            var nameStyle=new GUIStyle(_section){fontSize=18,normal={textColor=new Color(.93f,.82f,.56f)}};
            var bodyStyle=new GUIStyle(_label){fontSize=21,wordWrap=true,normal={textColor=new Color(.96f,.96f,.91f)}};
            HudText.Label(new Rect(x,panel.y+16,content,26),"소녀의 독백 · "+line.Chapter,nameStyle);
            var textRect=new Rect(x,panel.y+56,content,panel.height-126);
            float th=Mathf.Max(textRect.height,bodyStyle.CalcHeight(new GUIContent(line.Text),content-20));
            _storyTextScroll=GUI.BeginScrollView(textRect,_storyTextScroll,new Rect(0,0,content-20,th));
            HudText.Label(new Rect(0,0,content-20,th),line.Text,bodyStyle);GUI.EndScrollView();
            float buttonY=panel.yMax-60;
            if(UiTheme.Button(new Rect(x,buttonY,120,44),"건너뛰기",_smallBtn)) {GUI.color=old;DismissTutorial();return;}
            if(UiTheme.Button(new Rect(x+content-140,buttonY,140,44),"계속",_btn)) {GUI.color=old;AdvanceStoryDialogue();return;}
            GUI.color=old;
            if(Event.current.type==EventType.KeyDown && Event.current.keyCode==KeyCode.Return)
            {AdvanceStoryDialogue();Event.current.Use();}
        }
        private bool DrawStoryObjective()
        {
            if(_tutorialDismissed || _storyTutorial==null || (_storyTutorial.IsDialogue && !_storyTutorial.NpcGuided) || _storyTutorial.IsComplete) return false;
            var panel=GoalTrackerBounds;DrawQuestTrackerBacking(panel);
            QuestLabel(QuestTrackerLayout.Header(panel),_storyTutorial.Objective,new GUIStyle(_label){fontSize=16,fontStyle=FontStyle.Bold,normal={textColor=new Color(1,.94f,.70f)}});
            QuestLabel(new Rect(panel.x+16,panel.y+54,panel.width-32,82),_storyTutorial.Hint,new GUIStyle(_labelSubtle){fontSize=14,wordWrap=true,alignment=TextAnchor.UpperLeft,normal={textColor=Color.white}});
            return true;
        }
    }
}
