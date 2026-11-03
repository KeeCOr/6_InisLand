using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class RecruitConversationTests
    {
        [UnityTest] public IEnumerator SpaceAcceptsOnlyAfterOpeningKeyIsReleasedAndNewPressOccurs()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            hud.Player.transform.position=new Vector3(5000,5000,0);
            var go=new GameObject("Keyboard conversation candidate");go.transform.position=hud.Player.transform.position+Vector3.right;
            var npc=go.AddComponent<RecruitableNpc>();npc.Player=hud.Player.transform;
            Assert.That(hud.BeginRecruitConversation(npc),Is.True);
            var press=new Event{type=EventType.KeyDown,keyCode=KeyCode.Space};
            Assert.That(hud.HandleRecruitConversationKey(press),Is.True);
            Assert.That(press.type,Is.EqualTo(EventType.Used));
            Assert.That(go.GetComponent<Companion>(),Is.Null,"Opening key cannot accept in the same frame");
            yield return null;
            hud.HandleRecruitConversationKey(new Event{type=EventType.KeyDown,keyCode=KeyCode.Space});
            Assert.That(go.GetComponent<Companion>(),Is.Null,"Held-key repeat must wait for release");
            Assert.That(Time.timeScale,Is.Zero);
            hud.HandleRecruitConversationKey(new Event{type=EventType.KeyUp,keyCode=KeyCode.Space});
            hud.HandleRecruitConversationKey(new Event{type=EventType.KeyDown,keyCode=KeyCode.Space});
            Assert.That(go.GetComponents<Companion>().Length,Is.EqualTo(1));
            Assert.That(hud.RecruitmentConversationOpen,Is.False);
            Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(InputReader.HudBlocked,Is.False);
            Assert.That(hud.HandleRecruitConversationKey(new Event{type=EventType.KeyDown,keyCode=KeyCode.Space}),Is.False);
            Assert.That(go.GetComponents<Companion>().Length,Is.EqualTo(1));
            Object.Destroy(go);yield return SceneManager.LoadSceneAsync("BootScene");
        }

        [UnityTest] public IEnumerator EscapeDeclinesWithoutRecruiting()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            hud.Player.transform.position=new Vector3(5000,5000,0);
            var go=new GameObject("Declined keyboard candidate");go.transform.position=hud.Player.transform.position+Vector3.right;
            var npc=go.AddComponent<RecruitableNpc>();npc.Player=hud.Player.transform;
            Assert.That(hud.BeginRecruitConversation(npc),Is.True);
            Assert.That(hud.HandleRecruitConversationKey(new Event{type=EventType.KeyDown,keyCode=KeyCode.Escape}),Is.True);
            Assert.That(hud.RecruitmentConversationOpen,Is.False);Assert.That(npc.CanRecruit(),Is.True);
            Assert.That(go.GetComponent<Companion>(),Is.Null);Assert.That(Time.timeScale,Is.EqualTo(1));
            Object.Destroy(go);yield return SceneManager.LoadSceneAsync("BootScene");
        }

        [UnityTest] public IEnumerator LostCandidateAndDisabledHudReleaseConversationPause()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            hud.Player.transform.position=new Vector3(5000,5000,0);
            var go=new GameObject("Disappearing candidate");go.transform.position=hud.Player.transform.position+Vector3.right;
            var npc=go.AddComponent<RecruitableNpc>();npc.Player=hud.Player.transform;
            Assert.That(hud.BeginRecruitConversation(npc),Is.True);npc.enabled=false;
            yield return null;yield return null;
            Assert.That(hud.RecruitmentConversationOpen,Is.False);Assert.That(Time.timeScale,Is.EqualTo(1));
            Assert.That(InputReader.HudBlocked,Is.False);
            npc.enabled=true;Assert.That(hud.BeginRecruitConversation(npc),Is.True);
            hud.enabled=false;Assert.That(hud.RecruitmentConversationOpen,Is.False);
            Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(go.GetComponent<Companion>(),Is.Null);
            Object.Destroy(go);yield return SceneManager.LoadSceneAsync("BootScene");
        }
        [UnityTest] public IEnumerator AttackOffersConversationAndOnlyAcceptanceRecruits()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            var player=hud.Player;player.transform.position=new Vector3(5000,5000,0);
            var go=new GameObject("Conversation candidate");go.transform.position=player.transform.position+Vector3.right;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=SpriteBank.CompanionSpriteForRole("Hunter");
            var npc=go.AddComponent<RecruitableNpc>();npc.Player=player.transform;
            yield return null;
            Assert.That(hud.Attacker.TryManualAction(),Is.True);
            Assert.That(npc.DialoguePortrait,Is.SameAs(DialoguePortraits.ForRole(npc.Role)));
            Assert.That(npc.DialoguePortrait,Is.Not.Null,"Dialogue needs its character illustration");
            Assert.That(go.GetComponent<Companion>(),Is.Null,"Opening dialogue must not recruit");
            Assert.That(Time.timeScale,Is.Zero);Assert.That(InputReader.HudBlocked,Is.True);
            var decline=typeof(SimpleHud).GetMethod("DeclineRecruitConversation");
            Assert.That(decline,Is.Not.Null);decline.Invoke(hud,null);
            Assert.That(npc.CanRecruit(),Is.True);Assert.That(Time.timeScale,Is.EqualTo(1));
            yield return new WaitForSeconds(.4f);
            Assert.That(hud.Attacker.TryManualAction(),Is.True);
            var accept=typeof(SimpleHud).GetMethod("AcceptRecruitConversation");Assert.That(accept,Is.Not.Null);
            Assert.That((bool)accept.Invoke(hud,null),Is.True);
            Assert.That(go.GetComponents<Companion>().Length,Is.EqualTo(1));
            Assert.That((bool)accept.Invoke(hud,null),Is.False);
            Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(InputReader.HudBlocked,Is.False);
            Object.Destroy(go);yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
