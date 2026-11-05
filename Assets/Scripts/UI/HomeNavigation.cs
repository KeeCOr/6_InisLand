using System.Collections.Generic;
using UnityEngine;
namespace IL6
{
    public readonly struct HomeNavigationFrame
    {
        public readonly Vector2 Destination,Direction;
        public readonly float Distance;
        public readonly bool HasHouse;
        public bool Visible=>Distance>=12f;
        public string Label=>(HasHouse?"집까지 ":"마을까지 ")+Mathf.CeilToInt(Distance/ConstructionPlacement.CellSize)+"칸";
        public HomeNavigationFrame(Vector2 player,Vector2 destination,bool hasHouse)
        {Destination=destination;Direction=(destination-player).normalized;Distance=Vector2.Distance(player,destination);HasHouse=hasHouse;}
    }
    public static class HomeNavigation
    {
        public const string IconPath="UI/Navigation20261104/home_direction";
        public static Rect Bounds(float width)=>new Rect(width/2+112,22,164,32);
        public static HomeNavigationFrame Resolve(Vector2 player,IEnumerable<Building> buildings)
        {
            Vector2 village=new Vector2(GameConstants.VillageCenterX,GameConstants.VillageCenterY),destination=village;
            float best=float.PositiveInfinity;bool found=false;
            foreach(var building in buildings)
            {
                if(building==null || !building.isActiveAndEnabled || building.Kind!=BuildingKind.House || building.CurrentHp<=0)continue;
                Vector2 point=building.transform.position;float distance=(point-village).sqrMagnitude;
                if(distance<best || (Mathf.Approximately(distance,best) && (point.x<destination.x || (point.x==destination.x && point.y<destination.y))))
                {best=distance;destination=point;found=true;}
            }
            return new HomeNavigationFrame(player,destination,found);
        }
    }
}
