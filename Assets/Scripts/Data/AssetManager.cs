using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using NowHere.Game;

namespace NowHere.Data
{
    /// <summary>
    /// 에셋 동적 로딩 및 관리 시스템
    /// 메모리 효율적인 에셋 로딩과 언로딩을 관리
    /// </summary>
    public class AssetManager : MonoBehaviour
    {
        [Header("Asset Loading Settings")]
        [SerializeField] private bool enableAssetStreaming = true;
        [SerializeField] private bool enableAssetCaching = true;
        [SerializeField] private int maxCacheSize = 100;
        [SerializeField] private float cacheTimeout = 300f; // 5분
        [SerializeField] private bool enableAssetCompression = true;
        
        [Header("Asset Paths")]
        [SerializeField] private string localAssetPath = "Assets/StreamingAssets/";
        [SerializeField] private string remoteAssetPath = "https://assets.nowhere.com/";
        [SerializeField] private string cachePath = "AssetCache/";
        
        [Header("Asset Categories")]
        [SerializeField] private string[] modelPaths = { "Models/", "Characters/", "Weapons/" };
        [SerializeField] private string[] texturePaths = { "Textures/", "UI/", "Effects/" };
        [SerializeField] private string[] audioPaths = { "Audio/", "Music/", "SFX/" };
        [SerializeField] private string[] prefabPaths = { "Prefabs/", "UI/", "Effects/" };
        
        [Header("Loading Settings")]
        [SerializeField] private int maxConcurrentLoads = 3;
        [SerializeField] private float loadingTimeout = 30f;
        [SerializeField] private bool enableLoadingProgress = true;
        [SerializeField] private bool enableLoadingAnimations = true;
        
        // 에셋 관리 상태
        private bool isAssetManagerInitialized = false;
        private Dictionary<string, AssetData> loadedAssets = new Dictionary<string, AssetData>();
        private Dictionary<string, AssetData> assetCache = new Dictionary<string, AssetData>();
        private Queue<AssetLoadRequest> loadingQueue = new Queue<AssetLoadRequest>();
        private List<AssetLoadRequest> activeLoads = new List<AssetLoadRequest>();
        
        // 참조
        private GameManager gameManager;
        
        // 이벤트
        public System.Action<string, Object> OnAssetLoaded;
        public System.Action<string> OnAssetUnloaded;
        public System.Action<string, float> OnLoadingProgress;
        public System.Action<string> OnLoadingComplete;
        public System.Action<string, string> OnLoadingError;
        public System.Action<float> OnCacheSizeChanged;
        
        // 싱글톤 패턴
        public static AssetManager Instance { get; private set; }
        
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
            InitializeAssetManager();
        }
        
        private void Update()
        {
            if (isAssetManagerInitialized)
            {
                ProcessLoadingQueue();
                UpdateAssetCache();
            }
        }
        
        private void InitializeAssetManager()
        {
            Debug.Log("Asset Manager 초기화 시작...");
            
            // 컴포넌트 참조
            gameManager = FindObjectOfType<GameManager>();
            
            // 캐시 디렉토리 설정
            SetupCacheDirectory();
            
            // 에셋 경로 설정
            SetupAssetPaths();
            
            // 이벤트 구독
            SubscribeToEvents();
            
            isAssetManagerInitialized = true;
            Debug.Log("Asset Manager 초기화 완료");
        }
        
        private void SetupCacheDirectory()
        {
            if (enableAssetCaching)
            {
                string fullCachePath = Path.Combine(Application.persistentDataPath, cachePath);
                if (!Directory.Exists(fullCachePath))
                {
                    Directory.CreateDirectory(fullCachePath);
                }
                Debug.Log($"에셋 캐시 디렉토리: {fullCachePath}");
            }
        }
        
        private void SetupAssetPaths()
        {
            // 에셋 경로 설정
            if (string.IsNullOrEmpty(localAssetPath))
            {
                localAssetPath = Path.Combine(Application.streamingAssetsPath, "");
            }
            
            if (string.IsNullOrEmpty(cachePath))
            {
                cachePath = Path.Combine(Application.persistentDataPath, "AssetCache");
            }
        }
        
        private void SubscribeToEvents()
        {
            // 게임 매니저 이벤트 구독
            if (gameManager != null)
            {
                // 게임 상태 변경 이벤트 구독
            }
        }
        
        private void ProcessLoadingQueue()
        {
            // 로딩 큐 처리
            while (loadingQueue.Count > 0 && activeLoads.Count < maxConcurrentLoads)
            {
                AssetLoadRequest request = loadingQueue.Dequeue();
                StartCoroutine(LoadAssetCoroutine(request));
            }
        }
        
        private void UpdateAssetCache()
        {
            if (!enableAssetCaching) return;
            
            // 캐시 크기 관리
            if (assetCache.Count > maxCacheSize)
            {
                RemoveOldestCachedAsset();
            }
            
            // 캐시 타임아웃 관리
            List<string> expiredAssets = new List<string>();
            foreach (var kvp in assetCache)
            {
                if (Time.time - kvp.Value.lastAccessTime > cacheTimeout)
                {
                    expiredAssets.Add(kvp.Key);
                }
            }
            
            foreach (string assetKey in expiredAssets)
            {
                UnloadAsset(assetKey);
            }
        }
        
        private void RemoveOldestCachedAsset()
        {
            string oldestKey = null;
            float oldestTime = float.MaxValue;
            
            foreach (var kvp in assetCache)
            {
                if (kvp.Value.lastAccessTime < oldestTime)
                {
                    oldestTime = kvp.Value.lastAccessTime;
                    oldestKey = kvp.Key;
                }
            }
            
            if (oldestKey != null)
            {
                UnloadAsset(oldestKey);
            }
        }
        
        public void LoadAsset(string assetPath, System.Action<Object> onComplete = null, System.Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                onError?.Invoke("에셋 경로가 비어있습니다.");
                return;
            }
            
            // 이미 로드된 에셋인지 확인
            if (loadedAssets.ContainsKey(assetPath))
            {
                AssetData assetData = loadedAssets[assetPath];
                assetData.lastAccessTime = Time.time;
                assetData.referenceCount++;
                onComplete?.Invoke(assetData.asset);
                return;
            }
            
            // 캐시에서 확인
            if (enableAssetCaching && assetCache.ContainsKey(assetPath))
            {
                AssetData cachedAsset = assetCache[assetPath];
                cachedAsset.lastAccessTime = Time.time;
                loadedAssets[assetPath] = cachedAsset;
                onComplete?.Invoke(cachedAsset.asset);
                return;
            }
            
            // 로딩 요청 생성
            AssetLoadRequest request = new AssetLoadRequest
            {
                assetPath = assetPath,
                onComplete = onComplete,
                onError = onError,
                requestTime = Time.time
            };
            
            loadingQueue.Enqueue(request);
        }
        
        private IEnumerator LoadAssetCoroutine(AssetLoadRequest request)
        {
            activeLoads.Add(request);
            
            try
            {
                // 로컬 에셋 로드 시도
                Object asset = Resources.Load(request.assetPath);
                
                if (asset != null)
                {
                    // 로컬 에셋 로드 성공
                    yield return StartCoroutine(HandleAssetLoaded(request, asset));
                }
                else
                {
                    // 원격 에셋 로드 시도
                    yield return StartCoroutine(LoadRemoteAsset(request));
                }
            }
            catch (System.Exception e)
            {
                HandleLoadingError(request, e.Message);
            }
            finally
            {
                activeLoads.Remove(request);
            }
        }
        
        private IEnumerator LoadRemoteAsset(AssetLoadRequest request)
        {
            string remoteUrl = remoteAssetPath + request.assetPath;
            
            using (UnityWebRequest webRequest = UnityWebRequest.Get(remoteUrl))
            {
                webRequest.timeout = (int)loadingTimeout;
                
                yield return webRequest.SendWebRequest();
                
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    // 원격 에셋 로드 성공
                    byte[] data = webRequest.downloadHandler.data;
                    yield return StartCoroutine(ProcessRemoteAssetData(request, data));
                }
                else
                {
                    HandleLoadingError(request, $"원격 에셋 로드 실패: {webRequest.error}");
                }
            }
        }
        
        private IEnumerator ProcessRemoteAssetData(AssetLoadRequest request, byte[] data)
        {
            // 원격 에셋 데이터 처리
            // 실제 구현에서는 에셋 타입에 따라 적절한 처리
            
            // 임시로 텍스처로 처리
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(data);
            
            yield return StartCoroutine(HandleAssetLoaded(request, texture));
        }
        
        private IEnumerator HandleAssetLoaded(AssetLoadRequest request, Object asset)
        {
            // 에셋 데이터 생성
            AssetData assetData = new AssetData
            {
                asset = asset,
                assetPath = request.assetPath,
                loadTime = Time.time,
                lastAccessTime = Time.time,
                referenceCount = 1,
                isCached = false
            };
            
            // 로드된 에셋 등록
            loadedAssets[request.assetPath] = assetData;
            
            // 캐시에 추가
            if (enableAssetCaching)
            {
                assetCache[request.assetPath] = assetData;
                assetData.isCached = true;
            }
            
            // 완료 콜백 호출
            request.onComplete?.Invoke(asset);
            OnAssetLoaded?.Invoke(request.assetPath, asset);
            OnLoadingComplete?.Invoke(request.assetPath);
            
            Debug.Log($"에셋 로드 완료: {request.assetPath}");
            
            yield return null;
        }
        
        private void HandleLoadingError(AssetLoadRequest request, string error)
        {
            request.onError?.Invoke(error);
            OnLoadingError?.Invoke(request.assetPath, error);
            Debug.LogError($"에셋 로드 실패: {request.assetPath} - {error}");
        }
        
        public void UnloadAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;
            
            if (loadedAssets.ContainsKey(assetPath))
            {
                AssetData assetData = loadedAssets[assetPath];
                assetData.referenceCount--;
                
                if (assetData.referenceCount <= 0)
                {
                    // 참조 카운트가 0이면 언로드
                    if (assetData.asset != null)
                    {
                        Resources.UnloadAsset(assetData.asset);
                    }
                    
                    loadedAssets.Remove(assetPath);
                    OnAssetUnloaded?.Invoke(assetPath);
                    
                    Debug.Log($"에셋 언로드: {assetPath}");
                }
            }
            
            if (assetCache.ContainsKey(assetPath))
            {
                assetCache.Remove(assetPath);
            }
        }
        
        public void UnloadAllAssets()
        {
            List<string> assetPaths = new List<string>(loadedAssets.Keys);
            foreach (string assetPath in assetPaths)
            {
                UnloadAsset(assetPath);
            }
            
            assetCache.Clear();
            Debug.Log("모든 에셋 언로드 완료");
        }
        
        public void PreloadAssets(string[] assetPaths)
        {
            foreach (string assetPath in assetPaths)
            {
                LoadAsset(assetPath);
            }
        }
        
        public void PreloadAssetsByCategory(AssetCategory category)
        {
            string[] paths = GetAssetPathsByCategory(category);
            PreloadAssets(paths);
        }
        
        private string[] GetAssetPathsByCategory(AssetCategory category)
        {
            switch (category)
            {
                case AssetCategory.Models:
                    return modelPaths;
                case AssetCategory.Textures:
                    return texturePaths;
                case AssetCategory.Audio:
                    return audioPaths;
                case AssetCategory.Prefabs:
                    return prefabPaths;
                default:
                    return new string[0];
            }
        }
        
        public bool IsAssetLoaded(string assetPath)
        {
            return loadedAssets.ContainsKey(assetPath);
        }
        
        public Object GetLoadedAsset(string assetPath)
        {
            if (loadedAssets.ContainsKey(assetPath))
            {
                AssetData assetData = loadedAssets[assetPath];
                assetData.lastAccessTime = Time.time;
                return assetData.asset;
            }
            return null;
        }
        
        public int GetLoadedAssetCount()
        {
            return loadedAssets.Count;
        }
        
        public int GetCachedAssetCount()
        {
            return assetCache.Count;
        }
        
        public float GetCacheSize()
        {
            // 캐시 크기 계산 (MB)
            float totalSize = 0f;
            foreach (var kvp in assetCache)
            {
                if (kvp.Value.asset != null)
                {
                    // 에셋 크기 추정 (실제 구현에서는 정확한 크기 계산)
                    totalSize += 1f; // 임시값
                }
            }
            return totalSize;
        }
        
        public void ClearCache()
        {
            assetCache.Clear();
            Debug.Log("에셋 캐시 클리어 완료");
        }
        
        public void SetCacheSize(int maxSize)
        {
            maxCacheSize = maxSize;
        }
        
        public void SetCacheTimeout(float timeout)
        {
            cacheTimeout = timeout;
        }
        
        // 공개 메서드들
        public bool IsAssetManagerInitialized()
        {
            return isAssetManagerInitialized;
        }
        
        public int GetLoadingQueueCount()
        {
            return loadingQueue.Count;
        }
        
        public int GetActiveLoadCount()
        {
            return activeLoads.Count;
        }
        
        private void OnDestroy()
        {
            // 모든 에셋 언로드
            UnloadAllAssets();
        }
    }
    
    // 에셋 데이터 구조체들
    [System.Serializable]
    public class AssetData
    {
        public Object asset;
        public string assetPath;
        public float loadTime;
        public float lastAccessTime;
        public int referenceCount;
        public bool isCached;
    }
    
    [System.Serializable]
    public class AssetLoadRequest
    {
        public string assetPath;
        public System.Action<Object> onComplete;
        public System.Action<string> onError;
        public float requestTime;
    }
    
    public enum AssetCategory
    {
        Models,
        Textures,
        Audio,
        Prefabs,
        UI,
        Effects
    }
}
