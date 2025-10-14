using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using NowHere.Game;
using NowHere.XR;

namespace NowHere.UI
{
    /// <summary>
    /// 통합 UI 관리 시스템
    /// 모바일, VR, AR, MR 모든 플랫폼의 UI를 통합 관리
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Canvas References")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private Canvas mobileCanvas;
        [SerializeField] private Canvas vrCanvas;
        [SerializeField] private Canvas arCanvas;
        
        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameHUDPanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject notificationPanel;
        
        [Header("Mobile UI Components")]
        [SerializeField] private GameObject mobileJoystick;
        [SerializeField] private GameObject mobileButtons;
        [SerializeField] private GameObject mobileMenu;
        [SerializeField] private GameObject mobileInventory;
        
        [Header("XR UI Components")]
        [SerializeField] private GameObject vrMenu;
        [SerializeField] private GameObject arMenu;
        [SerializeField] private GameObject xrHUD;
        [SerializeField] private GameObject xrInventory;
        
        [Header("UI Settings")]
        [SerializeField] private bool enableMobileUI = true;
        [SerializeField] private bool enableXRUI = true;
        [SerializeField] private bool enableAdaptiveUI = true;
        [SerializeField] private float uiAnimationSpeed = 0.3f;
        [SerializeField] private bool enableUIAnimations = true;
        
        [Header("UI Themes")]
        [SerializeField] private Color primaryColor = Color.blue;
        [SerializeField] private Color secondaryColor = Color.white;
        [SerializeField] private Color accentColor = Color.yellow;
        [SerializeField] private Color backgroundColor = Color.black;
        
        // UI 상태 관리
        private UIState currentUIState = UIState.MainMenu;
        private PlatformType currentPlatform = PlatformType.Mobile;
        private bool isUIVisible = true;
        private bool isAnimating = false;
        
        // UI 패널 관리
        private Dictionary<string, GameObject> uiPanels = new Dictionary<string, GameObject>();
        private Dictionary<string, UIElement> uiElements = new Dictionary<string, UIElement>();
        private Stack<GameObject> panelStack = new Stack<GameObject>();
        
        // 참조
        private GameManager gameManager;
        private XRGameManager xrGameManager;
        private Camera mainCamera;
        
        // 이벤트
        public System.Action<UIState> OnUIStateChanged;
        public System.Action<PlatformType> OnPlatformChanged;
        public System.Action<string> OnUIPanelOpened;
        public System.Action<string> OnUIPanelClosed;
        public System.Action<bool> OnUIVisibilityChanged;
        
        // 싱글톤 패턴
        public static UIManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            InitializeUIManager();
        }
        
        private void Update()
        {
            UpdateUI();
            HandleUIInput();
        }
        
        private void InitializeUIManager()
        {
            Debug.Log("UI Manager 초기화 시작...");
            
            // 컴포넌트 참조
            gameManager = FindObjectOfType<GameManager>();
            xrGameManager = FindObjectOfType<XRGameManager>();
            mainCamera = Camera.main;
            
            // UI 패널 등록
            RegisterUIPanels();
            
            // 플랫폼 감지
            DetectPlatform();
            
            // UI 초기 설정
            SetupInitialUI();
            
            // 이벤트 구독
            SubscribeToEvents();
            
            Debug.Log("UI Manager 초기화 완료");
        }
        
        private void RegisterUIPanels()
        {
            // UI 패널들을 딕셔너리에 등록
            if (mainMenuPanel != null) uiPanels["MainMenu"] = mainMenuPanel;
            if (gameHUDPanel != null) uiPanels["GameHUD"] = gameHUDPanel;
            if (inventoryPanel != null) uiPanels["Inventory"] = inventoryPanel;
            if (settingsPanel != null) uiPanels["Settings"] = settingsPanel;
            if (pauseMenuPanel != null) uiPanels["PauseMenu"] = pauseMenuPanel;
            if (loadingPanel != null) uiPanels["Loading"] = loadingPanel;
            if (notificationPanel != null) uiPanels["Notification"] = notificationPanel;
            
            // 모든 패널 초기에는 비활성화
            foreach (var panel in uiPanels.Values)
            {
                if (panel != null)
                {
                    panel.SetActive(false);
                }
            }
        }
        
        private void DetectPlatform()
        {
            // 플랫폼 감지 로직
            if (xrGameManager != null && xrGameManager.IsXRGameInitialized())
            {
                currentPlatform = PlatformType.XR;
            }
            else if (Application.isMobilePlatform)
            {
                currentPlatform = PlatformType.Mobile;
            }
            else
            {
                currentPlatform = PlatformType.Desktop;
            }
            
            Debug.Log($"플랫폼 감지: {currentPlatform}");
        }
        
        private void SetupInitialUI()
        {
            // 플랫폼별 UI 설정
            switch (currentPlatform)
            {
                case PlatformType.Mobile:
                    SetupMobileUI();
                    break;
                case PlatformType.XR:
                    SetupXRUI();
                    break;
                case PlatformType.Desktop:
                    SetupDesktopUI();
                    break;
            }
            
            // 초기 UI 상태 설정
            SetUIState(UIState.MainMenu);
        }
        
        private void SetupMobileUI()
        {
            if (!enableMobileUI) return;
            
            // 모바일 UI 활성화
            if (mobileCanvas != null) mobileCanvas.gameObject.SetActive(true);
            if (mobileJoystick != null) mobileJoystick.SetActive(true);
            if (mobileButtons != null) mobileButtons.SetActive(true);
            
            // VR/AR UI 비활성화
            if (vrCanvas != null) vrCanvas.gameObject.SetActive(false);
            if (arCanvas != null) arCanvas.gameObject.SetActive(false);
            
            Debug.Log("모바일 UI 설정 완료");
        }
        
        private void SetupXRUI()
        {
            if (!enableXRUI) return;
            
            // XR UI 활성화
            if (xrGameManager != null)
            {
                var xrMode = xrGameManager.GetCurrentXRMode();
                switch (xrMode)
                {
                    case XRMode.VR:
                        if (vrCanvas != null) vrCanvas.gameObject.SetActive(true);
                        if (vrMenu != null) vrMenu.SetActive(true);
                        break;
                    case XRMode.AR:
                        if (arCanvas != null) arCanvas.gameObject.SetActive(true);
                        if (arMenu != null) arMenu.SetActive(true);
                        break;
                    case XRMode.MR:
                        if (vrCanvas != null) vrCanvas.gameObject.SetActive(true);
                        if (arCanvas != null) arCanvas.gameObject.SetActive(true);
                        break;
                }
            }
            
            // 모바일 UI 비활성화
            if (mobileCanvas != null) mobileCanvas.gameObject.SetActive(false);
            
            Debug.Log("XR UI 설정 완료");
        }
        
        private void SetupDesktopUI()
        {
            // 데스크톱 UI 설정
            if (mainCanvas != null) mainCanvas.gameObject.SetActive(true);
            
            Debug.Log("데스크톱 UI 설정 완료");
        }
        
        private void SubscribeToEvents()
        {
            // 게임 매니저 이벤트 구독
            if (gameManager != null)
            {
                // 게임 상태 변경 이벤트 구독
            }
            
            // XR 게임 매니저 이벤트 구독
            if (xrGameManager != null)
            {
                xrGameManager.OnXRModeSwitched += OnXRModeSwitched;
                xrGameManager.OnGameStateChanged += OnXRGameStateChanged;
            }
        }
        
        private void UpdateUI()
        {
            // UI 업데이트 로직
            if (enableAdaptiveUI)
            {
                UpdateAdaptiveUI();
            }
            
            // UI 애니메이션 업데이트
            if (enableUIAnimations)
            {
                UpdateUIAnimations();
            }
        }
        
        private void UpdateAdaptiveUI()
        {
            // 적응형 UI 업데이트
            // 화면 크기, 해상도, 플랫폼에 따른 UI 조정
        }
        
        private void UpdateUIAnimations()
        {
            // UI 애니메이션 업데이트
        }
        
        private void HandleUIInput()
        {
            // UI 입력 처리
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleBackButton();
            }
        }
        
        private void HandleBackButton()
        {
            // 뒤로가기 버튼 처리
            if (panelStack.Count > 0)
            {
                CloseCurrentPanel();
            }
            else
            {
                // 메인 메뉴로 돌아가기
                SetUIState(UIState.MainMenu);
            }
        }
        
        public void SetUIState(UIState state)
        {
            if (currentUIState == state || isAnimating) return;
            
            StartCoroutine(TransitionToUIState(state));
        }
        
        private IEnumerator TransitionToUIState(UIState newState)
        {
            isAnimating = true;
            
            // 현재 UI 상태 종료
            yield return StartCoroutine(ExitCurrentUIState());
            
            // 새 UI 상태 시작
            yield return StartCoroutine(EnterNewUIState(newState));
            
            currentUIState = newState;
            OnUIStateChanged?.Invoke(newState);
            
            isAnimating = false;
        }
        
        private IEnumerator ExitCurrentUIState()
        {
            // 현재 UI 상태 종료 애니메이션
            if (enableUIAnimations)
            {
                // 페이드 아웃 애니메이션
                yield return new WaitForSeconds(uiAnimationSpeed);
            }
        }
        
        private IEnumerator EnterNewUIState(UIState newState)
        {
            // 새 UI 상태 시작
            switch (newState)
            {
                case UIState.MainMenu:
                    ShowPanel("MainMenu");
                    break;
                case UIState.GameHUD:
                    ShowPanel("GameHUD");
                    break;
                case UIState.Inventory:
                    ShowPanel("Inventory");
                    break;
                case UIState.Settings:
                    ShowPanel("Settings");
                    break;
                case UIState.PauseMenu:
                    ShowPanel("PauseMenu");
                    break;
                case UIState.Loading:
                    ShowPanel("Loading");
                    break;
            }
            
            // 페이드 인 애니메이션
            if (enableUIAnimations)
            {
                yield return new WaitForSeconds(uiAnimationSpeed);
            }
        }
        
        public void ShowPanel(string panelName)
        {
            if (uiPanels.ContainsKey(panelName))
            {
                GameObject panel = uiPanels[panelName];
                if (panel != null)
                {
                    panel.SetActive(true);
                    panelStack.Push(panel);
                    OnUIPanelOpened?.Invoke(panelName);
                    
                    Debug.Log($"UI 패널 열림: {panelName}");
                }
            }
        }
        
        public void HidePanel(string panelName)
        {
            if (uiPanels.ContainsKey(panelName))
            {
                GameObject panel = uiPanels[panelName];
                if (panel != null)
                {
                    panel.SetActive(false);
                    OnUIPanelClosed?.Invoke(panelName);
                    
                    Debug.Log($"UI 패널 닫힘: {panelName}");
                }
            }
        }
        
        public void CloseCurrentPanel()
        {
            if (panelStack.Count > 0)
            {
                GameObject currentPanel = panelStack.Pop();
                if (currentPanel != null)
                {
                    currentPanel.SetActive(false);
                    OnUIPanelClosed?.Invoke(currentPanel.name);
                }
            }
        }
        
        public void ShowNotification(string message, float duration = 3f)
        {
            if (notificationPanel != null)
            {
                ShowPanel("Notification");
                
                // 알림 메시지 설정
                Text notificationText = notificationPanel.GetComponentInChildren<Text>();
                if (notificationText != null)
                {
                    notificationText.text = message;
                }
                
                // 자동 닫기
                StartCoroutine(AutoHideNotification(duration));
            }
        }
        
        private IEnumerator AutoHideNotification(float duration)
        {
            yield return new WaitForSeconds(duration);
            HidePanel("Notification");
        }
        
        public void SetUIVisibility(bool visible)
        {
            isUIVisible = visible;
            
            // 모든 UI 캔버스 가시성 설정
            if (mainCanvas != null) mainCanvas.gameObject.SetActive(visible);
            if (mobileCanvas != null) mobileCanvas.gameObject.SetActive(visible && currentPlatform == PlatformType.Mobile);
            if (vrCanvas != null) vrCanvas.gameObject.SetActive(visible && currentPlatform == PlatformType.XR);
            if (arCanvas != null) arCanvas.gameObject.SetActive(visible && currentPlatform == PlatformType.XR);
            
            OnUIVisibilityChanged?.Invoke(visible);
        }
        
        public void SetPlatform(PlatformType platform)
        {
            if (currentPlatform == platform) return;
            
            currentPlatform = platform;
            
            // 플랫폼별 UI 재설정
            SetupInitialUI();
            
            OnPlatformChanged?.Invoke(platform);
        }
        
        public void SetUITheme(Color primary, Color secondary, Color accent, Color background)
        {
            primaryColor = primary;
            secondaryColor = secondary;
            accentColor = accent;
            backgroundColor = background;
            
            // UI 테마 적용
            ApplyUITheme();
        }
        
        private void ApplyUITheme()
        {
            // UI 테마 적용 로직
            // 모든 UI 요소의 색상을 새 테마로 변경
        }
        
        // 이벤트 핸들러들
        private void OnXRModeSwitched(XRMode mode)
        {
            // XR 모드 변경 시 UI 재설정
            SetupXRUI();
        }
        
        private void OnXRGameStateChanged(XRGameState state)
        {
            // XR 게임 상태 변경 시 UI 상태 변경
            switch (state)
            {
                case XRGameState.MainMenu:
                    SetUIState(UIState.MainMenu);
                    break;
                case XRGameState.Playing:
                    SetUIState(UIState.GameHUD);
                    break;
                case XRGameState.Paused:
                    SetUIState(UIState.PauseMenu);
                    break;
            }
        }
        
        // 공개 메서드들
        public UIState GetCurrentUIState()
        {
            return currentUIState;
        }
        
        public PlatformType GetCurrentPlatform()
        {
            return currentPlatform;
        }
        
        public bool IsUIVisible()
        {
            return isUIVisible;
        }
        
        public bool IsAnimating()
        {
            return isAnimating;
        }
        
        public GameObject GetUIPanel(string panelName)
        {
            return uiPanels.ContainsKey(panelName) ? uiPanels[panelName] : null;
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (xrGameManager != null)
            {
                xrGameManager.OnXRModeSwitched -= OnXRModeSwitched;
                xrGameManager.OnGameStateChanged -= OnXRGameStateChanged;
            }
        }
    }
    
    // UI 데이터 구조체들
    [System.Serializable]
    public struct UIElement
    {
        public string elementName;
        public GameObject gameObject;
        public UIElementType elementType;
        public bool isInteractable;
        public bool isVisible;
    }
    
    public enum UIState
    {
        MainMenu,
        GameHUD,
        Inventory,
        Settings,
        PauseMenu,
        Loading,
        Notification
    }
    
    public enum PlatformType
    {
        Mobile,
        Desktop,
        XR
    }
    
    public enum UIElementType
    {
        Panel,
        Button,
        Text,
        Image,
        Slider,
        Toggle,
        InputField,
        Dropdown
    }
}