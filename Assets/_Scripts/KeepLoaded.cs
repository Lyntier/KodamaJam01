using UnityEngine;

namespace Alice
{
    public class KeepLoaded : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}