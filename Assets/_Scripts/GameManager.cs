using System.Collections;
using Alice.Components;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Alice
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] Animator transitionPanelAnim;
        static readonly int FadeOutTrigger = Animator.StringToHash("FadeOut");
        static readonly int FadeInTrigger = Animator.StringToHash("FadeIn");


        string _levelExit;

        void Awake()
        {
            SceneManager.sceneLoaded += SceneLoaded;
        }

        void Start()
        {
            // First loaded scene doesn't apply Cinemachine confines.
            // Do it again here.
            
            SetCinemachineConfines();
            
        }

        public void LoadScene(string sceneName, string levelExit)
        {
            StartCoroutine(Transition(sceneName, levelExit));
        }

        IEnumerator Transition(string sceneName, string levelExit)
        {
            
            // Store reference to level exit for player positioning later
            _levelExit = levelExit;

            // Execute transition
            transitionPanelAnim.SetTrigger(FadeOutTrigger);

            // Wait for transition to finish
            yield return new WaitForSeconds(1);

            // Load new scene
            SceneManager.LoadScene(sceneName);
            
            transitionPanelAnim.SetTrigger(FadeInTrigger);
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