using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using NowHere.XR;
using NowHere.RPG;
using NowHere.Audio;

namespace NowHere.XR
{
    /// <summary>
    /// VR 기반 전투 시스템을 관리하는 클래스
    /// VR 컨트롤러와 핸드 트래킹을 활용한 몰입감 있는 전투
    /// </summary>
    public class VRCombatSystem : MonoBehaviour
    {
        [Header("VR Combat Settings")]
        [SerializeField] private float attackForceThreshold = 0.5f;
        [SerializeField] private float defenseForceThreshold = 0.3f;
        [SerializeField] private float skillGestureThreshold = 0.8f;
        [SerializeField] private float combatRange = 5f;
        
        [Header("VR Weapons")]
        [SerializeField] private GameObject swordPrefab;
        [SerializeField] private GameObject shieldPrefab;
        [SerializeField] private GameObject bowPrefab;
        [SerializeField] private GameObject staffPrefab;
        
        [Header("VR Combat Effects")]
        [SerializeField] private GameObject swordTrailEffect;
        [SerializeField] private GameObject shieldBlockEffect;
        [SerializeField] private GameObject magicCastEffect;
        [SerializeField] private GameObject impactEffect;
        
        [Header("Audio Effects")]
        [SerializeField] private AudioClip swordSwingSound;
        [SerializeField] private AudioClip shieldBlockSound;
        [SerializeField] private AudioClip magicCastSound;
        [SerializeField] private AudioClip impactSound;
        
        // VR 전투 상태
        private bool isInVRCombat = false;
        private bool canAttack = true;
        private bool canDefend = true;
        private bool canUseSkill = true;
        
        // VR 무기 인스턴스
        private GameObject currentSword;
        private GameObject currentShield;
        private GameObject currentBow;
        private GameObject currentStaff;
        
        // VR 입력 데이터
        private XRInputData leftControllerData;
        private XRInputData rightControllerData;
        private HandTrackingData leftHandData;
        private HandTrackingData rightHandData;
        
        // 전투 데이터
        private VRCombatStats combatStats;
        private List<VRCombatAction> combatHistory = new List<VRCombatAction>();
        private Dictionary<GameObject, EnemyData> enemiesInRange = new Dictionary<GameObject, EnemyData>();
        
        // 참조
        private XRManager xrManager;
        private CharacterSystem characterSystem;
        private SkillSystem skillSystem;
        private AudioSource audioSource;
        
        // 이벤트
        public System.Action<VRCombatAction> OnVRCombatActionPerformed;
        public System.Action<GameObject, int> OnEnemyDamaged;
        public System.Action<GameObject> OnEnemyDefeated;
        public System.Action<int> OnPlayerDamaged;
        public System.Action<VRCombatStats> OnCombatStatsChanged;
        
        private void Start()
        {
            InitializeVRCombatSystem();
        }
        
        private void Update()
        {
            if (!isInVRCombat) return;
            
            UpdateVRCombatInput();
            ProcessVRCombatActions();
            UpdateEnemiesInRange();
        }
        
        private void InitializeVRCombatSystem()
        {
            // 컴포넌트 참조
            xrManager = FindObjectOfType<XRManager>();
            characterSystem = GetComponent<CharacterSystem>();
            skillSystem = GetComponent<SkillSystem>();
            audioSource = GetComponent<AudioSource>();
            
            // VR 전투 스탯 초기화
            combatStats = new VRCombatStats
            {
                attackDamage = 15,
                defense = 8,
                attackSpeed = 1.2f,
                criticalChance = 0.15f,
                criticalMultiplier = 2.5f,
                blockChance = 0.4f,
                dodgeChance = 0.25f,
                magicPower = 20
            };
            
            // XR 이벤트 구독
            if (xrManager != null)
            {
                xrManager.OnControllerInput += OnControllerInput;
                xrManager.OnHandTrackingData += OnHandTrackingData;
                xrManager.OnVoiceCommand += OnVoiceCommand;
            }
            
            Debug.Log("VR 전투 시스템이 초기화되었습니다.");
        }
        
        private void UpdateVRCombatInput()
        {
            if (xrManager == null) return;
            
            // 컨트롤러 데이터 업데이트
            leftControllerData = xrManager.GetLeftControllerData();
            rightControllerData = xrManager.GetRightControllerData();
            
            // 핸드 트래킹 데이터 업데이트
            leftHandData = xrManager.GetLeftHandData();
            rightHandData = xrManager.GetRightHandData();
        }
        
        private void ProcessVRCombatActions()
        {
            // 오른쪽 컨트롤러 공격 처리
            if (rightControllerData.isConnected)
            {
                ProcessControllerAttack(rightControllerData);
            }
            
            // 왼쪽 컨트롤러 방어 처리
            if (leftControllerData.isConnected)
            {
                ProcessControllerDefense(leftControllerData);
            }
            
            // 핸드 트래킹 기반 제스처 처리
            if (xrManager.IsHandTrackingActive())
            {
                ProcessHandGestureCombat();
            }
        }
        
        private void ProcessControllerAttack(XRInputData controllerData)
        {
            // 트리거를 당겨서 공격
            if (controllerData.triggerValue > attackForceThreshold && canAttack)
            {
                PerformVRAttack(controllerData.position, controllerData.rotation);
            }
            
            // 그립 버튼으로 강공격
            if (controllerData.gripValue > 0.8f && canAttack)
            {
                PerformVRHeavyAttack(controllerData.position, controllerData.rotation);
            }
            
            // 썸스틱으로 방향 공격
            if (controllerData.thumbstick.magnitude > 0.5f && canAttack)
            {
                PerformVRDirectionalAttack(controllerData.thumbstick);
            }
        }
        
        private void ProcessControllerDefense(XRInputData controllerData)
        {
            // 트리거로 방어
            if (controllerData.triggerValue > defenseForceThreshold && canDefend)
            {
                PerformVRDefense(controllerData.position, controllerData.rotation);
            }
            
            // 그립으로 강방어
            if (controllerData.gripValue > 0.7f && canDefend)
            {
                PerformVRShieldDefense(controllerData.position, controllerData.rotation);
            }
        }
        
        private void ProcessHandGestureCombat()
        {
            // 손가락 제스처 인식
            if (rightHandData.isTracked)
            {
                // 펀치 제스처 (주먹 쥐기)
                if (IsPunchGesture(rightHandData))
                {
                    PerformVRPunchAttack(rightHandData.position, rightHandData.rotation);
                }
                
                // 마법 제스처 (손가락 펼치기)
                if (IsMagicGesture(rightHandData))
                {
                    PerformVRMagicCast(rightHandData.position, rightHandData.rotation);
                }
            }
            
            if (leftHandData.isTracked)
            {
                // 방어 제스처 (손바닥 펼치기)
                if (IsDefenseGesture(leftHandData))
                {
                    PerformVRHandDefense(leftHandData.position, leftHandData.rotation);
                }
            }
        }
        
        private bool IsPunchGesture(HandTrackingData handData)
        {
            // 주먹 쥐기 제스처 감지
            if (handData.fingerBends != null && handData.fingerBends.Length >= 5)
            {
                float totalBend = 0f;
                for (int i = 0; i < 5; i++)
                {
                    totalBend += handData.fingerBends[i];
                }
                return totalBend > 4f; // 모든 손가락이 많이 구부러짐
            }
            return false;
        }
        
        private bool IsMagicGesture(HandTrackingData handData)
        {
            // 마법 제스처 감지 (손가락 펼치기)
            if (handData.fingerBends != null && handData.fingerBends.Length >= 5)
            {
                float totalBend = 0f;
                for (int i = 0; i < 5; i++)
                {
                    totalBend += handData.fingerBends[i];
                }
                return totalBend < 1f; // 모든 손가락이 펼쳐짐
            }
            return false;
        }
        
        private bool IsDefenseGesture(HandTrackingData handData)
        {
            // 방어 제스처 감지 (손바닥 펼치기)
            return handData.isPointing && !handData.isGrabbing;
        }
        
        public void PerformVRAttack(Vector3 position, Quaternion rotation)
        {
            if (!canAttack) return;
            
            // 검 공격
            if (currentSword != null)
            {
                PerformSwordAttack(position, rotation);
            }
            else
            {
                // 맨손 공격
                PerformHandAttack(position, rotation);
            }
            
            // 공격 효과
            PlayAttackEffect(position, rotation);
            
            // 쿨다운 설정
            canAttack = false;
            Invoke(nameof(ResetAttackCooldown), 1f / combatStats.attackSpeed);
            
            Debug.Log("VR 공격 실행!");
        }
        
        public void PerformVRHeavyAttack(Vector3 position, Quaternion rotation)
        {
            if (!canAttack) return;
            
            // 강공격 (더 큰 데미지, 더 긴 쿨다운)
            int damage = Mathf.RoundToInt(combatStats.attackDamage * 1.5f);
            
            // 적에게 데미지 적용
            ApplyDamageToEnemiesInRange(damage, position);
            
            // 강공격 효과
            PlayHeavyAttackEffect(position, rotation);
            
            // 쿨다운 설정
            canAttack = false;
            Invoke(nameof(ResetAttackCooldown), 2f / combatStats.attackSpeed);
            
            Debug.Log("VR 강공격 실행!");
        }
        
        public void PerformVRDirectionalAttack(Vector2 direction)
        {
            if (!canAttack) return;
            
            // 방향성 공격 (썸스틱 방향으로)
            Vector3 attackDirection = new Vector3(direction.x, 0, direction.y);
            
            // 방향에 따른 특수 공격
            if (direction.y > 0.5f)
            {
                PerformVRUppercut();
            }
            else if (direction.y < -0.5f)
            {
                PerformVRDownwardStrike();
            }
            else if (direction.x > 0.5f)
            {
                PerformVRRightSlash();
            }
            else if (direction.x < -0.5f)
            {
                PerformVRLeftSlash();
            }
        }
        
        public void PerformVRDefense(Vector3 position, Quaternion rotation)
        {
            if (!canDefend) return;
            
            // 방어 실행
            if (currentShield != null)
            {
                PerformShieldDefense(position, rotation);
            }
            else
            {
                // 맨손 방어
                PerformHandDefense(position, rotation);
            }
            
            // 방어 효과
            PlayDefenseEffect(position, rotation);
            
            // 쿨다운 설정
            canDefend = false;
            Invoke(nameof(ResetDefenseCooldown), 0.5f);
            
            Debug.Log("VR 방어 실행!");
        }
        
        public void PerformVRMagicCast(Vector3 position, Quaternion rotation)
        {
            if (!canUseSkill) return;
            
            // 마법 시전
            if (currentStaff != null)
            {
                PerformStaffMagic(position, rotation);
            }
            else
            {
                // 맨손 마법
                PerformHandMagic(position, rotation);
            }
            
            // 마법 효과
            PlayMagicEffect(position, rotation);
            
            // 쿨다운 설정
            canUseSkill = false;
            Invoke(nameof(ResetSkillCooldown), 2f);
            
            Debug.Log("VR 마법 시전!");
        }
        
        private void PerformSwordAttack(Vector3 position, Quaternion rotation)
        {
            // 검 공격 로직
            int damage = combatStats.attackDamage;
            bool isCritical = Random.Range(0f, 1f) < combatStats.criticalChance;
            
            if (isCritical)
            {
                damage = Mathf.RoundToInt(damage * combatStats.criticalMultiplier);
            }
            
            ApplyDamageToEnemiesInRange(damage, position);
            
            // 검 휘두르기 사운드
            if (audioSource != null && swordSwingSound != null)
            {
                audioSource.PlayOneShot(swordSwingSound);
            }
        }
        
        private void PerformShieldDefense(Vector3 position, Quaternion rotation)
        {
            // 방패 방어 로직
            float blockChance = combatStats.blockChance;
            
            if (Random.Range(0f, 1f) < blockChance)
            {
                // 성공적인 방어
                Debug.Log("방패로 공격을 막았습니다!");
                
                if (audioSource != null && shieldBlockSound != null)
                {
                    audioSource.PlayOneShot(shieldBlockSound);
                }
            }
        }
        
        private void ApplyDamageToEnemiesInRange(int damage, Vector3 attackPosition)
        {
            foreach (var enemy in enemiesInRange.Values)
            {
                if (enemy.isAlive)
                {
                    float distance = Vector3.Distance(attackPosition, enemy.gameObject.transform.position);
                    if (distance <= combatRange)
                    {
                        // 적에게 데미지 적용
                        EnemyController enemyController = enemy.gameObject.GetComponent<EnemyController>();
                        if (enemyController != null)
                        {
                            enemyController.TakeDamage(damage);
                            OnEnemyDamaged?.Invoke(enemy.gameObject, damage);
                            
                            if (enemyController.GetCurrentHealth() <= 0)
                            {
                                OnEnemyDefeated?.Invoke(enemy.gameObject);
                            }
                        }
                    }
                }
            }
        }
        
        private void PlayAttackEffect(Vector3 position, Quaternion rotation)
        {
            if (swordTrailEffect != null)
            {
                GameObject effect = Instantiate(swordTrailEffect, position, rotation);
                Destroy(effect, 2f);
            }
        }
        
        private void PlayDefenseEffect(Vector3 position, Quaternion rotation)
        {
            if (shieldBlockEffect != null)
            {
                GameObject effect = Instantiate(shieldBlockEffect, position, rotation);
                Destroy(effect, 1f);
            }
        }
        
        private void PlayMagicEffect(Vector3 position, Quaternion rotation)
        {
            if (magicCastEffect != null)
            {
                GameObject effect = Instantiate(magicCastEffect, position, rotation);
                Destroy(effect, 3f);
            }
        }
        
        private void PlayHeavyAttackEffect(Vector3 position, Quaternion rotation)
        {
            if (impactEffect != null)
            {
                GameObject effect = Instantiate(impactEffect, position, rotation);
                Destroy(effect, 2f);
            }
        }
        
        // 쿨다운 리셋 메서드들
        private void ResetAttackCooldown() { canAttack = true; }
        private void ResetDefenseCooldown() { canDefend = true; }
        private void ResetSkillCooldown() { canUseSkill = true; }
        
        // 특수 공격 메서드들
        private void PerformVRUppercut()
        {
            Debug.Log("VR 어퍼컷!");
            // 어퍼컷 로직
        }
        
        private void PerformVRDownwardStrike()
        {
            Debug.Log("VR 다운워드 스트라이크!");
            // 다운워드 스트라이크 로직
        }
        
        private void PerformVRRightSlash()
        {
            Debug.Log("VR 라이트 슬래시!");
            // 라이트 슬래시 로직
        }
        
        private void PerformVRLeftSlash()
        {
            Debug.Log("VR 레프트 슬래시!");
            // 레프트 슬래시 로직
        }
        
        private void PerformHandAttack(Vector3 position, Quaternion rotation)
        {
            // 맨손 공격 로직
            Debug.Log("맨손 공격!");
        }
        
        private void PerformHandDefense(Vector3 position, Quaternion rotation)
        {
            // 맨손 방어 로직
            Debug.Log("맨손 방어!");
        }
        
        private void PerformHandMagic(Vector3 position, Quaternion rotation)
        {
            // 맨손 마법 로직
            Debug.Log("맨손 마법!");
        }
        
        private void PerformStaffMagic(Vector3 position, Quaternion rotation)
        {
            // 지팡이 마법 로직
            Debug.Log("지팡이 마법!");
        }
        
        // 이벤트 핸들러들
        private void OnControllerInput(XRInputData data)
        {
            // 컨트롤러 입력 처리
        }
        
        private void OnHandTrackingData(HandTrackingData data)
        {
            // 핸드 트래킹 데이터 처리
        }
        
        private void OnVoiceCommand(string command)
        {
            // 음성 명령 처리
            switch (command.ToLower())
            {
                case "attack":
                    PerformVRAttack(Vector3.zero, Quaternion.identity);
                    break;
                case "defend":
                    PerformVRDefense(Vector3.zero, Quaternion.identity);
                    break;
                case "magic":
                    PerformVRMagicCast(Vector3.zero, Quaternion.identity);
                    break;
            }
        }
        
        private void UpdateEnemiesInRange()
        {
            // 범위 내 적 업데이트
            // 실제 구현에서는 적 탐지 로직 추가
        }
        
        public void SetVRCombatMode(bool active)
        {
            isInVRCombat = active;
        }
        
        public VRCombatStats GetCombatStats()
        {
            return combatStats;
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (xrManager != null)
            {
                xrManager.OnControllerInput -= OnControllerInput;
                xrManager.OnHandTrackingData -= OnHandTrackingData;
                xrManager.OnVoiceCommand -= OnVoiceCommand;
            }
        }
    }
    
    // VR 전투 데이터 구조체들
    [System.Serializable]
    public struct VRCombatStats
    {
        public int attackDamage;
        public float defense;
        public float attackSpeed;
        public float criticalChance;
        public float criticalMultiplier;
        public float blockChance;
        public float dodgeChance;
        public int magicPower;
    }
    
    [System.Serializable]
    public struct VRCombatAction
    {
        public VRCombatActionType actionType;
        public Vector3 position;
        public Quaternion rotation;
        public int damage;
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
    
    public enum VRCombatActionType
    {
        Attack,
        HeavyAttack,
        Defense,
        Magic,
        Special
    }
}
