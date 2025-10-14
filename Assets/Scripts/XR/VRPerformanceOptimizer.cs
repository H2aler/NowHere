using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace NowHere.XR
{
    /// <summary>
    /// VR 성능 최적화를 관리하는 클래스
    /// VR 환경에서 안정적인 성능을 위한 렌더링 및 시스템 최적화
    /// </summary>
    public class VRPerformanceOptimizer : MonoBehaviour
    {
        [Header("VR Performance Settings")]
        [SerializeField] private bool enablePerformanceOptimization = true;
        [SerializeField] private VRPerformanceMode performanceMode = VRPerformanceMode.Balanced;
        [SerializeField] private float targetFrameRate = 90f;
        [SerializeField] private bool enableAdaptiveQuality = true;
        
        [Header("Rendering Settings")]
        [SerializeField] private int maxTextureSize = 2048;
        [SerializeField] private int maxLODLevel = 2;
        [SerializeField] private float renderScale = 1.0f;
        [SerializeField] private bool enableMSAA = true;
        [SerializeField] private int msaaLevel = 4;
        
        [Header("LOD Settings")]
        [SerializeField] private float lodBias = 1.0f;
        [SerializeField] private float maxLODDistance = 100f;
        [SerializeField] private bool enableLODCulling = true;
        
        [Header("Culling Settings")]
        [SerializeField] private bool enableOcclusionCulling = true;
        [SerializeField] private bool enableFrustumCulling = true;
        [SerializeField] private float cullingDistance = 200f;
        
        [Header("Physics Settings")]
        [SerializeField] private int maxPhysicsSteps = 4;
        [SerializeField] private float physicsTimeStep = 0.02f;
        [SerializeField] private bool enablePhysicsOptimization = true;
        
        [Header("Audio Settings")]
        [SerializeField] private int maxAudioSources = 32;
        [SerializeField] private bool enableAudioOptimization = true;
        [SerializeField] private float audioDistance = 50f;
        
        // 성능 모니터링
        private float currentFrameRate = 0f;
        private float averageFrameRate = 0f;
        private float frameRateSum = 0f;
        private int frameCount = 0;
        private float lastFrameTime = 0f;
        
        // 적응형 품질 관리
        private float performanceTimer = 0f;
        private float performanceCheckInterval = 2f;
        private int qualityLevel = 2; // 0: Low, 1: Medium, 2: High
        
        // 원본 설정 저장
        private float originalRenderScale;
        private int originalMaxTextureSize;
        private int originalMaxLODLevel;
        private bool originalMSAA;
        private int originalMSAALevel;
        
        // 참조
        private Camera xrCamera;
        private AudioListener audioListener;
        
        // 이벤트
        public System.Action<float> OnFrameRateChanged;
        public System.Action<VRPerformanceMode> OnPerformanceModeChanged;
        public System.Action<int> OnQualityLevelChanged;
        
        // 싱글톤 패턴
        public static VRPerformanceOptimizer Instance { get; private set; }
        
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
            InitializeVRPerformanceOptimizer();
        }
        
        private void Update()
        {
            if (!enablePerformanceOptimization) return;
            
            UpdatePerformanceMonitoring();
            UpdateAdaptiveQuality();
        }
        
        private void InitializeVRPerformanceOptimizer()
        {
            Debug.Log("VR 성능 최적화 시스템 초기화 시작...");
            
            // 컴포넌트 참조
            xrCamera = Camera.main;
            if (xrCamera == null)
            {
                xrCamera = FindObjectOfType<Camera>();
            }
            
            audioListener = FindObjectOfType<AudioListener>();
            
            // 원본 설정 저장
            SaveOriginalSettings();
            
            // VR 성능 설정 적용
            ApplyVRPerformanceSettings();
            
            // XR 설정 최적화
            OptimizeXRSettings();
            
            // 렌더링 파이프라인 최적화
            OptimizeRenderingPipeline();
            
            // 물리 시스템 최적화
            OptimizePhysicsSystem();
            
            // 오디오 시스템 최적화
            OptimizeAudioSystem();
            
            Debug.Log("VR 성능 최적화 시스템이 초기화되었습니다.");
        }
        
        private void SaveOriginalSettings()
        {
            // 원본 설정 저장
            originalRenderScale = XRSettings.renderScale;
            originalMaxTextureSize = QualitySettings.masterTextureLimit;
            originalMaxLODLevel = QualitySettings.maximumLODLevel;
            originalMSAA = QualitySettings.antiAliasing > 0;
            originalMSAALevel = QualitySettings.antiAliasing;
        }
        
        private void ApplyVRPerformanceSettings()
        {
            // 성능 모드에 따른 설정 적용
            switch (performanceMode)
            {
                case VRPerformanceMode.Low:
                    ApplyLowPerformanceSettings();
                    break;
                case VRPerformanceMode.Balanced:
                    ApplyBalancedPerformanceSettings();
                    break;
                case VRPerformanceMode.High:
                    ApplyHighPerformanceSettings();
                    break;
            }
            
            // 렌더 스케일 설정
            XRSettings.renderScale = renderScale;
            
            // 텍스처 크기 제한
            QualitySettings.masterTextureLimit = maxTextureSize;
            
            // LOD 레벨 설정
            QualitySettings.maximumLODLevel = maxLODLevel;
            QualitySettings.lodBias = lodBias;
            
            // MSAA 설정
            if (enableMSAA)
            {
                QualitySettings.antiAliasing = msaaLevel;
            }
            else
            {
                QualitySettings.antiAliasing = 0;
            }
            
            // 프레임 레이트 설정
            Application.targetFrameRate = Mathf.RoundToInt(targetFrameRate);
            
            Debug.Log($"VR 성능 설정이 적용되었습니다. 모드: {performanceMode}");
        }
        
        private void ApplyLowPerformanceSettings()
        {
            renderScale = 0.7f;
            maxTextureSize = 1024;
            maxLODLevel = 1;
            enableMSAA = false;
            msaaLevel = 0;
            lodBias = 0.5f;
            qualityLevel = 0;
        }
        
        private void ApplyBalancedPerformanceSettings()
        {
            renderScale = 1.0f;
            maxTextureSize = 2048;
            maxLODLevel = 2;
            enableMSAA = true;
            msaaLevel = 2;
            lodBias = 1.0f;
            qualityLevel = 1;
        }
        
        private void ApplyHighPerformanceSettings()
        {
            renderScale = 1.2f;
            maxTextureSize = 4096;
            maxLODLevel = 3;
            enableMSAA = true;
            msaaLevel = 4;
            lodBias = 1.5f;
            qualityLevel = 2;
        }
        
        private void OptimizeXRSettings()
        {
            // XR 특화 설정
            XRSettings.eyeTextureResolutionScale = renderScale;
            XRSettings.renderViewportScale = renderScale;
            
            // XR 디바이스별 최적화
            if (XRSettings.loadedDeviceName.Contains("oculus"))
            {
                OptimizeOculusSettings();
            }
            else if (XRSettings.loadedDeviceName.Contains("openxr"))
            {
                OptimizeOpenXRSettings();
            }
            
            Debug.Log("XR 설정이 최적화되었습니다.");
        }
        
        private void OptimizeOculusSettings()
        {
            // Oculus 특화 최적화
            // 실제 구현에서는 Oculus SDK API 사용
            Debug.Log("Oculus 설정이 최적화되었습니다.");
        }
        
        private void OptimizeOpenXRSettings()
        {
            // OpenXR 특화 최적화
            // 실제 구현에서는 OpenXR API 사용
            Debug.Log("OpenXR 설정이 최적화되었습니다.");
        }
        
        private void OptimizeRenderingPipeline()
        {
            // 렌더링 파이프라인 최적화
            if (xrCamera != null)
            {
                // 카메라 설정 최적화
                xrCamera.allowMSAA = enableMSAA;
                xrCamera.allowHDR = false; // VR에서는 HDR 비활성화 권장
                
                // 컬링 설정
                if (enableFrustumCulling)
                {
                    xrCamera.cullingMask = -1; // 모든 레이어 렌더링
                }
                
                // 거리 기반 컬링
                xrCamera.farClipPlane = cullingDistance;
            }
            
            // 렌더링 품질 설정
            QualitySettings.shadowResolution = ShadowResolution.Medium;
            QualitySettings.shadowDistance = 50f;
            QualitySettings.shadowCascades = 2;
            QualitySettings.shadowProjection = ShadowProjection.CloseFit;
            
            // 텍스처 압축 설정
            QualitySettings.textureQuality = qualityLevel;
            
            // 파티클 설정
            QualitySettings.particleRaycastBudget = 256;
            
            Debug.Log("렌더링 파이프라인이 최적화되었습니다.");
        }
        
        private void OptimizePhysicsSystem()
        {
            if (!enablePhysicsOptimization) return;
            
            // 물리 시스템 최적화
            Time.fixedDeltaTime = physicsTimeStep;
            Physics.defaultSolverIterations = maxPhysicsSteps;
            Physics.defaultSolverVelocityIterations = 1;
            
            // 물리 레이어 설정
            Physics.queriesHitTriggers = false; // 트리거 히트 비활성화로 성능 향상
            
            // 물리 업데이트 빈도 조절
            Physics.autoSimulation = true;
            Physics.autoSyncTransforms = false; // 수동 동기화로 성능 향상
            
            Debug.Log("물리 시스템이 최적화되었습니다.");
        }
        
        private void OptimizeAudioSystem()
        {
            if (!enableAudioOptimization) return;
            
            // 오디오 시스템 최적화
            AudioSettings.GetConfiguration();
            
            // 오디오 소스 제한
            // 실제 구현에서는 오디오 매니저를 통해 관리
            
            Debug.Log("오디오 시스템이 최적화되었습니다.");
        }
        
        private void UpdatePerformanceMonitoring()
        {
            // 프레임 레이트 모니터링
            float currentTime = Time.time;
            float deltaTime = currentTime - lastFrameTime;
            
            if (deltaTime > 0)
            {
                currentFrameRate = 1f / deltaTime;
                frameRateSum += currentFrameRate;
                frameCount++;
                
                if (frameCount >= 60) // 1초마다 평균 계산
                {
                    averageFrameRate = frameRateSum / frameCount;
                    frameRateSum = 0f;
                    frameCount = 0;
                    
                    OnFrameRateChanged?.Invoke(averageFrameRate);
                }
            }
            
            lastFrameTime = currentTime;
        }
        
        private void UpdateAdaptiveQuality()
        {
            if (!enableAdaptiveQuality) return;
            
            performanceTimer += Time.deltaTime;
            
            if (performanceTimer >= performanceCheckInterval)
            {
                CheckAndAdjustQuality();
                performanceTimer = 0f;
            }
        }
        
        private void CheckAndAdjustQuality()
        {
            // 성능에 따른 품질 자동 조절
            if (averageFrameRate < targetFrameRate * 0.8f)
            {
                // 성능이 낮으면 품질 감소
                if (qualityLevel > 0)
                {
                    qualityLevel--;
                    ApplyQualityLevel(qualityLevel);
                    Debug.Log($"성능 저하 감지. 품질 레벨 감소: {qualityLevel}");
                }
            }
            else if (averageFrameRate > targetFrameRate * 1.1f)
            {
                // 성능이 좋으면 품질 증가
                if (qualityLevel < 2)
                {
                    qualityLevel++;
                    ApplyQualityLevel(qualityLevel);
                    Debug.Log($"성능 여유 감지. 품질 레벨 증가: {qualityLevel}");
                }
            }
        }
        
        private void ApplyQualityLevel(int level)
        {
            switch (level)
            {
                case 0: // Low
                    renderScale = 0.7f;
                    maxTextureSize = 1024;
                    enableMSAA = false;
                    break;
                case 1: // Medium
                    renderScale = 1.0f;
                    maxTextureSize = 2048;
                    enableMSAA = true;
                    msaaLevel = 2;
                    break;
                case 2: // High
                    renderScale = 1.2f;
                    maxTextureSize = 4096;
                    enableMSAA = true;
                    msaaLevel = 4;
                    break;
            }
            
            // 설정 적용
            XRSettings.renderScale = renderScale;
            QualitySettings.masterTextureLimit = maxTextureSize;
            QualitySettings.antiAliasing = enableMSAA ? msaaLevel : 0;
            
            OnQualityLevelChanged?.Invoke(level);
        }
        
        public void SetPerformanceMode(VRPerformanceMode mode)
        {
            performanceMode = mode;
            ApplyVRPerformanceSettings();
            OnPerformanceModeChanged?.Invoke(mode);
            
            Debug.Log($"성능 모드가 변경되었습니다: {mode}");
        }
        
        public void SetRenderScale(float scale)
        {
            renderScale = Mathf.Clamp(scale, 0.5f, 2.0f);
            XRSettings.renderScale = renderScale;
            
            Debug.Log($"렌더 스케일이 설정되었습니다: {renderScale}");
        }
        
        public void SetTargetFrameRate(float frameRate)
        {
            targetFrameRate = Mathf.Clamp(frameRate, 60f, 120f);
            Application.targetFrameRate = Mathf.RoundToInt(targetFrameRate);
            
            Debug.Log($"목표 프레임 레이트가 설정되었습니다: {targetFrameRate}");
        }
        
        public void EnableAdaptiveQuality(bool enable)
        {
            enableAdaptiveQuality = enable;
            Debug.Log($"적응형 품질이 {(enable ? "활성화" : "비활성화")}되었습니다.");
        }
        
        public float GetCurrentFrameRate()
        {
            return currentFrameRate;
        }
        
        public float GetAverageFrameRate()
        {
            return averageFrameRate;
        }
        
        public VRPerformanceMode GetPerformanceMode()
        {
            return performanceMode;
        }
        
        public int GetQualityLevel()
        {
            return qualityLevel;
        }
        
        public void ResetToOriginalSettings()
        {
            // 원본 설정으로 복원
            XRSettings.renderScale = originalRenderScale;
            QualitySettings.masterTextureLimit = originalMaxTextureSize;
            QualitySettings.maximumLODLevel = originalMaxLODLevel;
            QualitySettings.antiAliasing = originalMSAA ? originalMSAALevel : 0;
            
            Debug.Log("원본 설정으로 복원되었습니다.");
        }
        
        private void OnDestroy()
        {
            // 원본 설정으로 복원
            ResetToOriginalSettings();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            // 앱 일시정지 시 성능 최적화
            if (pauseStatus)
            {
                // 백그라운드에서 성능 최적화
                SetPerformanceMode(VRPerformanceMode.Low);
            }
            else
            {
                // 포그라운드 복귀 시 원래 설정 복원
                SetPerformanceMode(VRPerformanceMode.Balanced);
            }
        }
    }
    
    public enum VRPerformanceMode
    {
        Low,        // 최고 성능, 낮은 품질
        Balanced,   // 균형잡힌 성능과 품질
        High        // 최고 품질, 높은 성능 요구
    }
}
