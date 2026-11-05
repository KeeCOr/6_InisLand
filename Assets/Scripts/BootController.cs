using UnityEngine;
using UnityEngine.SceneManagement;

namespace IL6
{
    /// <summary>
    /// Boot 신: GameSession 초기화 후 OnboardingScene 으로 전환.
    /// 빈 씬에 GameSession 컴포넌트 + 이 BootController 컴포넌트 하나만 두면 됨.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class BootController : MonoBehaviour
    {
        public string OnboardingSceneName = "OnboardingScene";
        public static bool SkipLobbyOnce;
        public static bool FreshNpcTutorialOnce;

        private void Start()
        {
            if (GameSession.Instance == null)
            {
                var go = new GameObject("GameSession");
                go.AddComponent<GameSession>();
            }
            if(SkipLobbyOnce)
            {
                SkipLobbyOnce=false;
                GameSession.Instance.NpcTutorialEnabled=FreshNpcTutorialOnce;FreshNpcTutorialOnce=false;
                SceneManager.LoadScene(OnboardingSceneName);
                return;
            }
            GameSession.Instance.enabled=false;
            Time.timeScale=1;InputReader.HudBlocked=true;
            var lobby=gameObject.GetComponent<LobbyController>()??gameObject.AddComponent<LobbyController>();
            lobby.GameSceneName=OnboardingSceneName;
        }
    }
}
