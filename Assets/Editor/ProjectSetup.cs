using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IL6.EditorBuild
{
    /// <summary>
    /// 메뉴: IL6 > Setup Project
    /// 한 번 실행으로 씬 생성 + 컴포넌트 배치 + Build Settings 등록 완료.
    /// </summary>
    public static class ProjectSetup
    {
        private const string BootScenePath       = "Assets/Scenes/BootScene.unity";
        private const string OnboardingScenePath = "Assets/Scenes/OnboardingScene.unity";
        private const string SnowfieldScenePath  = "Assets/Scenes/SnowfieldScene.unity";

        [MenuItem("IL6/Setup Project", priority = 0)]
        public static void Setup()
        {
            EnsureScenesDirectory();
            SetupBootScene();
            SetupOnboardingScene();
            RegisterBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ProjectSetup] 완료! Build Settings 씬 순서: Boot → Onboarding → Snowfield");
            EditorUtility.DisplayDialog("IL6 Setup", "프로젝트 설정 완료!\n\nBoot → Onboarding → Snowfield 순서로 등록됐습니다.\n\nIL6 > Build > Windows Standalone 으로 빌드하세요.", "확인");
        }

        // ── Boot Scene ──────────────────────────────────────────────────────────
        private static void SetupBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // GameSession
            var sessionGo = new GameObject("GameSession");
            sessionGo.AddComponent<GameSession>();

            // BootController
            var bootGo = new GameObject("BootController");
            bootGo.AddComponent<BootController>();

            EditorSceneManager.SaveScene(scene, BootScenePath);
            Debug.Log($"[ProjectSetup] BootScene 생성 → {BootScenePath}");
        }

        // ── Onboarding Scene ────────────────────────────────────────────────────
        private static void SetupOnboardingScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var go = new GameObject("OnboardingController");
            go.AddComponent<OnboardingController>();

            EditorSceneManager.SaveScene(scene, OnboardingScenePath);
            Debug.Log($"[ProjectSetup] OnboardingScene 생성 → {OnboardingScenePath}");
        }

        // ── Build Settings ──────────────────────────────────────────────────────
        private static void RegisterBuildSettings()
        {
            // SnowfieldScene 은 이미 존재 — 없으면 경고
            if (!File.Exists(SnowfieldScenePath))
                Debug.LogWarning($"[ProjectSetup] {SnowfieldScenePath} 없음 — SnowfieldScene 을 먼저 만들어야 합니다.");

            var scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath,       true),
                new EditorBuildSettingsScene(OnboardingScenePath, true),
                new EditorBuildSettingsScene(SnowfieldScenePath,  File.Exists(SnowfieldScenePath)),
            };

            EditorBuildSettings.scenes = scenes;
            Debug.Log("[ProjectSetup] Build Settings 등록 완료");
        }

        private static void EnsureScenesDirectory()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
        }
    }
}
