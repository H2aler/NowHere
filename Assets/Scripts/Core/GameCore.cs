using UnityEngine;
using System.Collections.Generic;
using NowHere.Game;
using NowHere.XR;
using NowHere.UI;
using NowHere.Audio;
using NowHere.Data;
using NowHere.Analytics;
using NowHere.Testing;

namespace NowHere.Core
{
    /// <summary>
    /// 게임 코어 시스템
    /// 모든 게임 시스템을 통합하고 관리하는 핵심 클래스
    /// </summary>
    public class GameCore : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private XRGameManager xrGameManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private SaveSystem saveSystem;
        [SerializeField] private AssetManager assetManager;
        [SerializeField] private AnalyticsManager analyticsManager;
        [SerializeField] private GameTester gameTester;
        
        [Header("Core Settings")]
        [SerializeField] private bool enableCoreSystems = true;
        [SerializeField] private bool enableXRSystems = true;
        [SerializeField] private bool enableUISystems = true;
        [SerializeField] private bool enableAudioSystems = true;
        [SerializeField] private bool enableDataSystems = true;
        [SerializeField] private bool enableAnalyticsSystems = true;
        [SerializeField] private bool enableTestingSystems = true;
        
        [Header("Initialization Order")]
        [SerializeField] private bool initializeInOrder = true;
        [SerializeField] private float initializationDelay = 0.1f;
        
        [Header("Core Performance")]
        [SerializeField] private bool enableCoreOptimization = true;
        [SerializeField] private bool enableMemoryManagement = true;
        [SerializeField] private bool enablePerformanceMonitoring = true;
        [SerializeField] private float performanceCheckInterval = 5f;
        
        // 코어 상태
        private bool isCoreInitialized = false;
        private bool isInitializing = false;
        private CoreState currentCoreState = CoreState.Initializing;
        private List<CoreSystem> coreSystems = new List<CoreSystem>();
        private Dictionary<string, bool> systemStatus = new Dictionary<string, bool>();
        
        // 성능 모니터링
        private float lastPerformanceCheck = 0f;
        private CorePerformanceData performanceData = new CorePerformanceData();
        
        // 이벤트
        public System.Action<CoreState> OnCoreStateChanged;
        public System.Action<CoreSystem> OnSystemInitialized;
        public System.Action<CoreSystem> OnSystemFailed;
        public System.Action<CorePerformanceData> OnPerformanceUpdated;
        public System.Action<string> OnCoreError;
        
        // 싱글톤 패턴
        public static GameCore Instance { get; private set; }
        
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
            InitializeGameCore();
        }
        
        private void Update()
        {
            if (isCoreInitialized)
            {
                UpdateCore();
            }
        }
        
        private void InitializeGameCore()
        {
            Debug.Log("=== Game Core 초기화 시작 ===");
            
            SetCoreState(CoreState.Initializing);
            
            if (initializeInOrder)
            {
                StartCoroutine(InitializeSystemsInOrder());
            }
            else
            {
                InitializeAllSystems();
            }
        }
        
        private System.Collections.IEnumerator InitializeSystemsInOrder()
        {
            isInitializing = true;
            
            // 1. 기본 시스템 초기화
            yield return StartCoroutine(InitializeBasicSystems());
            
            // 2. 데이터 시스템 초기화
            if (enableDataSystems)
            {
                yield return StartCoroutine(InitializeDataSystems());
            }
            
            // 3. 오디오 시스템 초기화
            if (enableAudioSystems)
            {
                yield return StartCoroutine(InitializeAudioSystems());
            }
            
            // 4. UI 시스템 초기화
            if (enableUISystems)
            {
                yield return StartCoroutine(InitializeUISystems());
            }
            
            // 5. XR 시스템 초기화
            if (enableXRSystems)
            {
                yield return StartCoroutine(InitializeXRSystems());
            }
            
            // 6. 분석 시스템 초기화
            if (enableAnalyticsSystems)
            {
                yield return StartCoroutine(InitializeAnalyticsSystems());
            }
            
            // 7. 테스트 시스템 초기화
            if (enableTestingSystems)
            {
                yield return StartCoroutine(InitializeTestingSystems());
            }
            
            // 8. 게임 매니저 초기화
            if (enableCoreSystems)
            {
                yield return StartCoroutine(InitializeGameManager());
            }
            
            // 초기화 완료
            CompleteInitialization();
            
            isInitializing = false;
        }
        
        private System.Collections.IEnumerator InitializeBasicSystems()
        {
            Debug.Log("기본 시스템 초기화...");
            
            // 에셋 매니저 초기화
            if (assetManager == null)
            {
                assetManager = FindObjectOfType<AssetManager>();
            }
            
            if (assetManager != null)
            {
                yield return new WaitUntil(() => assetManager.IsAssetManagerInitialized());
                RegisterSystem("AssetManager", true);
            }
            
            yield return new WaitForSeconds(initializationDelay);
        }
        
        private System.Collections.IEnumerator InitializeDataSystems()
        {
            Debug.Log("데이터 시스템 초기화...");
            
            // 저장 시스템 초기화
            if (saveSystem == null)
            {
                saveSystem = FindObjectOfType<SaveSystem>();
            }
            
            if (saveSystem != null)
            {
                yield return new WaitUntil(() => saveSystem.IsSaveSystemInitialized());
                RegisterSystem("SaveSystem", true);
            }
            
            yield return new WaitForSeconds(initializationDelay);
        }
        
        private System.Collections.IEnumerator InitializeAudioSystems()
        {
            Debug.Log("오디오 시스템 초기화...");
            
            // 오디오 매니저 초기화
            if (audioManager == null)
            {
                audioManager = FindObjectOfType<AudioManager>();
            }
            
            if (audioManager != null)
            {
                yield return new WaitUntil(() => audioManager.IsAudioInitialized());
                RegisterSystem("AudioManager", true);
            }
            
            yield return new WaitForSeconds(initializationDelay);
        }
        
        private System.Collections.IEnumerator InitializeUISystems()
        {
            Debug.Log("UI 시스템 초기화...");
            
            // UI 매니저 초기화
            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
            }
            
            if (uiManager != null)
            {
                RegisterSystem("UIManager", true);
            }
            
            yield return new WaitForSeconds(initializationDelay);
        }
        
        private System.Collections.IEnumerator InitializeXRSystems()
        {
            Debug.Log("XR 시스템 초기화...");
            
            // XR 게임 매니저 초기화
            if (xrGameManager == null)
            {
                xrGameManager = FindObjectOfType<XRGameManager>();
            }
            
            if (xrGameManager != null)
            {
                yield return new WaitUntil(() => xrGameManager.IsXRGameInitialized());
                RegisterSystem("XRGameManager", true);
            }
            
            yield return new WaitForSeconds(initializationDelay);
        }
        
        private System.Collections.IEnumerator InitializeAnalyticsSystems()
        {
            Debug.Log("분석 시스템 초기화...");
            
            // 분석 매니저 초기화
            if (analyticsManager == null)
            {
                analyticsManager = FindObjectOfType<AnalyticsManager>();
            }
            
            if (analyticsManager != null)
            {
                yield return new WaitUntil(() => analyticsManager.IsAnalyticsInitialized());
                RegisterSystem("AnalyticsManager", true);
            }
            
            yield return new WaitForSeconds(initializationDelay);
        }
        
        private System.Collections.IEnumerator InitializeTestingSystems()
        {
            Debug.Log("테스트 시스템 초기화...");
            
            // 게임 테스터 초기화
            if (gameTester == null)
            {
                gameTester = FindObjectOfType<GameTester>();
            }
            
            if (gameTester != null)
            {
                yield return new WaitUntil(() => gameTester.IsTestingInitialized());
                RegisterSystem("GameTester", true);
            }
            
            yield return new WaitForSeconds(initializationDelay);
        }
        
        private System.Collections.IEnumerator InitializeGameManager()
        {
            Debug.Log("게임 매니저 초기화...");
            
            // 게임 매니저 초기화
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
            
            if (gameManager != null)
            {
                yield return new WaitUntil(() => gameManager.IsInitialized());
                RegisterSystem("GameManager", true);
            }
            
            yield return new WaitForSeconds(initializationDelay);
        }
        
        private void InitializeAllSystems()
        {
            // 모든 시스템 동시 초기화
            Debug.Log("모든 시스템 동시 초기화...");
            
            InitializeBasicSystems();
            if (enableDataSystems) InitializeDataSystems();
            if (enableAudioSystems) InitializeAudioSystems();
            if (enableUISystems) InitializeUISystems();
            if (enableXRSystems) InitializeXRSystems();
            if (enableAnalyticsSystems) InitializeAnalyticsSystems();
            if (enableTestingSystems) InitializeTestingSystems();
            if (enableCoreSystems) InitializeGameManager();
        }
        
        private void CompleteInitialization()
        {
            isCoreInitialized = true;
            SetCoreState(CoreState.Ready);
            
            Debug.Log("=== Game Core 초기화 완료 ===");
            Debug.Log($"초기화된 시스템: {coreSystems.Count}개");
            
            foreach (var system in coreSystems)
            {
                Debug.Log($"- {system.systemName}: {(system.isInitialized ? "성공" : "실패")}");
            }
        }
        
        private void RegisterSystem(string systemName, bool isInitialized)
        {
            CoreSystem system = new CoreSystem
            {
                systemName = systemName,
                isInitialized = isInitialized,
                initializationTime = Time.time
            };
            
            coreSystems.Add(system);
            systemStatus[systemName] = isInitialized;
            
            if (isInitialized)
            {
                OnSystemInitialized?.Invoke(system);
            }
            else
            {
                OnSystemFailed?.Invoke(system);
            }
        }
        
        private void UpdateCore()
        {
            // 코어 업데이트
            if (enableCoreOptimization)
            {
                UpdateCoreOptimization();
            }
            
            if (enableMemoryManagement)
            {
                UpdateMemoryManagement();
            }
            
            if (enablePerformanceMonitoring)
            {
                UpdatePerformanceMonitoring();
            }
        }
        
        private void UpdateCoreOptimization()
        {
            // 코어 최적화 업데이트
        }
        
        private void UpdateMemoryManagement()
        {
            // 메모리 관리 업데이트
            if (Time.time - lastPerformanceCheck >= performanceCheckInterval)
            {
                // 가비지 컬렉션 실행
                System.GC.Collect();
                lastPerformanceCheck = Time.time;
            }
        }
        
        private void UpdatePerformanceMonitoring()
        {
            // 성능 모니터링 업데이트
            if (Time.time - lastPerformanceCheck >= performanceCheckInterval)
            {
                UpdatePerformanceData();
                OnPerformanceUpdated?.Invoke(performanceData);
                lastPerformanceCheck = Time.time;
            }
        }
        
        private void UpdatePerformanceData()
        {
            performanceData.fps = 1f / Time.deltaTime;
            performanceData.memoryUsage = System.GC.GetTotalMemory(false) / 1024f / 1024f;
            performanceData.systemCount = coreSystems.Count;
            performanceData.initializedSystemCount = GetInitializedSystemCount();
            performanceData.updateTime = Time.time;
        }
        
        private int GetInitializedSystemCount()
        {
            int count = 0;
            foreach (var system in coreSystems)
            {
                if (system.isInitialized)
                {
                    count++;
                }
            }
            return count;
        }
        
        private void SetCoreState(CoreState state)
        {
            if (currentCoreState == state) return;
            
            currentCoreState = state;
            OnCoreStateChanged?.Invoke(state);
            
            Debug.Log($"코어 상태 변경: {state}");
        }
        
        // 공개 메서드들
        public bool IsCoreInitialized()
        {
            return isCoreInitialized;
        }
        
        public bool IsInitializing()
        {
            return isInitializing;
        }
        
        public CoreState GetCurrentCoreState()
        {
            return currentCoreState;
        }
        
        public List<CoreSystem> GetCoreSystems()
        {
            return coreSystems;
        }
        
        public bool IsSystemInitialized(string systemName)
        {
            return systemStatus.ContainsKey(systemName) && systemStatus[systemName];
        }
        
        public CorePerformanceData GetPerformanceData()
        {
            return performanceData;
        }
        
        public GameManager GetGameManager()
        {
            return gameManager;
        }
        
        public XRGameManager GetXRGameManager()
        {
            return xrGameManager;
        }
        
        public UIManager GetUIManager()
        {
            return uiManager;
        }
        
        public AudioManager GetAudioManager()
        {
            return audioManager;
        }
        
        public SaveSystem GetSaveSystem()
        {
            return saveSystem;
        }
        
        public AssetManager GetAssetManager()
        {
            return assetManager;
        }
        
        public AnalyticsManager GetAnalyticsManager()
        {
            return analyticsManager;
        }
        
        public GameTester GetGameTester()
        {
            return gameTester;
        }
        
        public void RestartCore()
        {
            Debug.Log("코어 재시작...");
            
            isCoreInitialized = false;
            isInitializing = false;
            coreSystems.Clear();
            systemStatus.Clear();
            
            SetCoreState(CoreState.Initializing);
            InitializeGameCore();
        }
        
        public void ShutdownCore()
        {
            Debug.Log("코어 종료...");
            
            SetCoreState(CoreState.ShuttingDown);
            
            // 모든 시스템 종료
            if (gameManager != null)
            {
                // 게임 매니저 종료
            }
            
            if (xrGameManager != null)
            {
                // XR 게임 매니저 종료
            }
            
            if (uiManager != null)
            {
                // UI 매니저 종료
            }
            
            if (audioManager != null)
            {
                // 오디오 매니저 종료
            }
            
            if (saveSystem != null)
            {
                // 저장 시스템 종료
            }
            
            if (assetManager != null)
            {
                // 에셋 매니저 종료
            }
            
            if (analyticsManager != null)
            {
                // 분석 매니저 종료
            }
            
            if (gameTester != null)
            {
                // 게임 테스터 종료
            }
            
            SetCoreState(CoreState.Shutdown);
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // 앱 일시정지 시 데이터 저장
                if (saveSystem != null)
                {
                    saveSystem.SaveGame();
                }
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // 앱 포커스 잃을 때 데이터 저장
                if (saveSystem != null)
                {
                    saveSystem.SaveGame();
                }
            }
        }
        
        private void OnDestroy()
        {
            ShutdownCore();
        }
    }
    
    // 코어 데이터 구조체들
    [System.Serializable]
    public class CoreSystem
    {
        public string systemName;
        public bool isInitialized;
        public float initializationTime;
    }
    
    [System.Serializable]
    public class CorePerformanceData
    {
        public float fps;
        public float memoryUsage;
        public int systemCount;
        public int initializedSystemCount;
        public float updateTime;
    }
    
    public enum CoreState
    {
        Initializing,
        Ready,
        Running,
        Paused,
        ShuttingDown,
        Shutdown
    }
}
