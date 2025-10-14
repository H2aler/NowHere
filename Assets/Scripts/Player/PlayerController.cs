using UnityEngine;
using Unity.Netcode;
using NowHere.AR;
using NowHere.Networking;

namespace NowHere.Player
{
    /// <summary>
    /// 플레이어 캐릭터를 제어하는 메인 컨트롤러
    /// AR 환경에서의 이동, 상호작용, 네트워크 동기화를 담당
    /// </summary>
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private LayerMask groundLayer = 1;
        
        [Header("AR Integration")]
        [SerializeField] private bool useARMovement = true;
        [SerializeField] private float arMovementSensitivity = 2f;
        
        [Header("Components")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private GameObject playerModel;
        
        [Header("Player Stats")]
        [SerializeField] private int playerLevel = 1;
        [SerializeField] private int experience = 0;
        [SerializeField] private int health = 100;
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int mana = 100;
        [SerializeField] private int maxMana = 100;
        
        // 입력 변수
        private Vector2 inputVector;
        private bool isJumping;
        private bool isGrounded;
        
        // AR 관련 변수
        private ARManager arManager;
        private Vector3 arMovementOffset;
        
        // 네트워크 변수
        private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
        private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();
        private NetworkVariable<int> networkHealth = new NetworkVariable<int>();
        private NetworkVariable<int> networkLevel = new NetworkVariable<int>();
        
        // 이벤트
        public System.Action<int> OnHealthChanged;
        public System.Action<int> OnLevelChanged;
        public System.Action<int> OnExperienceChanged;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // 네트워크 변수 초기화
            if (IsOwner)
            {
                networkHealth.Value = health;
                networkLevel.Value = playerLevel;
            }
            
            // AR 매니저 참조
            arManager = FindObjectOfType<ARManager>();
            
            // 네트워크 변수 변경 이벤트 구독
            networkHealth.OnValueChanged += OnHealthNetworkChanged;
            networkLevel.OnValueChanged += OnLevelNetworkChanged;
        }
        
        private void Start()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();
            
            if (animator == null)
                animator = GetComponent<Animator>();
            
            if (cameraTransform == null)
                cameraTransform = Camera.main.transform;
        }
        
        private void Update()
        {
            if (!IsOwner) return;
            
            HandleInput();
            CheckGrounded();
            UpdateARMovement();
        }
        
        private void FixedUpdate()
        {
            if (!IsOwner) return;
            
            HandleMovement();
            HandleRotation();
        }
        
        private void HandleInput()
        {
            // 키보드/터치 입력 처리
            inputVector.x = Input.GetAxis("Horizontal");
            inputVector.y = Input.GetAxis("Vertical");
            
            // 점프 입력
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                isJumping = true;
            }
            
            // AR 모드에서의 터치 입력
            if (useARMovement && arManager != null && arManager.IsARReady())
            {
                HandleARTouchInput();
            }
        }
        
        private void HandleARTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Moved)
                {
                    // 터치 이동에 따른 AR 이동
                    Vector2 touchDelta = touch.deltaPosition;
                    arMovementOffset += new Vector3(touchDelta.x, 0, touchDelta.y) * arMovementSensitivity * Time.deltaTime;
                }
            }
        }
        
        private void UpdateARMovement()
        {
            if (useARMovement && arManager != null && arManager.IsARReady())
            {
                // AR 환경에서의 위치 업데이트
                Vector3 arPosition = arManager.GetDetectedPlanePosition() + arMovementOffset;
                transform.position = Vector3.Lerp(transform.position, arPosition, Time.deltaTime * 5f);
            }
        }
        
        private void HandleMovement()
        {
            if (inputVector.magnitude > 0.1f)
            {
                // 카메라 방향 기준으로 이동
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;
                
                forward.y = 0;
                right.y = 0;
                
                forward.Normalize();
                right.Normalize();
                
                Vector3 moveDirection = forward * inputVector.y + right * inputVector.x;
                moveDirection.Normalize();
                
                // 이동 적용
                Vector3 targetVelocity = moveDirection * moveSpeed;
                targetVelocity.y = rb.linearVelocity.y;
                rb.linearVelocity = targetVelocity;
                
                // 애니메이션 업데이트
                if (animator != null)
                {
                    animator.SetFloat("Speed", inputVector.magnitude);
                }
            }
            else
            {
                // 정지
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                
                if (animator != null)
                {
                    animator.SetFloat("Speed", 0);
                }
            }
            
            // 점프 처리
            if (isJumping && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isJumping = false;
                
                if (animator != null)
                {
                    animator.SetTrigger("Jump");
                }
            }
        }
        
        private void HandleRotation()
        {
            if (inputVector.magnitude > 0.1f)
            {
                // 이동 방향으로 회전
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;
                
                forward.y = 0;
                right.y = 0;
                
                forward.Normalize();
                right.Normalize();
                
                Vector3 moveDirection = forward * inputVector.y + right * inputVector.x;
                moveDirection.Normalize();
                
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        
        private void CheckGrounded()
        {
            // 지면 감지
            RaycastHit hit;
            isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f, groundLayer);
            
            if (animator != null)
            {
                animator.SetBool("IsGrounded", isGrounded);
            }
        }
        
        public void TakeDamage(int damage)
        {
            if (!IsOwner) return;
            
            health = Mathf.Max(0, health - damage);
            networkHealth.Value = health;
            
            OnHealthChanged?.Invoke(health);
            
            if (health <= 0)
            {
                Die();
            }
        }
        
        public void Heal(int healAmount)
        {
            if (!IsOwner) return;
            
            health = Mathf.Min(maxHealth, health + healAmount);
            networkHealth.Value = health;
            
            OnHealthChanged?.Invoke(health);
        }
        
        public void AddExperience(int exp)
        {
            if (!IsOwner) return;
            
            experience += exp;
            OnExperienceChanged?.Invoke(experience);
            
            // 레벨업 체크
            int requiredExp = playerLevel * 100;
            if (experience >= requiredExp)
            {
                LevelUp();
            }
        }
        
        private void LevelUp()
        {
            playerLevel++;
            experience = 0;
            networkLevel.Value = playerLevel;
            
            // 레벨업 보상
            maxHealth += 10;
            maxMana += 10;
            health = maxHealth;
            mana = maxMana;
            
            OnLevelChanged?.Invoke(playerLevel);
            Debug.Log($"레벨업! 현재 레벨: {playerLevel}");
        }
        
        private void Die()
        {
            Debug.Log("플레이어가 사망했습니다.");
            // 사망 처리 로직
        }
        
        private void OnHealthNetworkChanged(int previousValue, int newValue)
        {
            health = newValue;
            OnHealthChanged?.Invoke(health);
        }
        
        private void OnLevelNetworkChanged(int previousValue, int newValue)
        {
            playerLevel = newValue;
            OnLevelChanged?.Invoke(playerLevel);
        }
        
        public int GetHealth()
        {
            return health;
        }
        
        public int GetMaxHealth()
        {
            return maxHealth;
        }
        
        public int GetLevel()
        {
            return playerLevel;
        }
        
        public int GetExperience()
        {
            return experience;
        }
        
        public void SetARMovement(bool enable)
        {
            useARMovement = enable;
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            // 이벤트 구독 해제
            networkHealth.OnValueChanged -= OnHealthNetworkChanged;
            networkLevel.OnValueChanged -= OnLevelNetworkChanged;
        }
    }
}
