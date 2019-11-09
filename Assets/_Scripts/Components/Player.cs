using UnityEngine;

namespace Components
{
    [RequireComponent(typeof(Collider2D))]
    public class Player : MonoBehaviour
    {


        Collider2D _collider;
        
        Vector2 _velocity;
        bool _jump;
        bool _attack;

        void Start()
        {
            _collider = GetComponent<Collider2D>();
        }

        void Update()
        {
            _velocity.x = Input.GetAxisRaw("Horizontal");
            _jump = Input.GetKeyDown(KeyCode.Z);
            _attack = Input.GetKeyDown(KeyCode.X);
        }
    }
}
