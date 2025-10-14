using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace NowHere.UI
{
    /// <summary>
    /// 가상 조이스틱 컴포넌트
    /// 모바일 터치 입력을 위한 가상 조이스틱
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Joystick Settings")]
        [SerializeField] private RectTransform joystickBackground;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private float joystickRange = 50f;
        [SerializeField] private bool returnToCenter = true;
        [SerializeField] private float returnSpeed = 5f;
        
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color pressedColor = Color.gray;
        [SerializeField] private bool enableVisualFeedback = true;
        [SerializeField] private float scaleMultiplier = 1.1f;
        
        [Header("Input Settings")]
        [SerializeField] private bool enableHapticFeedback = true;
        [SerializeField] private bool enableSoundFeedback = true;
        [SerializeField] private AudioClip joystickSound;
        
        // 조이스틱 상태
        private bool isPressed = false;
        private Vector2 inputVector = Vector2.zero;
        private Vector2 joystickCenter = Vector2.zero;
        private Vector2 joystickPosition = Vector2.zero;
        
        // 참조
        private Image backgroundImage;
        private Image handleImage;
        private AudioSource audioSource;
        
        // 이벤트
        public event Action<Vector2> OnJoystickMoved;
        public event Action OnJoystickPressed;
        public event Action OnJoystickReleased;
        
        private void Start()
        {
            InitializeJoystick();
        }
        
        private void Update()
        {
            if (returnToCenter && !isPressed)
            {
                ReturnToCenter();
            }
        }
        
        private void InitializeJoystick()
        {
            // 컴포넌트 참조
            if (joystickBackground != null)
            {
                backgroundImage = joystickBackground.GetComponent<Image>();
                joystickCenter = joystickBackground.anchoredPosition;
            }
            
            if (joystickHandle != null)
            {
                handleImage = joystickHandle.GetComponent<Image>();
            }
            
            // 오디오 소스
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // 초기 위치 설정
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = joystickCenter;
            }
            
            Debug.Log("Virtual Joystick 초기화 완료");
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            
            // 조이스틱 위치 업데이트
            UpdateJoystickPosition(eventData.position);
            
            // 시각적 피드백
            if (enableVisualFeedback)
            {
                SetJoystickPressed(true);
            }
            
            // 햅틱 피드백
            if (enableHapticFeedback)
            {
                Handheld.Vibrate();
            }
            
            // 사운드 피드백
            if (enableSoundFeedback && audioSource != null && joystickSound != null)
            {
                audioSource.PlayOneShot(joystickSound);
            }
            
            OnJoystickPressed?.Invoke();
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            
            // 입력 벡터 리셋
            inputVector = Vector2.zero;
            
            // 시각적 피드백
            if (enableVisualFeedback)
            {
                SetJoystickPressed(false);
            }
            
            // 이벤트 발생
            OnJoystickMoved?.Invoke(inputVector);
            OnJoystickReleased?.Invoke();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isPressed) return;
            
            // 조이스틱 위치 업데이트
            UpdateJoystickPosition(eventData.position);
        }
        
        private void UpdateJoystickPosition(Vector2 screenPosition)
        {
            if (joystickBackground == null || joystickHandle == null) return;
            
            // 스크린 좌표를 로컬 좌표로 변환
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickBackground, screenPosition, null, out localPosition);
            
            // 조이스틱 범위 내로 제한
            float distance = Vector2.Distance(Vector2.zero, localPosition);
            if (distance > joystickRange)
            {
                localPosition = localPosition.normalized * joystickRange;
            }
            
            // 핸들 위치 업데이트
            joystickHandle.anchoredPosition = localPosition;
            
            // 입력 벡터 계산
            inputVector = localPosition / joystickRange;
            
            // 이벤트 발생
            OnJoystickMoved?.Invoke(inputVector);
        }
        
        private void ReturnToCenter()
        {
            if (joystickHandle == null) return;
            
            // 핸들을 중심으로 부드럽게 이동
            Vector2 currentPosition = joystickHandle.anchoredPosition;
            Vector2 targetPosition = joystickCenter;
            
            Vector2 newPosition = Vector2.Lerp(currentPosition, targetPosition, returnSpeed * Time.deltaTime);
            joystickHandle.anchoredPosition = newPosition;
            
            // 입력 벡터 업데이트
            inputVector = Vector2.Lerp(inputVector, Vector2.zero, returnSpeed * Time.deltaTime);
            
            // 거리가 충분히 가까우면 완전히 리셋
            if (Vector2.Distance(newPosition, targetPosition) < 0.1f)
            {
                joystickHandle.anchoredPosition = targetPosition;
                inputVector = Vector2.zero;
            }
        }
        
        private void SetJoystickPressed(bool pressed)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = pressed ? pressedColor : normalColor;
            }
            
            if (handleImage != null)
            {
                handleImage.color = pressed ? pressedColor : normalColor;
            }
            
            // 스케일 효과
            if (pressed)
            {
                transform.localScale = Vector3.one * scaleMultiplier;
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }
        
        // 공개 메서드들
        public Vector2 GetInputVector()
        {
            return inputVector;
        }
        
        public bool IsPressed()
        {
            return isPressed;
        }
        
        public void SetJoystickRange(float range)
        {
            joystickRange = range;
        }
        
        public void SetReturnToCenter(bool returnToCenter)
        {
            this.returnToCenter = returnToCenter;
        }
        
        public void SetReturnSpeed(float speed)
        {
            returnSpeed = speed;
        }
        
        public void SetVisualFeedback(bool enabled)
        {
            enableVisualFeedback = enabled;
        }
        
        public void SetHapticFeedback(bool enabled)
        {
            enableHapticFeedback = enabled;
        }
        
        public void SetSoundFeedback(bool enabled)
        {
            enableSoundFeedback = enabled;
        }
        
        public void SetJoystickSound(AudioClip sound)
        {
            joystickSound = sound;
        }
        
        public void ResetJoystick()
        {
            isPressed = false;
            inputVector = Vector2.zero;
            
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = joystickCenter;
            }
            
            SetJoystickPressed(false);
        }
    }
}
