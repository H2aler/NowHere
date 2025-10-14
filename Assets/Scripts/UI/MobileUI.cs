using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using NowHere.UI;
using NowHere.Game;

namespace NowHere.UI
{
    /// <summary>
    /// 모바일 최적화 UI 시스템
    /// 터치 인터페이스와 모바일 환경에 최적화된 UI
    /// </summary>
    public class MobileUI : MonoBehaviour
    {
        [Header("Mobile UI Components")]
        [SerializeField] private GameObject mobileCanvas;
        [SerializeField] private GameObject joystickPanel;
        [SerializeField] private GameObject buttonPanel;
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject chatPanel;
        
        [Header("Virtual Joystick")]
        [SerializeField] private VirtualJoystick movementJoystick;
        [SerializeField] private VirtualJoystick cameraJoystick;
        [SerializeField] private float joystickSensitivity = 1f;
        [SerializeField] private bool enableJoystickHaptic = true;
        
        [Header("Mobile Buttons")]
        [SerializeField] private Button attackButton;
        [SerializeField] private Button jumpButton;
        [SerializeField] private Button interactButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button chatButton;
        
        [Header("Mobile Menu")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private GameObject inventoryMenu;
        [SerializeField] private GameObject chatMenu;
        [SerializeField] private GameObject pauseMenu;
        
        [Header("Mobile HUD")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider manaBar;
        [SerializeField] private Text levelText;
        [SerializeField] private Text goldText;
        [SerializeField] private Text experienceText;
        [SerializeField] private Image minimap;
        
        [Header("Mobile Settings")]
        [SerializeField] private bool enableMobileUI = true;
        [SerializeField] private bool enableVirtualJoystick = true;
        [SerializeField] private bool enableMobileButtons = true;
        [SerializeField] private bool enableMobileHUD = true;
        [SerializeField] private bool enableMobileMenu = true;
        
        [Header("Touch Settings")]
        [SerializeField] private float touchSensitivity = 1f;
        [SerializeField] private float doubleTapTime = 0.3f;
        [SerializeField] private float longPressTime = 1f;
        [SerializeField] private bool enableTouchFeedback = true;
        
        // 모바일 UI 상태
        private bool isMobileUIInitialized = false;
        private bool isMenuOpen = false;
        private bool isInventoryOpen = false;
        private bool isChatOpen = false;
        private bool isPaused = false;
        
        // 터치 입력 관리
        private Dictionary<int, TouchData> activeTouches = new Dictionary<int, TouchData>();
        private float lastTapTime = 0f;
        private Vector2 lastTapPosition = Vector2.zero;
        
        // 참조
        private UIManager uiManager;
        private GameManager gameManager;
        private PlayerController playerController;
        
        // 이벤트
        public System.Action<Vector2> OnJoystickInput;
        public System.Action<Vector2> OnCameraInput;
        public System.Action<string> OnButtonPressed;
        public System.Action<bool> OnMenuToggled;
        public System.Action<bool> OnInventoryToggled;
        public System.Action<bool> OnChatToggled;
        public System.Action<bool> OnPauseToggled;
        
        private void Start()
        {
            InitializeMobileUI();
        }
        
        private void Update()
        {
            if (!isMobileUIInitialized) return;
            
            HandleTouchInput();
            UpdateMobileHUD();
        }
        
        private void InitializeMobileUI()
        {
            Debug.Log("Mobile UI 초기화 시작...");
            
            // 컴포넌트 참조
            uiManager = FindObjectOfType<UIManager>();
            gameManager = FindObjectOfType<GameManager>();
            playerController = FindObjectOfType<PlayerController>();
            
            // 모바일 UI 설정
            SetupMobileUI();
            
            // 버튼 이벤트 설정
            SetupButtonEvents();
            
            // 조이스틱 설정
            SetupJoysticks();
            
            // HUD 설정
            SetupMobileHUD();
            
            isMobileUIInitialized = true;
            Debug.Log("Mobile UI 초기화 완료");
        }
        
        private void SetupMobileUI()
        {
            if (!enableMobileUI) return;
            
            // 모바일 캔버스 활성화
            if (mobileCanvas != null)
            {
                mobileCanvas.SetActive(true);
            }
            
            // 각 패널 설정
            if (joystickPanel != null && enableVirtualJoystick)
            {
                joystickPanel.SetActive(true);
            }
            
            if (buttonPanel != null && enableMobileButtons)
            {
                buttonPanel.SetActive(true);
            }
            
            if (menuPanel != null && enableMobileMenu)
            {
                menuPanel.SetActive(false); // 초기에는 비활성화
            }
            
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false); // 초기에는 비활성화
            }
            
            if (chatPanel != null)
            {
                chatPanel.SetActive(false); // 초기에는 비활성화
            }
        }
        
        private void SetupButtonEvents()
        {
            // 공격 버튼
            if (attackButton != null)
            {
                attackButton.onClick.AddListener(() => OnButtonPressed?.Invoke("Attack"));
            }
            
            // 점프 버튼
            if (jumpButton != null)
            {
                jumpButton.onClick.AddListener(() => OnButtonPressed?.Invoke("Jump"));
            }
            
            // 상호작용 버튼
            if (interactButton != null)
            {
                interactButton.onClick.AddListener(() => OnButtonPressed?.Invoke("Interact"));
            }
            
            // 메뉴 버튼
            if (menuButton != null)
            {
                menuButton.onClick.AddListener(ToggleMenu);
            }
            
            // 인벤토리 버튼
            if (inventoryButton != null)
            {
                inventoryButton.onClick.AddListener(ToggleInventory);
            }
            
            // 채팅 버튼
            if (chatButton != null)
            {
                chatButton.onClick.AddListener(ToggleChat);
            }
        }
        
        private void SetupJoysticks()
        {
            if (!enableVirtualJoystick) return;
            
            // 이동 조이스틱
            if (movementJoystick != null)
            {
                movementJoystick.OnJoystickMoved += OnMovementJoystickMoved;
            }
            
            // 카메라 조이스틱
            if (cameraJoystick != null)
            {
                cameraJoystick.OnJoystickMoved += OnCameraJoystickMoved;
            }
        }
        
        private void SetupMobileHUD()
        {
            if (!enableMobileHUD) return;
            
            // HUD 초기 설정
            UpdateHealthBar(100f, 100f);
            UpdateManaBar(100f, 100f);
            UpdateLevel(1);
            UpdateGold(0);
            UpdateExperience(0, 100);
        }
        
        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    ProcessTouch(touch);
                }
            }
            else
            {
                // 마우스 입력 처리 (에디터에서 테스트용)
                ProcessMouseInput();
            }
        }
        
        private void ProcessTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnTouchBegan(touch);
                    break;
                case TouchPhase.Moved:
                    OnTouchMoved(touch);
                    break;
                case TouchPhase.Ended:
                    OnTouchEnded(touch);
                    break;
                case TouchPhase.Canceled:
                    OnTouchCanceled(touch);
                    break;
            }
        }
        
        private void OnTouchBegan(Touch touch)
        {
            TouchData touchData = new TouchData
            {
                fingerId = touch.fingerId,
                startPosition = touch.position,
                currentPosition = touch.position,
                startTime = Time.time,
                isLongPress = false
            };
            
            activeTouches[touch.fingerId] = touchData;
            
            // 햅틱 피드백
            if (enableTouchFeedback)
            {
                Handheld.Vibrate();
            }
        }
        
        private void OnTouchMoved(Touch touch)
        {
            if (activeTouches.ContainsKey(touch.fingerId))
            {
                TouchData touchData = activeTouches[touch.fingerId];
                touchData.currentPosition = touch.position;
                touchData.deltaPosition = touch.deltaPosition;
                
                // 스와이프 감지
                if (touchData.deltaPosition.magnitude > 10f)
                {
                    ProcessSwipe(touchData);
                }
            }
        }
        
        private void OnTouchEnded(Touch touch)
        {
            if (activeTouches.ContainsKey(touch.fingerId))
            {
                TouchData touchData = activeTouches[touch.fingerId];
                float touchDuration = Time.time - touchData.startTime;
                
                // 탭 감지
                if (touchDuration < doubleTapTime)
                {
                    ProcessTap(touchData);
                }
                
                // 롱 프레스 감지
                if (touchDuration >= longPressTime)
                {
                    ProcessLongPress(touchData);
                }
                
                activeTouches.Remove(touch.fingerId);
            }
        }
        
        private void OnTouchCanceled(Touch touch)
        {
            if (activeTouches.ContainsKey(touch.fingerId))
            {
                activeTouches.Remove(touch.fingerId);
            }
        }
        
        private void ProcessMouseInput()
        {
            // 마우스 입력 처리 (에디터 테스트용)
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Input.mousePosition;
                ProcessTap(new TouchData
                {
                    startPosition = mousePosition,
                    currentPosition = mousePosition,
                    startTime = Time.time
                });
            }
        }
        
        private void ProcessTap(TouchData touchData)
        {
            // 더블 탭 감지
            if (Time.time - lastTapTime < doubleTapTime &&
                Vector2.Distance(touchData.startPosition, lastTapPosition) < 50f)
            {
                ProcessDoubleTap(touchData);
            }
            else
            {
                ProcessSingleTap(touchData);
            }
            
            lastTapTime = Time.time;
            lastTapPosition = touchData.startPosition;
        }
        
        private void ProcessSingleTap(TouchData touchData)
        {
            // 단일 탭 처리
            Debug.Log($"단일 탭: {touchData.startPosition}");
        }
        
        private void ProcessDoubleTap(TouchData touchData)
        {
            // 더블 탭 처리
            Debug.Log($"더블 탭: {touchData.startPosition}");
        }
        
        private void ProcessLongPress(TouchData touchData)
        {
            // 롱 프레스 처리
            Debug.Log($"롱 프레스: {touchData.startPosition}");
        }
        
        private void ProcessSwipe(TouchData touchData)
        {
            // 스와이프 처리
            Vector2 swipeDirection = touchData.deltaPosition.normalized;
            float swipeMagnitude = touchData.deltaPosition.magnitude;
            
            if (swipeMagnitude > 50f)
            {
                if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
                {
                    // 수평 스와이프
                    if (swipeDirection.x > 0)
                    {
                        Debug.Log("오른쪽 스와이프");
                    }
                    else
                    {
                        Debug.Log("왼쪽 스와이프");
                    }
                }
                else
                {
                    // 수직 스와이프
                    if (swipeDirection.y > 0)
                    {
                        Debug.Log("위쪽 스와이프");
                    }
                    else
                    {
                        Debug.Log("아래쪽 스와이프");
                    }
                }
            }
        }
        
        private void UpdateMobileHUD()
        {
            if (!enableMobileHUD) return;
            
            // HUD 업데이트 로직
            // 실제 구현에서는 게임 데이터에서 정보를 가져와서 업데이트
        }
        
        // 조이스틱 이벤트 핸들러들
        private void OnMovementJoystickMoved(Vector2 input)
        {
            Vector2 scaledInput = input * joystickSensitivity;
            OnJoystickInput?.Invoke(scaledInput);
            
            // 플레이어 이동
            if (playerController != null)
            {
                playerController.SetMovementInput(scaledInput);
            }
        }
        
        private void OnCameraJoystickMoved(Vector2 input)
        {
            Vector2 scaledInput = input * joystickSensitivity;
            OnCameraInput?.Invoke(scaledInput);
            
            // 카메라 회전
            if (playerController != null)
            {
                playerController.SetCameraInput(scaledInput);
            }
        }
        
        // UI 토글 메서드들
        public void ToggleMenu()
        {
            isMenuOpen = !isMenuOpen;
            
            if (menuPanel != null)
            {
                menuPanel.SetActive(isMenuOpen);
            }
            
            OnMenuToggled?.Invoke(isMenuOpen);
            Debug.Log($"메뉴 {(isMenuOpen ? "열림" : "닫힘")}");
        }
        
        public void ToggleInventory()
        {
            isInventoryOpen = !isInventoryOpen;
            
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(isInventoryOpen);
            }
            
            OnInventoryToggled?.Invoke(isInventoryOpen);
            Debug.Log($"인벤토리 {(isInventoryOpen ? "열림" : "닫힘")}");
        }
        
        public void ToggleChat()
        {
            isChatOpen = !isChatOpen;
            
            if (chatPanel != null)
            {
                chatPanel.SetActive(isChatOpen);
            }
            
            OnChatToggled?.Invoke(isChatOpen);
            Debug.Log($"채팅 {(isChatOpen ? "열림" : "닫힘")}");
        }
        
        public void TogglePause()
        {
            isPaused = !isPaused;
            
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(isPaused);
            }
            
            // 게임 일시정지
            Time.timeScale = isPaused ? 0f : 1f;
            
            OnPauseToggled?.Invoke(isPaused);
            Debug.Log($"게임 {(isPaused ? "일시정지" : "재개")}");
        }
        
        // HUD 업데이트 메서드들
        public void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            if (healthBar != null)
            {
                healthBar.value = currentHealth / maxHealth;
            }
        }
        
        public void UpdateManaBar(float currentMana, float maxMana)
        {
            if (manaBar != null)
            {
                manaBar.value = currentMana / maxMana;
            }
        }
        
        public void UpdateLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Lv.{level}";
            }
        }
        
        public void UpdateGold(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"{gold:N0}";
            }
        }
        
        public void UpdateExperience(int currentExp, int maxExp)
        {
            if (experienceText != null)
            {
                experienceText.text = $"{currentExp}/{maxExp}";
            }
        }
        
        // 공개 메서드들
        public bool IsMobileUIInitialized()
        {
            return isMobileUIInitialized;
        }
        
        public bool IsMenuOpen()
        {
            return isMenuOpen;
        }
        
        public bool IsInventoryOpen()
        {
            return isInventoryOpen;
        }
        
        public bool IsChatOpen()
        {
            return isChatOpen;
        }
        
        public bool IsPaused()
        {
            return isPaused;
        }
        
        public void SetMobileUIEnabled(bool enabled)
        {
            enableMobileUI = enabled;
            if (mobileCanvas != null)
            {
                mobileCanvas.SetActive(enabled);
            }
        }
        
        public void SetVirtualJoystickEnabled(bool enabled)
        {
            enableVirtualJoystick = enabled;
            if (joystickPanel != null)
            {
                joystickPanel.SetActive(enabled);
            }
        }
        
        public void SetMobileButtonsEnabled(bool enabled)
        {
            enableMobileButtons = enabled;
            if (buttonPanel != null)
            {
                buttonPanel.SetActive(enabled);
            }
        }
        
        public void SetMobileHUDEnabled(bool enabled)
        {
            enableMobileHUD = enabled;
            // HUD 요소들 활성화/비활성화
        }
    }
    
    // 터치 데이터 구조체
    [System.Serializable]
    public struct TouchData
    {
        public int fingerId;
        public Vector2 startPosition;
        public Vector2 currentPosition;
        public Vector2 deltaPosition;
        public float startTime;
        public bool isLongPress;
    }
}
