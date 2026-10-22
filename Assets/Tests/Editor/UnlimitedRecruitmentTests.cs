using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class UnlimitedRecruitmentTests
    {
        [Test] public void RecruitmentDoesNotRequireHousingOrFreePopulationCapacity()
        {
            var objects=new List<GameObject>();
            try {
                for(int i=0;i<32;i++) {
                    var go=new GameObject("Recruitment count test");objects.Add(go);go.AddComponent<Companion>().SetMaxHp(100,true);
                }
                var stranger=new GameObject("Unlimited recruit test");objects.Add(stranger);
                var npc=stranger.AddComponent<RecruitableNpc>();
                Assert.That(RecruitableNpc.CurrentCompanionCount(),Is.GreaterThanOrEqualTo(32));
                Assert.That(npc.CanRecruit(),Is.True,"Existing companions must not prevent recruitment");
            } finally {foreach(var go in objects) Object.DestroyImmediate(go);}
        }
    }
}
