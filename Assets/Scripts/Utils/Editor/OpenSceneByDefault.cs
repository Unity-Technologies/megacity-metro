using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Unity.MegacityMetro.EditorTools
{
    [InitializeOnLoad]
    public class OpenSceneByDefault
    {
        private const string LastOpenedKey = "LastOpened";
        public static float LastOpened { get; private set; }
        static OpenSceneByDefault()
        {
            LastOpened = EditorPrefs.GetFloat(LastOpenedKey, float.MaxValue);
            EditorApplication.delayCall += OnEditorLoaded;
        }

        private static void OnEditorLoaded()
        {
            var openedTime = LastOpened;
            var timeSinceStartup = (float) EditorApplication.timeSinceStartup;
        
            if (timeSinceStartup < openedTime)
            {
                LastOpened = timeSinceStartup;
                var defaultScene = EditorBuildSettings.scenes.FirstOrDefault(x => x.path.Equals("Assets/Scenes/Main.unity")) ?? EditorBuildSettings.scenes.FirstOrDefault();
                if (defaultScene != null && !string.IsNullOrWhiteSpace(defaultScene.path))
                {
                    EditorSceneManager.OpenScene(defaultScene.path);
                    Debug.Log($"[OpenSceneByDefault] Opened {defaultScene.path}!");
                }
                else Debug.Log($"[OpenSceneByDefault] Cannot open default scene, as none exist!");
            }

            EditorPrefs.SetFloat(LastOpenedKey, LastOpened);
            EditorApplication.delayCall -= OnEditorLoaded;
        }
    }
}