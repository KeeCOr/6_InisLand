using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public sealed class WeaponStatUpgradeGameplayTests
    {
        [UnityTest]
        public IEnumerator WeaponSpecificStatsChangeRealMeleeHitsAndArrowDamageIndependently()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            var actor=new GameObject("Stat upgrade actor");actor.SetActive(false);actor.tag="Player";
            actor.transform.position=new Vector3(6500,6500,0);actor.AddComponent<SpriteRenderer>();
            var p=actor.AddComponent<PlayerProgression>();var player=actor.AddComponent<PlayerController>();player.enabled=false;
            var melee=ScriptableObject.CreateInstance<WeaponDefinition>();melee.Id="longsword";melee.BaseDamage=20;melee.Range=5;melee.CooldownSec=.5f;melee.CritChance=0;
            var ranged=ScriptableObject.CreateInstance<WeaponDefinition>();ranged.Id="bow";ranged.BaseDamage=20;ranged.Range=5;ranged.ProjectileSpeed=10;ranged.CooldownSec=.5f;ranged.CritChance=0;
            var attack=actor.AddComponent<PlayerAttackController>();attack.Weapon=melee;attack.Progression=p;attack.enabled=false;
            var foe=new GameObject("Stat damage target");foe.SetActive(false);foe.transform.position=actor.transform.position+Vector3.right;
            var zombie=foe.AddComponent<Zombie>();zombie.enabled=false;
            actor.SetActive(true);foe.SetActive(true);zombie.InitHp(1000);
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            var hit=typeof(PlayerAttackController).GetMethod("TryAttackTarget",flags);
            var cooldown=typeof(PlayerAttackController).GetField("_cooldown",flags);
            try
            {
                p.ApplyRune(RuneKind.MeleeDamageUp);
                Assert.That((bool)hit.Invoke(attack,new object[]{zombie}),Is.True);
                Assert.That(zombie.CurrentHp,Is.EqualTo(975));
                attack.Weapon=ranged;cooldown.SetValue(attack,0f);player.Actions.ResetToIdle();
                Assert.That((bool)hit.Invoke(attack,new object[]{zombie}),Is.True);
                var arrows=Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None);
                var arrow=System.Array.Find(arrows,x=>x.OwnerProgression==p);Assert.That(arrow,Is.Not.Null);
                Assert.That(arrow.Damage,Is.EqualTo(20),"Melee upgrade must not improve arrows");Object.DestroyImmediate(arrow.gameObject);
                p.ApplyRune(RuneKind.RangedDamageUp);cooldown.SetValue(attack,0f);player.Actions.ResetToIdle();
                Assert.That((bool)hit.Invoke(attack,new object[]{zombie}),Is.True);
                arrow=System.Array.Find(Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None),x=>x.OwnerProgression==p);
                Assert.That(arrow.Damage,Is.EqualTo(25));Object.DestroyImmediate(arrow.gameObject);
                attack.Weapon=melee;cooldown.SetValue(attack,0f);player.Actions.ResetToIdle();
                Assert.That((bool)hit.Invoke(attack,new object[]{zombie}),Is.True);
                Assert.That(zombie.CurrentHp,Is.EqualTo(950),"Ranged upgrade must not improve melee damage");
                int hpBefore=player.MaxHp;p.ApplyRune(RuneKind.HpUp);
                Assert.That(player.MaxHp,Is.EqualTo(hpBefore+25));
            }
            finally {Object.Destroy(actor);Object.Destroy(foe);Object.Destroy(melee);Object.Destroy(ranged);Time.timeScale=1;InputReader.HudBlocked=false;}
            yield return new WaitForSeconds(.5f);
        }
    }
}
