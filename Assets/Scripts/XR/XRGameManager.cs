using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using NowHere.XR;
using NowHere.Game;
using NowHere.RPG;
using NowHere.Combat;
using NowHere.Networking;

namespace NowHere.XR
{
    /// <summary>
    /// XR 게임을 관리하는 통합 매니저 클래스
    /// VR, AR, MR 모드를 통합하여 게임 경험을 제공
    /// </summary>
    public class XRGameManager : MonoBehaviour
    {
        [Header("XR Game Settings")]
        [SerializeField] private XRMode defaultXRMode = XRMode.VR;
        [SerializeField] private bool enableModeSwitching = true;
        [SerializeField] private bool enableCrossPlatformPlay = true;
        [SerializeField] private bool enableAdaptiveUI = true;
        
        [Header("XR Game Components")]
        [SerializeField] private XRManager xrManager;
        [SerializeField] private VRCombatSystem vrCombatSystem;
        [SerializeField] private VRInteractionManager vrInteractionManager;
        [SerializeField] private VRUIManager vrUIManager;
        [SerializeField] private VRPerformanceOptimizer vrPerformanceOptimizer;
        
        [Header("Game Integration")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private CharacterSystem characterSystem;
        [SerializeField] private ARCombatSystem arCombatSystem;
        
        [Header("XR Scenes")]
        [SerializeField] private string vrMainMenuScene = "VRMainMenu";
        [SerializeField] private string vrGameScene = "VRGameScene";
        [SerializeField] private string arGameScene = "ARGameScene";
        [SerializeField] private string mrGameScene = "MRGameScene";
        
        // XR 게임 상태
        private XRGameState currentGameState = XRGameState.MainMenu;
        private XRMode currentXRMode;
        private bool isXRGameInitialized = false;
        private bool isModeSwitching = false;
        
        // XR 게임 데이터
        private XRGameData gameData;
        private List<XRPlayer> xrPlayers = new List<XRPlayer>();
        private Dictionary<string, XRGameSession> activeSessions = new Dictionary<string, XRGameSession>();
        
        // 이벤트
        public System.Action<XRMode> OnXRModeSwitched;
        public System.Action<XRGameState> OnGameStateChanged;
        public System.Action<XRPlayer> OnPlayerJoined;
        public System.Action<XRPlayer> OnPlayerLeft;
        public System.Action<XRGameSession> OnSessionCreated;
        public System.Action<XRGameSession> OnSessionEnded;
        
        // 싱글톤 패턴
        public static XRGameManager Instance { get; private set; }
        
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
            InitializeXRGame();
        }
        
        private void Update()
        {
            if (!isXRGameInitialized) return;
            
            UpdateXRGame();
            HandleXRInput();
        }
        
        private void InitializeXRGame()
        {
            Debug.Log("XR 게임 시스템 초기화 시작...");
            
            // 컴포넌트 참조
            InitializeComponents();
            
            // XR 시스템 초기화
            InitializeXRSystems();
            
            // 게임 데이터 초기화
            InitializeGameData();
            
            // 이벤트 구독
            SubscribeToEvents();
            
            // 기본 XR 모드 설정
            currentXRMode = defaultXRMode;
            SetXRMode(currentXRMode);
            
            // 게임 상태 설정
            SetGameState(XRGameState.MainMenu);
            
            isXRGameInitialized = true;
            Debug.Log("XR 게임 시스템이 초기화되었습니다.");
        }
        
        private void InitializeComponents()
        {
            // XR 컴포넌트들 찾기
            if (xrManager == null)
                xrManager = FindObjectOfType<XRManager>();
            
            if (vrCombatSystem == null)
                vrCombatSystem = FindObjectOfType<VRCombatSystem>();
            
            if (vrInteractionManager == null)
                vrInteractionManager = FindObjectOfType<VRInteractionManager>();
            
            if (vrUIManager == null)
                vrUIManager = FindObjectOfType<VRUIManager>();
            
            if (vrPerformanceOptimizer == null)
                vrPerformanceOptimizer = FindObjectOfType<VRPerformanceOptimizer>();
            
            // 게임 컴포넌트들 찾기
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
            
            if (networkManager == null)
                networkManager = FindObjectOfType<NetworkManager>();
            
            if (characterSystem == null)
                characterSystem = FindObjectOfType<CharacterSystem>();
            
            if (arCombatSystem == null)
                arCombatSystem = FindObjectOfType<ARCombatSystem>();
        }
        
        private void InitializeXRSystems()
        {
            // XR 매니저 초기화
            if (xrManager != null)
            {
                xrManager.OnXRModeChanged += OnXRModeChanged;
                xrManager.OnXRInitialized += OnXRInitialized;
            }
            
            // VR 전투 시스템 초기화
            if (vrCombatSystem != null)
            {
                vrCombatSystem.OnVRCombatActionPerformed += OnVRCombatActionPerformed;
            }
            
            // VR 상호작용 매니저 초기화
            if (vrInteractionManager != null)
            {
                vrInteractionManager.OnObjectGrabbed += OnObjectGrabbed;
                vrInteractionManager.OnObjectReleased += OnObjectReleased;
                vrInteractionManager.OnTeleport += OnTeleport;
            }
            
            // VR UI 매니저 초기화
            if (vrUIManager != null)
            {
                vrUIManager.OnUIElementClicked += OnUIElementClicked;
                vrUIManager.OnUIVisibilityChanged += OnUIVisibilityChanged;
            }
            
            // VR 성능 최적화 초기화
            if (vrPerformanceOptimizer != null)
            {
                vrPerformanceOptimizer.OnFrameRateChanged += OnFrameRateChanged;
                vrPerformanceOptimizer.OnPerformanceModeChanged += OnPerformanceModeChanged;
            }
        }
        
        private void InitializeGameData()
        {
            // XR 게임 데이터 초기화
            gameData = new XRGameData
            {
                gameVersion = "1.0.0",
                xrMode = currentXRMode,
                gameState = currentGameState,
                playerCount = 0,
                maxPlayers = 8,
                sessionId = System.Guid.NewGuid().ToString()
            };
            
            Debug.Log("XR 게임 데이터가 초기화되었습니다.");
        }
        
        private void SubscribeToEvents()
        {
            // 네트워크 이벤트 구독
            if (networkManager != null)
            {
                networkManager.OnPlayerConnected += OnNetworkPlayerConnected;
                networkManager.OnPlayerDisconnected += OnNetworkPlayerDisconnected;
            }
            
            // 게임 매니저 이벤트 구독
            if (gameManager != null)
            {
                // 게임 매니저 이벤트 구독
            }
        }
        
        private void UpdateXRGame()
        {
            // XR 게임 업데이트
            UpdateXRGameState();
            UpdateXRPlayers();
            UpdateXRSessions();
        }
        
        private void UpdateXRGameState()
        {
            // XR 게임 상태 업데이트
            switch (currentGameState)
            {
                case XRGameState.MainMenu:
                    UpdateMainMenuState();
                    break;
                case XRGameState.Lobby:
                    UpdateLobbyState();
                    break;
                case XRGameState.Playing:
                    UpdatePlayingState();
                    break;
                case XRGameState.Paused:
                    UpdatePausedState();
                    break;
                case XRGameState.GameOver:
                    UpdateGameOverState();
                    break;
            }
        }
        
        private void UpdateMainMenuState()
        {
            // 메인 메뉴 상태 업데이트
            if (vrUIManager != null && !vrUIManager.IsUIVisible())
            {
                vrUIManager.ShowVRUI();
            }
        }
        
        private void UpdateLobbyState()
        {
            // 로비 상태 업데이트
            // 플레이어 대기, 게임 준비 등
        }
        
        private void UpdatePlayingState()
        {
            // 게임 플레이 상태 업데이트
            // 전투, 상호작용, 네트워킹 등
        }
        
        private void UpdatePausedState()
        {
            // 일시정지 상태 업데이트
        }
        
        private void UpdateGameOverState()
        {
            // 게임 오버 상태 업데이트
        }
        
        private void UpdateXRPlayers()
        {
            // XR 플레이어들 업데이트
            foreach (XRPlayer player in xrPlayers)
            {
                if (player != null)
                {
                    player.UpdatePlayer();
                }
            }
        }
        
        private void UpdateXRSessions()
        {
            // XR 게임 세션들 업데이트
            foreach (XRGameSession session in activeSessions.Values)
            {
                if (session != null)
                {
                    session.UpdateSession();
                }
            }
        }
        
        private void HandleXRInput()
        {
            // XR 입력 처리
            if (xrManager != null)
            {
                // XR 모드 전환 입력
                if (enableModeSwitching)
                {
                    HandleModeSwitchingInput();
                }
                
                // 게임 상태 전환 입력
                HandleGameStateInput();
            }
        }
        
        private void HandleModeSwitchingInput()
        {
            // XR 모드 전환 입력 처리
            // 실제 구현에서는 컨트롤러 버튼이나 음성 명령 사용
        }
        
        private void HandleGameStateInput()
        {
            // 게임 상태 전환 입력 처리
            // 실제 구현에서는 UI 버튼이나 제스처 사용
        }
        
        public void SetXRMode(XRMode mode)
        {
            if (isModeSwitching || currentXRMode == mode) return;
            
            isModeSwitching = true;
            
            // XR 모드 전환
            if (xrManager != null)
            {
                xrManager.SetXRMode(mode);
            }
            
            // 게임 데이터 업데이트
            gameData.xrMode = mode;
            currentXRMode = mode;
            
            // 모드별 설정 적용
            ApplyXRModeSettings(mode);
            
            // 이벤트 발생
            OnXRModeSwitched?.Invoke(mode);
            
            isModeSwitching = false;
            
            Debug.Log($"XR 모드가 변경되었습니다: {mode}");
        }
        
        private void ApplyXRModeSettings(XRMode mode)
        {
            switch (mode)
            {
                case XRMode.VR:
                    ApplyVRSettings();
                    break;
                case XRMode.AR:
                    ApplyARSettings();
                    break;
                case XRMode.MR:
                    ApplyMRSettings();
                    break;
            }
        }
        
        private void ApplyVRSettings()
        {
            // VR 모드 설정
            if (vrUIManager != null)
            {
                vrUIManager.SetUIScale(1.0f);
                vrUIManager.SetUIDistance(2.0f);
            }
            
            if (vrCombatSystem != null)
            {
                vrCombatSystem.SetVRCombatMode(true);
            }
        }
        
        private void ApplyARSettings()
        {
            // AR 모드 설정
            if (vrUIManager != null)
            {
                vrUIManager.SetUIScale(0.8f);
                vrUIManager.SetUIDistance(1.5f);
            }
            
            if (vrCombatSystem != null)
            {
                vrCombatSystem.SetVRCombatMode(false);
            }
        }
        
        private void ApplyMRSettings()
        {
            // MR 모드 설정 (VR + AR 혼합)
            if (vrUIManager != null)
            {
                vrUIManager.SetUIScale(1.2f);
                vrUIManager.SetUIDistance(2.5f);
            }
            
            if (vrCombatSystem != null)
            {
                vrCombatSystem.SetVRCombatMode(true);
            }
        }
        
        public void SetGameState(XRGameState state)
        {
            if (currentGameState == state) return;
            
            XRGameState previousState = currentGameState;
            currentGameState = state;
            
            // 게임 데이터 업데이트
            gameData.gameState = state;
            
            // 상태별 처리
            switch (state)
            {
                case XRGameState.MainMenu:
                    LoadMainMenu();
                    break;
                case XRGameState.Lobby:
                    LoadLobby();
                    break;
                case XRGameState.Playing:
                    StartGame();
                    break;
                case XRGameState.Paused:
                    PauseGame();
                    break;
                case XRGameState.GameOver:
                    EndGame();
                    break;
            }
            
            // 이벤트 발생
            OnGameStateChanged?.Invoke(state);
            
            Debug.Log($"XR 게임 상태가 변경되었습니다: {previousState} -> {state}");
        }
        
        private void LoadMainMenu()
        {
            // 메인 메뉴 로드
            if (vrUIManager != null)
            {
                vrUIManager.ShowVRUI();
            }
        }
        
        private void LoadLobby()
        {
            // 로비 로드
            // 플레이어 대기, 게임 설정 등
        }
        
        private void StartGame()
        {
            // 게임 시작
            if (vrUIManager != null)
            {
                vrUIManager.HideVRUI();
            }
        }
        
        private void PauseGame()
        {
            // 게임 일시정지
            if (vrUIManager != null)
            {
                vrUIManager.ShowVRUI();
            }
        }
        
        private void EndGame()
        {
            // 게임 종료
            if (vrUIManager != null)
            {
                vrUIManager.ShowVRUI();
            }
        }
        
        public XRPlayer CreateXRPlayer(string playerId, string playerName)
        {
            XRPlayer newPlayer = new XRPlayer
            {
                playerId = playerId,
                playerName = playerName,
                xrMode = currentXRMode,
                isConnected = true,
                joinTime = Time.time
            };
            
            xrPlayers.Add(newPlayer);
            gameData.playerCount = xrPlayers.Count;
            
            OnPlayerJoined?.Invoke(newPlayer);
            
            Debug.Log($"XR 플레이어가 생성되었습니다: {playerName} ({playerId})");
            return newPlayer;
        }
        
        public void RemoveXRPlayer(string playerId)
        {
            XRPlayer player = xrPlayers.Find(p => p.playerId == playerId);
            if (player != null)
            {
                xrPlayers.Remove(player);
                gameData.playerCount = xrPlayers.Count;
                
                OnPlayerLeft?.Invoke(player);
                
                Debug.Log($"XR 플레이어가 제거되었습니다: {player.playerName} ({playerId})");
            }
        }
        
        public XRGameSession CreateGameSession(string sessionName, int maxPlayers)
        {
            XRGameSession newSession = new XRGameSession
            {
                sessionId = System.Guid.NewGuid().ToString(),
                sessionName = sessionName,
                maxPlayers = maxPlayers,
                currentPlayers = 0,
                xrMode = currentXRMode,
                gameState = XRGameState.Lobby,
                createdTime = Time.time
            };
            
            activeSessions[newSession.sessionId] = newSession;
            
            OnSessionCreated?.Invoke(newSession);
            
            Debug.Log($"XR 게임 세션이 생성되었습니다: {sessionName}");
            return newSession;
        }
        
        public void EndGameSession(string sessionId)
        {
            if (activeSessions.ContainsKey(sessionId))
            {
                XRGameSession session = activeSessions[sessionId];
                activeSessions.Remove(sessionId);
                
                OnSessionEnded?.Invoke(session);
                
                Debug.Log($"XR 게임 세션이 종료되었습니다: {session.sessionName}");
            }
        }
        
        // 이벤트 핸들러들
        private void OnXRModeChanged(XRMode mode)
        {
            Debug.Log($"XR 모드 변경 이벤트: {mode}");
        }
        
        private void OnXRInitialized()
        {
            Debug.Log("XR 초기화 완료 이벤트");
        }
        
        private void OnVRCombatActionPerformed(VRCombatAction action)
        {
            Debug.Log($"VR 전투 액션 이벤트: {action.actionType}");
        }
        
        private void OnObjectGrabbed(GameObject obj)
        {
            Debug.Log($"오브젝트 잡기 이벤트: {obj.name}");
        }
        
        private void OnObjectReleased(GameObject obj)
        {
            Debug.Log($"오브젝트 놓기 이벤트: {obj.name}");
        }
        
        private void OnTeleport(Vector3 position)
        {
            Debug.Log($"텔레포트 이벤트: {position}");
        }
        
        private void OnUIElementClicked(string elementName)
        {
            Debug.Log($"UI 요소 클릭 이벤트: {elementName}");
        }
        
        private void OnUIVisibilityChanged(bool isVisible)
        {
            Debug.Log($"UI 가시성 변경 이벤트: {isVisible}");
        }
        
        private void OnFrameRateChanged(float frameRate)
        {
            Debug.Log($"프레임 레이트 변경 이벤트: {frameRate}");
        }
        
        private void OnPerformanceModeChanged(VRPerformanceMode mode)
        {
            Debug.Log($"성능 모드 변경 이벤트: {mode}");
        }
        
        private void OnNetworkPlayerConnected(int playerId)
        {
            Debug.Log($"네트워크 플레이어 연결 이벤트: {playerId}");
        }
        
        private void OnNetworkPlayerDisconnected(int playerId)
        {
            Debug.Log($"네트워크 플레이어 연결 해제 이벤트: {playerId}");
        }
        
        // 공개 메서드들
        public XRMode GetCurrentXRMode()
        {
            return currentXRMode;
        }
        
        public XRGameState GetCurrentGameState()
        {
            return currentGameState;
        }
        
        public XRGameData GetGameData()
        {
            return gameData;
        }
        
        public List<XRPlayer> GetXRPlayers()
        {
            return xrPlayers;
        }
        
        public Dictionary<string, XRGameSession> GetActiveSessions()
        {
            return activeSessions;
        }
        
        public bool IsXRGameInitialized()
        {
            return isXRGameInitialized;
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (xrManager != null)
            {
                xrManager.OnXRModeChanged -= OnXRModeChanged;
                xrManager.OnXRInitialized -= OnXRInitialized;
            }
            
            if (vrCombatSystem != null)
            {
                vrCombatSystem.OnVRCombatActionPerformed -= OnVRCombatActionPerformed;
            }
            
            if (vrInteractionManager != null)
            {
                vrInteractionManager.OnObjectGrabbed -= OnObjectGrabbed;
                vrInteractionManager.OnObjectReleased -= OnObjectReleased;
                vrInteractionManager.OnTeleport -= OnTeleport;
            }
            
            if (vrUIManager != null)
            {
                vrUIManager.OnUIElementClicked -= OnUIElementClicked;
                vrUIManager.OnUIVisibilityChanged -= OnUIVisibilityChanged;
            }
            
            if (vrPerformanceOptimizer != null)
            {
                vrPerformanceOptimizer.OnFrameRateChanged -= OnFrameRateChanged;
                vrPerformanceOptimizer.OnPerformanceModeChanged -= OnPerformanceModeChanged;
            }
            
            if (networkManager != null)
            {
                networkManager.OnPlayerConnected -= OnNetworkPlayerConnected;
                networkManager.OnPlayerDisconnected -= OnNetworkPlayerDisconnected;
            }
        }
    }
    
    // XR 게임 데이터 구조체들
    [System.Serializable]
    public struct XRGameData
    {
        public string gameVersion;
        public XRMode xrMode;
        public XRGameState gameState;
        public int playerCount;
        public int maxPlayers;
        public string sessionId;
    }
    
    [System.Serializable]
    public struct XRPlayer
    {
        public string playerId;
        public string playerName;
        public XRMode xrMode;
        public bool isConnected;
        public float joinTime;
        public Vector3 position;
        public Quaternion rotation;
        
        public void UpdatePlayer()
        {
            // 플레이어 업데이트 로직
        }
    }
    
    [System.Serializable]
    public struct XRGameSession
    {
        public string sessionId;
        public string sessionName;
        public int maxPlayers;
        public int currentPlayers;
        public XRMode xrMode;
        public XRGameState gameState;
        public float createdTime;
        
        public void UpdateSession()
        {
            // 세션 업데이트 로직
        }
    }
    
    public enum XRGameState
    {
        MainMenu,
        Lobby,
        Playing,
        Paused,
        GameOver
    }
}
