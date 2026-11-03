using UnityEngine;
using UnityEngine.SceneManagement;
namespace IL6
{
    // The playable story now lives in SnowfieldScene; avoid a second slide tutorial.
    public sealed class OnboardingController : MonoBehaviour
    {
        public string SnowfieldSceneName="SnowfieldScene";
        private void Start() { SceneManager.LoadScene(SnowfieldSceneName); }
    }
}
