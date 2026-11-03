using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class VillageFenceRingTests
    {
        [Test] public void RepeatedInitializationPreservesMissingFencesAndManualConstruction()
        {
            var center=new Vector3(2000,2000,0);
            VillageStarter.SpawnStarterVillage(center);
            var manual=VillageStarter.SpawnFence(center+Vector3.right*30,0,FencePieceType.Center);
            BuildingFactory.SpawnHouse(center+Vector3.right*2);
            try {
                var root=GameObject.Find("VillageOuterFence");
                int before=root.GetComponentsInChildren<Building>().Length;
                Building missing=null;
                foreach(var b in root.GetComponentsInChildren<Building>()) if(b.GetComponent<Door>()==null) {missing=b;break;}
                Object.DestroyImmediate(missing.gameObject);
                foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
                    if(b.Kind==BuildingKind.Campfire && Vector2.Distance(b.transform.position,center)<2) Object.DestroyImmediate(b.gameObject);
                VillageStarter.SpawnStarterVillage(center);
                Assert.That(GameObject.Find("VillageOuterFence"),Is.SameAs(root));
                Assert.That(root.GetComponentsInChildren<Building>().Length,Is.EqualTo(before-1),"Destroyed pieces must not regenerate");
                int gates=0;
                foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None)) {
                    if(b.Kind!=BuildingKind.Fence || b.gameObject==manual) continue;
                    var d=b.transform.position-center;
                    Assert.That(Mathf.Max(Mathf.Abs(d.x),Mathf.Abs(d.y)),Is.EqualTo(VillageStarter.CurrentHalfSize).Within(.06f),"Only the current outer ring should remain");
                    if(b.GetComponent<Door>()!=null) gates++;
                }
                Assert.That(gates,Is.EqualTo(1));
                Assert.That(manual.activeInHierarchy,Is.True);
                VillageStarter.SpawnStarterVillage(center);
                Assert.That(VillageStarter.CurrentHalfSize,Is.EqualTo(5f),"Unrelated construction must not replace or expand the perimeter");
            } finally {
                foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
                    if(Vector2.Distance(b.transform.position,center)<50) Object.DestroyImmediate(b.gameObject);
                var ring=GameObject.Find("VillageOuterFence");if(ring!=null) Object.DestroyImmediate(ring);
            }
        }
    }
}
