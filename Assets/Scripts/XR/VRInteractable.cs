using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace NowHere.XR
{
    /// <summary>
    /// VR에서 상호작용 가능한 오브젝트를 위한 기본 클래스
    /// 모든 VR 상호작용 오브젝트는 이 클래스를 상속받아야 함
    /// </summary>
    public class VRInteractable : MonoBehaviour
    {
        [Header("VR Interaction Settings")]
        [SerializeField] private bool isGrabbable = true;
        [SerializeField] private bool isPointable = true;
        [SerializeField] private bool isTeleportable = false;
        [SerializeField] private float interactionDistance = 0.1f;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject highlightEffect;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private float highlightIntensity = 1.5f;
        
        [Header("Audio Feedback")]
        [SerializeField] private AudioClip grabSound;
        [SerializeField] private AudioClip releaseSound;
        [SerializeField] private AudioClip pointSound;
        [SerializeField] private AudioClip unpointSound;
        
        [Header("Haptic Feedback")]
        [SerializeField] private bool enableHapticFeedback = true;
        [SerializeField] private float hapticIntensity = 0.5f;
        [SerializeField] private float hapticDuration = 0.1f;
        
        // 상호작용 상태
        private bool isGrabbed = false;
        private bool isPointed = false;
        private bool isHighlighted = false;
        
        // 원본 머티리얼 저장
        private Material originalMaterial;
        private Renderer objectRenderer;
        
        // 오디오 소스
        private AudioSource audioSource;
        
        // 이벤트
        public System.Action<Vector3, Quaternion, bool> OnGrabbedEvent;
        public System.Action<Vector3, Quaternion, bool> OnReleasedEvent;
        public System.Action<bool> OnPointedEvent;
        public System.Action<bool> OnUnpointedEvent;
        public System.Action OnHighlightedEvent;
        public System.Action OnUnhighlightedEvent;
        
        private void Start()
        {
            InitializeVRInteractable();
        }
        
        private void InitializeVRInteractable()
        {
            // 렌더러 및 머티리얼 초기화
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer != null && objectRenderer.material != null)
            {
                originalMaterial = objectRenderer.material;
            }
            
            // 오디오 소스 초기화
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // XR Grab Interactable 컴포넌트 추가 (필요한 경우)
            if (isGrabbable && GetComponent<XRGrabInteractable>() == null)
            {
                XRGrabInteractable grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
                grabInteractable.interactionManager = FindObjectOfType<XRInteractionManager>();
            }
            
            Debug.Log($"VR 상호작용 오브젝트 초기화: {gameObject.name}");
        }
        
        public virtual void OnGrabbed(Vector3 position, Quaternion rotation, bool isLeftHand)
        {
            if (!isGrabbable) return;
            
            isGrabbed = true;
            
            // 시각적 피드백
            SetHighlight(true);
            
            // 오디오 피드백
            PlayAudio(grabSound);
            
            // 햅틱 피드백
            if (enableHapticFeedback)
            {
                TriggerHapticFeedback(isLeftHand);
            }
            
            // 이벤트 발생
            OnGrabbedEvent?.Invoke(position, rotation, isLeftHand);
            
            Debug.Log($"{gameObject.name}이(가) 잡혔습니다. ({(isLeftHand ? "왼손" : "오른손")})");
        }
        
        public virtual void OnReleased(Vector3 position, Quaternion rotation, bool isLeftHand)
        {
            if (!isGrabbable) return;
            
            isGrabbed = false;
            
            // 시각적 피드백
            SetHighlight(false);
            
            // 오디오 피드백
            PlayAudio(releaseSound);
            
            // 이벤트 발생
            OnReleasedEvent?.Invoke(position, rotation, isLeftHand);
            
            Debug.Log($"{gameObject.name}이(가) 놓아졌습니다. ({(isLeftHand ? "왼손" : "오른손")})");
        }
        
        public virtual void OnPointed(bool isLeftHand)
        {
            if (!isPointable) return;
            
            isPointed = true;
            
            // 시각적 피드백
            SetHighlight(true);
            
            // 오디오 피드백
            PlayAudio(pointSound);
            
            // 이벤트 발생
            OnPointedEvent?.Invoke(isLeftHand);
            
            Debug.Log($"{gameObject.name}이(가) 가리켜졌습니다. ({(isLeftHand ? "왼손" : "오른손")})");
        }
        
        public virtual void OnUnpointed(bool isLeftHand)
        {
            if (!isPointable) return;
            
            isPointed = false;
            
            // 시각적 피드백 (잡혀있지 않은 경우에만)
            if (!isGrabbed)
            {
                SetHighlight(false);
            }
            
            // 오디오 피드백
            PlayAudio(unpointSound);
            
            // 이벤트 발생
            OnUnpointedEvent?.Invoke(isLeftHand);
            
            Debug.Log($"{gameObject.name} 포인팅이 해제되었습니다. ({(isLeftHand ? "왼손" : "오른손")})");
        }
        
        public virtual void OnTeleported(Vector3 newPosition)
        {
            if (!isTeleportable) return;
            
            transform.position = newPosition;
            
            Debug.Log($"{gameObject.name}이(가) 텔레포트되었습니다: {newPosition}");
        }
        
        private void SetHighlight(bool highlight)
        {
            if (isHighlighted == highlight) return;
            
            isHighlighted = highlight;
            
            if (objectRenderer != null)
            {
                if (highlight)
                {
                    // 하이라이트 적용
                    if (highlightMaterial != null)
                    {
                        objectRenderer.material = highlightMaterial;
                    }
                    else
                    {
                        // 동적으로 하이라이트 머티리얼 생성
                        Material highlightMat = new Material(originalMaterial);
                        highlightMat.color = highlightColor;
                        highlightMat.SetFloat("_Emission", highlightIntensity);
                        objectRenderer.material = highlightMat;
                    }
                    
                    // 하이라이트 이펙트 활성화
                    if (highlightEffect != null)
                    {
                        highlightEffect.SetActive(true);
                    }
                    
                    OnHighlightedEvent?.Invoke();
                }
                else
                {
                    // 하이라이트 해제
                    objectRenderer.material = originalMaterial;
                    
                    // 하이라이트 이펙트 비활성화
                    if (highlightEffect != null)
                    {
                        highlightEffect.SetActive(false);
                    }
                    
                    OnUnhighlightedEvent?.Invoke();
                }
            }
        }
        
        private void PlayAudio(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        private void TriggerHapticFeedback(bool isLeftHand)
        {
            // 햅틱 피드백 트리거
            // 실제 구현에서는 XR Input System 사용
            Debug.Log($"햅틱 피드백: {gameObject.name}, {(isLeftHand ? "왼손" : "오른손")}, 강도: {hapticIntensity}");
        }
        
        // 공개 메서드들
        public bool IsGrabbable()
        {
            return isGrabbable;
        }
        
        public bool IsPointable()
        {
            return isPointable;
        }
        
        public bool IsTeleportable()
        {
            return isTeleportable;
        }
        
        public bool IsGrabbed()
        {
            return isGrabbed;
        }
        
        public bool IsPointed()
        {
            return isPointed;
        }
        
        public bool IsHighlighted()
        {
            return isHighlighted;
        }
        
        public float GetInteractionDistance()
        {
            return interactionDistance;
        }
        
        public void SetGrabbable(bool grabbable)
        {
            isGrabbable = grabbable;
        }
        
        public void SetPointable(bool pointable)
        {
            isPointable = pointable;
        }
        
        public void SetTeleportable(bool teleportable)
        {
            isTeleportable = teleportable;
        }
        
        public void SetHighlightColor(Color color)
        {
            highlightColor = color;
        }
        
        public void SetHighlightIntensity(float intensity)
        {
            highlightIntensity = intensity;
        }
        
        public void SetHapticIntensity(float intensity)
        {
            hapticIntensity = intensity;
        }
        
        // 거리 기반 상호작용 가능 여부 확인
        public bool CanInteract(Vector3 position)
        {
            float distance = Vector3.Distance(transform.position, position);
            return distance <= interactionDistance;
        }
        
        // 상호작용 우선순위 (더 가까운 오브젝트가 우선)
        public float GetInteractionPriority(Vector3 position)
        {
            float distance = Vector3.Distance(transform.position, position);
            return 1f / (distance + 0.1f); // 거리가 가까울수록 높은 우선순위
        }
        
        private void OnDestroy()
        {
            // 하이라이트 이펙트 정리
            if (highlightEffect != null)
            {
                Destroy(highlightEffect);
            }
        }
    }
}
