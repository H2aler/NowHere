using UnityEngine;
using NowHere.RPG;

namespace NowHere.AR
{
    /// <summary>
    /// AR 오브젝트의 기본 컴포넌트
    /// AR 환경에서 배치되는 모든 오브젝트의 공통 기능을 제공
    /// </summary>
    public class ARObject : MonoBehaviour
    {
        [Header("AR Object Settings")]
        [SerializeField] private ARObjectType objectType = ARObjectType.Environment;
        [SerializeField] private bool isInteractable = true;
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask playerLayer = 1;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject highlightEffect;
        [SerializeField] private AudioClip interactionSound;
        
        // AR 오브젝트 데이터
        private ARObjectData objectData;
        private bool isHighlighted = false;
        private AudioSource audioSource;
        
        // 이벤트
        public System.Action<ARObject> OnObjectInteracted;
        public System.Action<ARObject> OnObjectHighlighted;
        public System.Action<ARObject> OnObjectUnhighlighted;
        
        private void Start()
        {
            InitializeARObject();
        }
        
        private void Update()
        {
            // 플레이어와의 상호작용 범위 체크
            CheckPlayerInteraction();
        }
        
        private void InitializeARObject()
        {
            // 오디오 소스 설정
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // AR 오브젝트 데이터 초기화
            objectData = new ARObjectData
            {
                gameObject = gameObject,
                objectType = objectType,
                isInteractable = isInteractable,
                health = 100,
                maxHealth = 100,
                isDestroyed = false
            };
        }
        
        private void CheckPlayerInteraction()
        {
            if (!isInteractable) return;
            
            // 주변 플레이어 감지
            Collider[] players = Physics.OverlapSphere(transform.position, interactionRange, playerLayer);
            
            bool playerInRange = players.Length > 0;
            
            if (playerInRange && !isHighlighted)
            {
                HighlightObject(true);
            }
            else if (!playerInRange && isHighlighted)
            {
                HighlightObject(false);
            }
        }
        
        public void Interact()
        {
            if (!isInteractable) return;
            
            // 상호작용 사운드 재생
            if (interactionSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(interactionSound);
            }
            
            // 이벤트 발생
            OnObjectInteracted?.Invoke(this);
            
            Debug.Log($"AR 오브젝트 '{gameObject.name}'와 상호작용했습니다.");
        }
        
        public void HighlightObject(bool highlight)
        {
            isHighlighted = highlight;
            
            if (highlight)
            {
                // 하이라이트 효과 활성화
                if (highlightEffect != null)
                {
                    highlightEffect.SetActive(true);
                }
                
                // 시각적 하이라이트 (간단한 색상 변경)
                Renderer renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.green;
                }
                
                OnObjectHighlighted?.Invoke(this);
            }
            else
            {
                // 하이라이트 효과 비활성화
                if (highlightEffect != null)
                {
                    highlightEffect.SetActive(false);
                }
                
                // 원래 색상으로 복원
                Renderer renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.white;
                }
                
                OnObjectUnhighlighted?.Invoke(this);
            }
        }
        
        public void SetObjectType(ARObjectType type)
        {
            objectType = type;
            if (objectData.gameObject != null)
            {
                objectData.objectType = type;
            }
        }
        
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            if (objectData.gameObject != null)
            {
                objectData.isInteractable = interactable;
            }
        }
        
        public void TakeDamage(int damage)
        {
            if (objectData.gameObject == null) return;
            
            objectData.health -= damage;
            objectData.health = Mathf.Max(0, objectData.health);
            
            if (objectData.health <= 0)
            {
                DestroyObject();
            }
        }
        
        public void Heal(int healAmount)
        {
            if (objectData.gameObject == null) return;
            
            objectData.health += healAmount;
            objectData.health = Mathf.Min(objectData.maxHealth, objectData.health);
        }
        
        public void DestroyObject()
        {
            if (objectData.gameObject == null) return;
            
            objectData.isDestroyed = true;
            
            // 파괴 효과
            GameObject destroyEffect = new GameObject("DestroyEffect");
            destroyEffect.transform.position = transform.position;
            
            // 간단한 파괴 효과 (실제로는 파티클 시스템 사용 권장)
            Light light = destroyEffect.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Color.red;
            light.intensity = 5f;
            light.range = 3f;
            
            Destroy(destroyEffect, 1f);
            Destroy(gameObject);
        }
        
        // Getter 메서드들
        public ARObjectType GetObjectType() => objectType;
        public bool IsInteractable() => isInteractable;
        public bool IsHighlighted() => isHighlighted;
        public ARObjectData GetObjectData() => objectData;
        public int GetHealth() => objectData.health;
        public int GetMaxHealth() => objectData.maxHealth;
        public bool IsDestroyed() => objectData.isDestroyed;
        
        private void OnDrawGizmosSelected()
        {
            // 상호작용 범위 시각화
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
