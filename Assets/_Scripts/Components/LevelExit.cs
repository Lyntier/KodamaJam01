using System;
using Alice.Components;
using Alice.Ext;
using Alice.Managers;
using UnityEngine;
using UnityEngine.Assertions;

namespace Alice
{
    [RequireComponent(typeof(Collider2D))]
    public class LevelExit : MonoBehaviour
    {
        // Require a reference to the game manager
        // to let it know when to transition scenes.
        GameManager _gameManager;

        // Needed to check whether player is moving
        // in to or out of the level.
        [SerializeField] Vector2Int side = Vector2Int.zero;
        [SerializeField] SceneField referencingScene = null;
        [SerializeField] string towardsLevelExit = "";
        [SerializeField] bool shouldAudioFade = false;
        [SerializeField] string transitionName = "Fade";
        [SerializeField] float transitionTime = 1f;
        bool _isGameManagerNull = true;

        // Position at which player is spawned when
        // entering through this side of the level.
        public Vector2 PlayerSpawn { get; private set; }

        void Awake()
        {
            Bounds bounds = GetComponent<Collider2D>().bounds;

            // Bottom center as player spawn for every level exit.
            PlayerSpawn = new Vector2(bounds.center.x, bounds.min.y);
        }

        void Update()
        {
            if (_isGameManagerNull)
            {
                _gameManager = FindObjectOfType<GameManager>();
                if (_gameManager)
                {
                    _isGameManagerNull = false;
                }
                
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return; // Only consider players.
            Player player = other.GetComponent<Player>();
            Vector2Int playerDir = new Vector2Int(Math.Sign(player.Velocity.x), Math.Sign(player.Velocity.y));

            // Horizontal exits
            if (side.y == 0)
            {
                if (playerDir.x == side.x)
                    _gameManager.LoadScene(referencingScene, towardsLevelExit, transitionName, transitionTime, shouldAudioFade);
            }
        }
    }
}