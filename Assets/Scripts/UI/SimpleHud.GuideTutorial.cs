using UnityEngine;
namespace IL6
{
    public sealed partial class SimpleHud
    {
        private bool _guideConversationOpen,_guideSpaceArmed;
        private int _guideOpenedFrame;
        public bool NpcStoryWaiting=>false;
        public bool GuideConversationOpen=>_guideConversationOpen;
        private void EnsureStoryGuide()
        {
            if(_storyTutorial==null || !_storyTutorial.NpcGuided || _storyReplay)return;
            if(_storyTutorial.IsComplete || _tutorialDismissed || !_storyTutorial.IsDialogue || _guideConversationOpen
                || _paused || _buildPaused || _researchOpen || _workMenuOpen || _recruitCutPaused)return;
            _guideConversationOpen=true;_storyWaitingForGui=false;_guideOpenedFrame=Time.frameCount;
            _guideSpaceArmed=!Input.GetKey(KeyCode.Space);_storyTextScroll=Vector2.zero;
            _resourceDetailsOpen=false;PauseForStory();
        }
        private void DrawGuideConversation()
        {
            var line=_storyTutorial.CurrentLine;var layout=RecruitConversationLayout.ForSize(MobileHudLayout.Width,MobileHudLayout.Height);
            UiTheme.Rect(new Rect(0,0,MobileHudLayout.Width,MobileHudLayout.Height),new Color(.015f,.035f,.05f,.26f));
            var portrait=DialoguePortraits.ForRole("Hero");
            if(portrait!=null)GUI.DrawTexture(layout.Character,portrait,ScaleMode.ScaleToFit,true);
            UiTheme.Panel(layout.Bubble);
            HudText.Label(layout.Name,"소녀의 독백",new GUIStyle(_section){fontSize=22,normal={textColor=UiTheme.TextGold}});
            HudText.Label(layout.Role,line.Chapter,new GUIStyle(_labelSubtle){fontSize=14,alignment=TextAnchor.MiddleRight});
            var text=new GUIStyle(_label){fontSize=20,wordWrap=true,alignment=TextAnchor.UpperLeft};
            float height=Mathf.Max(layout.Dialogue.height,text.CalcHeight(new GUIContent(line.Text),layout.Dialogue.width-20));
            _storyTextScroll=GUI.BeginScrollView(layout.Dialogue,_storyTextScroll,new Rect(0,0,layout.Dialogue.width-20,height));
            HudText.Label(new Rect(0,0,layout.Dialogue.width-20,height),line.Text,text);GUI.EndScrollView();
            var next=new Rect(layout.Accept.x,layout.Accept.y,layout.Accept.width+layout.Decline.width+12,50);
            if(UiTheme.Button(next,Application.isMobilePlatform?"다음":"다음  [Space / Enter]",new GUIStyle(_label){fontSize=18,alignment=TextAnchor.MiddleCenter}))AdvanceStoryDialogue();
            HandleGuideConversationKey(Event.current);
        }
        public bool HandleGuideConversationKey(Event input)
        {
            if(!_guideConversationOpen || input==null)return false;
            if(input.keyCode==KeyCode.Space && input.type==EventType.KeyUp){_guideSpaceArmed=true;input.Use();}
            else if(input.type==EventType.KeyDown && (input.keyCode==KeyCode.Space || input.keyCode==KeyCode.Return))
            {
                bool advance=Time.frameCount>_guideOpenedFrame && (input.keyCode==KeyCode.Return || _guideSpaceArmed);
                _guideSpaceArmed=false;input.Use();if(advance)AdvanceStoryDialogue();
            }
            else return false;
            return true;
        }
    }
}
