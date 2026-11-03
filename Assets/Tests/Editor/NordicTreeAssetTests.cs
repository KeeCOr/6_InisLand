using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine.U2D;

namespace IL6.Tests
{
    public class NordicTreeAssetTests
    {
        [Test]
        public void ProceduralTreesUseAllSpeciesWithoutChangingTrunkColliders()
        {
            var create = typeof(ProceduralSpawner).GetMethod("CreateTree", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var seen = new System.Collections.Generic.HashSet<Sprite>();
            for (int i = 0; i < 90; i++)
            {
                var tree = (GameObject)create.Invoke(null, new object[] { i * 1.173f, -i * .839f });
                try
                {
                    var sprite = tree.GetComponent<SpriteRenderer>().sprite;
                    seen.Add(sprite);
                    var collider = tree.GetComponent<CircleCollider2D>();
                    Assert.That(collider.radius, Is.EqualTo(.16f));
                    Assert.That(collider.offset, Is.EqualTo(new Vector2(0, -.24f)));
                    Assert.That(tree.GetComponent<Gatherable>().IsTree, Is.True);
                    Assert.That(tree.GetComponent<TreeVisibility>(), Is.Not.Null);
                    Assert.That(tree.transform.localScale.x, Is.InRange(2.15f, 2.2f));
                    int hx=Mathf.FloorToInt(i*1.173f*37.17f),hy=Mathf.FloorToInt(-i*.839f*53.31f);
                    int oldVariant=(int)((uint)(hx*73856093^hy*19349663)%3);
                    Assert.That(tree.transform.localScale.x,Is.EqualTo(oldVariant==1?2.2f:2.15f),"Art diversity preserves each position's original physics scale");
                    Assert.That(new[] { "spruce", "pine", "birch", "fir", "sparse_spruce", "birch_cluster", "oak", "cedar", "larch" }, Does.Contain(sprite.name));
                }
                finally { Object.DestroyImmediate(tree); }
            }
            Assert.That(seen.Count, Is.EqualTo(SpriteBank.TreeVariantCount));
        }

        [Test]
        public void NineDistinctIllustratedTreesHaveConsistentScaleAndGroundAnchor()
        {
            Assert.That(SpriteBank.TreeVariantCount,Is.EqualTo(9));
            var seen = new System.Collections.Generic.HashSet<Sprite>();
            for (int i = 0; i < SpriteBank.TreeVariantCount; i++)
            {
                var sprite = SpriteBank.TreeVariant(i);
                Assert.That(sprite, Is.Not.Null);
                Assert.That(seen.Add(sprite), Is.True);
                Assert.That(sprite.texture.width, Is.GreaterThanOrEqualTo(512));
                int minY = sprite.texture.height, maxY = 0;
                var source = new Texture2D(2, 2);
                source.LoadImage(System.IO.File.ReadAllBytes(UnityEditor.AssetDatabase.GetAssetPath(sprite.texture)));
                var pixels = source.GetPixels32();
                for (int y = 0; y < sprite.texture.height; y++)
                    for (int x = 0; x < sprite.texture.width; x++)
                        if (pixels[y * sprite.texture.width + x].a > 16)
                        { minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y); }
                float height = (maxY - minY + 1) / sprite.pixelsPerUnit;
                Assert.That(height, Is.InRange(1.34f, 1.61f));
                Assert.That((minY - sprite.pivot.y) / sprite.pixelsPerUnit, Is.EqualTo(-.24f).Within(.005f));
                Assert.That(minY, Is.GreaterThan(8));
                Assert.That(maxY, Is.LessThan(sprite.texture.height - 8));
                // Procedural root scales 2.15–2.2; trees stay around three adult character heights.
                Assert.That(height * 2.2f, Is.InRange(2.9f, 3.6f));
                Object.DestroyImmediate(source);
            }
            Assert.That(SpriteBank.TreeVariant(int.MinValue), Is.Not.Null);
        }
        [Test] public void ResizedTreeTexturesCannotBePackedIntoSpriteAtlases()
        {
            foreach(var guid in AssetDatabase.FindAssets("t:SpriteAtlas"))
            {
                var atlas=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(guid));
                foreach(var packed in atlas.GetPackables())
                {
                    string path=AssetDatabase.GetAssetPath(packed).Replace('\\','/');
                    for(int i=0;i<SpriteBank.TreeVariantCount;i++)
                    {
                        string tree=AssetDatabase.GetAssetPath(SpriteBank.TreeVariant(i).texture).Replace('\\','/');
                        Assert.That(path==tree || AssetDatabase.IsValidFolder(path) && tree.StartsWith(path+"/"),Is.False,tree+" must keep its original texture/rect in player builds");
                    }
                }
            }
        }
    }
}
