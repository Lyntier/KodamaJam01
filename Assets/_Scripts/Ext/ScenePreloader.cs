// Used to load a preload scene before returning to the current scene.


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;


namespace Alice.Ext
{
    [InitializeOnLoad]
    public static class ScenePreloader
    {
        public static string preloadScene = "scn_preload";
        public static string nextScene = SceneManager.GetActiveScene().name;
        
        static ScenePreloader()
        {
            EditorApplication.playModeStateChanged += LoadScene;
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (scene.name == preloadScene) SceneManager.UnloadSceneAsync(preloadScene);
            };
        }

        static void LoadScene(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                Debug.Log("Preloading");
                SceneManager.LoadScene(preloadScene, LoadSceneMode.Additive);
            }
        }
    }
}
#endif
