using UnityEngine;
using UnityEngine.SceneManagement;

namespace IL6
{
    public sealed class OnboardingController : MonoBehaviour
    {
        public string SnowfieldSceneName = "SnowfieldScene";
        private Texture2D titleLogo;

        private void Awake()
        {
            titleLogo = Resources.Load<Texture2D>("Brand/title-logo");
        }

        private void OnGUI()
        {
            if (titleLogo == null) return;
            const float width = 560f;
            float height = width * titleLogo.height / titleLogo.width;
            GUI.DrawTexture(new Rect((Screen.width - width) * 0.5f, 30f, width, height), titleLogo, ScaleMode.ScaleToFit, true);
        }

        private void Update()
        {
            if (Input.anyKeyDown || Input.touchCount > 0)
                SceneManager.LoadScene(SnowfieldSceneName);
        }
    }
}
