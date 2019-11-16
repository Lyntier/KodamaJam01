using System.Collections;
using Alice.Components;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Alice.Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] Animator transitionPanelAnim;

        string _levelExit;

        SceneDataContainer _sceneData;
        AudioManager _audioManager;
        
        void Awake()
        {
            SceneManager.sceneLoaded += SceneLoaded;
        }

        void Start()
        {
            // First loaded scene doesn't apply Cinemachine confines.
            // Do it again here.
            SetCinemachineConfines();

            _sceneData = FindObjectOfType<SceneDataContainer>();
            _audioManager = FindObjectOfType<AudioManager>();
        }



        public void LoadScene(string sceneName, string levelExit, string transitionName, bool shouldAudioFade = false)
        {
            StartCoroutine(Transition(sceneName, levelExit, transitionName, shouldAudioFade)); 
        }

        IEnumerator Transition(string sceneName, string levelExit, string transitionName, bool shouldAudioFade)
        {
            if (transitionName.Equals("")) transitionName = "Fade";
            float transitionTime = 1f;

            // Store reference to level exit for player positioning later
            _levelExit = levelExit;

            // Execute transition
            transitionPanelAnim.SetTrigger(transitionName + "Out");
            
            if(_sceneData.backgroundAudio && shouldAudioFade) _audioManager.FadeOut(transitionTime);

            // Wait for transition to finish
            yield return new WaitForSeconds(transitionTime);

            // Load new scene
            SceneManager.LoadScene(sceneName);
            
            transitionPanelAnim.SetTrigger(transitionName + "In");

            _sceneData = FindObjectOfType<SceneDataContainer>();
            if(_sceneData.backgroundAudio && shouldAudioFade) _audioManager.FadeIn(transitionTime);
        }

        void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_levelExit == null) return; // Probably entering from preload.
            
            Player player = FindObjectOfType<Player>();
            GameObject refExit = GameObject.Find(_levelExit);
            
            Vector2 spawn = refExit.GetComponent<LevelExit>().PlayerSpawn;
            
            // Make sure player doesn't spawn inside ground.
            spawn.y += player.GetComponent<Collider2D>().bounds.size.y / 2f;
            
            player.transform.position = spawn;
            
            
            SetCinemachineConfines();
        }

        void SetCinemachineConfines()
        {
            // Set camera bounds to the one in the level, if any.
            PolygonCollider2D cameraBounds = GameObject.FindWithTag("CameraBoundingBox").GetComponent<PolygonCollider2D>();
            CinemachineConfiner confiner = FindObjectOfType<CinemachineConfiner>();
            confiner.m_BoundingShape2D = cameraBounds;
            // Cinemachine doesn't automatically update performance path.
            // This makes sure the path is updated.
            confiner.InvalidatePathCache();
            
        }
    }
}