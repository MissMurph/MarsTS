using System;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarsTS.Editor
{
    [InitializeOnLoad]
    public class QuickSceneLauncher
    {
        private Scene _playingScene;

        private const string LaunchScenePath = "Assets/Scenes/LaunchScene.unity";
        private const string ActiveEditorScene = "PreviousScenePath";
        private const string IsEditorInitialization = "EditorInitialization";
        private const string TestSceneName = "NetworkTest";
        
        static QuickSceneLauncher()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                {
                    string sceneName = SceneManager.GetActiveScene().name;
                    EditorPrefs.SetString(ActiveEditorScene, sceneName);

                    SceneAsset sceneToLoad = AssetDatabase.LoadAssetAtPath<SceneAsset>(LaunchScenePath);
                    EditorPrefs.SetBool(IsEditorInitialization, true);
                    EditorSceneManager.playModeStartScene = sceneToLoad;
                    break;
                }
                case PlayModeStateChange.EnteredPlayMode 
                when EditorPrefs.GetBool(IsEditorInitialization):
                {
                    NetworkManager.Singleton.OnServerStarted += OnServerStarted;
                    NetworkManager.Singleton.StartHost();

                    break;
                }
                case PlayModeStateChange.EnteredEditMode:
                {
                    EditorPrefs.SetBool(IsEditorInitialization, false);
                    break;
                }
            }
        }

        private static void OnServerStarted()
        {
            string prevScene = EditorPrefs.GetString(ActiveEditorScene);

            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneLoaded;
            NetworkManager.Singleton.SceneManager.LoadScene(prevScene, LoadSceneMode.Additive);
        }

        private static void OnSceneLoaded(SceneEvent evnt)
        {
            if (evnt.Scene.name == TestSceneName) 
                SceneManager.SetActiveScene(evnt.Scene);
        }
    }
}