using UnityEngine;
namespace IL6
{
    public sealed partial class SimpleHud
    {
        private bool _researchOpen;
        private int _researchPage;
        private const int ResearchRowsPerPage=6;
        private float _researchPreviousScale=1;
        public bool OpenResearchCatalog()
        {
            if(_researchOpen || Player==null || Player.IsDead || GameSession.Instance==null || (ControlsBlocked && !_paused)) return false;
            _researchPreviousScale=Time.timeScale;
            Gather?.Cancel(); _resourceDetailsOpen=false;
            _researchPage=0;
            for(int i=0;i<VillageResearch.Catalog.Length;i++)
                if(NearbyResearchStation(VillageResearch.Catalog[i])!=null) {_researchPage=i/ResearchRowsPerPage;break;}
            _researchOpen=true;Time.timeScale=0;InputReader.HudBlocked=true;
            return true;
        }
        public void CloseResearchCatalog()
        {
            if(!_researchOpen) return;
            _researchOpen=false;
            bool other=_paused || _buildPaused || _tutorialPaused || _recruitCutPaused || (Player!=null && Player.IsDead) || (Progression!=null && Progression.LevelUpPending);
            Time.timeScale=other ? 0 : Mathf.Max(.0001f,_researchPreviousScale);
            InputReader.HudBlocked=ControlsBlocked;
        }
        private Building NearbyResearchStation(ResearchDefinition item)
        {
            if(Player==null) return null;
            Building best=null;float distance=float.MaxValue;
            foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
            {
                if(!VillageResearch.CanUse(item,b,Player.transform.position)) continue;
                float d=(b.transform.position-Player.transform.position).sqrMagnitude;
                if(d<distance) {best=b;distance=d;}
            }
            return best;
        }
        public bool TryResearch(ResearchId id)
        {
            if(!_researchOpen || Player==null || Player.IsDead || (Progression!=null && Progression.LevelUpPending) || (int)id<0 || (int)id>=VillageResearch.Catalog.Length) return false;
            var session=GameSession.Instance;if(session==null) return false;
            var station=NearbyResearchStation(VillageResearch.Catalog[(int)id]);
            if(!session.Research.TryUpgrade(id,station,Player.transform.position,session.Resources)) return false;
            session.SaveNow();Sfx.Build();return true;
        }
        private void AddResearchAction(System.Collections.Generic.List<ContextAction> actions)
        {
            foreach(var item in VillageResearch.Catalog)
                if(NearbyResearchStation(item)!=null)
                { actions.Add(new ContextAction(25,"연구 · 건물별 업그레이드",true,()=>OpenResearchCatalog(),"upgrade"));return; }
        }
        private void DrawResearchCatalog()
        {
            if(!_researchOpen) return;
            if(Player==null || Player.IsDead) {CloseResearchCatalog();return;}
            if(Event.current.type==EventType.KeyDown && Event.current.keyCode==KeyCode.Escape)
            {CloseResearchCatalog();Event.current.Use();return;}
            UiTheme.Rect(new Rect(0,0,MobileHudLayout.Width,MobileHudLayout.Height),new Color(0,0,0,.75f));
            var panel=HudLayout.CenterModal(900,620);UiTheme.Panel(panel);
            HudText.Label(new Rect(panel.x+24,panel.y+12,660,30),"마을 연구 · 건물별 업그레이드",_title);
            if(UiTheme.Button(new Rect(panel.xMax-132,panel.y+10,108,44),"닫기 · ESC",_smallBtn)) {CloseResearchCatalog();return;}
            HudText.Label(new Rect(panel.x+24,panel.y+48,840,22),$"건설 완료한 해당 건물 근처(3.5칸)에서 연구합니다. 각 연구는 최대 {VillageResearch.MaxLevel}단계입니다.",_labelSubtle);
            var session=GameSession.Instance;if(session==null) return;
            int pages=Mathf.CeilToInt((float)VillageResearch.Catalog.Length/ResearchRowsPerPage);
            _researchPage=Mathf.Clamp(_researchPage,0,pages-1);
            int first=_researchPage*ResearchRowsPerPage,last=Mathf.Min(first+ResearchRowsPerPage,VillageResearch.Catalog.Length);
            for(int i=first;i<last;i++)
            {
                var item=VillageResearch.Catalog[i];int level=session.Research.Level(item.Id);
                float y=panel.y+80+(i-first)*72;
                UiTheme.Rect(new Rect(panel.x+20,y,860,68),i%2==0?new Color(.10f,.23f,.22f,.9f):new Color(.05f,.14f,.14f,.8f));
                HudText.Label(new Rect(panel.x+32,y+5,160,24),$"{item.Category} · {BuildingName(item.Station)}",_section);
                HudText.Label(new Rect(panel.x+32,y+31,160,22),$"연구 {level} / {VillageResearch.MaxLevel}단계",_labelSubtle);
                HudText.Label(new Rect(panel.x+206,y+5,340,24),item.Name,_section);
                string effect=item.Id==ResearchId.Fence?$"울타리 HP {40+20*level} → {(level<VillageResearch.MaxLevel?40+20*(level+1):40+20*VillageResearch.MaxLevel)}":item.Effect;
                HudText.Label(new Rect(panel.x+206,y+31,340,22),effect,_label);
                var cost=item.Cost(level);
                HudText.Label(new Rect(panel.x+552,y+8,150,44),level>=VillageResearch.MaxLevel?"최고 단계":$"목재 {cost.Wood} / 석재 {cost.Stone}\n광석 {cost.Ore}",_labelSubtle);
                var station=NearbyResearchStation(item);
                bool built=BuildingUpgradeRules.HighestLevel(item.Station)>0;
                bool rewardFits=VillageResearch.RewardFits(item.Id,session.Resources);
                bool enabled=level<VillageResearch.MaxLevel && station!=null && rewardFits && cost.CanPay(session.Resources);
                bool needsStorage=cost.Wood>session.Resources.GetCap(ResourceKind.Wood) || cost.Stone>session.Resources.GetCap(ResourceKind.Stone) || cost.Ore>session.Resources.GetCap(ResourceKind.Ore);
                string button=level>=VillageResearch.MaxLevel?"연구 완료":!built?"건설 필요":station==null?"건물로 이동":!rewardFits?"광석 공간 부족":needsStorage?"창고 확장 필요":!cost.CanPay(session.Resources)?"자원 부족":$"{level+1}단계 연구";
                if(UiTheme.Button(new Rect(panel.x+712,y+8,150,44),button,_smallBtn,enabled)) TryResearch(item.Id);
            }
            float navY=panel.yMax-84;
            if(UiTheme.Button(new Rect(panel.center.x-184,navY,120,44),"이전",_smallBtn,_researchPage>0)) _researchPage--;
            HudText.Label(new Rect(panel.center.x-56,navY,112,44),$"{_researchPage+1} / {pages}",new GUIStyle(_label){alignment=TextAnchor.MiddleCenter});
            if(UiTheme.Button(new Rect(panel.center.x+64,navY,120,44),"다음",_smallBtn,_researchPage<pages-1)) _researchPage++;
            HudText.Label(new Rect(panel.x+24,panel.yMax-30,850,24),"완료 단계는 저장됩니다. 추가 연구는 해당 건물 근처에서 진행하세요.",_labelSubtle);
        }
    }
}
