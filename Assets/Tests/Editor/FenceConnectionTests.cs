using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class FenceConnectionTests
    {
        [Test] public void GateStatesShareGroundAnchorAndOpeningWidth()
        {
            var gate=VillageStarter.SpawnGate(Vector3.zero);
            try {
                foreach(bool open in new[]{false,true}) {
                    gate.GetComponent<Door>().SetOpen(open);
                    var art=gate.transform.Find("__FenceGateArt");Assert.That(art,Is.Not.Null);
                    var bounds=art.GetComponent<SpriteRenderer>().bounds;
                    Assert.That(bounds.min.y,Is.EqualTo(0).Within(.005f));
                    Assert.That(bounds.size.x,Is.EqualTo(1.04f).Within(.005f));
                    Assert.That(bounds.size.y,Is.EqualTo(.84f).Within(.005f));
                }
                Assert.That(gate.GetComponent<BoxCollider2D>().size,Is.EqualTo(new Vector2(1,.8f)));
            } finally {Object.DestroyImmediate(gate);}
        }
        [Test] public void RebuildingMissingRingPreservesCornerAndVerticalTypes()
        {
            var center=new Vector3(4000,4000,0);VillageStarter.SpawnStarterVillage(center);
            try {
                foreach(var building in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
                    if(building.Kind==BuildingKind.Fence && (building.transform.position==center+new Vector3(-5,5,0)
                        || building.transform.position==center+new Vector3(-5,0,0)))Object.DestroyImmediate(building.gameObject);
                Assert.That(VillageStarter.RebuildMissingOuterFences(center),Is.EqualTo(2));
                foreach(var building in Object.FindObjectsByType<Building>(FindObjectsSortMode.None)) {
                    if(building.transform.position==center+new Vector3(-5,5,0))Assert.That(building.GetComponent<SpriteRenderer>().sprite.name,Is.EqualTo("Fence_Wood_TopLeftCorner"));
                    if(building.transform.position==center+new Vector3(-5,0,0))Assert.That(building.GetComponent<SpriteRenderer>().sprite.name,Is.EqualTo("verticalFence_Wood_Center"));
                }
            } finally {
                foreach(var building in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
                    if(Vector3.Distance(building.transform.position,center)<15)Object.DestroyImmediate(building.gameObject);
                var ring=GameObject.Find("VillageOuterFence");if(ring!=null)Object.DestroyImmediate(ring);
            }
        }
        [Test] public void HorizontalVerticalAndCornerSharePostHeightAndGroundAnchor()
        {
            var h=VillageStarter.SpawnFence(Vector3.zero,0,FencePieceType.Center);
            var v=VillageStarter.SpawnVerticalFence(Vector3.right*3,VerticalFencePieceType.Center);
            var c=VillageStarter.SpawnFenceCorner(Vector3.right*6,FenceCornerPieceType.BottomLeft);
            try {
                foreach(var go in new[]{h,v,c}) {
                    var post=go.transform.Find("__FenceArt/Post");Assert.That(post,Is.Not.Null);
                    var bounds=post.GetComponent<SpriteRenderer>().bounds;
                    Assert.That(bounds.size.y,Is.EqualTo(.78f).Within(.005f));
                    Assert.That(bounds.min.y,Is.EqualTo(go.transform.position.y).Within(.005f));
                    Assert.That(go.transform.localScale,Is.EqualTo(Vector3.one));
                }
                Assert.That(h.GetComponent<BoxCollider2D>().size,Is.EqualTo(new Vector2(.82f,.58f)));
                Assert.That(v.GetComponent<BoxCollider2D>().size,Is.EqualTo(new Vector2(.8f,.9f)));
            } finally {Object.DestroyImmediate(h);Object.DestroyImmediate(v);Object.DestroyImmediate(c);}
        }
        [Test] public void NeighboringRailsOverlapWithoutChangingFenceSpacing()
        {
            var a=VillageStarter.SpawnFence(Vector3.zero,0,FencePieceType.Center);
            var b=VillageStarter.SpawnFence(Vector3.right,0,FencePieceType.Center);
            var v=VillageStarter.SpawnVerticalFence(Vector3.right*3,VerticalFencePieceType.Center);
            var w=VillageStarter.SpawnVerticalFence(Vector3.right*3+Vector3.up,VerticalFencePieceType.Center);
            try {
                var ar=a.transform.Find("__FenceArt/Rail0");Assert.That(ar,Is.Not.Null);
                var br=b.transform.Find("__FenceArt/Rail0");
                Assert.That(ar.GetComponent<SpriteRenderer>().bounds.max.x,Is.GreaterThan(br.GetComponent<SpriteRenderer>().bounds.min.x));
                var vr=v.transform.Find("__FenceArt/Rail0");var wr=w.transform.Find("__FenceArt/Rail0");
                Assert.That(vr.GetComponent<SpriteRenderer>().bounds.max.y,Is.GreaterThan(wr.GetComponent<SpriteRenderer>().bounds.min.y));
            } finally {foreach(var go in new[]{a,b,v,w})Object.DestroyImmediate(go);}
        }
    }
}
