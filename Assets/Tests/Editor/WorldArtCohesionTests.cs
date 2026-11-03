using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;

namespace IL6.Tests
{
    public sealed class WorldArtCohesionTests
    {
        [Serializable] private class Manifest { public Entry[] entries; public Piece[] modular_entries; }
        [Serializable] private class Entry { public string key,path; public float ppu,painted_width,foot_y; }
        [Serializable] private class Piece { public string key,path,guid; public int[] old_rect; public float old_ppu; public long local_id; }
        private static Manifest ReadManifest()=>JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/art/generated/environment/art_cohesion20261103/generation_manifest.json"));
        [Test] public void RedrawnArtPreservesBindingsFootingAndUsesTransparentSmoothSprites()
        {
            var manifest=ReadManifest();
            Assert.That(manifest.entries.Length,Is.EqualTo(22));
            foreach(var e in manifest.entries)
            {
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(e.path);
                Assert.That(sprite,Is.Not.Null,e.key);
                Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Bilinear),e.key);
                Assert.That(sprite.pixelsPerUnit,Is.EqualTo(e.ppu).Within(.01f),e.key);
                Assert.That(Resources.Load<Sprite>(e.path.Substring("Assets/Resources/".Length).Replace(".png","")),Is.SameAs(sprite),e.key);
                var tex=sprite.texture;var pixels=tex.GetPixels32();int x0=tex.width,x1=-1,y0=tex.height;
                for(int y=0;y<tex.height;y++)for(int x=0;x<tex.width;x++)if(pixels[y*tex.width+x].a>16)
                {x0=Mathf.Min(x0,x);x1=Mathf.Max(x1,x);y0=Mathf.Min(y0,y);}
                Assert.That((x1-x0+1)/sprite.pixelsPerUnit,Is.EqualTo(e.painted_width).Within(.005f),e.key+" visible width");
                Assert.That((y0-sprite.pivot.y)/sprite.pixelsPerUnit,Is.EqualTo(e.foot_y).Within(.005f),e.key+" ground baseline");
                Assert.That(pixels[0].a,Is.LessThanOrEqualTo(4),e.key+" transparent corner");
            }
            Assert.That(ConstructionPlacement.Scale(BuildingKind.Fence),Is.EqualTo(Vector3.one));
        }
        [Test] public void EveryBossLoadsOneDedicatedHighResolutionSprite()
        {
            foreach(var sprite in new[]{SpriteBank.BossFrostZombie(),SpriteBank.BossWinterKnight(),SpriteBank.BossIronGiant(),SpriteBank.BossFrostLich()})
            {Assert.That(sprite,Is.Not.Null);Assert.That(sprite.texture.width,Is.GreaterThanOrEqualTo(1024));Assert.That(sprite.rect.size,Is.EqualTo(new Vector2(sprite.texture.width,sprite.texture.height)));}
        }
        [Test] public void ModularArtKeepsSerializedIdsAndWorldTileSizes()
        {
            var pieces=ReadManifest().modular_entries;
            Assert.That(pieces,Is.Not.Null);Assert.That(pieces.Length,Is.EqualTo(17));
            foreach(var e in pieces)
            {
                var sprite=SpriteBank.LoadSubSprite(e.path.Substring("Assets/Resources/Sprites/".Length).Replace(".png",""),e.key);
                Assert.That(sprite,Is.Not.Null,e.key);
                Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Bilinear),e.key);
                Assert.That(sprite.pixelsPerUnit,Is.EqualTo(e.old_ppu*4),e.key);
                Assert.That(sprite.rect.width/sprite.pixelsPerUnit,Is.EqualTo((e.old_rect[2]-e.old_rect[0])/e.old_ppu).Within(.001f),e.key);
                Assert.That(sprite.rect.height/sprite.pixelsPerUnit,Is.EqualTo((e.old_rect[3]-e.old_rect[1])/e.old_ppu).Within(.001f),e.key);
                Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite,out string guid,out long id),Is.True);
                Assert.That(guid,Is.EqualTo(e.guid),e.key);Assert.That(id,Is.EqualTo(e.local_id),e.key+" serialized Sprite reference");
            }
        }
    }
}
