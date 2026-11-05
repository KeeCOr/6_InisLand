using UnityEngine;
namespace IL6
{
    public sealed partial class SimpleHud
    {
        private bool _resourceDetailsOpen;
        private int _hoveredResource = -1;
        private static readonly ResourceKind[] ResourceKinds={ResourceKind.Wood,ResourceKind.Stone,ResourceKind.Ore,ResourceKind.Meat,ResourceKind.Food,ResourceKind.Frostbloom};
        private static readonly string[] ResourceNames={"목재","석재","광석","고기","식량","서리꽃"};
        private Rect ResourceDetailsRect => new Rect(MobileHudLayout.Width-360,MobileHudLayout.Experience.yMax+8,344,270);
        private void DrawResourceBar()
        {
            var session=GameSession.Instance;if(session==null) return;
            _hoveredResource=-1;
            if(ControlsBlocked) _resourceDetailsOpen=false;
            var count=new GUIStyle(_label){fontSize=16,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft,normal={textColor=Color.white}};
            var shadow=new GUIStyle(count){normal={textColor=new Color(0,0,0,.95f)}};
            for(int i=0;i<ResourceKinds.Length;i++)
            {
                var r=MobileHudLayout.ResourceChip(i);
                if(!Application.isMobilePlatform && r.Contains(Event.current.mousePosition)) _hoveredResource=i;
                var backing=new Rect(r.x,r.y+1,r.width-2,30);
                if(!GeneratedUiSkin.Pill(backing,new Color(.32f,.42f,.45f,.55f)))
                    MobileButtonTheme.RoundRect(backing,7,new Color(.025f,.06f,.07f,.55f));
                float pulse=ResourceKinds[i]==WoodPickup.LastPickupKind ? Mathf.Clamp01(1f-(Time.time-WoodPickup.LastPickupTime)/.6f) : 0;
                var icon=MobileHudLayout.ResourceIcon(i);
                float grow=Mathf.Sin(pulse*Mathf.PI)*3;
                DrawHudIcon(new Rect(icon.x-grow,icon.y-grow,icon.width+2*grow,icon.height+2*grow),ResourceIconKey(ResourceKinds[i]));
                int amount=session.Resources.Get(ResourceKinds[i]);
                string text=ResourceHudFormat.Compact(amount);
                var label=new Rect(r.x+29,r.y+1,r.width-31,28);
                HudText.Label(new Rect(label.x-1,label.y,label.width,label.height),text,shadow);
                HudText.Label(new Rect(label.x+1,label.y+1,label.width,label.height),text,shadow);
                count.normal.textColor=amount>=session.Resources.GetCap(ResourceKinds[i]) || pulse>0 ? UiTheme.TextGold : Color.white;
                HudText.Label(label,text,count);
                if(pulse>0)
                    HudText.Label(new Rect(label.x,label.y-18-(1-pulse)*10,56,20),$"+{WoodPickup.LastPickupAmount}",new GUIStyle(_label){fontSize=14,fontStyle=FontStyle.Bold,normal={textColor=new Color(1,.88f,.45f,pulse)}});
            }
            bool released=Event.current.type==EventType.MouseUp;
            if(!ControlsBlocked && GUI.Button(MobileHudLayout.ResourceTouchBounds,"",GUIStyle.none) && released) _resourceDetailsOpen=!_resourceDetailsOpen;
            if(_resourceDetailsOpen && Event.current.type==EventType.MouseDown
                && !MobileHudLayout.ResourceTouchBounds.Contains(Event.current.mousePosition) && !ResourceDetailsRect.Contains(Event.current.mousePosition)) _resourceDetailsOpen=false;
        }
        private void DrawResourceDetails()
        {
            if(!_resourceDetailsOpen && _hoveredResource>=0)
            {
                var chip=MobileHudLayout.ResourceChip(_hoveredResource);
                var tooltip=new Rect(chip.x,MobileHudLayout.Experience.yMax+8,160,28);
                GeneratedUiSkin.Pill(tooltip,Color.white);
                HudText.Label(tooltip,ResourceNames[_hoveredResource],new GUIStyle(_label){alignment=TextAnchor.MiddleCenter});
            }
            if(!_resourceDetailsOpen) return;
            var session=GameSession.Instance;if(session==null) return;
            var panel=ResourceDetailsRect;UiTheme.Panel(panel);UiTheme.TitleBar(panel,"자원 · 보유량 / 보관 한도",_section);
            for(int i=0;i<ResourceKinds.Length;i++)
            {
                float y=panel.y+43+i*34;
                DrawHudIcon(new Rect(panel.x+14,y,26,26),ResourceIconKey(ResourceKinds[i]));
                HudText.Label(new Rect(panel.x+48,y+1,86,26),ResourceNames[i],_label);
                HudText.Label(new Rect(panel.x+132,y+1,192,26),$"{session.Resources.Get(ResourceKinds[i]):N0} / {session.Resources.GetCap(ResourceKinds[i]):N0}",new GUIStyle(_label){fontSize=17,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleRight});
            }
            HudText.Label(new Rect(panel.x+14,panel.yMax-26,316,22),"다시 누르거나 바깥을 눌러 닫기",_labelSubtle);
        }
    }
}
