using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class BatchAudioTests
    {
        [UnityTest] public IEnumerator BatchStartupAndSceneChangesStaySilent()
        {
            if(!Application.isBatchMode)Assert.Ignore("Batch-only audio gate");
            Assert.That(AudioListener.volume,Is.Zero);
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            Assert.That(AudioListener.volume,Is.Zero);
        }
    }
}
