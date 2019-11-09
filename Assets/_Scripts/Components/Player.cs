using UnityEngine;

namespace Components
{
    [RequireComponent(typeof(Collider2D))]
    public class Player : MonoBehaviour
    {
        struct CollisionInfo
        {
            public bool Top, Bottom;
            public bool Left, Right;
        }
        CollisionInfo _collisionInfo;
        
        [SerializeField] LayerMask collisionMask;
        [SerializeField] float skinWidth;
        float _rayDistance;
        int _horizontalRayCount, _verticalRayCount;
        
        [SerializeField] float jumpHeight;
        [SerializeField] float timeToApex;
        float _jumpForce;
        float _gravity;
        
        Collider2D _collider;
        
        Vector2 _velocity;
        bool _jump;
        bool _attack;
        
        
        void Start()
        {
            _collider = GetComponent<Collider2D>();

            _jumpForce = 2 * jumpHeight / timeToApex;
            _gravity = _jumpForce / timeToApex;
        }

        void Update()
        {
            _velocity.x = Input.GetAxisRaw("Horizontal");
            _jump = Input.GetKeyDown(KeyCode.Z);
            _attack = Input.GetKeyDown(KeyCode.X);

            CheckHorizontalCollisions();
            CheckVerticalCollisions();
        }

        void CheckHorizontalCollisions()
        {
            
        }

        void CheckVerticalCollisions()
        {
            
        }
    }
}
