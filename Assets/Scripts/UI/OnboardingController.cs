using UnityEngine;
using UnityEngine.SceneManagement;
namespace IL6
{
    // The playable story now lives in SnowfieldScene; avoid a second slide tutorial.
    public sealed class OnboardingController : MonoBehaviour
    {
        public string SnowfieldSceneName="SnowfieldScene";
        private System.Collections.IEnumerator Start()
        {
            var session=GameSession.Instance;
            if(session!=null && session.NpcTutorialEnabled && !session.StoryIntroSeen)
                yield return gameObject.AddComponent<NewAdventureLoading>().LoadScene(SnowfieldSceneName);
            else SceneManager.LoadScene(SnowfieldSceneName);
        }
    }
}
