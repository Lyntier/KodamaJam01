using UnityEngine;

namespace Alice.Ext
{
    public class ScenePreloader : ScriptableObject
    {

        [RuntimeInitializeOnLoadMethod]
        static void EnsurePreloaderExists()
        {
            GameObject rootPreloader = Resources.Load("RootPreloader") as GameObject;
            
            if (!GameObject.Find("RootPreloader"))
            {
                Object o = Instantiate(rootPreloader);
                DontDestroyOnLoad(o);
            }
        }

    }
}