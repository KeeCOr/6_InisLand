using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class VillageSquareLayoutTests
    {
        [UnityTest]
        public IEnumerator StarterResourcesArePreservedOutsideTheSquareAndGate()
        {
            var center=new Vector2(5000,5000);
            var tree=new GameObject("Square tree");tree.transform.position=center+Vector2.one;
            tree.AddComponent<CircleCollider2D>();var wood=tree.AddComponent<Gatherable>();wood.YieldAmount=7;
            var rock=new GameObject("Square rock");rock.transform.position=center-Vector2.one;
            rock.AddComponent<CircleCollider2D>();var stone=rock.AddComponent<Gatherable>();stone.YieldKind=ResourceKind.Stone;
            var far=new GameObject("Far tree");far.transform.position=center+Vector2.right*30;far.AddComponent<Gatherable>();
            var original=far.transform.position;
            Physics2D.SyncTransforms();VillageSquareLayout.MoveNaturalResourcesToOutskirts(center,5);
            foreach(var go in new[]{tree,rock})
            {
                Vector2 offset=(Vector2)go.transform.position-center;
                Assert.That(Mathf.Max(Mathf.Abs(offset.x),Mathf.Abs(offset.y)),Is.GreaterThanOrEqualTo(8f-.001f));
                Assert.That(offset.y>=0 || Mathf.Abs(offset.x)>=2f,Is.True);
                Assert.That(go.GetComponent<Gatherable>(),Is.Not.Null);
            }
            Assert.That(wood.YieldAmount,Is.EqualTo(7));Assert.That(far.transform.position,Is.EqualTo(original));
            var position=tree.transform.position;VillageSquareLayout.MoveNaturalResourcesToOutskirts(center,5);
            Assert.That(tree.transform.position,Is.EqualTo(position),"Loading once again must not keep pushing resources outward");
            Object.Destroy(tree);Object.Destroy(rock);Object.Destroy(far);yield return null;
        }
    }
}
