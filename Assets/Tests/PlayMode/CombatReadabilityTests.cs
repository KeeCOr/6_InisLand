using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace IL6.Tests
{
    public class CombatReadabilityTests
    {
        [UnityTest]
        public IEnumerator ArrowUsesNeutralDirectionalArtWithoutOrbGlowOrGhosts()
        {
            Time.timeScale = 1f;
            foreach(var night in Object.FindObjectsByType<NightController>(FindObjectsSortMode.None)) Object.Destroy(night);
            yield return null;
            var go = new GameObject("Readable arrow test");
            go.transform.position = new Vector3(3000,3000);
            var body = go.AddComponent<SpriteRenderer>(); body.color = Color.yellow;
            var arrow = go.AddComponent<Projectile>(); arrow.Speed = 0f;
            arrow.UseArrowAppearance(); arrow.AimDirection(Vector2.up);
            Assert.That(body.sprite, Is.EqualTo(Resources.Load<Sprite>("Sprites/FX/nordic_arrow")));
            Assert.That(body.color, Is.EqualTo(Color.white));
            Assert.That(body.sprite.bounds.size.x * go.transform.lossyScale.x, Is.InRange(.65f,.85f));
            Assert.That(Vector2.Dot(go.transform.right,Vector2.up), Is.GreaterThan(.999f));
            yield return null; yield return null;
            Assert.That(go.transform.Find("__glow"), Is.Null);
            foreach(var sprite in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
                if(sprite.gameObject.name == "__pjtrail") Assert.That(Vector2.Distance(sprite.transform.position,go.transform.position), Is.GreaterThanOrEqualTo(1f));
            arrow.AimDirection(Vector2.left);
            Assert.That(Vector2.Dot(go.transform.right,Vector2.left), Is.GreaterThan(.999f));
            var foe = new GameObject("Arrow turn target"); foe.transform.position = go.transform.position + Vector3.down * 5f;
            var zombie = foe.AddComponent<Zombie>(); zombie.InitHp(1000); zombie.enabled = false;
            arrow.Aim(zombie,go.transform.position);
            foe.transform.position = go.transform.position + Vector3.right * 5f;
            yield return null; yield return null;
            Assert.That(Vector2.Dot(go.transform.right,Vector2.right), Is.GreaterThan(.999f), "Homing must rotate the shaft with the updated travel direction");
            Object.Destroy(foe);
            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator CombatBoundariesUseRuneRangeAndHideWhilePausedOrBlocked()
        {
            Time.timeScale = 1f; InputReader.HudBlocked = false;
            yield return SceneManager.LoadSceneAsync("BootScene");
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");
            yield return new WaitForSeconds(2f);
            var hud = Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            hud.enabled = false;
            foreach(var night in Object.FindObjectsByType<NightController>(FindObjectsSortMode.None)) Object.Destroy(night);
            // Test an empty combat area; teleporting otherwise streams new nearby resources/NPCs.
            foreach(var spawner in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None)) spawner.enabled = false;
            yield return null;
            hud.Player.transform.position = new Vector3(3000,3000);
            hud.Progression.ApplyRune(RuneKind.RangeUp);
            Assert.That(hud.Progression.BonusRange, Is.GreaterThan(0f));
            hud.Attacker.SwitchToWeapon(0);
            yield return null; yield return null;
            var melee = hud.Player.GetComponent<MeleeRangeIndicator>();
            Assert.That(ManualWorldInteraction.Resolve(hud.Player).Kind, Is.EqualTo(ManualInteractionKind.Attack), "The range fixture must have no nearby work target");
            Assert.That(melee.Visible, Is.True);
            Assert.That(melee.DisplayedRange, Is.EqualTo(hud.Attacker.Weapon.Range+hud.Progression.BonusRange));
            var contrast = melee.Decal;
            Assert.That(contrast.Visible, Is.True);
            Assert.That(contrast.Renderer.bounds.extents.x, Is.EqualTo(melee.DisplayedRange).Within(.001f));
            Assert.That(contrast.transform.parent, Is.Null, "Decals must not inherit the actor SortingGroup or scale");
            Assert.That(contrast.Renderer.sortingOrder, Is.InRange(-9499,-9001));
            hud.Attacker.SwitchToWeapon(1);
            yield return null; yield return null;
            var preview = hud.Player.GetComponent<PlayerInteractionPreview>();
            Assert.That(melee.Visible, Is.False);
            Assert.That(preview.BowRangeVisible, Is.True);
            Assert.That(preview.DisplayedRange, Is.EqualTo(hud.Attacker.Weapon.Range+hud.Progression.BonusRange));
            Assert.That(hud.Attacker.Weapon.Range, Is.EqualTo(10f));
            var bow = preview.BowDecal;
            Assert.That(bow.Renderer.bounds.extents.x, Is.EqualTo(preview.DisplayedRange).Within(.001f));
            Assert.That(bow.Renderer.bounds.extents.y, Is.EqualTo(preview.DisplayedRange).Within(.001f));
            Assert.That(bow.transform.parent, Is.Null);
            Assert.That(bow.Renderer.sortingOrder, Is.InRange(-9499,-9001));
            hud.Player.transform.position += Vector3.right * 2;
            yield return null; yield return null;
            Assert.That(bow.transform.position, Is.EqualTo(hud.Player.transform.position));
            var rangedTarget = new GameObject("Extended bow range target");
            rangedTarget.transform.position = hud.Player.transform.position + Vector3.right * (preview.DisplayedRange - .1f);
            rangedTarget.AddComponent<CircleCollider2D>().radius = .3f;
            var zombie = rangedTarget.AddComponent<Zombie>(); zombie.InitHp(1000); zombie.enabled = false;
            Physics2D.SyncTransforms();
            Assert.That(hud.Attacker.FindManualCombatTarget(), Is.EqualTo(zombie), "The enlarged decal must actually allow targets beyond the previous 7-unit range");
            rangedTarget.transform.position = hud.Player.transform.position + Vector3.right * (preview.DisplayedRange + .1f);
            Physics2D.SyncTransforms();
            Assert.That(hud.Attacker.FindManualCombatTarget(), Is.Null, "Collider overlap must not admit a target center outside the decal");
            Object.Destroy(rangedTarget);
            Time.timeScale = 0f; yield return null; yield return null;
            Assert.That(preview.BowRangeVisible, Is.False); Assert.That(preview.Visible, Is.False);
            Time.timeScale = 1f; InputReader.HudBlocked = true;
            yield return null; yield return null;
            Assert.That(preview.BowRangeVisible, Is.False); Assert.That(contrast.Visible, Is.False);
            InputReader.HudBlocked = false;
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
