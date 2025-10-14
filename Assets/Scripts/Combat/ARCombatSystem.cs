using UnityEngine;
using System.Collections.Generic;
using NowHere.AR;
using NowHere.RPG;
using NowHere.Sensors;
using NowHere.Motion;
using NowHere.Audio;
using NowHere.AI;
using NowHere.Player;

namespace NowHere.Combat
{
    /// <summary>
    /// AR 기반 전투 시스템을 관리하는 클래스
    /// 현실 세계에서의 공격, 방어, 스킬 사용을 처리
    /// </summary>
    public class ARCombatSystem : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private float defenseRange = 2f;
        [SerializeField] private float skillRange = 5f;
        [SerializeField] private float combatCooldown = 1f;
        
        [Header("Attack Settings")]
        [SerializeField] private int baseAttackDamage = 10;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float criticalChance = 0.1f;
        [SerializeField] private float criticalMultiplier = 2f;
        
        [Header("Defense Settings")]
        [SerializeField] private float baseDefense = 5f;
        [SerializeField] private float blockChance = 0.3f;
        [SerializeField] private float dodgeChance = 0.2f;
        [SerializeField] private float parryChance = 0.15f;
        
        [Header("AR Combat Features")]
        [SerializeField] private bool enableGestureCombat = true;
        [SerializeField] private bool enableMotionCombat = true;
        [SerializeField] private bool enableVoiceCombat = true;
        [SerializeField] private bool enableEyeTracking = false;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject attackEffect;
        [SerializeField] private GameObject defenseEffect;
        [SerializeField] private GameObject skillEffect;
        [SerializeField] private GameObject criticalEffect;
        [SerializeField] private GameObject blockEffect;
        [SerializeField] private GameObject dodgeEffect;
        
        [Header("Audio Effects")]
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioClip defenseSound;
        [SerializeField] private AudioClip skillSound;
        [SerializeField] private AudioClip criticalSound;
        [SerializeField] private AudioClip blockSound;
        [SerializeField] private AudioClip dodgeSound;
        
        // 전투 상태
        private bool isInCombat = false;
        private bool canAttack = true;
        private bool canDefend = true;
        private bool canUseSkill = true;
        private float lastAttackTime;
        private float lastDefenseTime;
        private float lastSkillTime;
        
        // 전투 데이터
        private CombatStats combatStats;
        private List<CombatAction> combatHistory = new List<CombatAction>();
        private Dictionary<GameObject, EnemyData> enemiesInRange = new Dictionary<GameObject, EnemyData>();
        
        // AR 전투 관련
        private Vector3 lastAttackDirection;
        private Vector3 lastDefenseDirection;
        private float lastMotionIntensity;
        private bool isGestureDetected = false;
        private GestureType currentGesture = GestureType.None;
        
        // 참조
        private CharacterSystem characterSystem;
        private SkillSystem skillSystem;
        private ARObjectManager arObjectManager;
        private MobileSensorManager sensorManager;
        private MotionDetectionManager motionManager;
        private VoiceChatManager voiceManager;
        
        // 이벤트
        public System.Action<CombatAction> OnCombatActionPerformed;
        public System.Action<GameObject, int> OnEnemyDamaged;
        public System.Action<GameObject> OnEnemyDefeated;
        public System.Action<int> OnPlayerDamaged;
        public System.Action<CombatStats> OnCombatStatsChanged;
        public System.Action<bool> OnCombatStateChanged;
        
        private void Start()
        {
            InitializeCombatSystem();
        }
        
        public void Update()
        {
            UpdateCombatState();
            ProcessCombatInput();
            UpdateEnemiesInRange();
        }
        
        private void InitializeCombatSystem()
        {
            // 컴포넌트 참조
            characterSystem = GetComponent<CharacterSystem>();
            skillSystem = GetComponent<SkillSystem>();
            arObjectManager = FindObjectOfType<ARObjectManager>();
            sensorManager = FindObjectOfType<MobileSensorManager>();
            motionManager = FindObjectOfType<MotionDetectionManager>();
            voiceManager = FindObjectOfType<VoiceChatManager>();
            
            // 전투 스탯 초기화
            combatStats = new CombatStats
            {
                attackDamage = baseAttackDamage,
                defense = baseDefense,
                attackSpeed = attackSpeed,
                criticalChance = criticalChance,
                criticalMultiplier = criticalMultiplier,
                blockChance = blockChance,
                dodgeChance = dodgeChance,
                parryChance = parryChance
            };
            
            // 이벤트 구독
            if (motionManager != null)
            {
                motionManager.OnMotionDetected += OnMotionDetected;
            }
            
            if (sensorManager != null)
            {
                sensorManager.OnMotionDataUpdated += OnMotionDataUpdated;
            }
            
            Debug.Log("AR 전투 시스템이 초기화되었습니다.");
        }
        
        private void UpdateCombatState()
        {
            // 전투 상태 업데이트
            bool wasInCombat = isInCombat;
            isInCombat = enemiesInRange.Count > 0;
            
            if (wasInCombat != isInCombat)
            {
                OnCombatStateChanged?.Invoke(isInCombat);
            }
            
            // 쿨다운 업데이트
            UpdateCooldowns();
        }
        
        private void ProcessCombatInput()
        {
            if (!isInCombat) return;
            
            // 터치 입력 처리
            ProcessTouchCombat();
            
            // 모션 입력 처리
            if (enableMotionCombat)
            {
                ProcessMotionCombat();
            }
            
            // 제스처 입력 처리
            if (enableGestureCombat)
            {
                ProcessGestureCombat();
            }
            
            // 음성 입력 처리
            if (enableVoiceCombat)
            {
                ProcessVoiceCombat();
            }
        }
        
        private void ProcessTouchCombat()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Began)
                {
                    // 터치 위치에 따른 전투 액션
                    Vector3 touchWorldPos = GetWorldPositionFromTouch(touch.position);
                    GameObject target = GetTargetAtPosition(touchWorldPos);
                    
                    if (target != null)
                    {
                        if (IsEnemy(target))
                        {
                            PerformAttack(target);
                        }
                        else if (IsAlly(target))
                        {
                            PerformDefense(target);
                        }
                    }
                }
            }
        }
        
        private void ProcessMotionCombat()
        {
            if (motionManager == null) return;
            
            // 모션 감지에 따른 전투 액션
            if (motionManager.IsDodging())
            {
                PerformDodge();
            }
            
            // 흔들기 모션으로 공격
            if (lastMotionIntensity > 2f)
            {
                PerformMotionAttack();
            }
        }
        
        private void ProcessGestureCombat()
        {
            // 제스처 감지 처리
            if (isGestureDetected)
            {
                switch (currentGesture)
                {
                    case GestureType.SwipeRight:
                        PerformSwipeAttack(Vector3.right);
                        break;
                    case GestureType.SwipeLeft:
                        PerformSwipeAttack(Vector3.left);
                        break;
                    case GestureType.SwipeUp:
                        PerformSwipeAttack(Vector3.forward);
                        break;
                    case GestureType.SwipeDown:
                        PerformSwipeAttack(Vector3.back);
                        break;
                    case GestureType.Circle:
                        PerformCircularAttack();
                        break;
                    case GestureType.Triangle:
                        PerformTriangularDefense();
                        break;
                }
                
                isGestureDetected = false;
                currentGesture = GestureType.None;
            }
        }
        
        private void ProcessVoiceCombat()
        {
            // 음성 명령 처리
            // 실제로는 음성 인식 시스템과 연동
        }
        
        private void UpdateEnemiesInRange()
        {
            enemiesInRange.Clear();
            
            // 주변 적 감지
            Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange);
            
            foreach (var enemy in enemies)
            {
                if (IsEnemy(enemy.gameObject))
                {
                    EnemyData enemyData = new EnemyData
                    {
                        gameObject = enemy.gameObject,
                        distance = Vector3.Distance(transform.position, enemy.transform.position),
                        lastSeenTime = Time.time,
                        isAlive = true
                    };
                    
                    enemiesInRange[enemy.gameObject] = enemyData;
                }
            }
        }
        
        private void UpdateCooldowns()
        {
            // 공격 쿨다운
            if (Time.time - lastAttackTime >= combatCooldown)
            {
                canAttack = true;
            }
            
            // 방어 쿨다운
            if (Time.time - lastDefenseTime >= combatCooldown)
            {
                canDefend = true;
            }
            
            // 스킬 쿨다운
            if (Time.time - lastSkillTime >= combatCooldown * 2)
            {
                canUseSkill = true;
            }
        }
        
        public void PerformAttack(GameObject target)
        {
            if (!canAttack || !IsEnemy(target)) return;
            
            // 공격 실행
            int damage = CalculateDamage();
            bool isCritical = IsCriticalHit();
            
            if (isCritical)
            {
                damage = Mathf.RoundToInt(damage * combatStats.criticalMultiplier);
            }
            
            // 적에게 데미지 적용
            EnemyController enemy = target.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                OnEnemyDamaged?.Invoke(target, damage);
                
                if (enemy.GetCurrentHealth() <= 0)
                {
                    OnEnemyDefeated?.Invoke(target);
                }
            }
            
            // 공격 효과
            PlayAttackEffect(target.transform.position, isCritical);
            
            // 전투 액션 기록
            CombatAction action = new CombatAction
            {
                actionType = CombatActionType.Attack,
                target = target,
                damage = damage,
                isCritical = isCritical,
                timestamp = Time.time
            };
            
            combatHistory.Add(action);
            OnCombatActionPerformed?.Invoke(action);
            
            // 쿨다운 설정
            canAttack = false;
            lastAttackTime = Time.time;
            
            Debug.Log($"공격 실행: {damage} 데미지 (크리티컬: {isCritical})");
        }
        
        public void PerformDefense(GameObject target)
        {
            if (!canDefend) return;
            
            // 방어 실행
            float defenseValue = CalculateDefense();
            
            // 방어 효과
            PlayDefenseEffect(transform.position);
            
            // 전투 액션 기록
            CombatAction action = new CombatAction
            {
                actionType = CombatActionType.Defense,
                target = target,
                defenseValue = defenseValue,
                timestamp = Time.time
            };
            
            combatHistory.Add(action);
            OnCombatActionPerformed?.Invoke(action);
            
            // 쿨다운 설정
            canDefend = false;
            lastDefenseTime = Time.time;
            
            Debug.Log($"방어 실행: {defenseValue} 방어력");
        }
        
        public void PerformSkill(int skillId, GameObject target = null)
        {
            if (!canUseSkill || skillSystem == null) return;
            
            // 스킬 사용
            bool success = skillSystem.UseSkill(skillId, target != null ? target.transform.position : transform.position);
            
            if (success)
            {
                // 스킬 효과
                PlaySkillEffect(transform.position);
                
                // 전투 액션 기록
                CombatAction action = new CombatAction
                {
                    actionType = CombatActionType.Skill,
                    target = target,
                    skillId = skillId,
                    timestamp = Time.time
                };
                
                combatHistory.Add(action);
                OnCombatActionPerformed?.Invoke(action);
                
                // 쿨다운 설정
                canUseSkill = false;
                lastSkillTime = Time.time;
                
                Debug.Log($"스킬 사용: {skillId}");
            }
        }
        
        public void PerformDodge()
        {
            // 회피 실행
            PlayDodgeEffect(transform.position);
            
            // 전투 액션 기록
            CombatAction action = new CombatAction
            {
                actionType = CombatActionType.Dodge,
                timestamp = Time.time
            };
            
            combatHistory.Add(action);
            OnCombatActionPerformed?.Invoke(action);
            
            Debug.Log("회피 실행");
        }
        
        private void PerformMotionAttack()
        {
            // 모션 기반 공격
            GameObject nearestEnemy = GetNearestEnemy();
            if (nearestEnemy != null)
            {
                PerformAttack(nearestEnemy);
            }
        }
        
        private void PerformSwipeAttack(Vector3 direction)
        {
            // 스와이프 공격
            lastAttackDirection = direction;
            
            // 방향에 있는 적들 공격
            Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange);
            
            foreach (var enemy in enemies)
            {
                if (IsEnemy(enemy.gameObject))
                {
                    Vector3 toEnemy = (enemy.transform.position - transform.position).normalized;
                    float dot = Vector3.Dot(direction, toEnemy);
                    
                    if (dot > 0.5f) // 60도 이내
                    {
                        PerformAttack(enemy.gameObject);
                    }
                }
            }
        }
        
        private void PerformCircularAttack()
        {
            // 원형 공격
            Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange);
            
            foreach (var enemy in enemies)
            {
                if (IsEnemy(enemy.gameObject))
                {
                    PerformAttack(enemy.gameObject);
                }
            }
        }
        
        private void PerformTriangularDefense()
        {
            // 삼각형 방어
            PerformDefense(null);
        }
        
        private int CalculateDamage()
        {
            int damage = combatStats.attackDamage;
            
            // 캐릭터 스탯 반영
            if (characterSystem != null)
            {
                var stats = characterSystem.GetCurrentStats();
                damage += stats.attack;
            }
            
            return damage;
        }
        
        private float CalculateDefense()
        {
            float defense = combatStats.defense;
            
            // 캐릭터 스탯 반영
            if (characterSystem != null)
            {
                var stats = characterSystem.GetCurrentStats();
                defense += stats.defense;
            }
            
            return defense;
        }
        
        private bool IsCriticalHit()
        {
            return Random.Range(0f, 1f) < combatStats.criticalChance;
        }
        
        private GameObject GetNearestEnemy()
        {
            GameObject nearestEnemy = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var enemy in enemiesInRange.Values)
            {
                if (enemy.isAlive && enemy.distance < nearestDistance)
                {
                    nearestDistance = enemy.distance;
                    nearestEnemy = enemy.gameObject;
                }
            }
            
            return nearestEnemy;
        }
        
        private Vector3 GetWorldPositionFromTouch(Vector2 touchPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(touchPosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                return hit.point;
            }
            
            return ray.GetPoint(5f);
        }
        
        private GameObject GetTargetAtPosition(Vector3 position)
        {
            Collider[] colliders = Physics.OverlapSphere(position, 1f);
            
            foreach (var collider in colliders)
            {
                if (IsEnemy(collider.gameObject) || IsAlly(collider.gameObject))
                {
                    return collider.gameObject;
                }
            }
            
            return null;
        }
        
        private bool IsEnemy(GameObject obj)
        {
            return obj.GetComponent<EnemyController>() != null;
        }
        
        private bool IsAlly(GameObject obj)
        {
            return obj.GetComponent<PlayerController>() != null;
        }
        
        private void PlayAttackEffect(Vector3 position, bool isCritical)
        {
            GameObject effect = isCritical ? criticalEffect : attackEffect;
            
            if (effect != null)
            {
                GameObject effectInstance = Instantiate(effect, position, Quaternion.identity);
                Destroy(effectInstance, 2f);
            }
            
            AudioClip sound = isCritical ? criticalSound : attackSound;
            if (sound != null)
            {
                AudioSource.PlayClipAtPoint(sound, position);
            }
        }
        
        private void PlayDefenseEffect(Vector3 position)
        {
            if (defenseEffect != null)
            {
                GameObject effectInstance = Instantiate(defenseEffect, position, Quaternion.identity);
                Destroy(effectInstance, 2f);
            }
            
            if (defenseSound != null)
            {
                AudioSource.PlayClipAtPoint(defenseSound, position);
            }
        }
        
        private void PlaySkillEffect(Vector3 position)
        {
            if (skillEffect != null)
            {
                GameObject effectInstance = Instantiate(skillEffect, position, Quaternion.identity);
                Destroy(effectInstance, 3f);
            }
            
            if (skillSound != null)
            {
                AudioSource.PlayClipAtPoint(skillSound, position);
            }
        }
        
        private void PlayDodgeEffect(Vector3 position)
        {
            if (dodgeEffect != null)
            {
                GameObject effectInstance = Instantiate(dodgeEffect, position, Quaternion.identity);
                Destroy(effectInstance, 1f);
            }
            
            if (dodgeSound != null)
            {
                AudioSource.PlayClipAtPoint(dodgeSound, position);
            }
        }
        
        // 이벤트 핸들러
        private void OnMotionDetected(MotionType motionType, Vector3 direction)
        {
            switch (motionType)
            {
                case MotionType.Shake:
                    PerformMotionAttack();
                    break;
                case MotionType.Tilt:
                    PerformDodge();
                    break;
            }
        }
        
        private void OnMotionDataUpdated(NowHere.Sensors.MotionData motionData)
        {
            lastMotionIntensity = motionData.accelerationMagnitude;
        }
        
        // 공개 메서드들
        public bool IsInCombat()
        {
            return isInCombat;
        }
        
        public CombatStats GetCombatStats()
        {
            return combatStats;
        }
        
        public List<CombatAction> GetCombatHistory()
        {
            return new List<CombatAction>(combatHistory);
        }
        
        public void SetGesture(GestureType gesture)
        {
            currentGesture = gesture;
            isGestureDetected = true;
        }
        
        public void TakeDamage(int damage)
        {
            // 플레이어 데미지 처리
            if (characterSystem != null)
            {
                characterSystem.TakeDamage(damage);
            }
            
            OnPlayerDamaged?.Invoke(damage);
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (motionManager != null)
            {
                motionManager.OnMotionDetected -= OnMotionDetected;
            }
            
            if (sensorManager != null)
            {
                sensorManager.OnMotionDataUpdated -= OnMotionDataUpdated;
            }
        }
    }
    
    [System.Serializable]
    public struct CombatStats
    {
        public int attackDamage;
        public float defense;
        public float attackSpeed;
        public float criticalChance;
        public float criticalMultiplier;
        public float blockChance;
        public float dodgeChance;
        public float parryChance;
    }
    
    [System.Serializable]
    public struct CombatAction
    {
        public CombatActionType actionType;
        public GameObject target;
        public int damage;
        public float defenseValue;
        public int skillId;
        public bool isCritical;
        public float timestamp;
    }
    
    [System.Serializable]
    public struct EnemyData
    {
        public GameObject gameObject;
        public float distance;
        public float lastSeenTime;
        public bool isAlive;
    }
    
    public enum CombatActionType
    {
        Attack,
        Defense,
        Skill,
        Dodge,
        Block,
        Parry
    }
    
    public enum GestureType
    {
        None,
        SwipeRight,
        SwipeLeft,
        SwipeUp,
        SwipeDown,
        Circle,
        Triangle,
        Square
    }
}
