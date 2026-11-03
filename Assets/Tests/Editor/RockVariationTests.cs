using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class RockVariationTests
    {
        [Test] public void StableRockSelectionUsesSixDistinctSpritesWithoutChangingPhysics()
        {
            Assert.That(RockVisualStyle.VariantCount,Is.EqualTo(6));
            var sprites=new HashSet<Sprite>();for(int i=0;i<6;i++){var s=RockVisualStyle.SpriteFor(i);Assert.That(s,Is.Not.Null);Assert.That(s.name,Is.Not.EqualTo("slab"));sprites.Add(s);}
            Assert.That(sprites.Count,Is.EqualTo(6));
            var variants=new HashSet<int>();
            var go=new GameObject("Rock variant test");go.transform.localScale=Vector3.one*1.8f;
            try
            {
                var sr=go.AddComponent<SpriteRenderer>();var collider=go.AddComponent<CircleCollider2D>();collider.radius=.35f;
                for(int i=0;i<30;i++)
                {
                    go.transform.position=new Vector3(i*1.17f,-i*.83f,0);
                    int v=RockVisualStyle.VariantFor(go.transform.position);variants.Add(v);RockVisualStyle.Apply(sr);
                    Assert.That(sr.sprite,Is.SameAs(RockVisualStyle.SpriteFor(v)));
                    RockVisualStyle.Apply(sr);Assert.That(sr.sprite,Is.SameAs(RockVisualStyle.SpriteFor(v)));
                    Assert.That(go.transform.localScale,Is.EqualTo(Vector3.one*1.8f));Assert.That(collider.radius,Is.EqualTo(.35f));
                }
                Assert.That(variants.Count,Is.EqualTo(6));
            }
            finally{Object.DestroyImmediate(go);}
        }
        [Test] public void FlatSlabIsNeverChosenForAnyWorldPosition()
        {
            for(int i=0;i<256;i++)
            {
                var sprite=RockVisualStyle.SpriteFor(RockVisualStyle.VariantFor(new Vector2(i*1.31f,-i*.79f)));
                Assert.That(sprite,Is.Not.Null);Assert.That(sprite.name,Is.Not.EqualTo("slab"));
            }
        }
    }
}
