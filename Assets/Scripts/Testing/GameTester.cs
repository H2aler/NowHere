using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using NowHere.Game;
using NowHere.XR;
using NowHere.UI;
using NowHere.Audio;
using NowHere.Data;

namespace NowHere.Testing
{
    /// <summary>
    /// 게임 테스트 및 디버깅 도구
    /// 자동화된 테스트와 디버깅 기능을 제공
    /// </summary>
    public class GameTester : MonoBehaviour
    {
        [Header("Testing Settings")]
        [SerializeField] private bool enableTesting = true;
        [SerializeField] private bool enableAutoTesting = false;
        [SerializeField] private bool enablePerformanceTesting = true;
        [SerializeField] private bool enableXRTesting = true;
        [SerializeField] private bool enableUITesting = true;
        [SerializeField] private bool enableAudioTesting = true;
        
        [Header("Test Configuration")]
        [SerializeField] private float testDuration = 60f;
        [SerializeField] private int maxTestIterations = 100;
        [SerializeField] private bool enableStressTesting = false;
        [SerializeField] private bool enableRegressionTesting = false;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugMode = true;
        [SerializeField] private bool enableDebugUI = true;
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool enableDebugVisualization = true;
        
        [Header("Test Results")]
        [SerializeField] private bool saveTestResults = true;
        [SerializeField] private string testResultsPath = "TestResults/";
        [SerializeField] private bool enableTestReporting = true;
        
        // 테스트 상태
        private bool isTestingInitialized = false;
        private bool isTestRunning = false;
        private bool isAutoTestRunning = false;
        private int currentTestIteration = 0;
        private float testStartTime = 0f;
        
        // 테스트 결과
        private List<TestResult> testResults = new List<TestResult>();
        private Dictionary<string, TestSuite> testSuites = new Dictionary<string, TestSuite>();
        private PerformanceTestData performanceData = new PerformanceTestData();
        
        // 참조
        private GameManager gameManager;
        private XRGameManager xrGameManager;
        private UIManager uiManager;
        private AudioManager audioManager;
        private SaveSystem saveSystem;
        private AnalyticsManager analyticsManager;
        
        // 이벤트
        public System.Action<TestResult> OnTestCompleted;
        public System.Action<TestSuite> OnTestSuiteCompleted;
        public System.Action<PerformanceTestData> OnPerformanceTestCompleted;
        public System.Action<string> OnTestError;
        public System.Action<bool> OnTestingStateChanged;
        
        // 싱글톤 패턴
        public static GameTester Instance { get; private set; }
        
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
            InitializeGameTester();
        }
        
        private void Update()
        {
            if (isTestingInitialized)
            {
                UpdateTesting();
                HandleDebugInput();
            }
        }
        
        private void InitializeGameTester()
        {
            if (!enableTesting) return;
            
            Debug.Log("Game Tester 초기화 시작...");
            
            // 컴포넌트 참조
            gameManager = FindObjectOfType<GameManager>();
            xrGameManager = FindObjectOfType<XRGameManager>();
            uiManager = FindObjectOfType<UIManager>();
            audioManager = FindObjectOfType<AudioManager>();
            saveSystem = FindObjectOfType<SaveSystem>();
            analyticsManager = FindObjectOfType<AnalyticsManager>();
            
            // 테스트 슈트 초기화
            InitializeTestSuites();
            
            // 디버그 UI 설정
            if (enableDebugUI)
            {
                SetupDebugUI();
            }
            
            // 자동 테스트 시작
            if (enableAutoTesting)
            {
                StartAutoTesting();
            }
            
            isTestingInitialized = true;
            Debug.Log("Game Tester 초기화 완료");
        }
        
        private void InitializeTestSuites()
        {
            // 기본 테스트 슈트 생성
            CreateTestSuite("BasicFunctionality", "기본 기능 테스트");
            CreateTestSuite("Performance", "성능 테스트");
            CreateTestSuite("XR", "XR 기능 테스트");
            CreateTestSuite("UI", "UI 테스트");
            CreateTestSuite("Audio", "오디오 테스트");
            CreateTestSuite("SaveLoad", "저장/로드 테스트");
            CreateTestSuite("Network", "네트워크 테스트");
            CreateTestSuite("Stress", "스트레스 테스트");
        }
        
        private void CreateTestSuite(string suiteName, string description)
        {
            TestSuite suite = new TestSuite
            {
                suiteName = suiteName,
                description = description,
                tests = new List<TestResult>(),
                isCompleted = false,
                startTime = 0f,
                endTime = 0f
            };
            
            testSuites[suiteName] = suite;
        }
        
        private void SetupDebugUI()
        {
            // 디버그 UI 설정
            // 실제 구현에서는 디버그 UI 생성
            Debug.Log("디버그 UI 설정 완료");
        }
        
        private void UpdateTesting()
        {
            if (isTestRunning)
            {
                // 테스트 실행 중 업데이트
                UpdateCurrentTest();
            }
            
            if (isAutoTestRunning)
            {
                // 자동 테스트 실행 중 업데이트
                UpdateAutoTest();
            }
        }
        
        private void HandleDebugInput()
        {
            if (!enableDebugMode) return;
            
            // 디버그 입력 처리
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ToggleDebugUI();
            }
            
            if (Input.GetKeyDown(KeyCode.F2))
            {
                RunAllTests();
            }
            
            if (Input.GetKeyDown(KeyCode.F3))
            {
                RunPerformanceTest();
            }
            
            if (Input.GetKeyDown(KeyCode.F4))
            {
                RunXRTest();
            }
            
            if (Input.GetKeyDown(KeyCode.F5))
            {
                RunUITest();
            }
            
            if (Input.GetKeyDown(KeyCode.F6))
            {
                RunAudioTest();
            }
            
            if (Input.GetKeyDown(KeyCode.F7))
            {
                RunSaveLoadTest();
            }
            
            if (Input.GetKeyDown(KeyCode.F8))
            {
                RunStressTest();
            }
        }
        
        private void UpdateCurrentTest()
        {
            // 현재 테스트 업데이트
            float testTime = Time.time - testStartTime;
            
            if (testTime >= testDuration)
            {
                CompleteCurrentTest();
            }
        }
        
        private void UpdateAutoTest()
        {
            // 자동 테스트 업데이트
            if (currentTestIteration >= maxTestIterations)
            {
                CompleteAutoTest();
            }
        }
        
        public void RunAllTests()
        {
            if (isTestRunning) return;
            
            StartCoroutine(RunAllTestsCoroutine());
        }
        
        private IEnumerator RunAllTestsCoroutine()
        {
            isTestRunning = true;
            OnTestingStateChanged?.Invoke(true);
            
            Debug.Log("모든 테스트 실행 시작");
            
            // 기본 기능 테스트
            yield return StartCoroutine(RunBasicFunctionalityTest());
            
            // 성능 테스트
            if (enablePerformanceTesting)
            {
                yield return StartCoroutine(RunPerformanceTest());
            }
            
            // XR 테스트
            if (enableXRTesting)
            {
                yield return StartCoroutine(RunXRTest());
            }
            
            // UI 테스트
            if (enableUITesting)
            {
                yield return StartCoroutine(RunUITest());
            }
            
            // 오디오 테스트
            if (enableAudioTesting)
            {
                yield return StartCoroutine(RunAudioTest());
            }
            
            // 저장/로드 테스트
            yield return StartCoroutine(RunSaveLoadTest());
            
            // 스트레스 테스트
            if (enableStressTesting)
            {
                yield return StartCoroutine(RunStressTest());
            }
            
            // 테스트 결과 저장
            if (saveTestResults)
            {
                SaveTestResults();
            }
            
            isTestRunning = false;
            OnTestingStateChanged?.Invoke(false);
            
            Debug.Log("모든 테스트 실행 완료");
        }
        
        private IEnumerator RunBasicFunctionalityTest()
        {
            Debug.Log("기본 기능 테스트 시작");
            
            TestResult result = new TestResult
            {
                testName = "BasicFunctionality",
                testType = TestType.Functionality,
                startTime = Time.time,
                isPassed = true,
                errors = new List<string>()
            };
            
            try
            {
                // 게임 매니저 테스트
                if (gameManager != null)
                {
                    // 게임 초기화 테스트
                    if (!gameManager.IsInitialized())
                    {
                        result.errors.Add("게임 매니저 초기화 실패");
                        result.isPassed = false;
                    }
                }
                
                // XR 게임 매니저 테스트
                if (xrGameManager != null)
                {
                    if (!xrGameManager.IsXRGameInitialized())
                    {
                        result.errors.Add("XR 게임 매니저 초기화 실패");
                        result.isPassed = false;
                    }
                }
                
                // UI 매니저 테스트
                if (uiManager != null)
                {
                    if (!uiManager.IsUIVisible())
                    {
                        result.errors.Add("UI 매니저 초기화 실패");
                        result.isPassed = false;
                    }
                }
                
                // 오디오 매니저 테스트
                if (audioManager != null)
                {
                    if (!audioManager.IsAudioInitialized())
                    {
                        result.errors.Add("오디오 매니저 초기화 실패");
                        result.isPassed = false;
                    }
                }
                
                // 저장 시스템 테스트
                if (saveSystem != null)
                {
                    if (!saveSystem.IsSaveSystemInitialized())
                    {
                        result.errors.Add("저장 시스템 초기화 실패");
                        result.isPassed = false;
                    }
                }
                
                // 분석 매니저 테스트
                if (analyticsManager != null)
                {
                    if (!analyticsManager.IsAnalyticsInitialized())
                    {
                        result.errors.Add("분석 매니저 초기화 실패");
                        result.isPassed = false;
                    }
                }
            }
            catch (System.Exception e)
            {
                result.errors.Add($"기본 기능 테스트 중 오류: {e.Message}");
                result.isPassed = false;
            }
            
            result.endTime = Time.time;
            result.duration = result.endTime - result.startTime;
            
            testResults.Add(result);
            testSuites["BasicFunctionality"].tests.Add(result);
            OnTestCompleted?.Invoke(result);
            
            Debug.Log($"기본 기능 테스트 완료: {(result.isPassed ? "성공" : "실패")}");
            
            yield return null;
        }
        
        private IEnumerator RunPerformanceTest()
        {
            Debug.Log("성능 테스트 시작");
            
            TestResult result = new TestResult
            {
                testName = "Performance",
                testType = TestType.Performance,
                startTime = Time.time,
                isPassed = true,
                errors = new List<string>()
            };
            
            try
            {
                // 성능 데이터 수집
                float startTime = Time.time;
                float fpsSum = 0f;
                int fpsCount = 0;
                float memorySum = 0f;
                int memoryCount = 0;
                
                // 10초간 성능 데이터 수집
                while (Time.time - startTime < 10f)
                {
                    float fps = 1f / Time.deltaTime;
                    float memory = System.GC.GetTotalMemory(false) / 1024f / 1024f;
                    
                    fpsSum += fps;
                    fpsCount++;
                    memorySum += memory;
                    memoryCount++;
                    
                    yield return new WaitForSeconds(0.1f);
                }
                
                // 평균 성능 계산
                float averageFPS = fpsSum / fpsCount;
                float averageMemory = memorySum / memoryCount;
                
                // 성능 임계값 확인
                if (averageFPS < 30f)
                {
                    result.errors.Add($"평균 FPS가 너무 낮음: {averageFPS:F1}");
                    result.isPassed = false;
                }
                
                if (averageMemory > 500f)
                {
                    result.errors.Add($"메모리 사용량이 너무 높음: {averageMemory:F1}MB");
                    result.isPassed = false;
                }
                
                // 성능 데이터 저장
                performanceData.averageFPS = averageFPS;
                performanceData.averageMemoryUsage = averageMemory;
                performanceData.testDuration = 10f;
                
                OnPerformanceTestCompleted?.Invoke(performanceData);
            }
            catch (System.Exception e)
            {
                result.errors.Add($"성능 테스트 중 오류: {e.Message}");
                result.isPassed = false;
            }
            
            result.endTime = Time.time;
            result.duration = result.endTime - result.startTime;
            
            testResults.Add(result);
            testSuites["Performance"].tests.Add(result);
            OnTestCompleted?.Invoke(result);
            
            Debug.Log($"성능 테스트 완료: {(result.isPassed ? "성공" : "실패")}");
            
            yield return null;
        }
        
        private IEnumerator RunXRTest()
        {
            Debug.Log("XR 테스트 시작");
            
            TestResult result = new TestResult
            {
                testName = "XR",
                testType = TestType.XR,
                startTime = Time.time,
                isPassed = true,
                errors = new List<string>()
            };
            
            try
            {
                if (xrGameManager != null)
                {
                    // XR 모드 테스트
                    var xrMode = xrGameManager.GetCurrentXRMode();
                    if (xrMode == XRMode.VR)
                    {
                        // VR 모드 테스트
                        Debug.Log("VR 모드 테스트");
                    }
                    else if (xrMode == XRMode.AR)
                    {
                        // AR 모드 테스트
                        Debug.Log("AR 모드 테스트");
                    }
                    else if (xrMode == XRMode.MR)
                    {
                        // MR 모드 테스트
                        Debug.Log("MR 모드 테스트");
                    }
                }
                else
                {
                    result.errors.Add("XR 게임 매니저가 없습니다");
                    result.isPassed = false;
                }
            }
            catch (System.Exception e)
            {
                result.errors.Add($"XR 테스트 중 오류: {e.Message}");
                result.isPassed = false;
            }
            
            result.endTime = Time.time;
            result.duration = result.endTime - result.startTime;
            
            testResults.Add(result);
            testSuites["XR"].tests.Add(result);
            OnTestCompleted?.Invoke(result);
            
            Debug.Log($"XR 테스트 완료: {(result.isPassed ? "성공" : "실패")}");
            
            yield return null;
        }
        
        private IEnumerator RunUITest()
        {
            Debug.Log("UI 테스트 시작");
            
            TestResult result = new TestResult
            {
                testName = "UI",
                testType = TestType.UI,
                startTime = Time.time,
                isPassed = true,
                errors = new List<string>()
            };
            
            try
            {
                if (uiManager != null)
                {
                    // UI 상태 테스트
                    var uiState = uiManager.GetCurrentUIState();
                    var platform = uiManager.GetCurrentPlatform();
                    
                    Debug.Log($"UI 상태: {uiState}, 플랫폼: {platform}");
                }
                else
                {
                    result.errors.Add("UI 매니저가 없습니다");
                    result.isPassed = false;
                }
            }
            catch (System.Exception e)
            {
                result.errors.Add($"UI 테스트 중 오류: {e.Message}");
                result.isPassed = false;
            }
            
            result.endTime = Time.time;
            result.duration = result.endTime - result.startTime;
            
            testResults.Add(result);
            testSuites["UI"].tests.Add(result);
            OnTestCompleted?.Invoke(result);
            
            Debug.Log($"UI 테스트 완료: {(result.isPassed ? "성공" : "실패")}");
            
            yield return null;
        }
        
        private IEnumerator RunAudioTest()
        {
            Debug.Log("오디오 테스트 시작");
            
            TestResult result = new TestResult
            {
                testName = "Audio",
                testType = TestType.Audio,
                startTime = Time.time,
                isPassed = true,
                errors = new List<string>()
            };
            
            try
            {
                if (audioManager != null)
                {
                    // 오디오 시스템 테스트
                    float masterVolume = audioManager.GetMasterVolume();
                    float musicVolume = audioManager.GetMusicVolume();
                    float sfxVolume = audioManager.GetSFXVolume();
                    
                    Debug.Log($"오디오 볼륨 - Master: {masterVolume}, Music: {musicVolume}, SFX: {sfxVolume}");
                }
                else
                {
                    result.errors.Add("오디오 매니저가 없습니다");
                    result.isPassed = false;
                }
            }
            catch (System.Exception e)
            {
                result.errors.Add($"오디오 테스트 중 오류: {e.Message}");
                result.isPassed = false;
            }
            
            result.endTime = Time.time;
            result.duration = result.endTime - result.startTime;
            
            testResults.Add(result);
            testSuites["Audio"].tests.Add(result);
            OnTestCompleted?.Invoke(result);
            
            Debug.Log($"오디오 테스트 완료: {(result.isPassed ? "성공" : "실패")}");
            
            yield return null;
        }
        
        private IEnumerator RunSaveLoadTest()
        {
            Debug.Log("저장/로드 테스트 시작");
            
            TestResult result = new TestResult
            {
                testName = "SaveLoad",
                testType = TestType.SaveLoad,
                startTime = Time.time,
                isPassed = true,
                errors = new List<string>()
            };
            
            try
            {
                if (saveSystem != null)
                {
                    // 저장 테스트
                    saveSystem.SaveGame(0);
                    yield return new WaitForSeconds(1f);
                    
                    // 로드 테스트
                    saveSystem.LoadGame(0);
                    yield return new WaitForSeconds(1f);
                    
                    Debug.Log("저장/로드 테스트 성공");
                }
                else
                {
                    result.errors.Add("저장 시스템이 없습니다");
                    result.isPassed = false;
                }
            }
            catch (System.Exception e)
            {
                result.errors.Add($"저장/로드 테스트 중 오류: {e.Message}");
                result.isPassed = false;
            }
            
            result.endTime = Time.time;
            result.duration = result.endTime - result.startTime;
            
            testResults.Add(result);
            testSuites["SaveLoad"].tests.Add(result);
            OnTestCompleted?.Invoke(result);
            
            Debug.Log($"저장/로드 테스트 완료: {(result.isPassed ? "성공" : "실패")}");
            
            yield return null;
        }
        
        private IEnumerator RunStressTest()
        {
            Debug.Log("스트레스 테스트 시작");
            
            TestResult result = new TestResult
            {
                testName = "Stress",
                testType = TestType.Stress,
                startTime = Time.time,
                isPassed = true,
                errors = new List<string>()
            };
            
            try
            {
                // 스트레스 테스트 로직
                for (int i = 0; i < 100; i++)
                {
                    // 많은 오브젝트 생성/삭제
                    GameObject testObject = new GameObject($"StressTest_{i}");
                    yield return new WaitForSeconds(0.01f);
                    Destroy(testObject);
                }
                
                Debug.Log("스트레스 테스트 성공");
            }
            catch (System.Exception e)
            {
                result.errors.Add($"스트레스 테스트 중 오류: {e.Message}");
                result.isPassed = false;
            }
            
            result.endTime = Time.time;
            result.duration = result.endTime - result.startTime;
            
            testResults.Add(result);
            testSuites["Stress"].tests.Add(result);
            OnTestCompleted?.Invoke(result);
            
            Debug.Log($"스트레스 테스트 완료: {(result.isPassed ? "성공" : "실패")}");
            
            yield return null;
        }
        
        private void StartAutoTesting()
        {
            isAutoTestRunning = true;
            currentTestIteration = 0;
            StartCoroutine(AutoTestCoroutine());
        }
        
        private IEnumerator AutoTestCoroutine()
        {
            while (isAutoTestRunning && currentTestIteration < maxTestIterations)
            {
                yield return StartCoroutine(RunAllTests());
                currentTestIteration++;
                yield return new WaitForSeconds(10f); // 10초 대기
            }
            
            CompleteAutoTest();
        }
        
        private void CompleteAutoTest()
        {
            isAutoTestRunning = false;
            Debug.Log($"자동 테스트 완료: {currentTestIteration}회 실행");
        }
        
        private void CompleteCurrentTest()
        {
            isTestRunning = false;
            Debug.Log("현재 테스트 완료");
        }
        
        private void SaveTestResults()
        {
            // 테스트 결과 저장
            Debug.Log("테스트 결과 저장");
        }
        
        private void ToggleDebugUI()
        {
            // 디버그 UI 토글
            Debug.Log("디버그 UI 토글");
        }
        
        // 공개 메서드들
        public bool IsTestingInitialized()
        {
            return isTestingInitialized;
        }
        
        public bool IsTestRunning()
        {
            return isTestRunning;
        }
        
        public bool IsAutoTestRunning()
        {
            return isAutoTestRunning;
        }
        
        public List<TestResult> GetTestResults()
        {
            return testResults;
        }
        
        public Dictionary<string, TestSuite> GetTestSuites()
        {
            return testSuites;
        }
        
        public PerformanceTestData GetPerformanceData()
        {
            return performanceData;
        }
        
        public void SetTestingEnabled(bool enabled)
        {
            enableTesting = enabled;
        }
        
        public void SetAutoTestingEnabled(bool enabled)
        {
            enableAutoTesting = enabled;
        }
        
        public void SetDebugModeEnabled(bool enabled)
        {
            enableDebugMode = enabled;
        }
    }
    
    // 테스트 데이터 구조체들
    [System.Serializable]
    public class TestResult
    {
        public string testName;
        public TestType testType;
        public float startTime;
        public float endTime;
        public float duration;
        public bool isPassed;
        public List<string> errors;
    }
    
    [System.Serializable]
    public class TestSuite
    {
        public string suiteName;
        public string description;
        public List<TestResult> tests;
        public bool isCompleted;
        public float startTime;
        public float endTime;
    }
    
    [System.Serializable]
    public class PerformanceTestData
    {
        public float averageFPS;
        public float averageMemoryUsage;
        public float testDuration;
        public int testIterations;
    }
    
    public enum TestType
    {
        Functionality,
        Performance,
        XR,
        UI,
        Audio,
        SaveLoad,
        Network,
        Stress
    }
}