using System.Collections.Generic;
using UnityEngine;

namespace IL6
{
    public sealed class WoodPickup : MonoBehaviour
    {
        private enum Phase { Ground, Flying, Delivered }
        private static readonly List<WoodPickup> All=new();
        public static float LastPickupTime { get; private set; }=-10f;
        public static int TotalWoodDelivered { get; private set; }
        public static int LastPickupAmount { get; private set; }
        public int Amount { get; private set; }
        public ResourceKind Kind { get; private set; }=ResourceKind.Wood;
        public static ResourceKind LastPickupKind { get; private set; }=ResourceKind.Wood;
        public bool IsFlying => _phase==Phase.Flying;
        private const float FlightSeconds=.55f,PickupRadius=1.1f,LandingSeconds=.35f;
        private Phase _phase;
        private ResourceStore _store;
        private PlayerController _player;
        private SpriteRenderer _visual,_shadow;
        private float _age,_flightTime;
        private Vector2 _screenStart;

        public static WoodPickup Spawn(Vector3 position,int amount,ResourceStore store,ResourceKind kind=ResourceKind.Wood)
        {
            if(amount<=0 || store==null) return null;
            var go=new GameObject("Dropped_"+kind);go.transform.position=position+new Vector3(.25f,-.15f,0);
            var pickup=go.AddComponent<WoodPickup>();pickup.Amount=amount;pickup._store=store;pickup.Kind=kind;
            var art=new GameObject("WoodArt");art.transform.SetParent(go.transform,false);
            pickup._visual=art.AddComponent<SpriteRenderer>();pickup._visual.sortingOrder=9;
            pickup._visual.sprite=ResourceIconSkin.WorldSprite(kind);
            YSort.Ensure(art);
            var shadowSprite=Resources.Load<Sprite>("Sprites/Environment/Snowfield/actor_shadow");
            if(shadowSprite!=null)
            {
                var shadow=new GameObject("PickupShadow");shadow.transform.SetParent(go.transform,false);
                shadow.transform.localScale=new Vector3(.5f,.24f,1);
                pickup._shadow=shadow.AddComponent<SpriteRenderer>();pickup._shadow.sprite=shadowSprite;
                pickup._shadow.sortingOrder=-9350;pickup._shadow.color=new Color(.35f,.45f,.55f,.18f);
            }
            if(pickup._visual.sprite==null)
            {
                var fallback=art.AddComponent<ColorFallback>();fallback.Tint=new Color(.65f,.4f,.16f);fallback.PixelSize=32;
            }
            return pickup;
        }
        public static int HudIndex(ResourceKind kind) => kind switch {ResourceKind.Wood=>0,ResourceKind.Stone=>1,ResourceKind.Ore=>2,ResourceKind.Meat=>3,ResourceKind.Food=>4,_=>5};
        public static string IconKey(ResourceKind kind) => kind switch {ResourceKind.Wood=>"wood",ResourceKind.Stone=>"stone",ResourceKind.Ore=>"ore",ResourceKind.Meat=>"meat",ResourceKind.Food=>"food",_=>"frostbloom"};
        public static string ResourceName(ResourceKind kind) => kind switch {ResourceKind.Wood=>"목재",ResourceKind.Stone=>"석재",ResourceKind.Ore=>"광석",ResourceKind.Meat=>"고기",ResourceKind.Food=>"식량",_=>"서리꽃"};
        private void Awake() { All.Add(this); }
        private void Update()
        {
            if(Time.timeScale<=0f) return;
            if(_phase==Phase.Flying)
            {
                _flightTime+=Time.deltaTime;
                if(_flightTime>=FlightSeconds) FinishFlight(false);
                return;
            }
            if(_phase!=Phase.Ground) return;
            _age+=Time.deltaTime;
            if(_visual!=null)
            {
                float bounce=_age<LandingSeconds ? Mathf.Sin(_age/LandingSeconds*Mathf.PI)*.38f : 0;
                _visual.transform.localPosition=Vector3.up*(.04f+bounce);
            }
            if(_player==null) _player=Object.FindFirstObjectByType<PlayerController>();
            TryCollect(_player);
        }
        public bool TryCollect(PlayerController player)
        {
            if(_phase!=Phase.Ground || _age<LandingSeconds || Amount<=0 || _store==null || player==null || player.IsDead
                || Time.timeScale<=0f || InputReader.HudBlocked) return false;
            if(Vector2.Distance(player.transform.position,transform.position)>PickupRadius) return false;
            if(_store.Get(Kind)>=_store.GetCap(Kind)) return false;
            var cam=Camera.main;
            var screen=cam!=null ? cam.WorldToScreenPoint(_visual!=null?_visual.transform.position:transform.position)
                : new Vector3(Screen.width*.5f,Screen.height*.5f,1);
            _screenStart=new Vector2(screen.x/Mathf.Max(1,Screen.width),1f-screen.y/Mathf.Max(1,Screen.height));
            _phase=Phase.Flying;_flightTime=0;
            if(_visual!=null) _visual.enabled=false;
            if(_shadow!=null) _shadow.enabled=false;
            return true;
        }
        private void FinishFlight(bool tearingDown)
        {
            if(_phase!=Phase.Flying || _store==null) return;
            int credit=Mathf.Min(Amount,Mathf.Max(0,_store.GetCap(Kind)-_store.Get(Kind)));
            Amount-=credit;
            _phase=Amount==0?Phase.Delivered:Phase.Ground;
            _age=0;_flightTime=0;
            if(credit>0)
            {
                _store.Add(Kind,credit);
                if(Kind==ResourceKind.Wood) TotalWoodDelivered+=credit;
                LastPickupTime=Time.time;LastPickupAmount=credit;LastPickupKind=Kind;
                if(!tearingDown) Sfx.Pickup();
            }
            if(tearingDown) return;
            if(Amount==0) Destroy(gameObject);
            else
            {
                if(_visual!=null)_visual.enabled=true;
                if(_shadow!=null)_shadow.enabled=true;
            }
        }
        private void OnDestroy()
        {
            // Collected wood remains owned if scene teardown interrupts the short UI flight.
            if(IsFlying && GameSession.Instance!=null && ReferenceEquals(GameSession.Instance.Resources,_store)) FinishFlight(true);
            All.Remove(this);
        }
        private Vector2 FlightPoint(float progress)
        {
            var start=new Vector2(_screenStart.x*MobileHudLayout.Width,_screenStart.y*MobileHudLayout.Height);
            var target=MobileHudLayout.ResourceIcon(HudIndex(Kind)).center;
            float t=1f-Mathf.Pow(1f-Mathf.Clamp01(progress),3);
            return Vector2.Lerp(start,target,t)+Vector2.up*(Mathf.Sin(t*Mathf.PI)*-90f);
        }
        public static void DrawHud()
        {
            var cam=Camera.main;
            var label=new GUIStyle(GUI.skin.label){font=UiTheme.HudFont,fontSize=12,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,normal={textColor=UiTheme.TextCream}};
            foreach(var pickup in All)
            {
                if(pickup==null) continue;
                if(pickup.IsFlying)
                {
                    float t=Mathf.Clamp01(pickup._flightTime/FlightSeconds);
                    var point=pickup.FlightPoint(t);float size=Mathf.Lerp(36,24,t);
                    ResourceIconSkin.Draw(new Rect(point.x-size/2,point.y-size/2,size,size),IconKey(pickup.Kind));
                }
                else if(pickup._phase==Phase.Ground && cam!=null)
                {
                    var screen=cam.WorldToScreenPoint(pickup.transform.position);
                    if(screen.z<=0 || screen.x<0 || screen.x>Screen.width || screen.y<0 || screen.y>Screen.height) continue;
                    var p=MobileHudLayout.GuiPoint(screen);
                    var rect=new Rect(p.x-40,p.y+8,80,22);
                    MobileButtonTheme.RoundRect(rect,5,new Color(.025f,.065f,.045f,.8f));
                    HudText.Label(rect,$"{ResourceName(pickup.Kind)} ×{pickup.Amount}",label);
                }
            }
        }
    }
}
