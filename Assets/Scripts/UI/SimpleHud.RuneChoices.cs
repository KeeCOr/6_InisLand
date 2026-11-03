using UnityEngine;
namespace IL6
{
    public sealed partial class SimpleHud
    {
        private int _runeDetailIndex=-1;
        private Texture2D _runeStatAtlas;
        private readonly System.Collections.Generic.Dictionary<RuneUpgradeCategory,Texture2D> _runeScopeIcons=new();
        private Texture2D RuneScopeIcon(RuneKind kind)
        {
            var scope=RuneUpgradeRules.Category(kind);
            if(!_runeScopeIcons.TryGetValue(scope,out var icon))
            {icon=Resources.Load<Texture2D>(RuneChoicePresentation.CategoryIconPath(kind));_runeScopeIcons[scope]=icon;}
            return icon;
        }
        private Texture2D RuneChoiceIcon(RuneKind kind)
        {
            if(_runeIconCache.TryGetValue(kind,out var icon)) return icon;
            icon=Resources.Load<Texture2D>(RuneChoicePresentation.IconPath(kind)??"UI/hud/hud-upgrade");
            _runeIconCache[kind]=icon;return icon;
        }
        private bool ShowRuneDetails(int index)
        {
            if(Progression==null || !Progression.LevelUpPending || _runeOffer==null || index<0 || index>=_runeOffer.Count) return false;
            _runeDetailIndex=_runeDetailIndex==index?-1:index;return true;
        }
        private bool TryChooseRune(int index)
        {
            if(Progression==null || !Progression.LevelUpPending || _runeOffer==null || index<0 || index>=_runeOffer.Count
                || (Player!=null && Player.IsDead)) return false;
            var kind=_runeOffer[index];if(Progression.GetStacks(kind)>=PlayerProgression.MaxStacks) return false;
            Sfx.Click();Progression.ApplyRune(kind);_runeOffer=null;_runeDetailIndex=-1;return true;
        }
        private void DrawRuneStatIcon(Rect target,RuneStatIcon icon,RuneKind kind)
        {
            if(icon==RuneStatIcon.Rune)
            {
                var primary=RuneChoiceIcon(kind);if(primary!=null) GUI.DrawTexture(target,primary,ScaleMode.ScaleToFit,true);return;
            }
            if(icon>=RuneStatIcon.Wood)
            {
                string key=icon switch {RuneStatIcon.Wood=>"wood",RuneStatIcon.Stone=>"stone",RuneStatIcon.Food=>"food",_=>"frostbloom"};
                ResourceIconSkin.Draw(target,key);return;
            }
            if(_runeStatAtlas==null) _runeStatAtlas=Resources.Load<Texture2D>("UI/Generated20261018/rune_stats");
            if(_runeStatAtlas==null) return;
            int i=(int)icon;
            GUI.DrawTextureWithTexCoords(target,_runeStatAtlas,new Rect((i%4)/4f,(1-i/4)/2f,.25f,.5f),true);
        }
        private void DrawRuneChoiceCards()
        {
            var layout=new RuneChoiceLayout(MobileHudLayout.Width,MobileHudLayout.Height);
            UiTheme.Rect(new Rect(0,0,MobileHudLayout.Width,MobileHudLayout.Height),new Color(.015f,.035f,.045f,.86f));
            UiTheme.Panel(layout.Panel);
            UiTheme.TitleBar(layout.Panel,$"특성 선택 · Lv. {Progression.Level}",_title);
            var centered=new GUIStyle(_labelSubtle){fontSize=14,alignment=TextAnchor.MiddleCenter};
            var number=new GUIStyle(_section){fontSize=22,alignment=TextAnchor.MiddleLeft};
            for(int i=0;i<_runeOffer.Count;i++)
            {
                var kind=_runeOffer[i];var card=layout.Card(i);var info=RuneChoiceLayout.Info(card);
                int current=Progression.GetStacks(kind),next=Mathf.Min(current+1,PlayerProgression.MaxStacks);
                UiTheme.Panel(card);
                Color scopeColor=RuneChoicePresentation.CategoryColor(kind);
                UiTheme.Rect(new Rect(card.x+12,card.y+3,card.width-24,3),scopeColor);
                UiTheme.Rect(new Rect(card.x+4,card.y+12,3,card.height-24),scopeColor);
                HudText.Label(RuneChoiceLayout.Title(card),RuneChoicePresentation.Name(kind),new GUIStyle(_weapon){fontSize=20,wordWrap=true,alignment=TextAnchor.MiddleCenter,padding=new RectOffset(0,0,0,0)});
                var icon=RuneChoiceIcon(kind);if(icon!=null) GUI.DrawTexture(RuneChoiceLayout.Icon(card),icon,ScaleMode.ScaleToFit,true);
                var scopeLine=RuneChoiceLayout.Trigger(card);
                var scopeIcon=RuneScopeIcon(kind);
                if(scopeIcon!=null)GUI.DrawTexture(new Rect(scopeLine.center.x-100,scopeLine.y,22,22),scopeIcon,ScaleMode.ScaleToFit,true);
                HudText.Label(new Rect(scopeLine.center.x-72,scopeLine.y,172,22),
                    RuneChoicePresentation.CategoryLabel(kind)+" · "+RuneChoicePresentation.Trigger(kind,next),
                    new GUIStyle(centered){fontSize=13,fontStyle=FontStyle.Bold,normal={textColor=scopeColor}});
                var progress=RuneChoiceLayout.Progress(card);
                for(int dot=0;dot<PlayerProgression.MaxStacks;dot++)
                {
                    Color color=dot<current?MobileButtonTheme.Jade:dot==current?UiTheme.TextGold:new Color(.2f,.3f,.3f);
                    MobileButtonTheme.RoundRect(new Rect(progress.x+dot*30,progress.y,12,12),6,color);
                }
                GUI.Label(progress,new GUIContent("",$"강화 {current} → {next} / {PlayerProgression.MaxStacks}"),GUIStyle.none);
                var stats=RuneChoicePresentation.Stats(kind,next);
                for(int row=0;row<stats.Count;row++)
                {
                    var stat=stats[row];var line=RuneChoiceLayout.Stat(card,row,stats.Count);
                    DrawRuneStatIcon(new Rect(line.x,line.y,32,32),stat.Icon,kind);
                    HudText.Label(new Rect(line.x+44,line.y,line.width-44,line.height),stat.Value,number);
                    GUI.Label(line,new GUIContent("",stat.Hint+" · "+stat.Value),GUIStyle.none);
                }
                if(UiTheme.Button(info,"i",new GUIStyle(_btn){fontSize=20})) ShowRuneDetails(i);
                bool pointerUp=Event.current.type==EventType.MouseUp;
                bool outsideInfo=!info.Contains(MobileHudLayout.GuiPoint(Input.mousePosition));
                bool clicked=GUI.Button(card,new GUIContent("",GUI.tooltip),GUIStyle.none);
                if(outsideInfo && clicked && pointerUp && TryChooseRune(i)) return;
            }
            var detail=layout.Details;
            if(_runeDetailIndex>=0 && _runeDetailIndex<_runeOffer.Count)
            {
                var kind=_runeOffer[_runeDetailIndex];int current=Progression.GetStacks(kind),next=Mathf.Min(current+1,PlayerProgression.MaxStacks);
                MobileButtonTheme.RoundRect(detail,8,UiTheme.PanelInnerBg);
                HudText.Label(new Rect(detail.x+14,detail.y+10,176,28),RuneChoicePresentation.Name(kind),new GUIStyle(_section){fontSize=18});
                HudText.Label(new Rect(detail.x+14,detail.y+44,176,30),$"{current} → {next} / {PlayerProgression.MaxStacks}",centered);
                string description=RuneChoicePresentation.CategoryLabel(kind)+"\n"+PlayerProgression.DescribeAt(kind,next);
                if(kind==RuneKind.PoisonBlade || kind==RuneKind.IceArrow || kind==RuneKind.LightningStrike) description+="\n원소 2종 완성: 공격력 +50%";
                HudText.Label(new Rect(detail.x+202,detail.y+10,detail.width-216,detail.height-20),description,new GUIStyle(_label){fontSize=17,wordWrap=true,alignment=TextAnchor.MiddleLeft});
            }
            else HudText.Label(detail,string.IsNullOrEmpty(GUI.tooltip)?"카드를 눌러 선택 · i 자세히":GUI.tooltip,centered);
        }
    }
}
