using UnityEngine;
namespace IL6
{
    public sealed partial class SimpleHud
    {
        private sealed class DeathSummary
        {
            public int Day, Kills, Companions, Lost, Score;
        }
        private DeathSummary _deathSummary;
        private bool _deathScreenActive, _deathRestarting;
        private float _deathButtonReadyAt;

        private bool UpdateDeathScreenState()
        {
            if (Player == null || !Player.IsDead) { _deathScreenActive=false; return false; }
            if (!_deathScreenActive)
            {
                _deathScreenActive=true;
                _deathRestarting=false;
                _deathButtonReadyAt=Time.unscaledTime+.45f;
                Gather?.Cancel();
                _researchOpen=false; _buildCatalogOpen=false; _buildPaused=false;
                _pendingBuildKind=null; _paused=false; _tutorialPaused=false;
                _recruitCutPaused=false; _recruitCutLeft=0;
                _resourceDetailsOpen=false; _showCompanions=false;
                _runeOffer=null; _damageFlashAmount=0;
                var session=GameSession.Instance;
                int day=session != null ? Mathf.Max(1,session.Cycle != null ? session.Cycle.Day : 1) : 1;
                if (session != null) session.MarkPlayerDied(day);
                _deathSummary=new DeathSummary {
                    Day=session != null ? session.PlayerDeathDay : day,
                    Kills=session != null ? session.TotalKills : 0,
                    Companions=session != null ? session.MaxCompanionsAtOnce : 0,
                    Lost=session != null ? session.CompanionsLost : 0,
                    Score=session != null ? session.Score : 0
                };
            }
            Time.timeScale=0f;
            InputReader.HudBlocked=true;
            return true;
        }

        private void DrawDeathOverlay()
        {
            var layout=new DeathScreenLayout(MobileHudLayout.Width,MobileHudLayout.Height);
            UiTheme.Rect(new Rect(0,0,MobileHudLayout.Width,MobileHudLayout.Height),new Color(.015f,.035f,.045f,.87f));
            UiTheme.Panel(layout.Panel);
            var centered=new GUIStyle(_label) {fontSize=16,alignment=TextAnchor.MiddleCenter,normal={textColor=UiTheme.TextSubtle}};
            var heading=new GUIStyle(centered) {fontSize=32,fontStyle=FontStyle.Bold,normal={textColor=UiTheme.TextCream}};
            var value=new GUIStyle(_label) {fontSize=28,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft,normal={textColor=UiTheme.TextCream}};
            HudText.Label(layout.Kicker,"모험 기록",new GUIStyle(centered) {fontSize=14,normal={textColor=UiTheme.TextGold}});
            HudText.Label(layout.Title,"이번 모험은 여기까지",heading);
            HudText.Label(layout.Subtitle,"섬에서 함께한 여정을 돌아보세요.",centered);
            string[] names={"생존 일수","적 처치","최대 동료 수","잃은 동료"};
            string[] values={$"{_deathSummary.Day:N0}일",$"{_deathSummary.Kills:N0}기",$"{_deathSummary.Companions:N0}명",$"{_deathSummary.Lost:N0}명"};
            for(int i=0;i<4;i++)
            {
                var card=layout.Metric(i);
                MobileButtonTheme.RoundRect(card,8,new Color(.07f,.15f,.17f,.9f));
                HudText.Label(new Rect(card.x+16,card.y+8,card.width-32,24),names[i],new GUIStyle(_labelSubtle){fontSize=14});
                HudText.Label(new Rect(card.x+16,card.y+33,card.width-32,36),values[i],value);
            }
            var score=layout.Score;
            UiTheme.Separator(new Rect(score.x,score.y,score.width,1));
            HudText.Label(new Rect(score.x+4,score.y+11,110,32),"최종 점수",new GUIStyle(_section){fontSize=17});
            HudText.Label(new Rect(score.x+122,score.y+6,score.width-126,42),$"{_deathSummary.Score:N0}",new GUIStyle(value){fontSize=30,alignment=TextAnchor.MiddleRight,normal={textColor=UiTheme.TextGold}});
            bool oldEnabled=GUI.enabled;
            GUI.enabled=oldEnabled && !_deathRestarting && Time.unscaledTime>=_deathButtonReadyAt && GameSession.Instance!=null;
            if(UiTheme.Button(layout.Restart,_deathRestarting ? "시작하는 중…" : "새 모험 시작",new GUIStyle(_btn){fontSize=19})) RestartAfterDeath();
            GUI.enabled=oldEnabled;
            HudText.Label(layout.Hint,"현재 진행을 초기화하고 처음부터 시작합니다.",new GUIStyle(centered){fontSize=14});
        }

        private bool RestartAfterDeath()
        {
            if (!_deathScreenActive || Player==null || !Player.IsDead || _deathRestarting
                || Time.unscaledTime<_deathButtonReadyAt || GameSession.Instance==null) return false;
            _deathRestarting=true;
            Sfx.Click();
            Time.timeScale=1f;
            InputReader.HudBlocked=false;
            GameSession.Instance.HardReset();
            return true;
        }
    }
}
