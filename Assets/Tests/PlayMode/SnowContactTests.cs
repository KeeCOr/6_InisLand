using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class SnowContactTests
    {
        [UnityTest] public IEnumerator SnowBaseStaysOnGroundWithoutChangingPhysicsOrLeavingOrphans()
        {
            var rock=new GameObject("Snow contact test");rock.transform.position=Vector3.one*5000;rock.transform.localScale=Vector3.one*1.8f;
            var body=rock.AddComponent<SpriteRenderer>();body.sprite=SpriteBank.SnowRock();
            var col=rock.AddComponent<CircleCollider2D>();col.radius=.35f;
            var resource=rock.AddComponent<Gatherable>();resource.YieldKind=ResourceKind.Stone;
            yield return null;yield return null;
            var contact=rock.GetComponent<SnowContact>();Assert.That(contact,Is.Not.Null);
            var pad=contact.Decal;Assert.That(pad,Is.Not.Null);Assert.That(pad.transform.parent,Is.Null);
            Assert.That(pad.sortingOrder,Is.GreaterThan(-10000).And.LessThan(-9500));
            Assert.That(pad.GetComponent<Collider2D>(),Is.Null);
            Assert.That(rock.transform.localScale,Is.EqualTo(Vector3.one*1.8f));Assert.That(col.radius,Is.EqualTo(.35f));
            rock.transform.position+=Vector3.right*2;yield return null;
            Assert.That(pad.transform.position.x,Is.EqualTo(rock.transform.position.x));
            body.enabled=false;yield return null;Assert.That(pad.enabled,Is.False);
            Object.Destroy(rock);yield return null;yield return null;
            Assert.That(pad==null,Is.True,"Removing or harvesting the object must remove its snow base");
        }
    }
}
