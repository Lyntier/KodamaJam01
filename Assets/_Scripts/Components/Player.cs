using System.Runtime.InteropServices;
using UnityEngine;

namespace Alice.Components
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Animator))]
    public class Player : MonoBehaviour
    {
        // I actually have to do this because the Unity devs are a bunch of 
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vkey);

        public const int LEFT_ARROW_KEY = 0x25;
        public const int RIGHT_ARROW_KEY = 0x27;
        
        public Vector3 Velocity => _velocity;

        
        public float maxSlopeAngle = 80;

        public CollisionInfo collisions;
        [HideInInspector] public Vector2 playerInput;

        [SerializeField] float attackDelay = 0.5f;
        float _currentAttackDelay;

        Animator _animator;
        
        static readonly int AttackTrigger = Animator.StringToHash("Attack");

        void Start()
        {
            _gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
            _maxJumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
            _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * minJumpHeight);

            _animator = GetComponent<Animator>();
            
            CalculateRaySpacing();
            collisions.FaceDir = 1;
        }

        void Update()
        {
            CalculateVelocity();


            // Input axes are not saved between scene switches.
            // To circumvent this, we have to use Input.GetKeyDown directly.
            
            // _directionalInput.x = Input.GetAxisRaw("Horizontal");
            _directionalInput.x = 0;
            if ((GetAsyncKeyState(LEFT_ARROW_KEY) & 0x8000) > 0)
            {
                _directionalInput.x -= 1;
            }

            if ((GetAsyncKeyState(RIGHT_ARROW_KEY) & 0x8000) > 0)
            {
                _directionalInput.x += 1;
            }
            
            
            if (Input.GetKeyDown(KeyCode.Space)) OnJumpInputDown();
            if (Input.GetKeyUp(KeyCode.Space)) OnJumpInputUp();
            PerformAttack();

            Move(_velocity * Time.deltaTime, _directionalInput);

            if (collisions.Above || collisions.Below)
            {
                if (collisions.SlidingDownMaxSlope)
                {
                    _velocity.y += collisions.SlopeNormal.y * -_gravity * Time.deltaTime;
                }
                else
                {
                    _velocity.y = 0;
                }
            }
        }

        void PerformAttack()
        {
            if (_currentAttackDelay > Time.deltaTime)
            {
                _currentAttackDelay -= Time.deltaTime;
                
            } else if (Input.GetKeyDown(KeyCode.E))
            {
                _currentAttackDelay = attackDelay;
                _animator.SetTrigger(AttackTrigger);
            }
        }

        public void Move(Vector2 moveAmount, bool standingOnPlatform)
        {
            Move(moveAmount, Vector2.zero, standingOnPlatform);
        }

        public void Move(Vector2 moveAmount, Vector2 input, bool standingOnPlatform = false)
        {
            UpdateRaycastOrigins();

            collisions.Reset();
            collisions.MoveAmountOld = moveAmount;
            playerInput = input;

            if (moveAmount.y < 0)
            {
                DescendSlope(ref moveAmount);
            }

            if (Mathf.Abs(moveAmount.x) > 0.01f)
            {
                collisions.FaceDir = (int) Mathf.Sign(moveAmount.x);
                _animator.SetBool(Mov, true);
            }
            else
            {
                _animator.SetBool(Mov, false);
            }
            

            HorizontalCollisions(ref moveAmount);
            if (moveAmount.y != 0)
            {
                VerticalCollisions(ref moveAmount);
            }

            transform.Translate(moveAmount, Space.World);

            if (standingOnPlatform)
            {
                collisions.Below = true;
            }
        }

        void HorizontalCollisions(ref Vector2 moveAmount)
        {
            float directionX = collisions.FaceDir;
            if(directionX < 0) transform.rotation = Quaternion.Euler(0, 180, 0);
            if (directionX > 0) transform.rotation = Quaternion.identity;
            float rayLength = Mathf.Abs(moveAmount.x) + SkinWidth;

            if (Mathf.Abs(moveAmount.x) < SkinWidth)
            {
                rayLength = 2 * SkinWidth;
            }

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? rayCastOrigins.BottomLeft : rayCastOrigins.BottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

                if (hit)
                {
                    if (hit.distance == 0)
                    {
                        continue;
                    }

                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                    if (i == 0 && slopeAngle <= maxSlopeAngle)
                    {
                        if (collisions.DescendingSlope)
                        {
                            collisions.DescendingSlope = false;
                            moveAmount = collisions.MoveAmountOld;
                        }

                        float distanceToSlopeStart = 0;
                        if (slopeAngle != collisions.SlopeAngleOld)
                        {
                            distanceToSlopeStart = hit.distance - SkinWidth;
                            moveAmount.x -= distanceToSlopeStart * directionX;
                        }

                        ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
                        moveAmount.x += distanceToSlopeStart * directionX;
                    }

                    if (!collisions.ClimbingSlope || slopeAngle > maxSlopeAngle)
                    {
                        moveAmount.x = (hit.distance - SkinWidth) * directionX;
                        rayLength = hit.distance;

                        if (collisions.ClimbingSlope)
                        {
                            moveAmount.y = Mathf.Tan(collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
                        }

                        collisions.Left = directionX == -1;
                        collisions.Right = directionX == 1;
                    }
                }
            }
        }

        void VerticalCollisions(ref Vector2 moveAmount)
        {
            float directionY = Mathf.Sign(moveAmount.y);
            float rayLength = Mathf.Abs(moveAmount.y) + SkinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? rayCastOrigins.BottomLeft : rayCastOrigins.TopLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

                if (hit)
                {
                    moveAmount.y = (hit.distance - SkinWidth) * directionY;
                    rayLength = hit.distance;

                    if (collisions.ClimbingSlope)
                    {
                        moveAmount.x = moveAmount.y / Mathf.Tan(collisions.SlopeAngle * Mathf.Deg2Rad) *
                                       Mathf.Sign(moveAmount.x);
                    }

                    collisions.Below = directionY == -1;
                    collisions.Above = directionY == 1;
                }
            }

            if (collisions.ClimbingSlope)
            {
                float directionX = Mathf.Sign(moveAmount.x);
                rayLength = Mathf.Abs(moveAmount.x) + SkinWidth;
                Vector2 rayOrigin = (directionX == -1 ? rayCastOrigins.BottomLeft : rayCastOrigins.BottomRight) +
                                    Vector2.up * moveAmount.y;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (!Mathf.Approximately(slopeAngle, collisions.SlopeAngle))
                    {
                        moveAmount.x = (hit.distance - SkinWidth) * directionX;
                        collisions.SlopeAngle = slopeAngle;
                        collisions.SlopeNormal = hit.normal;
                    }
                }
            }
        }

        void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal)
        {
            float moveDistance = Mathf.Abs(moveAmount.x);
            float climbMoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

            if (moveAmount.y <= climbMoveAmountY)
            {
                moveAmount.y = climbMoveAmountY;
                moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                collisions.Below = true;
                collisions.ClimbingSlope = true;
                collisions.SlopeAngle = slopeAngle;
                collisions.SlopeNormal = slopeNormal;
            }
        }

        void DescendSlope(ref Vector2 moveAmount)
        {
            RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(rayCastOrigins.BottomLeft, Vector2.down,
                Mathf.Abs(moveAmount.y) + SkinWidth, collisionMask);
            RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(rayCastOrigins.BottomRight, Vector2.down,
                Mathf.Abs(moveAmount.y) + SkinWidth, collisionMask);
            if (maxSlopeHitLeft ^ maxSlopeHitRight)
            {
                SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
                SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
            }

            if (!collisions.SlidingDownMaxSlope)
            {
                float directionX = Mathf.Sign(moveAmount.x);
                Vector2 rayOrigin = (directionX == -1) ? rayCastOrigins.BottomRight : rayCastOrigins.BottomLeft;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                    {
                        if (Mathf.Sign(hit.normal.x) == directionX)
                        {
                            if (hit.distance - SkinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                            {
                                float moveDistance = Mathf.Abs(moveAmount.x);
                                float descendMoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                                moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance *
                                               Mathf.Sign(moveAmount.x);
                                moveAmount.y -= descendMoveAmountY;

                                collisions.SlopeAngle = slopeAngle;
                                collisions.DescendingSlope = true;
                                collisions.Below = true;
                                collisions.SlopeNormal = hit.normal;
                            }
                        }
                    }
                }
            }
        }

        void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount)
        {
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle > maxSlopeAngle)
                {
                    moveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(moveAmount.y) - hit.distance) /
                                   Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                    collisions.SlopeAngle = slopeAngle;
                    collisions.SlidingDownMaxSlope = true;
                    collisions.SlopeNormal = hit.normal;
                }
            }
        }

        void ResetFallingThroughPlatform()
        {
            collisions.FallingThroughPlatform = false;
        }

        public struct CollisionInfo
        {
            public bool Above, Below;
            public bool Left, Right;

            public bool ClimbingSlope;
            public bool DescendingSlope;
            public bool SlidingDownMaxSlope;

            public float SlopeAngle, SlopeAngleOld;
            public Vector2 SlopeNormal;
            public Vector2 MoveAmountOld;
            public int FaceDir;
            public bool FallingThroughPlatform;

            public void Reset()
            {
                Above = Below = false;
                Left = Right = false;
                ClimbingSlope = false;
                DescendingSlope = false;
                SlidingDownMaxSlope = false;
                SlopeNormal = Vector2.zero;

                SlopeAngleOld = SlopeAngle;
                SlopeAngle = 0;
            }
        }


        public LayerMask collisionMask;

        const float SkinWidth = .015f;
        const float DstBetweenRays = .25f;
        [HideInInspector] public int horizontalRayCount;
        [HideInInspector] public int verticalRayCount;

        [HideInInspector] public float horizontalRaySpacing;
        [HideInInspector] public float verticalRaySpacing;

        BoxCollider2D _collider;
        public RaycastOrigins rayCastOrigins;

        public void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
        }

        public void UpdateRaycastOrigins()
        {
            Bounds bounds = _collider.bounds;
            bounds.Expand(SkinWidth * -2);

            rayCastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            rayCastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
            rayCastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
            rayCastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        public void CalculateRaySpacing()
        {
            Bounds bounds = _collider.bounds;
            bounds.Expand(SkinWidth * -2);

            float boundsWidth = bounds.size.x;
            float boundsHeight = bounds.size.y;

            horizontalRayCount = Mathf.RoundToInt(boundsHeight / DstBetweenRays);
            verticalRayCount = Mathf.RoundToInt(boundsWidth / DstBetweenRays);

            horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        }

        public struct RaycastOrigins
        {
            public Vector2 TopLeft, TopRight;
            public Vector2 BottomLeft, BottomRight;
        }


        public float maxJumpHeight = 4;
        public float minJumpHeight = 1;
        public float timeToJumpApex = .4f;
        float _accelerationTimeAirborne = .2f;
        float _accelerationTimeGrounded = .1f;
        [SerializeField] float moveSpeed = 6;


        float _gravity;
        float _maxJumpVelocity;
        float _minJumpVelocity;
        Vector3 _velocity;
        float _velocityXSmoothing;

        Vector2 _directionalInput;
        bool _wallSliding;
        int _wallDirX;
        static readonly int Mov = Animator.StringToHash("Mov");


        public void OnJumpInputDown()
        {
            if (collisions.Below)
            {
                if (collisions.SlidingDownMaxSlope)
                {
                    if (_directionalInput.x != -Mathf.Sign(collisions.SlopeNormal.x))
                    {
                        // not jumping against max slope
                        _velocity.y = _maxJumpVelocity * collisions.SlopeNormal.y;
                        _velocity.x = _maxJumpVelocity * collisions.SlopeNormal.x;
                    }
                }
                else
                {
                    _velocity.y = _maxJumpVelocity;
                }
            }
        }

        public void OnJumpInputUp()
        {
            if (_velocity.y > _minJumpVelocity)
            {
                _velocity.y = _minJumpVelocity;
            }
        }

        void CalculateVelocity()
        {
            float targetVelocityX = _directionalInput.x * moveSpeed;
            _velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _velocityXSmoothing,
                (collisions.Below) ? _accelerationTimeGrounded : _accelerationTimeAirborne);
            _velocity.y += _gravity * Time.deltaTime;
        }
    }
}