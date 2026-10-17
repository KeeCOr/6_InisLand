using UnityEngine;

namespace IL6
{
    public sealed partial class SimpleHud
    {
        private bool _buildCatalogOpen;
        private bool _tutorialPaused;
        private float _tutorialPreviousScale=1;
        private RecruitableNpc _ignoredRecruit;

        public bool TrySelectBuilding(BuildingKind kind)
        {
            if (!_buildCatalogOpen || !BuildUiAvailable) return false;
            var session=GameSession.Instance;
            if(session==null || !System.Enum.IsDefined(typeof(BuildingKind),kind)) return false;
            if(kind==BuildingKind.Farm && FarmBuilding.CurrentFarmCount()>=FarmBuilding.MaxFarmsAllowed()) return false;
            var cost=BuildingUpgradeRules.BuildCost(kind,CostMul(kind));
            if(!cost.CanPay(session.Resources)) return false;
            _pendingBuildKind=kind;
            _buildCatalogOpen=false;
            Sfx.Click();
            return true;
        }

        private void CloseBuildCatalog()
        {
            _buildCatalogOpen=false;
            SetHudMode(Attacker != null && Attacker.WeaponIndex == 1 ? HudMode.Ranged : HudMode.Melee);
        }

        private static string BuildingName(BuildingKind kind)=>kind switch
        {
            BuildingKind.Campfire=>"모닥불", BuildingKind.Barricade=>"바리케이드",
            BuildingKind.Fence=>"울타리", BuildingKind.House=>"거처",
            BuildingKind.Storage=>"자원 창고", BuildingKind.Farm=>"농장",
            BuildingKind.Watchtower=>"망루", BuildingKind.Infirmary=>"의무실",
            BuildingKind.HuntersHut=>"사냥꾼의 집", BuildingKind.Brazier=>"화로",
            BuildingKind.Blacksmith=>"대장간", BuildingKind.SeedStorage=>"씨앗 창고",
            BuildingKind.Carpenter=>"목공소", BuildingKind.TrainingCamp=>"훈련소",
            BuildingKind.FoodStorage=>"식량 창고", BuildingKind.LookoutPost=>"경계 초소",
            BuildingKind.Sawmill=>"제재소", BuildingKind.Church=>"예배당", _=>kind.ToString()
        };

        private static string RoleName(string role)=>role switch
        {
            "Hunter"=>"사냥꾼", "Warrior"=>"전사", "Farmer"=>"농부",
            "Cook"=>"요리사", "Healer"=>"치유사", "Child"=>"아이", _=>role
        };

        private void DrawBuildHotbar()
        {
            if(!_buildCatalogOpen || Player==null || _paused || _tutorialPaused || (Progression!=null && Progression.LevelUpPending)) return;
            var session=GameSession.Instance;if(session==null) return;
            UiTheme.Rect(new Rect(0,0,MobileHudLayout.Width,MobileHudLayout.Height),new Color(.02f,.055f,.06f,.72f));
            var panel=HudLayout.CenterModal(760,450);
            UiTheme.Panel(panel);
            UiTheme.TitleBar(panel,"건설 모드 · 게임 정지",_title);
            var kinds=(BuildingKind[])System.Enum.GetValues(typeof(BuildingKind));
            var nameStyle=new GUIStyle(_section){fontSize=13};
            var costStyle=new GUIStyle(_labelSubtle){fontSize=12};
            for(int i=0;i<kinds.Length;i++)
            {
                var kind=kinds[i];var r=HudLayout.CatalogCard(i);
                var cost=BuildingUpgradeRules.BuildCost(kind,CostMul(kind));
                bool farmAllowed=kind!=BuildingKind.Farm || FarmBuilding.CurrentFarmCount()<FarmBuilding.MaxFarmsAllowed();
                bool available=farmAllowed && cost.CanPay(session.Resources);
                if(UiTheme.Button(r,"",_btn,available)) TrySelectBuilding(kind);
                var sprite=SpriteBank.BuildingByKind(kind);
                var color=GUI.color;GUI.color=available?Color.white:new Color(.68f,.72f,.7f,.8f);
                if(sprite!=null) DrawSpriteInRect(sprite,new Rect(r.x+7,r.y+14,44,44));
                GUI.color=color;
                HudText.Label(new Rect(r.x+57,r.y+5,80,23),BuildingName(kind),nameStyle);
                HudText.Label(new Rect(r.x+57,r.y+27,80,20),$"목재 {cost.Wood}",costStyle);
                HudText.Label(new Rect(r.x+57,r.y+47,80,20),farmAllowed ? $"석재 {cost.Stone}" : "씨앗창고 필요",costStyle);
                if(!available) HudText.Label(new Rect(r.x+4,r.yMax-19,48,17),farmAllowed?"부족":"조건",new GUIStyle(costStyle){fontSize=11,fontStyle=FontStyle.Bold,normal={textColor=UiTheme.TextDanger}});
            }
            var hint=new GUIStyle(_labelSubtle){fontSize=13};
            HudText.Label(new Rect(panel.x+18,panel.yMax-48,480,24),$"보유 목재 {session.Resources.Get(ResourceKind.Wood):N0} · 석재 {session.Resources.Get(ResourceKind.Stone):N0}  | 선택 후 땅을 눌러 배치",hint);
            if(UiTheme.Button(new Rect(panel.xMax-180,panel.yMax-54,160,44),"닫기 · ESC",_btn)) CloseBuildCatalog();
        }

        private void DrawRecruitDialog()
        {
            if(ControlsBlocked || _pendingBuildKind!=null) return;
            if(_resourceDetailsOpen) return;
            var npc=NearestRecruitable();
            if(npc==null) { _ignoredRecruit=null;return; }
            if(npc==_ignoredRecruit) return;
            var panel=HudLayout.RecruitDialog();UiTheme.Panel(panel);
            var portrait=new Rect(panel.x+16,panel.y+16,108,146);
            MobileButtonTheme.RoundRect(portrait,10,UiTheme.PanelInnerBg);
            var sr=npc.GetComponent<SpriteRenderer>();
            if(sr!=null && sr.sprite!=null) DrawSpriteInRect(sr.sprite,InnerRect(portrait,8));
            float x=panel.x+142,w=panel.width-158;
            HudText.Label(new Rect(x,panel.y+12,w,28),$"{npc.DisplayNamePublic} · {RoleName(npc.Role)}",new GUIStyle(_section){fontSize=18});
            HudText.Label(new Rect(x,panel.y+46,w,38),npc.DialogText,new GUIStyle(_label){fontSize=15,wordWrap=true});
            HudText.Label(new Rect(x,panel.y+84,w,22),$"전투 {Stars(npc.CombatRating)}   농사 {Stars(npc.FarmRating)}",_labelSubtle);
            bool canRecruit=npc.CanRecruit();
            HudText.Label(new Rect(x,panel.y+106,w,22),$"마을 인원 {RecruitableNpc.CurrentCompanionCount()} / {RecruitableNpc.VillageCapacity()}",_labelSubtle);
            if(UiTheme.Button(new Rect(panel.x+200,panel.yMax-58,164,44),canRecruit?"함께하기 · V":"거처가 부족해요",_btn,canRecruit)) npc.Recruit();
            if(UiTheme.Button(new Rect(panel.x+376,panel.yMax-58,164,44),"나중에",_btn)) _ignoredRecruit=npc;
        }

        private void DrawWaveStanceBar()
        {
            DrawSettlementGoals();
            var session=GameSession.Instance;if(session==null) return;
            var companions=Object.FindObjectsByType<Companion>(FindObjectsSortMode.None);
            int live=0;foreach(var c in companions) if(c!=null && !c.IsDead && c.CurrentMode!=Companion.Mode.Hiding) live++;
            var panel=MobileHudLayout.CompanionSummary(_showCompanions);UiTheme.Panel(panel);
            var phase=Night!=null?Night.CurrentPhase:session.Cycle.Phase;
            var guidance=HudGuidanceText.Build(phase,Night!=null?Night.ActiveZombies:0,Night!=null?Night.WavePending:0,Night!=null && Night.IsBlizzard,session.LastFoodShortage);
            string objective=phase==Phase.Night ? (Night!=null && Night.IsBlizzard?"눈보라 밤 · 마을 방어":"밤 습격 · 마을 방어") : session.LastFoodShortage>0?"식량 확보":"탐험 · 자원 확보";
            string risk=phase==Phase.Night?$"적 {Night?.ActiveZombies??0} · 대기 {Night?.WavePending??0}":session.LastFoodShortage>0?$"식량 {session.LastFoodShortage} 부족":"마을 주변을 탐색하세요";
            HudText.Label(new Rect(panel.x+12,panel.y+7,panel.width-24,20),objective,new GUIStyle(_section){fontSize=13});
            HudText.Label(new Rect(panel.x+12,panel.y+28,panel.width-24,16),risk,new GUIStyle(_labelSubtle){fontSize=12,normal={textColor=GuidanceToneColor(guidance.ImmediateRisk.Tone)}});
            var stance=Companion.Stance.Follow;foreach(var c in companions) if(c!=null && !c.IsDead) {stance=c.CurrentStance;break;}
            string stanceName=stance==Companion.Stance.Follow?"따라오기":stance==Companion.Stance.Hold?"사수":"공세";
            if(UiTheme.Button(new Rect(panel.x+12,panel.y+46,panel.width-24,44),$"동료 {live}명 · {stanceName}",_smallBtn,live>0 && !ControlsBlocked))
            {
                var next=stance==Companion.Stance.Follow?Companion.Stance.Hold:stance==Companion.Stance.Hold?Companion.Stance.Aggressive:Companion.Stance.Follow;
                foreach(var c in companions) if(c!=null) c.SetStance(next);
            }
            if(!_showCompanions) return;
            string recommendation=phase==Phase.Night?"문 주변에서 동료와 함께 방어하세요":"목재·석재를 모으고 주변을 탐색하세요";
            HudText.Label(new Rect(panel.x+12,panel.y+98,panel.width-24,32),recommendation,new GUIStyle(_labelSubtle){fontSize=12,wordWrap=true});
            int shown=0;foreach(var c in companions)
            {
                if(c==null || c.IsDead || c.CurrentMode==Companion.Mode.Hiding) continue;
                if(shown>=4) break;
                float y=panel.y+140+shown*20;int hp=c.EffectiveMaxHp();
                UiTheme.Bar(new Rect(panel.x+12,y+3,panel.width-102,10),hp>0?(float)c.CurrentHp/hp:0,UiTheme.BarHpFill);
                HudText.Label(new Rect(panel.xMax-80,y,68,18),$"{c.CurrentHp}/{hp}",new GUIStyle(_labelSubtle){fontSize=12});shown++;
            }
        }
    }
}
