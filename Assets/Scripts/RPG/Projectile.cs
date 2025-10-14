using UnityEngine;
using NowHere.RPG;
using NowHere.Player;
using NowHere.AI;

namespace NowHere.RPG
{
    /// <summary>
    /// 투사체를 관리하는 클래스
    /// 스킬에서 발사되는 투사체의 이동, 충돌, 효과를 담당
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private bool destroyOnHit = true;
        [SerializeField] private LayerMask targetLayers = 1;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject hitEffect;
        [SerializeField] private GameObject trailEffect;
        [SerializeField] private AudioClip hitSound;
        
        // 투사체 데이터
        private Skill skill;
        private Vector3 targetPosition;
        private Vector3 direction;
        private float startTime;
        private bool hasHit = false;
        
        // 컴포넌트 참조
        private Rigidbody rb;
        private AudioSource audioSource;
        
        // 이벤트
        public System.Action<Projectile, Collider> OnProjectileHit;
        public System.Action<Projectile> OnProjectileDestroyed;
        
        private void Start()
        {
            InitializeProjectile();
        }
        
        private void Update()
        {
            // 수명 체크
            if (Time.time - startTime >= lifetime)
            {
                DestroyProjectile();
            }
        }
        
        private void InitializeProjectile()
        {
            startTime = Time.time;
            
            // 리지드바디 설정
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.useGravity = false;
            rb.linearDamping = 0f;
            
            // 오디오 소스 설정
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // 방향 설정
            direction = (targetPosition - transform.position).normalized;
            
            // 속도 설정
            rb.linearVelocity = direction * speed;
            
            // 트레일 효과 생성
            if (trailEffect != null)
            {
                GameObject trail = Instantiate(trailEffect, transform);
            }
        }
        
        public void Initialize(Skill skillData, Vector3 targetPos)
        {
            skill = skillData;
            targetPosition = targetPos;
            
            // 스킬에 따른 투사체 설정
            if (skill != null)
            {
                speed = skill.range * 2f; // 스킬 범위에 따른 속도 조정
                lifetime = skill.range / speed + 1f; // 도달 시간 + 여유 시간
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (hasHit) return;
            
            // 타겟 레이어 체크
            if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
                return;
            
            // 자신과의 충돌 방지
            if (other.transform == transform.parent)
                return;
            
            // 충돌 처리
            HandleCollision(other);
        }
        
        private void HandleCollision(Collider other)
        {
            hasHit = true;
            
            // 히트 효과 재생
            PlayHitEffects(other.transform.position);
            
            // 스킬 효과 적용
            ApplySkillEffect(other);
            
            // 이벤트 발생
            OnProjectileHit?.Invoke(this, other);
            
            // 투사체 제거
            if (destroyOnHit)
            {
                DestroyProjectile();
            }
        }
        
        private void PlayHitEffects(Vector3 hitPosition)
        {
            // 시각적 효과
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, hitPosition, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // 사운드 효과
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
        }
        
        private void ApplySkillEffect(Collider target)
        {
            if (skill == null) return;
            
            // 타겟이 플레이어인지 확인
            PlayerController playerController = target.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // 공격 스킬인 경우 데미지 적용
                if (skill.skillType == SkillType.Attack || skill.skillType == SkillType.Magic)
                {
                    playerController.TakeDamage(skill.damage);
                }
                
                // 힐 스킬인 경우 회복 적용
                if (skill.skillType == SkillType.Heal)
                {
                    playerController.Heal(skill.healAmount);
                }
            }
            
            // 타겟이 적인 경우
            EnemyController enemyController = target.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                // 공격 스킬인 경우 데미지 적용
                if (skill.skillType == SkillType.Attack || skill.skillType == SkillType.Magic)
                {
                    enemyController.TakeDamage(skill.damage);
                }
            }
        }
        
        private void DestroyProjectile()
        {
            OnProjectileDestroyed?.Invoke(this);
            Destroy(gameObject);
        }
        
        public Skill GetSkill()
        {
            return skill;
        }
        
        public Vector3 GetTargetPosition()
        {
            return targetPosition;
        }
        
        public bool HasHit()
        {
            return hasHit;
        }
        
        public float GetRemainingLifetime()
        {
            return lifetime - (Time.time - startTime);
        }
    }
}
