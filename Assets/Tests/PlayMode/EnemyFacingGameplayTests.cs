using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public sealed class EnemyFacingGameplayTests
    {
        [UnityTest] public IEnumerator MovementAndStationaryAttacksFaceTheirActualTarget()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            var go=new GameObject("Enemy facing test");go.transform.position=new Vector3(7000,7000,0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=SpriteBank.QuadrupedRobot();
            var enemy=go.AddComponent<Zombie>();enemy.enabled=false;
            var target=new GameObject("Facing building target");target.transform.position=go.transform.position+Vector3.left*2;
            target.AddComponent<Building>().Initialize(BuildingKind.Fence);
            var tick=typeof(Zombie).GetMethod("FixedUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
            try {
                tick.Invoke(enemy,null);Assert.That(go.GetComponent<Rigidbody2D>().velocity.x,Is.LessThan(0));
                Assert.That(renderer.flipX,Is.True,"Chasing left must turn the sprite left");
                target.transform.position=go.transform.position+Vector3.right*.2f;tick.Invoke(enemy,null);
                Assert.That(go.GetComponent<Rigidbody2D>().velocity,Is.EqualTo(Vector2.zero));
                Assert.That(renderer.flipX,Is.False,"A stopped melee attacker must face the new target");
                target.transform.position=go.transform.position+Vector3.up*.2f;tick.Invoke(enemy,null);
                Assert.That(renderer.sprite.name,Does.EndWith("_back"));
                target.transform.position=go.transform.position+Vector3.down*.2f;tick.Invoke(enemy,null);
                Assert.That(renderer.sprite.name,Does.EndWith("_front"));
                enemy.IsRanged=true;
                target.transform.position=go.transform.position+Vector3.left*2;tick.Invoke(enemy,null);
                Assert.That(go.GetComponent<Rigidbody2D>().velocity,Is.EqualTo(Vector2.zero));
                Assert.That(renderer.flipX,Is.True,"A ranged attacker must aim left even while standing still");
                target.transform.position=go.transform.position+Vector3.up*2;tick.Invoke(enemy,null);
                Assert.That(renderer.sprite.name,Does.EndWith("_back"));
                var frozen=renderer.sprite;MeleeKnockback.Apply(enemy,(Vector2)go.transform.position+Vector2.right);
                target.transform.position=go.transform.position+Vector3.right;tick.Invoke(enemy,null);
                Assert.That(renderer.sprite,Is.SameAs(frozen),"Knockback must not turn an enemy to face its forced displacement");
            } finally {Object.Destroy(go);Object.Destroy(target);foreach(var shot in Object.FindObjectsByType<ZombieProjectile>(FindObjectsSortMode.None))Object.Destroy(shot.gameObject);Time.timeScale=1;InputReader.HudBlocked=false;}
            yield return null;
        }
    }
}
