using System.Collections.Generic;
using UnityEngine;
namespace IL6
{
    public readonly struct ThreatNotice
    {
        public readonly Vector3 Position;
        public readonly int Count;
        public readonly bool Companion;
        public bool Visible=>Count>0;
        public string Label=>(Companion?"동료 부상":"시설 피격")+(Count>1?$" · {Count}":"");
        public ThreatNotice(Vector3 position,int count,bool companion){Position=position;Count=count;Companion=companion;}
    }
    public static class ThreatNavigation
    {
        public const float RecentSeconds=4;
        public static Rect Bounds(float width,int row)=>new Rect(width/2+112,62+row*40,164,36);
        private static bool Outside(Camera camera,Vector3 position)
        {var p=camera.WorldToViewportPoint(position);return p.z<=0 || p.x<0 || p.x>1 || p.y<0 || p.y>1;}
        public static ThreatNotice Collect(Camera camera,Vector3 player,IEnumerable<Building> buildings,IEnumerable<Companion> companions,float now,bool ally)
        {
            if(camera==null)return default;
            int count=0;Vector3 position=default;float newest=float.NegativeInfinity,nearest=float.PositiveInfinity;
            void Add(Vector3 point,float time)
            {
                if(time<0 || now-time>RecentSeconds || !Outside(camera,point))return;
                count++;float distance=(point-player).sqrMagnitude;
                if(time>newest || (Mathf.Approximately(time,newest) && distance<nearest)){position=point;newest=time;nearest=distance;}
            }
            if(ally)
            {foreach(var c in companions)if(c!=null && c.isActiveAndEnabled && !c.IsDead)Add(c.transform.position,c.LastDamagedAt);}
            else
            {foreach(var b in buildings)if(b!=null && b.isActiveAndEnabled && b.CurrentHp>0)Add(b.transform.position,b.LastAttackedAt);}
            return new ThreatNotice(position,count,ally);
        }
    }
}
