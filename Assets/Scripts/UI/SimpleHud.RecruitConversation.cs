using UnityEngine;

namespace IL6
{
    public sealed partial class SimpleHud
    {
        private RecruitableNpc _recruitCandidate;
        private Texture2D _recruitSpeechBubble;
        private int _recruitOpenedFrame;
        private bool _recruitSpaceArmed;
        public bool RecruitmentConversationOpen => _recruitCutPaused;

        private bool RecruitCandidateValid => _recruitCandidate!=null && _recruitCandidate.isActiveAndEnabled
            && _recruitCandidate.CanRecruit() && Player!=null && !Player.IsDead
            && Vector2.Distance(Player.transform.position,_recruitCandidate.transform.position)<=_recruitCandidate.RecruitRange;

        public bool BeginRecruitConversation(RecruitableNpc npc)
        {
            if(npc==null || !npc.isActiveAndEnabled || !npc.CanRecruit() || Player==null || ControlsBlocked
                || _pendingBuildKind!=null || _hudMode==HudMode.Build
                || (Player.Actions!=null && Player.Actions.IsBusy)
                || Vector2.Distance(Player.transform.position,npc.transform.position)>npc.RecruitRange) return false;
            _recruitCandidate=npc;npc.Player=Player.transform;
            _recruitCutPrevTimeScale=Time.timeScale;
            _recruitCutPaused=true;_resourceDetailsOpen=false;_recruitDialogueScroll=Vector2.zero;
            _recruitOpenedFrame=Time.frameCount;
            _recruitSpaceArmed=!Input.GetKey(KeyCode.Space);
            var body=Player.GetComponent<Rigidbody2D>();if(body!=null) body.velocity=Vector2.zero;
            Time.timeScale=0;InputReader.HudBlocked=true;
            return true;
        }

        public bool AcceptRecruitConversation()
        {
            if(!_recruitCutPaused) return false;
            if(!RecruitCandidateValid || (Progression!=null && Progression.LevelUpPending))
            { DeclineRecruitConversation();return false; }
            var npc=_recruitCandidate;
            Player.GetComponent<InputReader>()?.ConsumeInteraction();
            DeclineRecruitConversation();
            npc.Recruit();Sfx.Click();
            return true;
        }

        public void DeclineRecruitConversation()
        {
            if(!_recruitCutPaused) return;
            _recruitCandidate=null;_recruitCutPaused=false;
            _recruitSpaceArmed=false;
            bool anotherPause=_paused || _researchOpen || _workMenuOpen || _buildPaused || _tutorialPaused
                || (Player!=null && Player.IsDead) || (Progression!=null && Progression.LevelUpPending);
            Time.timeScale=anotherPause?0f:Mathf.Max(.0001f,_recruitCutPrevTimeScale);
            InputReader.HudBlocked=ControlsBlocked;
        }

        private void ValidateRecruitConversation()
        {
            if(_recruitCutPaused && (!RecruitCandidateValid || (Progression!=null && Progression.LevelUpPending)))
                DeclineRecruitConversation();
        }

        public bool HandleRecruitConversationKey(Event input)
        {
            if(!_recruitCutPaused || input==null) return false;
            if(input.type==EventType.KeyDown && input.keyCode==KeyCode.Escape)
            { DeclineRecruitConversation();input.Use();return true; }
            if(input.keyCode!=KeyCode.Space) return false;
            if(input.type==EventType.KeyUp)
            { _recruitSpaceArmed=true;input.Use();return true; }
            if(input.type!=EventType.KeyDown) return false;
            bool accept=_recruitSpaceArmed && Time.frameCount>_recruitOpenedFrame;
            // Consume the opening press and held-key repeats. Only a fresh press can accept.
            _recruitSpaceArmed=false;input.Use();
            if(accept) AcceptRecruitConversation();
            return true;
        }

        private void DrawRecruitConversation()
        {
            if(!RecruitCandidateValid) { DeclineRecruitConversation();return; }
            HandleRecruitConversationKey(Event.current);
            if(!_recruitCutPaused) return;
            var layout=RecruitConversationLayout.ForSize(MobileHudLayout.Width,MobileHudLayout.Height);
            var npc=_recruitCandidate;
            var portrait=npc.DialoguePortrait;
            if(portrait!=null)
            {
                float fit=Mathf.Min(layout.Character.width/portrait.width,layout.Character.height/portrait.height);
                var image=new Rect(layout.Character.center.x-portrait.width*fit*.5f,
                    layout.Character.yMax-portrait.height*fit,portrait.width*fit,portrait.height*fit);
                GUI.DrawTexture(image,portrait,ScaleMode.StretchToFill,true);
            }
            else
            {
                var sprite=SpriteBank.CompanionSpriteForRole(npc.Role);
                if(sprite!=null) DrawSpriteInRect(sprite,layout.Character);
            }
            if(_recruitSpeechBubble==null) _recruitSpeechBubble=Resources.Load<Texture2D>("UI/RecruitConversation/speech_bubble");
            // Trim only transparent canvas padding in the generated asset; keep choices inside the opaque panel.
            if(_recruitSpeechBubble!=null) GUI.DrawTextureWithTexCoords(layout.Bubble,_recruitSpeechBubble,new Rect(0,.12f,1,.81f),true);
            else MobileButtonTheme.RoundRect(layout.Bubble,24,new Color(.97f,.95f,.87f,.98f));
            var nameStyle=new GUIStyle(_section){fontSize=22,alignment=TextAnchor.MiddleLeft,normal={textColor=new Color(.17f,.28f,.26f)}};
            var textStyle=new GUIStyle(_label){fontSize=20,wordWrap=true,alignment=TextAnchor.UpperLeft,normal={textColor=new Color(.18f,.25f,.24f)}};
            var detailStyle=new GUIStyle(textStyle){fontSize=14};
            var roleStyle=new GUIStyle(detailStyle){alignment=TextAnchor.MiddleRight,fontSize=16};
            HudText.Label(layout.Name,npc.DisplayNamePublic,nameStyle);
            HudText.Label(layout.Role,RoleName(npc.Role),roleStyle);
            string dialogue=string.IsNullOrWhiteSpace(npc.DialogText)?"당신과 함께해도 될까요?":npc.DialogText;
            // Scroll long dialogue rather than clipping it or shrinking the text.
            float textHeight=Mathf.Max(layout.Dialogue.height,textStyle.CalcHeight(new GUIContent(dialogue),layout.Dialogue.width-20));
            _recruitDialogueScroll=GUI.BeginScrollView(layout.Dialogue,_recruitDialogueScroll,new Rect(0,0,layout.Dialogue.width-20,textHeight));
            HudText.Label(new Rect(0,0,layout.Dialogue.width-20,textHeight),dialogue,textStyle);GUI.EndScrollView();
            HudText.Label(layout.Details,$"영입 비용 없음 · 전투 {Stars(npc.CombatRating)} · 농사 {Stars(npc.FarmRating)}",detailStyle);
            var choiceStyle=new GUIStyle(_btn){fontSize=18,alignment=TextAnchor.MiddleCenter};
            string acceptLabel=Application.isMobilePlatform?"함께 가요":"함께 가요  [Space]";
            string declineLabel=Application.isMobilePlatform?"다음에요":"다음에요  [Esc]";
            var acceptStyle=HudText.Fit(layout.Accept,new GUIContent(acceptLabel),choiceStyle);
            var declineStyle=HudText.Fit(layout.Decline,new GUIContent(declineLabel),choiceStyle);
            if(UiTheme.Button(layout.Accept,acceptLabel,acceptStyle)) { AcceptRecruitConversation();return; }
            if(UiTheme.Button(layout.Decline,declineLabel,declineStyle)) DeclineRecruitConversation();
        }
        private Vector2 _recruitDialogueScroll;
    }
}
