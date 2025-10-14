using UnityEngine;
using UnityEngine.AI;
using NowHere.RPG;
using NowHere.Player;

namespace NowHere.AI
{
    /// <summary>
    /// 적 AI를 관리하는 컨트롤러
    /// 적의 행동, 전투, AI 로직을 담당
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy Stats")]
        [SerializeField] private EnemyStats enemyStats;
        [SerializeField] private int currentHealth;
        [SerializeField] private int currentMana;
        
        [Header("AI Settings")]
        [SerializeField] private AIState currentState = AIState.Idle;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 5f;
        
        [Header("Combat Settings")]
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private LayerMask playerLayer = 1;
        
        [Header("Components")]
        [SerializeField] private NavMeshAgent navAgent;
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Collider enemyCollider;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject deathEffect;
        [SerializeField] private GameObject hitEffect;
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioClip hitSound;
        
        // AI 상태
        private Transform target;
        private float lastAttackTime;
        private float stateChangeTime;
        private Vector3 lastKnownPlayerPosition;
        private bool isDead = false;
        
        // 컴포넌트 참조
        private AudioSource audioSource;
        private CharacterSystem characterSystem;
        
        // 이벤트
        public System.Action<EnemyController> OnEnemyDeath;
        public System.Action<EnemyController, int> OnEnemyDamaged;
        public System.Action<EnemyController, PlayerController> OnEnemyAttack;
        
        private void Start()
        {
            InitializeEnemy();
        }
        
        private void Update()
        {
            if (isDead) return;
            
            UpdateAI();
            UpdateAnimation();
        }
        
        private void InitializeEnemy()
        {
            // 컴포넌트 참조 설정
            if (navAgent == null)
                navAgent = GetComponent<NavMeshAgent>();
            
            if (animator == null)
                animator = GetComponent<Animator>();
            
            if (rb == null)
                rb = GetComponent<Rigidbody>();
            
            if (enemyCollider == null)
                enemyCollider = GetComponent<Collider>();
            
            // 오디오 소스 설정
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // 적 스탯 초기화
            if (enemyStats != null)
            {
                currentHealth = enemyStats.maxHealth;
                currentMana = enemyStats.maxMana;
            }
            
            // 네비게이션 에이전트 설정
            if (navAgent != null)
            {
                navAgent.speed = moveSpeed;
                navAgent.angularSpeed = rotationSpeed * 10f;
                navAgent.stoppingDistance = attackRange;
            }
            
            // 초기 상태 설정
            SetAIState(AIState.Idle);
        }
        
        private void UpdateAI()
        {
            switch (currentState)
            {
                case AIState.Idle:
                    UpdateIdleState();
                    break;
                case AIState.Patrol:
                    UpdatePatrolState();
                    break;
                case AIState.Chase:
                    UpdateChaseState();
                    break;
                case AIState.Attack:
                    UpdateAttackState();
                    break;
                case AIState.Search:
                    UpdateSearchState();
                    break;
                case AIState.Return:
                    UpdateReturnState();
                    break;
            }
        }
        
        private void UpdateIdleState()
        {
            // 플레이어 감지
            if (DetectPlayer())
            {
                SetAIState(AIState.Chase);
                return;
            }
            
            // 일정 시간 후 패트롤 상태로 전환
            if (Time.time - stateChangeTime > 3f)
            {
                SetAIState(AIState.Patrol);
            }
        }
        
        private void UpdatePatrolState()
        {
            // 플레이어 감지
            if (DetectPlayer())
            {
                SetAIState(AIState.Chase);
                return;
            }
            
            // 패트롤 로직 (간단한 랜덤 이동)
            if (navAgent != null && !navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                Vector3 randomDirection = Random.insideUnitSphere * 10f;
                randomDirection += transform.position;
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, 10f, 1))
                {
                    navAgent.SetDestination(hit.position);
                }
            }
        }
        
        private void UpdateChaseState()
        {
            if (target == null)
            {
                SetAIState(AIState.Search);
                return;
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            // 공격 범위 내에 있으면 공격 상태로 전환
            if (distanceToTarget <= attackRange)
            {
                SetAIState(AIState.Attack);
                return;
            }
            
            // 감지 범위를 벗어나면 탐색 상태로 전환
            if (distanceToTarget > detectionRange * 1.5f)
            {
                SetAIState(AIState.Search);
                return;
            }
            
            // 타겟 추적
            if (navAgent != null)
            {
                navAgent.SetDestination(target.position);
                lastKnownPlayerPosition = target.position;
            }
        }
        
        private void UpdateAttackState()
        {
            if (target == null)
            {
                SetAIState(AIState.Search);
                return;
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            // 공격 범위를 벗어나면 추적 상태로 전환
            if (distanceToTarget > attackRange * 1.2f)
            {
                SetAIState(AIState.Chase);
                return;
            }
            
            // 타겟을 바라보기
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            // 공격 실행
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
        
        private void UpdateSearchState()
        {
            // 마지막으로 알려진 플레이어 위치로 이동
            if (navAgent != null)
            {
                navAgent.SetDestination(lastKnownPlayerPosition);
                
                // 목적지에 도달했거나 일정 시간이 지나면 복귀 상태로 전환
                if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
                {
                    SetAIState(AIState.Return);
                }
            }
            
            // 플레이어 재감지
            if (DetectPlayer())
            {
                SetAIState(AIState.Chase);
            }
        }
        
        private void UpdateReturnState()
        {
            // 원래 위치로 복귀
            if (navAgent != null)
            {
                navAgent.SetDestination(transform.position);
            }
            
            // 복귀 완료 후 대기 상태로 전환
            if (navAgent != null && !navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                SetAIState(AIState.Idle);
            }
        }
        
        private bool DetectPlayer()
        {
            // 플레이어 감지
            Collider[] players = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);
            
            foreach (var player in players)
            {
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    target = player.transform;
                    return true;
                }
            }
            
            return false;
        }
        
        private void Attack()
        {
            if (target == null) return;
            
            // 공격 애니메이션
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            // 공격 사운드
            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound);
            }
            
            // 타겟에게 데미지 적용
            PlayerController playerController = target.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(attackDamage);
                OnEnemyAttack?.Invoke(this, playerController);
            }
            
            Debug.Log($"적이 {attackDamage} 데미지를 입혔습니다.");
        }
        
        public void TakeDamage(int damage)
        {
            if (isDead) return;
            
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            // 히트 효과
            PlayHitEffect();
            
            // 이벤트 발생
            OnEnemyDamaged?.Invoke(this, damage);
            
            // 사망 체크
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // 데미지를 받으면 플레이어를 추적
                if (currentState == AIState.Idle || currentState == AIState.Patrol)
                {
                    SetAIState(AIState.Chase);
                }
            }
        }
        
        public void Heal(int healAmount)
        {
            if (isDead) return;
            
            currentHealth = Mathf.Min(enemyStats.maxHealth, currentHealth + healAmount);
        }
        
        private void Die()
        {
            isDead = true;
            
            // 사망 애니메이션
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }
            
            // 사망 효과
            PlayDeathEffect();
            
            // 사망 사운드
            if (deathSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
            
            // 네비게이션 에이전트 비활성화
            if (navAgent != null)
            {
                navAgent.enabled = false;
            }
            
            // 콜라이더 비활성화
            if (enemyCollider != null)
            {
                enemyCollider.enabled = false;
            }
            
            // 아이템 드롭
            DropItems();
            
            // 이벤트 발생
            OnEnemyDeath?.Invoke(this);
            
            // 오브젝트 제거
            Destroy(gameObject, 3f);
            
            Debug.Log("적이 사망했습니다.");
        }
        
        private void PlayHitEffect()
        {
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
        }
        
        private void PlayDeathEffect()
        {
            if (deathEffect != null)
            {
                GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
                Destroy(effect, 3f);
            }
        }
        
        private void DropItems()
        {
            // 아이템 드롭 로직
            ItemSystem itemSystem = FindObjectOfType<ItemSystem>();
            if (itemSystem != null && enemyStats != null)
            {
                // 랜덤 아이템 드롭
                if (Random.Range(0f, 1f) < enemyStats.dropRate)
                {
                    Item droppedItem = itemSystem.CreateRandomItem();
                    if (droppedItem != null)
                    {
                        itemSystem.DropItem(droppedItem, transform.position);
                    }
                }
            }
        }
        
        private void SetAIState(AIState newState)
        {
            if (currentState == newState) return;
            
            currentState = newState;
            stateChangeTime = Time.time;
            
            // 상태별 초기화
            switch (newState)
            {
                case AIState.Idle:
                    if (navAgent != null)
                        navAgent.ResetPath();
                    break;
                case AIState.Patrol:
                    // 패트롤 시작
                    break;
                case AIState.Chase:
                    // 추적 시작
                    break;
                case AIState.Attack:
                    // 공격 준비
                    break;
                case AIState.Search:
                    // 탐색 시작
                    break;
                case AIState.Return:
                    // 복귀 시작
                    break;
            }
        }
        
        private void UpdateAnimation()
        {
            if (animator == null) return;
            
            // 이동 애니메이션
            float speed = navAgent != null ? navAgent.velocity.magnitude : 0f;
            animator.SetFloat("Speed", speed);
            
            // 상태 애니메이션
            animator.SetInteger("AIState", (int)currentState);
        }
        
        // Getter 메서드들
        public AIState GetCurrentState() => currentState;
        public int GetCurrentHealth() => currentHealth;
        public int GetMaxHealth() => enemyStats != null ? enemyStats.maxHealth : 0;
        public bool IsDead() => isDead;
        public Transform GetTarget() => target;
        
        private void OnDrawGizmosSelected()
        {
            // 감지 범위 시각화
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // 공격 범위 시각화
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
    
    [System.Serializable]
    public class EnemyStats
    {
        public string enemyName;
        public int maxHealth;
        public int maxMana;
        public int attack;
        public int defense;
        public int speed;
        public int level;
        public int experience;
        public float dropRate;
        public int goldReward;
        
        public EnemyStats()
        {
            enemyName = "Enemy";
            maxHealth = 100;
            maxMana = 50;
            attack = 10;
            defense = 5;
            speed = 3;
            level = 1;
            experience = 10;
            dropRate = 0.3f;
            goldReward = 5;
        }
    }
    
    public enum AIState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Search,
        Return
    }
}
