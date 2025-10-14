using UnityEngine;
using UnityEngine.SceneManagement;
using NowHere.AR;
using NowHere.Networking;
using NowHere.Player;
using NowHere.RPG;
using NowHere.Sensors;
using NowHere.Interaction;
using NowHere.Audio;
using NowHere.Motion;
using NowHere.Combat;

namespace NowHere.Game
{
    /// <summary>
    /// 게임의 전체적인 상태와 흐름을 관리하는 메인 매니저
    /// AR, 네트워킹, RPG 시스템을 통합하여 관리
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private bool isGamePaused = false;
        [SerializeField] private float gameTime = 0f;
        
        [Header("Game Components")]
        [SerializeField] private ARManager arManager;
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private CharacterSystem characterSystem;
        [SerializeField] private ItemSystem itemSystem;
        
        [Header("Advanced Systems")]
        [SerializeField] private MobileSensorManager sensorManager;
        [SerializeField] private TouchInteractionManager touchManager;
        [SerializeField] private VoiceChatManager voiceManager;
        [SerializeField] private MotionDetectionManager motionManager;
        [SerializeField] private ARObjectManager arObjectManager;
        [SerializeField] private ARCombatSystem combatSystem;
        
        [Header("Game Settings")]
        [SerializeField] private bool enableARMode = true;
        [SerializeField] private bool enableMultiplayer = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5분마다 자동 저장
        
        [Header("UI References")]
        [SerializeField] private GameObject mainMenuUI;
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private GameObject settingsUI;
        
        // 싱글톤 패턴
        public static GameManager Instance { get; private set; }
        
        // 게임 상태
        private float lastAutoSaveTime;
        private bool isInitialized = false;
        
        // 이벤트
        public System.Action<GameState> OnGameStateChanged;
        public System.Action OnGamePaused;
        public System.Action OnGameResumed;
        public System.Action OnGameSaved;
        public System.Action OnGameLoaded;
        
        private void Awake()
        {
            // 싱글톤 설정
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            InitializeGame();
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            // 게임 시간 업데이트
            if (currentState == GameState.Playing && !isGamePaused)
            {
                gameTime += Time.deltaTime;
            }
            
            // 자동 저장 체크
            CheckAutoSave();
            
            // 입력 처리
            HandleInput();
        }
        
        private void InitializeGame()
        {
            // 컴포넌트 참조 설정
            if (arManager == null)
                arManager = FindObjectOfType<ARManager>();
            
            if (networkManager == null)
                networkManager = FindObjectOfType<NetworkManager>();
            
            if (playerController == null)
                playerController = FindObjectOfType<PlayerController>();
            
            if (characterSystem == null)
                characterSystem = FindObjectOfType<CharacterSystem>();
            
            if (itemSystem == null)
                itemSystem = FindObjectOfType<ItemSystem>();
            
            // 고급 시스템 참조 설정
            if (sensorManager == null)
                sensorManager = FindObjectOfType<MobileSensorManager>();
            
            if (touchManager == null)
                touchManager = FindObjectOfType<TouchInteractionManager>();
            
            if (voiceManager == null)
                voiceManager = FindObjectOfType<VoiceChatManager>();
            
            if (motionManager == null)
                motionManager = FindObjectOfType<MotionDetectionManager>();
            
            if (arObjectManager == null)
                arObjectManager = FindObjectOfType<ARObjectManager>();
            
            if (combatSystem == null)
                combatSystem = FindObjectOfType<ARCombatSystem>();
            
            // 이벤트 구독
            SubscribeToEvents();
            
            // 초기 상태 설정
            SetGameState(GameState.MainMenu);
            
            isInitialized = true;
            Debug.Log("게임 매니저가 초기화되었습니다.");
        }
        
        private void SubscribeToEvents()
        {
            // AR 이벤트 구독
            if (arManager != null)
            {
                arManager.OnARInitialized += OnARInitialized;
                arManager.OnPlaneDetected += OnPlaneDetected;
                arManager.OnVirtualWorldPlaced += OnVirtualWorldPlaced;
            }
            
            // 네트워크 이벤트 구독
            if (networkManager != null)
            {
                networkManager.OnClientConnected += OnNetworkConnected;
                networkManager.OnClientDisconnected += OnNetworkDisconnected;
                networkManager.OnPlayerConnected += OnPlayerJoined;
                networkManager.OnPlayerDisconnected += OnPlayerLeft;
            }
            
            // 캐릭터 시스템 이벤트 구독
            if (characterSystem != null)
            {
                characterSystem.OnLevelChanged += OnCharacterLevelChanged;
                characterSystem.OnStatsChanged += OnCharacterStatsChanged;
            }
        }
        
        private void HandleInput()
        {
            // ESC 키로 일시정지/재개
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == GameState.Playing)
                {
                    TogglePause();
                }
                else if (currentState == GameState.Paused)
                {
                    ResumeGame();
                }
            }
            
            // F1 키로 설정 메뉴 토글
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ToggleSettings();
            }
        }
        
        private void CheckAutoSave()
        {
            if (Time.time - lastAutoSaveTime >= autoSaveInterval)
            {
                SaveGame();
                lastAutoSaveTime = Time.time;
            }
        }
        
        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            if (currentState == GameState.MainMenu)
            {
                SetGameState(GameState.Playing);
                Debug.Log("게임이 시작되었습니다!");
                
                // AR 모드 활성화
                if (enableARMode && arManager != null)
                {
                    // ARManager에 StartAR 메서드가 있다면 호출
                    Debug.Log("AR 모드 활성화");
                }
                
                // 네트워킹 시작
                if (enableMultiplayer && networkManager != null)
                {
                    // NetworkManager에 ConnectToServer 메서드가 있다면 호출
                    Debug.Log("네트워킹 시작");
                }
                
                // UI 업데이트
                Debug.Log("UI 업데이트");
            }
        }
        
        public void SetGameState(GameState newState)
        {
            if (currentState == newState) return;
            
            GameState previousState = currentState;
            currentState = newState;
            
            // 상태별 처리
            switch (newState)
            {
                case GameState.MainMenu:
                    ShowMainMenu();
                    break;
                case GameState.Loading:
                    ShowLoadingScreen();
                    break;
                case GameState.Playing:
                    StartGameplay();
                    break;
                case GameState.Paused:
                    PauseGame();
                    break;
                case GameState.Settings:
                    ShowSettings();
                    break;
                case GameState.GameOver:
                    ShowGameOver();
                    break;
            }
            
            OnGameStateChanged?.Invoke(newState);
            Debug.Log($"게임 상태 변경: {previousState} -> {newState}");
        }
        
        private void ShowMainMenu()
        {
            if (mainMenuUI != null)
                mainMenuUI.SetActive(true);
            
            if (gameUI != null)
                gameUI.SetActive(false);
            
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);
            
            if (settingsUI != null)
                settingsUI.SetActive(false);
        }
        
        private void ShowLoadingScreen()
        {
            // 로딩 화면 표시
            Debug.Log("로딩 중...");
        }
        
        private void StartGameplay()
        {
            if (mainMenuUI != null)
                mainMenuUI.SetActive(false);
            
            if (gameUI != null)
                gameUI.SetActive(true);
            
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);
            
            if (settingsUI != null)
                settingsUI.SetActive(false);
            
            isGamePaused = false;
        }
        
        private void PauseGame()
        {
            isGamePaused = true;
            Time.timeScale = 0f;
            
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(true);
            
            OnGamePaused?.Invoke();
        }
        
        private void ShowSettings()
        {
            if (settingsUI != null)
                settingsUI.SetActive(true);
        }
        
        private void ShowGameOver()
        {
            // 게임 오버 화면 표시
            Debug.Log("게임 오버!");
        }
        
        public void StartNewGame()
        {
            SetGameState(GameState.Loading);
            
            // AR 모드 활성화
            if (enableARMode && arManager != null)
            {
                arManager.TogglePlaneDetection(true);
            }
            
            // 멀티플레이어 연결
            if (enableMultiplayer && networkManager != null)
            {
                networkManager.StartClient();
            }
            
            // 게임플레이 시작
            SetGameState(GameState.Playing);
        }
        
        public void LoadGame()
        {
            SetGameState(GameState.Loading);
            
            // 저장된 게임 데이터 로드
            LoadGameData();
            
            SetGameState(GameState.Playing);
        }
        
        public void SaveGame()
        {
            // 게임 데이터 저장
            SaveGameData();
            
            OnGameSaved?.Invoke();
            Debug.Log("게임이 저장되었습니다.");
        }
        
        public void LoadGameData()
        {
            // 실제로는 PlayerPrefs나 파일 시스템을 사용
            Debug.Log("게임 데이터를 로드합니다.");
            OnGameLoaded?.Invoke();
        }
        
        public void SaveGameData()
        {
            // 실제로는 PlayerPrefs나 파일 시스템을 사용
            Debug.Log("게임 데이터를 저장합니다.");
        }
        
        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
        
        public void ResumeGame()
        {
            isGamePaused = false;
            Time.timeScale = 1f;
            
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);
            
            SetGameState(GameState.Playing);
            OnGameResumed?.Invoke();
        }
        
        public void ToggleSettings()
        {
            if (currentState == GameState.Settings)
            {
                SetGameState(GameState.Playing);
            }
            else
            {
                SetGameState(GameState.Settings);
            }
        }
        
        public void QuitGame()
        {
            // 게임 종료
            Application.Quit();
        }
        
        // 이벤트 핸들러들
        private void OnARInitialized()
        {
            Debug.Log("AR 시스템이 초기화되었습니다.");
        }
        
        private void OnPlaneDetected()
        {
            Debug.Log("AR 평면이 감지되었습니다.");
        }
        
        private void OnVirtualWorldPlaced(Vector3 position, Quaternion rotation)
        {
            Debug.Log($"가상 세계가 배치되었습니다: {position}");
        }
        
        private void OnNetworkConnected()
        {
            Debug.Log("네트워크에 연결되었습니다.");
        }
        
        private void OnNetworkDisconnected()
        {
            Debug.Log("네트워크 연결이 끊어졌습니다.");
        }
        
        private void OnPlayerJoined(int clientId)
        {
            Debug.Log($"플레이어 {clientId}가 게임에 참여했습니다.");
        }
        
        private void OnPlayerLeft(int clientId)
        {
            Debug.Log($"플레이어 {clientId}가 게임을 떠났습니다.");
        }
        
        private void OnCharacterLevelChanged(int newLevel)
        {
            Debug.Log($"캐릭터 레벨이 {newLevel}로 변경되었습니다.");
        }
        
        private void OnCharacterStatsChanged(CharacterStats stats)
        {
            // 캐릭터 스탯 변경 처리
        }
        
        // Getter 메서드들
        public GameState GetCurrentState() => currentState;
        public bool IsGamePaused() => isGamePaused;
        public float GetGameTime() => gameTime;
        public ARManager GetARManager() => arManager;
        public NetworkManager GetNetworkManager() => networkManager;
        public PlayerController GetPlayerController() => playerController;
        public CharacterSystem GetCharacterSystem() => characterSystem;
        public ItemSystem GetItemSystem() => itemSystem;
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (arManager != null)
            {
                arManager.OnARInitialized -= OnARInitialized;
                arManager.OnPlaneDetected -= OnPlaneDetected;
                arManager.OnVirtualWorldPlaced -= OnVirtualWorldPlaced;
            }
            
            if (networkManager != null)
            {
                networkManager.OnClientConnected -= OnNetworkConnected;
                networkManager.OnClientDisconnected -= OnNetworkDisconnected;
                networkManager.OnPlayerConnected -= OnPlayerJoined;
                networkManager.OnPlayerDisconnected -= OnPlayerLeft;
            }
            
            if (characterSystem != null)
            {
                characterSystem.OnLevelChanged -= OnCharacterLevelChanged;
                characterSystem.OnStatsChanged -= OnCharacterStatsChanged;
            }
        }
    }
    
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        Settings,
        GameOver
    }
}
