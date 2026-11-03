using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class TreeVisualStyleTests
    {
        [Test]
        public void ResizableTreeTexturesAreNotIncludedThroughAtlasFolders()
        {
            const string trees="Assets/Resources/Sprites/Props/NordicTrees20261102";
            foreach(var guid in UnityEditor.AssetDatabase.FindAssets("t:SpriteAtlas"))
            {
                var atlas=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.U2D.SpriteAtlas>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
                foreach(var item in UnityEditor.U2D.SpriteAtlasExtensions.GetPackables(atlas))
                {
                    string path=UnityEditor.AssetDatabase.GetAssetPath(item);
                    Assert.That(path==trees || path.StartsWith(trees+"/"),Is.False);
                    if(UnityEditor.AssetDatabase.IsValidFolder(path)) Assert.That(trees.StartsWith(path+"/"),Is.False);
                }
            }
        }
        [TestCase(0,1f)] [TestCase(1,1f)] [TestCase(2,1f)]
        [TestCase(0,2.15f)] [TestCase(1,2.2f)] [TestCase(2,2.2f)]
        public void SceneAndProceduralTreesShareWorldHeightWithoutChangingPhysics(int variant,float scale)
        {
            var go=new GameObject("Tree style size");go.transform.localScale=Vector3.one*scale;
            try
            {
                var renderer=go.AddComponent<SpriteRenderer>();
                var collider=go.AddComponent<CircleCollider2D>();collider.radius=.16f;collider.offset=new Vector2(0,-.24f);
                TreeVisualStyle.Apply(renderer,variant);
                var sprite=renderer.sprite;Assert.That(sprite,Is.Not.Null);
                var original=SpriteBank.TreeVariant(variant);
                float oldHeight=original.name=="pine"?1.35f:original.name=="birch"?1.6f:1.5f;
                float worldHeight=oldHeight*original.pixelsPerUnit/sprite.pixelsPerUnit*scale;
                Assert.That(worldHeight,Is.InRange(3.09f,3.36f));
                float minY=original.pivot.y-.24f*original.pixelsPerUnit;
                Assert.That((minY-sprite.pivot.y)/sprite.pixelsPerUnit,Is.EqualTo(-.24f).Within(.001f));
                Assert.That(go.transform.localScale,Is.EqualTo(Vector3.one*scale));
                Assert.That(collider.radius,Is.EqualTo(.16f));Assert.That(collider.offset,Is.EqualTo(new Vector2(0,-.24f)));
                TreeVisualStyle.Apply(renderer,variant);Assert.That(renderer.sprite,Is.SameAs(sprite),"Reuse sprite geometry rather than allocating every call");
            }
            finally {Object.DestroyImmediate(go);}
        }
    }
}
