using UnityEngine;
using UnityEngine.Rendering;

namespace NowHere.Mobile
{
    /// <summary>
    /// 모바일 기기 최적화를 담당하는 클래스
    /// 안드로이드 기기의 성능에 맞춰 게임 설정을 조정
    /// </summary>
    public class MobileOptimizer : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private bool enableAutoOptimization = true;
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private float targetRenderScale = 1.0f;
        
        [Header("Quality Settings")]
        [SerializeField] private int maxTextureSize = 1024;
        [SerializeField] private int maxLODLevel = 2;
        [SerializeField] private float shadowDistance = 50f;
        [SerializeField] private int shadowResolution = 1024;
        
        [Header("AR Optimization")]
        [SerializeField] private bool enableAROptimization = true;
        [SerializeField] private int arTargetFrameRate = 30;
        [SerializeField] private float arRenderScale = 0.8f;
        
        [Header("Memory Management")]
        [SerializeField] private bool enableMemoryOptimization = true;
        [SerializeField] private int maxMemoryUsage = 1024; // MB
        [SerializeField] private float garbageCollectionInterval = 30f;
        
        // 성능 모니터링
        private float lastGCTime;
        private int frameCount;
        private float deltaTime;
        private float fps;
        
        // 이벤트
        public System.Action<float> OnFPSChanged;
        public System.Action<int> OnMemoryUsageChanged;
        public System.Action<PerformanceLevel> OnPerformanceLevelChanged;
        
        private void Start()
        {
            InitializeMobileOptimization();
        }
        
        private void Update()
        {
            if (enableAutoOptimization)
            {
                MonitorPerformance();
                OptimizeBasedOnPerformance();
            }
            
            // 가비지 컬렉션 관리
            if (enableMemoryOptimization && Time.time - lastGCTime > garbageCollectionInterval)
            {
                System.GC.Collect();
                lastGCTime = Time.time;
            }
        }
        
        private void InitializeMobileOptimization()
        {
            // 안드로이드 기기 정보 확인
            CheckDeviceCapabilities();
            
            // 초기 품질 설정 적용
            ApplyInitialQualitySettings();
            
            // AR 최적화 설정
            if (enableAROptimization)
            {
                ApplyAROptimization();
            }
            
            // 메모리 최적화 설정
            if (enableMemoryOptimization)
            {
                ApplyMemoryOptimization();
            }
            
            Debug.Log("모바일 최적화가 초기화되었습니다.");
        }
        
        private void CheckDeviceCapabilities()
        {
            // 기기 정보 출력
            Debug.Log($"기기 모델: {SystemInfo.deviceModel}");
            Debug.Log($"기기 이름: {SystemInfo.deviceName}");
            Debug.Log($"운영체제: {SystemInfo.operatingSystem}");
            Debug.Log($"프로세서: {SystemInfo.processorType}");
            Debug.Log($"프로세서 개수: {SystemInfo.processorCount}");
            Debug.Log($"메모리: {SystemInfo.systemMemorySize}MB");
            Debug.Log($"그래픽 카드: {SystemInfo.graphicsDeviceName}");
            Debug.Log($"그래픽 메모리: {SystemInfo.graphicsMemorySize}MB");
            Debug.Log($"그래픽 API: {SystemInfo.graphicsDeviceType}");
        }
        
        private void ApplyInitialQualitySettings()
        {
            // 프레임 레이트 설정
            Application.targetFrameRate = targetFrameRate;
            
            // 품질 설정
            QualitySettings.SetQualityLevel(GetOptimalQualityLevel());
            
            // 텍스처 품질 설정
            QualitySettings.globalTextureMipmapLimit = GetOptimalTextureLimit();
            
            // 그림자 설정
            QualitySettings.shadowDistance = shadowDistance;
            QualitySettings.shadowResolution = (ShadowResolution)shadowResolution;
            
            // 안티앨리어싱 설정
            QualitySettings.antiAliasing = GetOptimalAntiAliasing();
            
            // LOD 바이어스 설정
            QualitySettings.lodBias = GetOptimalLODBias();
        }
        
        private void ApplyAROptimization()
        {
            // AR 모드에서의 성능 최적화
            if (Application.isMobilePlatform)
            {
                // AR 모드에서는 더 낮은 프레임 레이트 사용
                Application.targetFrameRate = arTargetFrameRate;
                
                // 렌더 스케일 조정
                // Unity 6000에서는 XRSettings가 변경됨
                // XRSettings.renderScale = arRenderScale;
                
                // AR 모드에서 불필요한 효과 비활성화
                DisableNonEssentialEffects();
            }
        }
        
        private void ApplyMemoryOptimization()
        {
            // 메모리 사용량 모니터링
            int currentMemoryUsage = (int)(System.GC.GetTotalMemory(false) / (1024 * 1024));
            
            if (currentMemoryUsage > maxMemoryUsage)
            {
                // 메모리 사용량이 임계값을 초과하면 최적화 실행
                OptimizeMemoryUsage();
            }
        }
        
        private void MonitorPerformance()
        {
            // FPS 계산
            frameCount++;
            deltaTime += Time.deltaTime;
            
            if (deltaTime >= 1.0f)
            {
                fps = frameCount / deltaTime;
                frameCount = 0;
                deltaTime = 0;
                
                OnFPSChanged?.Invoke(fps);
            }
            
            // 메모리 사용량 모니터링
            int memoryUsage = (int)(System.GC.GetTotalMemory(false) / (1024 * 1024));
            OnMemoryUsageChanged?.Invoke(memoryUsage);
        }
        
        private void OptimizeBasedOnPerformance()
        {
            PerformanceLevel currentLevel = GetPerformanceLevel();
            
            if (fps < 30f)
            {
                // FPS가 낮으면 품질을 낮춤
                if (currentLevel != PerformanceLevel.Low)
                {
                    SetPerformanceLevel(PerformanceLevel.Low);
                }
            }
            else if (fps < 45f)
            {
                // FPS가 중간이면 중간 품질
                if (currentLevel != PerformanceLevel.Medium)
                {
                    SetPerformanceLevel(PerformanceLevel.Medium);
                }
            }
            else if (fps > 55f)
            {
                // FPS가 높으면 고품질
                if (currentLevel != PerformanceLevel.High)
                {
                    SetPerformanceLevel(PerformanceLevel.High);
                }
            }
        }
        
        private PerformanceLevel GetPerformanceLevel()
        {
            if (fps < 30f)
                return PerformanceLevel.Low;
            else if (fps < 45f)
                return PerformanceLevel.Medium;
            else
                return PerformanceLevel.High;
        }
        
        private void SetPerformanceLevel(PerformanceLevel level)
        {
            switch (level)
            {
                case PerformanceLevel.Low:
                    SetLowQualitySettings();
                    break;
                case PerformanceLevel.Medium:
                    SetMediumQualitySettings();
                    break;
                case PerformanceLevel.High:
                    SetHighQualitySettings();
                    break;
            }
            
            OnPerformanceLevelChanged?.Invoke(level);
            Debug.Log($"성능 레벨이 {level}로 변경되었습니다.");
        }
        
        private void SetLowQualitySettings()
        {
            QualitySettings.SetQualityLevel(0);
            QualitySettings.globalTextureMipmapLimit = 2;
            QualitySettings.shadowDistance = 20f;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            QualitySettings.antiAliasing = 0;
            QualitySettings.lodBias = 0.5f;
            Application.targetFrameRate = 30;
        }
        
        private void SetMediumQualitySettings()
        {
            QualitySettings.SetQualityLevel(2);
            QualitySettings.globalTextureMipmapLimit = 1;
            QualitySettings.shadowDistance = 40f;
            QualitySettings.shadowResolution = ShadowResolution.Medium;
            QualitySettings.antiAliasing = 2;
            QualitySettings.lodBias = 1.0f;
            Application.targetFrameRate = 45;
        }
        
        private void SetHighQualitySettings()
        {
            QualitySettings.SetQualityLevel(4);
            QualitySettings.globalTextureMipmapLimit = 0;
            QualitySettings.shadowDistance = 80f;
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.antiAliasing = 4;
            QualitySettings.lodBias = 1.5f;
            Application.targetFrameRate = 60;
        }
        
        private int GetOptimalQualityLevel()
        {
            // 기기 성능에 따른 품질 레벨 결정
            if (SystemInfo.systemMemorySize < 2048) // 2GB 미만
                return 0; // Low
            else if (SystemInfo.systemMemorySize < 4096) // 4GB 미만
                return 2; // Medium
            else
                return 4; // High
        }
        
        private int GetOptimalTextureLimit()
        {
            if (SystemInfo.graphicsMemorySize < 1024) // 1GB 미만
                return 2;
            else if (SystemInfo.graphicsMemorySize < 2048) // 2GB 미만
                return 1;
            else
                return 0;
        }
        
        private int GetOptimalAntiAliasing()
        {
            if (SystemInfo.graphicsMemorySize < 1024)
                return 0;
            else if (SystemInfo.graphicsMemorySize < 2048)
                return 2;
            else
                return 4;
        }
        
        private float GetOptimalLODBias()
        {
            if (SystemInfo.graphicsMemorySize < 1024)
                return 0.5f;
            else if (SystemInfo.graphicsMemorySize < 2048)
                return 1.0f;
            else
                return 1.5f;
        }
        
        private void DisableNonEssentialEffects()
        {
            // AR 모드에서 불필요한 효과 비활성화
            QualitySettings.softVegetation = false;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.billboardsFaceCameraPosition = false;
        }
        
        private void OptimizeMemoryUsage()
        {
            // 메모리 사용량 최적화
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            
            Debug.Log("메모리 사용량 최적화가 실행되었습니다.");
        }
        
        public void SetTargetFrameRate(int frameRate)
        {
            targetFrameRate = frameRate;
            Application.targetFrameRate = frameRate;
        }
        
        public void SetRenderScale(float scale)
        {
            targetRenderScale = scale;
            // Unity 6000에서는 XRSettings가 변경됨
            // XRSettings.renderScale = scale;
        }
        
        public float GetCurrentFPS()
        {
            return fps;
        }
        
        public int GetCurrentMemoryUsage()
        {
            return (int)(System.GC.GetTotalMemory(false) / (1024 * 1024));
        }
        
        public PerformanceLevel GetCurrentPerformanceLevel()
        {
            return GetPerformanceLevel();
        }
    }
    
    public enum PerformanceLevel
    {
        Low,
        Medium,
        High
    }
}
