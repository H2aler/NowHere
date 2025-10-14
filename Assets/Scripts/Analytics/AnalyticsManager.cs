using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using NowHere.Game;
using NowHere.XR;

namespace NowHere.Analytics
{
    /// <summary>
    /// 게임 분석 및 통계 시스템
    /// 플레이어 행동, 게임 성능, 사용자 경험을 분석
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        [Header("Analytics Settings")]
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool enableRealTimeAnalytics = true;
        [SerializeField] private bool enablePerformanceAnalytics = true;
        [SerializeField] private bool enableUserBehaviorAnalytics = true;
        [SerializeField] private bool enableXRAnalytics = true;
        
        [Header("Data Collection")]
        [SerializeField] private float dataCollectionInterval = 60f; // 1분마다
        [SerializeField] private int maxDataBufferSize = 1000;
        [SerializeField] private bool enableDataCompression = true;
        [SerializeField] private bool enableDataEncryption = true;
        
        [Header("Analytics Services")]
        [SerializeField] private bool enableGoogleAnalytics = false;
        [SerializeField] private bool enableFirebaseAnalytics = false;
        [SerializeField] private bool enableCustomAnalytics = true;
        [SerializeField] private string analyticsServerURL = "https://analytics.nowhere.com/api/";
        
        [Header("Privacy Settings")]
        [SerializeField] private bool enableDataAnonymization = true;
        [SerializeField] private bool enableConsentManagement = true;
        [SerializeField] private bool enableDataRetention = true;
        [SerializeField] private int dataRetentionDays = 365;
        
        // 분석 시스템 상태
        private bool isAnalyticsInitialized = false;
        private bool isDataCollectionActive = false;
        private bool hasUserConsent = false;
        
        // 데이터 관리
        private List<AnalyticsEvent> eventBuffer = new List<AnalyticsEvent>();
        private Dictionary<string, AnalyticsMetric> metrics = new Dictionary<string, AnalyticsMetric>();
        private Dictionary<string, UserSession> userSessions = new Dictionary<string, UserSession>();
        
        // 성능 데이터
        private PerformanceData performanceData = new PerformanceData();
        private XRPerformanceData xrPerformanceData = new XRPerformanceData();
        
        // 참조
        private GameManager gameManager;
        private XRGameManager xrGameManager;
        private AudioManager audioManager;
        private UIManager uiManager;
        
        // 이벤트
        public System.Action<AnalyticsEvent> OnEventRecorded;
        public System.Action<AnalyticsMetric> OnMetricUpdated;
        public System.Action<UserSession> OnSessionStarted;
        public System.Action<UserSession> OnSessionEnded;
        public System.Action<PerformanceData> OnPerformanceDataUpdated;
        public System.Action<bool> OnConsentChanged;
        
        // 싱글톤 패턴
        public static AnalyticsManager Instance { get; private set; }
        
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
            InitializeAnalytics();
        }
        
        private void Update()
        {
            if (isAnalyticsInitialized && isDataCollectionActive)
            {
                UpdateAnalytics();
            }
        }
        
        private void InitializeAnalytics()
        {
            if (!enableAnalytics) return;
            
            Debug.Log("Analytics Manager 초기화 시작...");
            
            // 컴포넌트 참조
            gameManager = FindObjectOfType<GameManager>();
            xrGameManager = FindObjectOfType<XRGameManager>();
            audioManager = FindObjectOfType<AudioManager>();
            uiManager = FindObjectOfType<UIManager>();
            
            // 사용자 동의 확인
            if (enableConsentManagement)
            {
                CheckUserConsent();
            }
            else
            {
                hasUserConsent = true;
            }
            
            // 분석 서비스 초기화
            InitializeAnalyticsServices();
            
            // 이벤트 구독
            SubscribeToEvents();
            
            // 데이터 수집 시작
            if (hasUserConsent)
            {
                StartDataCollection();
            }
            
            isAnalyticsInitialized = true;
            Debug.Log("Analytics Manager 초기화 완료");
        }
        
        private void CheckUserConsent()
        {
            // 사용자 동의 확인
            // 실제 구현에서는 GDPR, CCPA 등 개인정보 보호 규정에 따른 동의 관리
            hasUserConsent = PlayerPrefs.GetInt("AnalyticsConsent", 0) == 1;
            
            if (!hasUserConsent)
            {
                // 동의 요청 UI 표시
                RequestUserConsent();
            }
        }
        
        private void RequestUserConsent()
        {
            // 사용자 동의 요청
            // 실제 구현에서는 동의 다이얼로그 표시
            Debug.Log("사용자 동의 요청");
        }
        
        public void SetUserConsent(bool consent)
        {
            hasUserConsent = consent;
            PlayerPrefs.SetInt("AnalyticsConsent", consent ? 1 : 0);
            
            if (consent)
            {
                StartDataCollection();
            }
            else
            {
                StopDataCollection();
            }
            
            OnConsentChanged?.Invoke(consent);
        }
        
        private void InitializeAnalyticsServices()
        {
            // Google Analytics 초기화
            if (enableGoogleAnalytics)
            {
                InitializeGoogleAnalytics();
            }
            
            // Firebase Analytics 초기화
            if (enableFirebaseAnalytics)
            {
                InitializeFirebaseAnalytics();
            }
            
            // 커스텀 분석 서비스 초기화
            if (enableCustomAnalytics)
            {
                InitializeCustomAnalytics();
            }
        }
        
        private void InitializeGoogleAnalytics()
        {
            // Google Analytics 초기화
            Debug.Log("Google Analytics 초기화");
        }
        
        private void InitializeFirebaseAnalytics()
        {
            // Firebase Analytics 초기화
            Debug.Log("Firebase Analytics 초기화");
        }
        
        private void InitializeCustomAnalytics()
        {
            // 커스텀 분석 서비스 초기화
            Debug.Log("커스텀 분석 서비스 초기화");
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
            
            // 오디오 매니저 이벤트 구독
            if (audioManager != null)
            {
                audioManager.OnMusicChanged += OnMusicChanged;
                audioManager.OnVolumeChanged += OnVolumeChanged;
            }
            
            // UI 매니저 이벤트 구독
            if (uiManager != null)
            {
                uiManager.OnUIStateChanged += OnUIStateChanged;
                uiManager.OnPlatformChanged += OnPlatformChanged;
            }
        }
        
        private void StartDataCollection()
        {
            isDataCollectionActive = true;
            
            // 사용자 세션 시작
            StartUserSession();
            
            // 주기적 데이터 수집 시작
            StartCoroutine(PeriodicDataCollection());
            
            Debug.Log("데이터 수집 시작");
        }
        
        private void StopDataCollection()
        {
            isDataCollectionActive = false;
            
            // 사용자 세션 종료
            EndUserSession();
            
            Debug.Log("데이터 수집 중지");
        }
        
        private void StartUserSession()
        {
            string sessionId = System.Guid.NewGuid().ToString();
            UserSession session = new UserSession
            {
                sessionId = sessionId,
                startTime = DateTime.Now,
                platform = Application.platform.ToString(),
                deviceModel = SystemInfo.deviceModel,
                deviceType = SystemInfo.deviceType.ToString(),
                operatingSystem = SystemInfo.operatingSystem,
                isActive = true
            };
            
            userSessions[sessionId] = session;
            OnSessionStarted?.Invoke(session);
            
            // 세션 시작 이벤트 기록
            RecordEvent("session_started", new Dictionary<string, object>
            {
                {"session_id", sessionId},
                {"platform", session.platform},
                {"device_model", session.deviceModel}
            });
        }
        
        private void EndUserSession()
        {
            foreach (var kvp in userSessions)
            {
                if (kvp.Value.isActive)
                {
                    kvp.Value.endTime = DateTime.Now;
                    kvp.Value.duration = (float)(kvp.Value.endTime - kvp.Value.startTime).TotalSeconds;
                    kvp.Value.isActive = false;
                    
                    OnSessionEnded?.Invoke(kvp.Value);
                    
                    // 세션 종료 이벤트 기록
                    RecordEvent("session_ended", new Dictionary<string, object>
                    {
                        {"session_id", kvp.Key},
                        {"duration", kvp.Value.duration}
                    });
                }
            }
        }
        
        private IEnumerator PeriodicDataCollection()
        {
            while (isDataCollectionActive)
            {
                yield return new WaitForSeconds(dataCollectionInterval);
                
                // 주기적 데이터 수집
                CollectPerformanceData();
                CollectUserBehaviorData();
                CollectXRData();
                
                // 데이터 전송
                if (eventBuffer.Count > 0)
                {
                    SendAnalyticsData();
                }
            }
        }
        
        private void UpdateAnalytics()
        {
            // 실시간 분석 업데이트
            if (enableRealTimeAnalytics)
            {
                UpdateRealTimeMetrics();
            }
        }
        
        private void UpdateRealTimeMetrics()
        {
            // 실시간 메트릭 업데이트
            UpdateMetric("fps", 1f / Time.deltaTime);
            UpdateMetric("memory_usage", System.GC.GetTotalMemory(false) / 1024f / 1024f);
            UpdateMetric("play_time", Time.time);
        }
        
        private void CollectPerformanceData()
        {
            if (!enablePerformanceAnalytics) return;
            
            // 성능 데이터 수집
            performanceData.fps = 1f / Time.deltaTime;
            performanceData.memoryUsage = System.GC.GetTotalMemory(false) / 1024f / 1024f;
            performanceData.cpuUsage = SystemInfo.processorCount;
            performanceData.gpuMemory = SystemInfo.graphicsMemorySize;
            performanceData.batteryLevel = SystemInfo.batteryLevel;
            performanceData.batteryStatus = SystemInfo.batteryStatus.ToString();
            
            OnPerformanceDataUpdated?.Invoke(performanceData);
            
            // 성능 이벤트 기록
            RecordEvent("performance_data", new Dictionary<string, object>
            {
                {"fps", performanceData.fps},
                {"memory_usage", performanceData.memoryUsage},
                {"battery_level", performanceData.batteryLevel}
            });
        }
        
        private void CollectUserBehaviorData()
        {
            if (!enableUserBehaviorAnalytics) return;
            
            // 사용자 행동 데이터 수집
            // 실제 구현에서는 게임 내 사용자 행동 추적
        }
        
        private void CollectXRData()
        {
            if (!enableXRAnalytics || xrGameManager == null) return;
            
            // XR 데이터 수집
            xrPerformanceData.xrMode = xrGameManager.GetCurrentXRMode().ToString();
            xrPerformanceData.isXRInitialized = xrGameManager.IsXRGameInitialized();
            
            // XR 이벤트 기록
            RecordEvent("xr_data", new Dictionary<string, object>
            {
                {"xr_mode", xrPerformanceData.xrMode},
                {"xr_initialized", xrPerformanceData.isXRInitialized}
            });
        }
        
        public void RecordEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!isAnalyticsInitialized || !hasUserConsent) return;
            
            AnalyticsEvent analyticsEvent = new AnalyticsEvent
            {
                eventName = eventName,
                parameters = parameters ?? new Dictionary<string, object>(),
                timestamp = DateTime.Now,
                sessionId = GetCurrentSessionId()
            };
            
            eventBuffer.Add(analyticsEvent);
            OnEventRecorded?.Invoke(analyticsEvent);
            
            // 버퍼 크기 제한
            if (eventBuffer.Count > maxDataBufferSize)
            {
                eventBuffer.RemoveAt(0);
            }
            
            Debug.Log($"이벤트 기록: {eventName}");
        }
        
        public void UpdateMetric(string metricName, float value)
        {
            if (!isAnalyticsInitialized || !hasUserConsent) return;
            
            if (!metrics.ContainsKey(metricName))
            {
                metrics[metricName] = new AnalyticsMetric
                {
                    metricName = metricName,
                    value = value,
                    count = 1,
                    minValue = value,
                    maxValue = value,
                    averageValue = value
                };
            }
            else
            {
                AnalyticsMetric metric = metrics[metricName];
                metric.count++;
                metric.value = value;
                metric.minValue = Mathf.Min(metric.minValue, value);
                metric.maxValue = Mathf.Max(metric.maxValue, value);
                metric.averageValue = (metric.averageValue * (metric.count - 1) + value) / metric.count;
            }
            
            OnMetricUpdated?.Invoke(metrics[metricName]);
        }
        
        private string GetCurrentSessionId()
        {
            foreach (var kvp in userSessions)
            {
                if (kvp.Value.isActive)
                {
                    return kvp.Key;
                }
            }
            return null;
        }
        
        private void SendAnalyticsData()
        {
            if (eventBuffer.Count == 0) return;
            
            StartCoroutine(SendDataCoroutine());
        }
        
        private IEnumerator SendDataCoroutine()
        {
            // 데이터 전송
            List<AnalyticsEvent> eventsToSend = new List<AnalyticsEvent>(eventBuffer);
            eventBuffer.Clear();
            
            // Google Analytics 전송
            if (enableGoogleAnalytics)
            {
                yield return StartCoroutine(SendToGoogleAnalytics(eventsToSend));
            }
            
            // Firebase Analytics 전송
            if (enableFirebaseAnalytics)
            {
                yield return StartCoroutine(SendToFirebaseAnalytics(eventsToSend));
            }
            
            // 커스텀 분석 서비스 전송
            if (enableCustomAnalytics)
            {
                yield return StartCoroutine(SendToCustomAnalytics(eventsToSend));
            }
        }
        
        private IEnumerator SendToGoogleAnalytics(List<AnalyticsEvent> events)
        {
            // Google Analytics 전송
            Debug.Log($"Google Analytics로 {events.Count}개 이벤트 전송");
            yield return null;
        }
        
        private IEnumerator SendToFirebaseAnalytics(List<AnalyticsEvent> events)
        {
            // Firebase Analytics 전송
            Debug.Log($"Firebase Analytics로 {events.Count}개 이벤트 전송");
            yield return null;
        }
        
        private IEnumerator SendToCustomAnalytics(List<AnalyticsEvent> events)
        {
            // 커스텀 분석 서비스 전송
            Debug.Log($"커스텀 분석 서비스로 {events.Count}개 이벤트 전송");
            yield return null;
        }
        
        // 이벤트 핸들러들
        private void OnXRModeSwitched(XRMode mode)
        {
            RecordEvent("xr_mode_switched", new Dictionary<string, object>
            {
                {"new_mode", mode.ToString()}
            });
        }
        
        private void OnXRGameStateChanged(XRGameState state)
        {
            RecordEvent("xr_game_state_changed", new Dictionary<string, object>
            {
                {"new_state", state.ToString()}
            });
        }
        
        private void OnMusicChanged(AudioClip clip)
        {
            RecordEvent("music_changed", new Dictionary<string, object>
            {
                {"music_name", clip != null ? clip.name : "none"}
            });
        }
        
        private void OnVolumeChanged(float volume)
        {
            UpdateMetric("master_volume", volume);
        }
        
        private void OnUIStateChanged(UIState state)
        {
            RecordEvent("ui_state_changed", new Dictionary<string, object>
            {
                {"new_state", state.ToString()}
            });
        }
        
        private void OnPlatformChanged(PlatformType platform)
        {
            RecordEvent("platform_changed", new Dictionary<string, object>
            {
                {"new_platform", platform.ToString()}
            });
        }
        
        // 공개 메서드들
        public bool IsAnalyticsInitialized()
        {
            return isAnalyticsInitialized;
        }
        
        public bool HasUserConsent()
        {
            return hasUserConsent;
        }
        
        public bool IsDataCollectionActive()
        {
            return isDataCollectionActive;
        }
        
        public int GetEventBufferCount()
        {
            return eventBuffer.Count;
        }
        
        public int GetMetricsCount()
        {
            return metrics.Count;
        }
        
        public AnalyticsMetric GetMetric(string metricName)
        {
            return metrics.ContainsKey(metricName) ? metrics[metricName] : null;
        }
        
        public PerformanceData GetPerformanceData()
        {
            return performanceData;
        }
        
        public XRPerformanceData GetXRPerformanceData()
        {
            return xrPerformanceData;
        }
        
        public void FlushAnalyticsData()
        {
            if (eventBuffer.Count > 0)
            {
                SendAnalyticsData();
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // 앱 일시정지 시 데이터 플러시
                FlushAnalyticsData();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // 앱 포커스 잃을 때 데이터 플러시
                FlushAnalyticsData();
            }
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (xrGameManager != null)
            {
                xrGameManager.OnXRModeSwitched -= OnXRModeSwitched;
                xrGameManager.OnGameStateChanged -= OnXRGameStateChanged;
            }
            
            if (audioManager != null)
            {
                audioManager.OnMusicChanged -= OnMusicChanged;
                audioManager.OnVolumeChanged -= OnVolumeChanged;
            }
            
            if (uiManager != null)
            {
                uiManager.OnUIStateChanged -= OnUIStateChanged;
                uiManager.OnPlatformChanged -= OnPlatformChanged;
            }
            
            // 최종 데이터 플러시
            FlushAnalyticsData();
        }
    }
    
    // 분석 데이터 구조체들
    [System.Serializable]
    public class AnalyticsEvent
    {
        public string eventName;
        public Dictionary<string, object> parameters;
        public DateTime timestamp;
        public string sessionId;
    }
    
    [System.Serializable]
    public class AnalyticsMetric
    {
        public string metricName;
        public float value;
        public int count;
        public float minValue;
        public float maxValue;
        public float averageValue;
    }
    
    [System.Serializable]
    public class UserSession
    {
        public string sessionId;
        public DateTime startTime;
        public DateTime endTime;
        public float duration;
        public string platform;
        public string deviceModel;
        public string deviceType;
        public string operatingSystem;
        public bool isActive;
    }
    
    [System.Serializable]
    public class PerformanceData
    {
        public float fps;
        public float memoryUsage;
        public int cpuUsage;
        public int gpuMemory;
        public float batteryLevel;
        public string batteryStatus;
    }
    
    [System.Serializable]
    public class XRPerformanceData
    {
        public string xrMode;
        public bool isXRInitialized;
        public float xrFrameRate;
        public float xrLatency;
    }
}
