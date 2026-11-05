using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class CompanionGatherLeashTests
    {
        [UnityTest] public IEnumerator DistantPlayerCancelsWorkAndReturnsWithoutAnotherChop()
        {
            Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return null;
            var player=new GameObject("Leash player");player.transform.position=new Vector3(5000,5000);
            var worker=new GameObject("Leash worker");worker.transform.position=player.transform.position+Vector3.right;
            var companion=worker.AddComponent<Companion>();companion.enabled=false;companion.IsCombat=false;companion.Player=player.transform;
            var tree=new GameObject("Leash tree");tree.transform.position=worker.transform.position;
            var node=tree.AddComponent<Gatherable>();node.DurationSec=.8f;node.DestroyOnGather=false;
            var hp=node.EnsureTreeHealth();companion.AssignGather(node);
            var step=typeof(Companion).GetMethod("FixedUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
            for(int i=0;i<12;i++)step.Invoke(companion,null);
            Assert.That(companion.CurrentMode,Is.EqualTo(Companion.Mode.Working));
            Assert.That(hp.CurrentHp,Is.LessThan(hp.MaxHp));
            int remaining=hp.CurrentHp;
            player.transform.position+=Vector3.right*9;
            for(int i=0;i<20;i++)step.Invoke(companion,null);
            Assert.That(companion.Target,Is.Null);
            Assert.That(companion.CurrentMode,Is.EqualTo(Companion.Mode.Follow));
            Assert.That(hp.CurrentHp,Is.EqualTo(remaining),"Leaving must not finish another chop");
            Assert.That(worker.GetComponent<Rigidbody2D>().velocity.x,Is.GreaterThan(0));
            player.transform.position=worker.transform.position+Vector3.left;
            step.Invoke(companion,null);
            Assert.That(companion.CurrentMode,Is.EqualTo(Companion.Mode.Follow),"Returning must not silently resume the old tree");
            Object.Destroy(tree);Object.Destroy(worker);Object.Destroy(player);
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
