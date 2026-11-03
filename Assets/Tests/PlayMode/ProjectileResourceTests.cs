using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class ProjectileResourceTests
    {
        [UnityTest]
        public IEnumerator DefaultAllyProjectileIsSmallStoneWithoutOrbGlow()
        {
            var go=new GameObject("Stone resource test");go.transform.position=Vector3.one*5000;
            var sr=go.AddComponent<SpriteRenderer>();sr.color=Color.green;
            var p=go.AddComponent<Projectile>();p.Speed=0;
            yield return null;
            Assert.That(sr.sprite,Is.EqualTo(Resources.Load<Sprite>("Sprites/FX/throwing_stone")));
            Assert.That(sr.sprite,Is.Not.Null);Assert.That(sr.color,Is.EqualTo(Color.white));
            Assert.That(sr.bounds.size.x,Is.InRange(.3f,.45f));
            Assert.That(go.transform.Find("__glow"),Is.Null);
            Object.Destroy(go);
        }
        [UnityTest]
        public IEnumerator TowerAndEnemyShotsUseDirectionalArrows()
        {
            var target=new GameObject("Projectile target");target.transform.position=Vector3.one*5000;
            var zombie=target.AddComponent<Zombie>();zombie.InitHp(100);zombie.enabled=false;
            var towerGo=new GameObject("Tower test");towerGo.transform.position=target.transform.position+Vector3.left*3;
            var tower=towerGo.AddComponent<Watchtower>();tower.enabled=false;
            typeof(Watchtower).GetMethod("FireAt",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(tower,new object[]{zombie});
            var shot=GameObject.Find("WatchtowerArrow");Assert.That(shot,Is.Not.Null);
            Assert.That(shot.GetComponent<SpriteRenderer>().sprite,Is.EqualTo(Resources.Load<Sprite>("Sprites/FX/nordic_arrow")));
            var enemyGo=new GameObject("Enemy shot test");enemyGo.transform.position=target.transform.position+Vector3.down*3;
            var sr=enemyGo.AddComponent<SpriteRenderer>();var enemy=enemyGo.AddComponent<ZombieProjectile>();enemy.Speed=0;enemy.Aim(target.transform);
            Assert.That(sr.sprite,Is.EqualTo(Resources.Load<Sprite>("Sprites/FX/nordic_arrow")));
            Assert.That(Vector2.Dot(enemyGo.transform.right,Vector2.up),Is.GreaterThan(.999f));
            yield return null;Assert.That(enemyGo.transform.Find("__glow"),Is.Null);
            Object.Destroy(shot);Object.Destroy(enemyGo);Object.Destroy(towerGo);Object.Destroy(target);
        }
    }
}
