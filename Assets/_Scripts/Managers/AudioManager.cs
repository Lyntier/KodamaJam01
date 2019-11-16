using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Alice.Managers
{
    public class AudioManager : MonoBehaviour
    {
        AudioSource BGM;
        float startVolume;

        SceneDataContainer _sceneData;

        void Start()
        {
            BGM = GetComponent<AudioSource>();
            startVolume = BGM.volume;

            SceneManager.sceneLoaded += (scene, mode) =>
                _sceneData = FindObjectOfType<SceneDataContainer>();

            // Kick it to get going.
            FadeIn(1f);
        }

        public void FadeOut(float fadeTime) => StartCoroutine(FadeOutRoutine(fadeTime));

        IEnumerator FadeOutRoutine(float fadeTime)
        {
            while (BGM.volume > 0)
            {
                BGM.volume -= startVolume * Time.deltaTime / fadeTime;
                yield return null;
            }

            BGM.Stop();
        }

        public void FadeIn(float fadeTime)
        {
            StartCoroutine(FadeInRoutine(fadeTime));
        }

        IEnumerator FadeInRoutine(float fadeTime)
        {
            if (!_sceneData)
                _sceneData = FindObjectOfType<SceneDataContainer>();
            BGM.clip = _sceneData.backgroundAudio;
            BGM.Play();
            while (BGM.volume < startVolume)
            {
                BGM.volume += startVolume * Time.deltaTime / fadeTime;
                yield return null;
            }
        }
    }
}