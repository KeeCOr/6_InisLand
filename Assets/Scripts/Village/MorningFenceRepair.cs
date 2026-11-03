using System.Collections.Generic;
using UnityEngine;

namespace IL6
{
    public sealed class MorningFenceRepair
    {
        private readonly List<Building> _damaged=new List<Building>();
        private readonly List<FenceWreckage> _broken=new List<FenceWreckage>();
        public int DamagedCount {get {int n=0;foreach(var b in _damaged)if(b!=null && b.CurrentHp>0 && b.CurrentHp<b.MaxHp)n++;return n;}}
        public int BrokenCount {get {int n=0;foreach(var b in _broken)if(b!=null && b.CanRestore)n++;return n;}}
        public int WoodCost=>DamagedCount+BrokenCount;
        public static MorningFenceRepair Capture()
        {
            var plan=new MorningFenceRepair();
            foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
                if(b.Kind==BuildingKind.Fence && b.CurrentHp>0 && b.CurrentHp<b.MaxHp)plan._damaged.Add(b);
            foreach(var b in Object.FindObjectsByType<FenceWreckage>(FindObjectsSortMode.None))if(b.CanRestore)plan._broken.Add(b);
            return plan;
        }
        public bool TryApply(ResourceStore store)
        {
            int cost=WoodCost;if(cost<=0 || store==null || !store.Spend(ResourceKind.Wood,cost))return false;
            foreach(var b in _damaged)if(b!=null && b.CurrentHp>0 && b.CurrentHp<b.MaxHp)b.RepairHp(b.MaxHp-b.CurrentHp);
            foreach(var b in _broken)if(b!=null && b.CanRestore)b.Restore();
            return true;
        }
    }
}
