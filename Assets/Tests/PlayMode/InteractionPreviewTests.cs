using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class InteractionPreviewTests
    {
        [UnityTest]
        public IEnumerator ResolverKeepsNpcAboveResourcesAndRepairRequiresHammer()
        {
            Time.timeScale = 1f; InputReader.HudBlocked = false;
            yield return SceneManager.LoadSceneAsync("BootScene");
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");
            yield return new WaitForSeconds(2f);
            var hud = Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(hud, null);
            // Direct-action fixture: prevent HUD Update from opening the build catalog after hammer equip.
            hud.enabled = false;
            foreach (var night in Object.FindObjectsByType<NightController>(FindObjectsSortMode.None)) Object.Destroy(night);
            yield return null;
            var player = hud.Player;
            var attack = player.GetComponent<PlayerAttackController>();
            // This probe invokes manual actions explicitly; automatic combat is covered separately.
            attack.enabled = false;
            player.transform.position = new Vector3(1000, 1000);
            var house = BuildingFactory.SpawnHouse(player.transform.position + Vector3.right).GetComponent<Building>();
            house.TakeDamage(30);
            attack.SwitchToWeapon(0);
            var facilityWork = ManualWorldInteraction.Resolve(player);
            Assert.That(facilityWork.Kind, Is.EqualTo(ManualInteractionKind.Work));
            Assert.That(facilityWork.Kind, Is.Not.EqualTo(ManualInteractionKind.Repair));
            Assert.That(facilityWork.Target, Is.Null, "Combat equipment offers facility work rather than hammer repair");
            Assert.That(facilityWork.Label, Is.EqualTo("연구 · 건물별 업그레이드"));
            Assert.That(facilityWork.Callback, Is.Not.Null);
            attack.SwitchToWeapon(2);
            Assert.That(ManualWorldInteraction.Resolve(player).Target, Is.EqualTo(house));
            var npcObject = new GameObject("Preview dialogue priority");
            npcObject.transform.position = player.transform.position + Vector3.up;
            var npc = npcObject.AddComponent<RecruitableNpc>(); npc.Player = player.transform;
            Assert.That(ManualWorldInteraction.Resolve(player).Target, Is.EqualTo(npc));
            Assert.That(ManualWorldInteraction.Hint(player), Is.EqualTo("대화"));
            InputReader.HudBlocked = true;
            Assert.That(ManualWorldInteraction.Resolve(player).Kind, Is.EqualTo(ManualInteractionKind.None));
            InputReader.HudBlocked = false; Time.timeScale = 0f;
            Assert.That(ManualWorldInteraction.Resolve(player).Kind, Is.EqualTo(ManualInteractionKind.None));
            Time.timeScale = 1f;
            Object.Destroy(npcObject); Object.Destroy(house.gameObject);
            yield return null;
            attack.SwitchToWeapon(0);
            var resource = new GameObject("Combat priority tree");
            resource.transform.position = player.transform.position + Vector3.right;
            resource.AddComponent<CircleCollider2D>(); resource.AddComponent<Gatherable>();
            var foe = new GameObject("Combat priority target");
            foe.transform.position = player.transform.position + Vector3.left * .5f;
            foe.AddComponent<CircleCollider2D>();
            var zombie = foe.AddComponent<Zombie>(); zombie.InitHp(1000); zombie.enabled = false;
            Physics2D.SyncTransforms();
            yield return null; yield return null;
            Assert.That(ManualWorldInteraction.Resolve(player).Target, Is.EqualTo(zombie));
            Assert.That(player.GetComponent<PlayerInteractionPreview>().DisplayedTarget.Target, Is.EqualTo(zombie));
            Assert.That(attack.TryManualAction(), Is.True);
            Assert.That(zombie.CurrentHp, Is.LessThan(1000));
            Assert.That(hud.Gather.IsActive, Is.False);
            Object.Destroy(resource); Object.Destroy(foe);
            yield return SceneManager.LoadSceneAsync("BootScene");
        }

        [UnityTest]
        public IEnumerator PreviewAndSpaceKeepTheSameGatherTargetUntilWorkEnds()
        {
            Time.timeScale = 1f; InputReader.HudBlocked = false;
            yield return SceneManager.LoadSceneAsync("BootScene");
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");
            yield return new WaitForSeconds(2f);
            var hud = Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(hud, null);
            // Keep synthetic HudBlocked state stable and isolate probes from dawn cleanup.
            hud.enabled = false;
            foreach (var night in Object.FindObjectsByType<NightController>(FindObjectsSortMode.None)) Object.Destroy(night);
            yield return null;
            var player = hud.Player; player.transform.position = new Vector3(1000,1000);
            hud.Attacker.SwitchToWeapon(0);
            var tree = new GameObject("Preview tree"); tree.transform.position = player.transform.position + Vector3.right;
            tree.AddComponent<CircleCollider2D>();
            var target = tree.AddComponent<Gatherable>(); target.DurationSec = 20f;
            var rock = new GameObject("Preview rock"); rock.transform.position = player.transform.position + Vector3.left * 2f;
            rock.AddComponent<CircleCollider2D>().radius = .2f;
            rock.AddComponent<Gatherable>().YieldKind = ResourceKind.Stone;
            Physics2D.SyncTransforms();
            yield return null;
            var preview = player.GetComponent<PlayerInteractionPreview>();
            Assert.That(preview, Is.Not.Null);
            // Merely showing a preview must never begin work.
            Assert.That(hud.Gather.IsActive, Is.False);
            yield return null;
            Assert.That(preview.DisplayedTarget.Target, Is.EqualTo(target));
            Assert.That(preview.Visible, Is.True);
            var marker=(LineRenderer)typeof(PlayerInteractionPreview).GetField("_targetRing",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(preview);
            Assert.That(marker.loop,Is.False,"Selection must not wrap the object in a circle");
            Assert.That(marker.startColor.b,Is.GreaterThan(marker.startColor.r),"Selection uses cool snow tones instead of yellow");
            Assert.That(marker.sortingOrder,Is.LessThan(-9000),"Selection belongs on the ground below actors");
            Assert.That(hud.Attacker.TryManualAction(), Is.True);
            Assert.That(hud.Gather.ActiveTarget, Is.EqualTo(target));
            var foe = new GameObject("Preview combat during gathering");
            foe.transform.position = player.transform.position + Vector3.down * .4f;
            foe.AddComponent<CircleCollider2D>();
            var zombie = foe.AddComponent<Zombie>(); zombie.InitHp(1000); zombie.enabled = false;
            rock.transform.position = player.transform.position;
            Physics2D.SyncTransforms();
            Assert.That(ManualWorldInteraction.Resolve(player).Target, Is.EqualTo(target));
            Assert.That(hud.Attacker.TryManualAction(), Is.True);
            Assert.That(hud.Gather.ActiveTarget, Is.EqualTo(target));
            Assert.That(zombie.CurrentHp, Is.EqualTo(1000), "Active gathering stays exclusive even when an enemy approaches");
            InputReader.HudBlocked = true;
            yield return null; yield return null;
            Assert.That(preview.Visible, Is.False);
            InputReader.HudBlocked = false;
            hud.Gather.Cancel(); Object.Destroy(tree); Object.Destroy(rock); Object.Destroy(foe);
            yield return SceneManager.LoadSceneAsync("BootScene");
        }

        [UnityTest]
        public IEnumerator CampfireFallbackSharesLabelAndCallbackWithoutPreviewRefuelling()
        {
            Time.timeScale = 1f; InputReader.HudBlocked = false;
            yield return SceneManager.LoadSceneAsync("BootScene");
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");
            yield return new WaitForSeconds(2f);
            var hud = Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(hud, null);
            hud.enabled = false;
            foreach (var night in Object.FindObjectsByType<NightController>(FindObjectsSortMode.None)) Object.Destroy(night);
            yield return null;
            var player = hud.Player; player.transform.position = new Vector3(1000,1000);
            hud.Attacker.SwitchToWeapon(0);
            var fire = VillageStarter.SpawnCampfire(player.transform.position + Vector3.right);
            var aura = fire.GetComponent<CampfireAura>(); aura.Fuel = 20f; aura.enabled = false;
            var store = GameSession.Instance.Resources; store.Add(ResourceKind.Wood,10);
            int wood = store.Get(ResourceKind.Wood);
            yield return null; yield return null;
            var resolved = ManualWorldInteraction.Resolve(player);
            Assert.That(resolved.Kind, Is.EqualTo(ManualInteractionKind.Work));
            Assert.That(resolved.Callback, Is.Not.Null);
            Assert.That(resolved.Label, Does.StartWith("불 보충"));
            Assert.That(hud.Attacker.ManualActionLabel, Is.EqualTo(resolved.Label));
            Assert.That(hud.Attacker.HasManualActionTarget, Is.True);
            Assert.That(player.GetComponent<PlayerInteractionPreview>().DisplayedTarget.Kind, Is.EqualTo(ManualInteractionKind.Work));
            Assert.That(player.GetComponent<PlayerInteractionPreview>().Visible, Is.False, "Work fallback must not show a combat aim line");
            Assert.That(player.GetComponent<MeleeRangeIndicator>().Visible, Is.False);
            Assert.That(aura.Fuel, Is.EqualTo(20f), "Resolving and rendering cannot execute the callback");
            Assert.That(store.Get(ResourceKind.Wood), Is.EqualTo(wood));
            hud.UseUnifiedAction();
            Assert.That(aura.Fuel, Is.EqualTo(50f));
            Assert.That(store.Get(ResourceKind.Wood), Is.EqualTo(wood-1));
            Assert.That(hud.Attacker.TryManualAction(), Is.False, "Fallback work must respect the interaction cooldown");
            Assert.That(aura.Fuel, Is.EqualTo(50f));
            Object.Destroy(fire);
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
