using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public sealed class CharacterHpBarAlignmentTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void EveryActorUsesSameHeadGapWithNarrowerNonPlayerBars(int kind)
        {
            var parent=new GameObject("Scaled parent");
            parent.transform.localScale=new Vector3(.9f,1.2f,1);
            var go=new GameObject("Actor");go.SetActive(false);
            go.transform.SetParent(parent.transform);
            go.transform.localScale=new Vector3(.75f,1.4f,1);
            var texture=new Texture2D(32,64);
            var sprite=Sprite.Create(texture,new Rect(0,0,32,64),new Vector2(.5f,0),32);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;
            var bar=go.AddComponent<HpBarUi>();
            if(kind==0)bar.Player=go.AddComponent<PlayerController>();
            if(kind==1)bar.CompanionRef=go.AddComponent<Companion>();
            if(kind==2)bar.Zombie=go.AddComponent<Zombie>();
            if(kind==3)bar.AnimalRef=go.AddComponent<DeerAi>();
            var flags=BindingFlags.NonPublic|BindingFlags.Instance;
            typeof(HpBarUi).GetMethod("Awake",flags).Invoke(bar,null);
            bar.Offset=new Vector2(5,12); // Legacy per-spawner offsets must not shift actors.
            typeof(HpBarUi).GetMethod("LateUpdate",flags).Invoke(bar,null);
            var root=go.transform.Find("HpBar");
            Assert.That(root.position.x,Is.EqualTo(go.transform.position.x).Within(.001f));
            Assert.That(root.position.y-renderer.bounds.max.y,Is.EqualTo(.18f).Within(.001f));
            Assert.That(Mathf.Abs(root.lossyScale.x),Is.EqualTo(bar.Size.x*(kind==0?1f:.7f)).Within(.001f));
            Assert.That(Mathf.Abs(root.lossyScale.y),Is.EqualTo(Mathf.Min(bar.Size.y,kind==0?.12f:.10f)).Within(.001f));
            Object.DestroyImmediate(parent);Object.DestroyImmediate(sprite);Object.DestroyImmediate(texture);
        }
    }
}
