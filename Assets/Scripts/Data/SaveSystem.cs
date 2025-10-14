using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
using NowHere.Game;
using NowHere.RPG;
using NowHere.XR;

namespace NowHere.Data
{
    /// <summary>
    /// 게임 데이터 저장/로드 시스템
    /// 플레이어 데이터, 게임 설정, 진행 상황을 관리
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "gameSave.json";
        [SerializeField] private string settingsFileName = "gameSettings.json";
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5분마다 자동 저장
        [SerializeField] private int maxSaveSlots = 5;
        [SerializeField] private bool enableCloudSave = false;
        
        [Header("Data Encryption")]
        [SerializeField] private bool enableEncryption = true;
        [SerializeField] private string encryptionKey = "NowHere2024SecretKey";
        [SerializeField] private bool enableCompression = true;
        
        [Header("Backup Settings")]
        [SerializeField] private bool enableBackup = true;
        [SerializeField] private int maxBackups = 3;
        [SerializeField] private bool enableCloudBackup = false;
        
        // 저장 시스템 상태
        private bool isSaveSystemInitialized = false;
        private bool isSaving = false;
        private bool isLoading = false;
        private float lastSaveTime = 0f;
        private string saveDirectory;
        private string settingsDirectory;
        
        // 게임 데이터
        private GameSaveData currentSaveData;
        private GameSettingsData currentSettingsData;
        private Dictionary<int, GameSaveData> saveSlots = new Dictionary<int, GameSaveData>();
        
        // 참조
        private GameManager gameManager;
        private XRGameManager xrGameManager;
        private CharacterSystem characterSystem;
        private ItemSystem itemSystem;
        
        // 이벤트
        public System.Action<GameSaveData> OnGameSaved;
        public System.Action<GameSaveData> OnGameLoaded;
        public System.Action<GameSettingsData> OnSettingsSaved;
        public System.Action<GameSettingsData> OnSettingsLoaded;
        public System.Action<string> OnSaveError;
        public System.Action<string> OnLoadError;
        public System.Action<float> OnSaveProgress;
        public System.Action<float> OnLoadProgress;
        
        // 싱글톤 패턴
        public static SaveSystem Instance { get; private set; }
        
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
            InitializeSaveSystem();
        }
        
        private void Update()
        {
            if (enableAutoSave && isSaveSystemInitialized)
            {
                CheckAutoSave();
            }
        }
        
        private void InitializeSaveSystem()
        {
            Debug.Log("Save System 초기화 시작...");
            
            // 컴포넌트 참조
            gameManager = FindObjectOfType<GameManager>();
            xrGameManager = FindObjectOfType<XRGameManager>();
            characterSystem = FindObjectOfType<CharacterSystem>();
            itemSystem = FindObjectOfType<ItemSystem>();
            
            // 저장 디렉토리 설정
            SetupSaveDirectories();
            
            // 초기 데이터 생성
            InitializeGameData();
            
            // 기존 저장 파일 로드
            LoadSettings();
            LoadSaveSlots();
            
            // 이벤트 구독
            SubscribeToEvents();
            
            isSaveSystemInitialized = true;
            Debug.Log("Save System 초기화 완료");
        }
        
        private void SetupSaveDirectories()
        {
            // 저장 디렉토리 설정
            saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
            settingsDirectory = Path.Combine(Application.persistentDataPath, "Settings");
            
            // 디렉토리 생성
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
            
            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }
            
            Debug.Log($"저장 디렉토리: {saveDirectory}");
            Debug.Log($"설정 디렉토리: {settingsDirectory}");
        }
        
        private void InitializeGameData()
        {
            // 초기 게임 데이터 생성
            currentSaveData = new GameSaveData
            {
                saveVersion = "1.0.0",
                saveDate = DateTime.Now,
                playTime = 0f,
                playerData = new PlayerSaveData(),
                gameProgress = new GameProgressData(),
                inventoryData = new InventorySaveData(),
                settingsData = new GameSettingsData()
            };
            
            currentSettingsData = new GameSettingsData
            {
                masterVolume = 1f,
                musicVolume = 0.8f,
                sfxVolume = 1f,
                voiceVolume = 1f,
                graphicsQuality = 2,
                enableAR = true,
                enableVR = true,
                enableVoiceChat = true,
                enableMotionDetection = true,
                language = "Korean",
                autoSave = true,
                autoSaveInterval = 300f
            };
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
                xrGameManager.OnGameStateChanged += OnGameStateChanged;
            }
        }
        
        private void CheckAutoSave()
        {
            if (Time.time - lastSaveTime >= autoSaveInterval)
            {
                SaveGame();
            }
        }
        
        public void SaveGame(int slotIndex = 0)
        {
            if (isSaving) return;
            
            StartCoroutine(SaveGameCoroutine(slotIndex));
        }
        
        private System.Collections.IEnumerator SaveGameCoroutine(int slotIndex)
        {
            isSaving = true;
            OnSaveProgress?.Invoke(0f);
            
            try
            {
                // 1. 현재 게임 데이터 수집
                yield return StartCoroutine(CollectGameData());
                OnSaveProgress?.Invoke(0.3f);
                
                // 2. 데이터 검증
                yield return StartCoroutine(ValidateSaveData());
                OnSaveProgress?.Invoke(0.5f);
                
                // 3. 데이터 직렬화
                string jsonData = SerializeSaveData(currentSaveData);
                OnSaveProgress?.Invoke(0.7f);
                
                // 4. 파일 저장
                string savePath = GetSaveFilePath(slotIndex);
                yield return StartCoroutine(WriteToFile(savePath, jsonData));
                OnSaveProgress?.Invoke(0.9f);
                
                // 5. 백업 생성
                if (enableBackup)
                {
                    yield return StartCoroutine(CreateBackup(slotIndex));
                }
                
                // 6. 클라우드 저장
                if (enableCloudSave)
                {
                    yield return StartCoroutine(SaveToCloud(slotIndex, jsonData));
                }
                
                // 저장 슬롯 업데이트
                saveSlots[slotIndex] = currentSaveData;
                
                lastSaveTime = Time.time;
                OnSaveProgress?.Invoke(1f);
                OnGameSaved?.Invoke(currentSaveData);
                
                Debug.Log($"게임 저장 완료: 슬롯 {slotIndex}");
            }
            catch (Exception e)
            {
                OnSaveError?.Invoke($"저장 중 오류 발생: {e.Message}");
                Debug.LogError($"게임 저장 실패: {e.Message}");
            }
            finally
            {
                isSaving = false;
            }
        }
        
        private System.Collections.IEnumerator CollectGameData()
        {
            // 게임 데이터 수집
            currentSaveData.saveDate = DateTime.Now;
            currentSaveData.playTime += Time.time - lastSaveTime;
            
            // 플레이어 데이터 수집
            if (characterSystem != null)
            {
                currentSaveData.playerData = characterSystem.GetSaveData();
            }
            
            // 게임 진행 상황 수집
            if (gameManager != null)
            {
                currentSaveData.gameProgress = gameManager.GetProgressData();
            }
            
            // 인벤토리 데이터 수집
            if (itemSystem != null)
            {
                currentSaveData.inventoryData = itemSystem.GetSaveData();
            }
            
            // XR 설정 수집
            if (xrGameManager != null)
            {
                currentSaveData.xrData = xrGameManager.GetSaveData();
            }
            
            yield return null;
        }
        
        private System.Collections.IEnumerator ValidateSaveData()
        {
            // 저장 데이터 검증
            if (currentSaveData == null)
            {
                throw new Exception("저장 데이터가 null입니다.");
            }
            
            if (string.IsNullOrEmpty(currentSaveData.saveVersion))
            {
                currentSaveData.saveVersion = "1.0.0";
            }
            
            yield return null;
        }
        
        private string SerializeSaveData(GameSaveData data)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                
                if (enableEncryption)
                {
                    json = EncryptData(json);
                }
                
                if (enableCompression)
                {
                    json = CompressData(json);
                }
                
                return json;
            }
            catch (Exception e)
            {
                throw new Exception($"데이터 직렬화 실패: {e.Message}");
            }
        }
        
        private System.Collections.IEnumerator WriteToFile(string filePath, string data)
        {
            try
            {
                File.WriteAllText(filePath, data);
                yield return null;
            }
            catch (Exception e)
            {
                throw new Exception($"파일 쓰기 실패: {e.Message}");
            }
        }
        
        private System.Collections.IEnumerator CreateBackup(int slotIndex)
        {
            try
            {
                string originalPath = GetSaveFilePath(slotIndex);
                string backupPath = GetBackupFilePath(slotIndex);
                
                if (File.Exists(originalPath))
                {
                    File.Copy(originalPath, backupPath, true);
                }
                
                yield return null;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"백업 생성 실패: {e.Message}");
            }
        }
        
        private System.Collections.IEnumerator SaveToCloud(int slotIndex, string data)
        {
            // 클라우드 저장 로직
            // 실제 구현에서는 클라우드 서비스 API 사용
            yield return null;
        }
        
        public void LoadGame(int slotIndex = 0)
        {
            if (isLoading) return;
            
            StartCoroutine(LoadGameCoroutine(slotIndex));
        }
        
        private System.Collections.IEnumerator LoadGameCoroutine(int slotIndex)
        {
            isLoading = true;
            OnLoadProgress?.Invoke(0f);
            
            try
            {
                // 1. 파일 읽기
                string savePath = GetSaveFilePath(slotIndex);
                if (!File.Exists(savePath))
                {
                    throw new Exception($"저장 파일을 찾을 수 없습니다: {savePath}");
                }
                
                string jsonData = File.ReadAllText(savePath);
                OnLoadProgress?.Invoke(0.3f);
                
                // 2. 데이터 역직렬화
                GameSaveData loadedData = DeserializeSaveData(jsonData);
                OnLoadProgress?.Invoke(0.5f);
                
                // 3. 데이터 검증
                yield return StartCoroutine(ValidateLoadedData(loadedData));
                OnLoadProgress?.Invoke(0.7f);
                
                // 4. 게임 데이터 적용
                yield return StartCoroutine(ApplyGameData(loadedData));
                OnLoadProgress?.Invoke(0.9f);
                
                currentSaveData = loadedData;
                saveSlots[slotIndex] = loadedData;
                
                OnLoadProgress?.Invoke(1f);
                OnGameLoaded?.Invoke(loadedData);
                
                Debug.Log($"게임 로드 완료: 슬롯 {slotIndex}");
            }
            catch (Exception e)
            {
                OnLoadError?.Invoke($"로드 중 오류 발생: {e.Message}");
                Debug.LogError($"게임 로드 실패: {e.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }
        
        private GameSaveData DeserializeSaveData(string jsonData)
        {
            try
            {
                string processedData = jsonData;
                
                if (enableCompression)
                {
                    processedData = DecompressData(processedData);
                }
                
                if (enableEncryption)
                {
                    processedData = DecryptData(processedData);
                }
                
                return JsonConvert.DeserializeObject<GameSaveData>(processedData);
            }
            catch (Exception e)
            {
                throw new Exception($"데이터 역직렬화 실패: {e.Message}");
            }
        }
        
        private System.Collections.IEnumerator ValidateLoadedData(GameSaveData data)
        {
            // 로드된 데이터 검증
            if (data == null)
            {
                throw new Exception("로드된 데이터가 null입니다.");
            }
            
            if (string.IsNullOrEmpty(data.saveVersion))
            {
                data.saveVersion = "1.0.0";
            }
            
            yield return null;
        }
        
        private System.Collections.IEnumerator ApplyGameData(GameSaveData data)
        {
            // 게임 데이터 적용
            if (characterSystem != null && data.playerData != null)
            {
                characterSystem.LoadSaveData(data.playerData);
            }
            
            if (gameManager != null && data.gameProgress != null)
            {
                gameManager.LoadProgressData(data.gameProgress);
            }
            
            if (itemSystem != null && data.inventoryData != null)
            {
                itemSystem.LoadSaveData(data.inventoryData);
            }
            
            if (xrGameManager != null && data.xrData != null)
            {
                xrGameManager.LoadSaveData(data.xrData);
            }
            
            yield return null;
        }
        
        public void SaveSettings()
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(currentSettingsData, Formatting.Indented);
                string settingsPath = Path.Combine(settingsDirectory, settingsFileName);
                
                File.WriteAllText(settingsPath, jsonData);
                
                OnSettingsSaved?.Invoke(currentSettingsData);
                Debug.Log("설정 저장 완료");
            }
            catch (Exception e)
            {
                OnSaveError?.Invoke($"설정 저장 실패: {e.Message}");
                Debug.LogError($"설정 저장 실패: {e.Message}");
            }
        }
        
        public void LoadSettings()
        {
            try
            {
                string settingsPath = Path.Combine(settingsDirectory, settingsFileName);
                
                if (File.Exists(settingsPath))
                {
                    string jsonData = File.ReadAllText(settingsPath);
                    currentSettingsData = JsonConvert.DeserializeObject<GameSettingsData>(jsonData);
                    
                    ApplySettings();
                    OnSettingsLoaded?.Invoke(currentSettingsData);
                    Debug.Log("설정 로드 완료");
                }
                else
                {
                    Debug.Log("설정 파일이 없습니다. 기본 설정을 사용합니다.");
                }
            }
            catch (Exception e)
            {
                OnLoadError?.Invoke($"설정 로드 실패: {e.Message}");
                Debug.LogError($"설정 로드 실패: {e.Message}");
            }
        }
        
        private void ApplySettings()
        {
            // 설정 적용
            if (currentSettingsData != null)
            {
                // 오디오 설정 적용
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.SetMasterVolume(currentSettingsData.masterVolume);
                    AudioManager.Instance.SetMusicVolume(currentSettingsData.musicVolume);
                    AudioManager.Instance.SetSFXVolume(currentSettingsData.sfxVolume);
                    AudioManager.Instance.SetVoiceVolume(currentSettingsData.voiceVolume);
                }
                
                // 그래픽 설정 적용
                QualitySettings.SetQualityLevel(currentSettingsData.graphicsQuality);
                
                // 자동 저장 설정 적용
                enableAutoSave = currentSettingsData.autoSave;
                autoSaveInterval = currentSettingsData.autoSaveInterval;
            }
        }
        
        private void LoadSaveSlots()
        {
            // 저장 슬롯들 로드
            for (int i = 0; i < maxSaveSlots; i++)
            {
                string savePath = GetSaveFilePath(i);
                if (File.Exists(savePath))
                {
                    try
                    {
                        string jsonData = File.ReadAllText(savePath);
                        GameSaveData saveData = DeserializeSaveData(jsonData);
                        saveSlots[i] = saveData;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"저장 슬롯 {i} 로드 실패: {e.Message}");
                    }
                }
            }
        }
        
        private string GetSaveFilePath(int slotIndex)
        {
            return Path.Combine(saveDirectory, $"save_{slotIndex}.json");
        }
        
        private string GetBackupFilePath(int slotIndex)
        {
            return Path.Combine(saveDirectory, $"backup_{slotIndex}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        }
        
        private string EncryptData(string data)
        {
            // 데이터 암호화
            // 실제 구현에서는 AES 암호화 사용
            return data; // 임시로 원본 반환
        }
        
        private string DecryptData(string data)
        {
            // 데이터 복호화
            // 실제 구현에서는 AES 복호화 사용
            return data; // 임시로 원본 반환
        }
        
        private string CompressData(string data)
        {
            // 데이터 압축
            // 실제 구현에서는 GZip 압축 사용
            return data; // 임시로 원본 반환
        }
        
        private string DecompressData(string data)
        {
            // 데이터 압축 해제
            // 실제 구현에서는 GZip 압축 해제 사용
            return data; // 임시로 원본 반환
        }
        
        // 이벤트 핸들러들
        private void OnGameStateChanged(XRGameState state)
        {
            // 게임 상태 변경 시 자동 저장
            if (enableAutoSave && state == XRGameState.Playing)
            {
                SaveGame();
            }
        }
        
        // 공개 메서드들
        public bool IsSaveSystemInitialized()
        {
            return isSaveSystemInitialized;
        }
        
        public bool IsSaving()
        {
            return isSaving;
        }
        
        public bool IsLoading()
        {
            return isLoading;
        }
        
        public GameSaveData GetCurrentSaveData()
        {
            return currentSaveData;
        }
        
        public GameSettingsData GetCurrentSettingsData()
        {
            return currentSettingsData;
        }
        
        public Dictionary<int, GameSaveData> GetSaveSlots()
        {
            return saveSlots;
        }
        
        public bool HasSaveInSlot(int slotIndex)
        {
            return saveSlots.ContainsKey(slotIndex);
        }
        
        public void DeleteSave(int slotIndex)
        {
            try
            {
                string savePath = GetSaveFilePath(slotIndex);
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
                
                if (saveSlots.ContainsKey(slotIndex))
                {
                    saveSlots.Remove(slotIndex);
                }
                
                Debug.Log($"저장 슬롯 {slotIndex} 삭제 완료");
            }
            catch (Exception e)
            {
                Debug.LogError($"저장 슬롯 {slotIndex} 삭제 실패: {e.Message}");
            }
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (xrGameManager != null)
            {
                xrGameManager.OnGameStateChanged -= OnGameStateChanged;
            }
        }
    }
    
    // 저장 데이터 구조체들
    [System.Serializable]
    public class GameSaveData
    {
        public string saveVersion;
        public DateTime saveDate;
        public float playTime;
        public PlayerSaveData playerData;
        public GameProgressData gameProgress;
        public InventorySaveData inventoryData;
        public XRSaveData xrData;
        public GameSettingsData settingsData;
    }
    
    [System.Serializable]
    public class PlayerSaveData
    {
        public string playerName;
        public int level;
        public int experience;
        public int health;
        public int maxHealth;
        public int mana;
        public int maxMana;
        public Vector3 position;
        public Quaternion rotation;
        public Dictionary<string, int> stats;
        public List<string> unlockedSkills;
    }
    
    [System.Serializable]
    public class GameProgressData
    {
        public int currentLevel;
        public List<string> completedQuests;
        public List<string> unlockedAreas;
        public Dictionary<string, bool> achievements;
        public float totalPlayTime;
    }
    
    [System.Serializable]
    public class InventorySaveData
    {
        public List<ItemSaveData> items;
        public int gold;
        public int gems;
        public Dictionary<string, int> currencies;
    }
    
    [System.Serializable]
    public class ItemSaveData
    {
        public string itemId;
        public int quantity;
        public Dictionary<string, object> properties;
    }
    
    [System.Serializable]
    public class XRSaveData
    {
        public XRMode preferredMode;
        public Dictionary<string, object> xrSettings;
        public List<string> unlockedXRFeatures;
    }
    
    [System.Serializable]
    public class GameSettingsData
    {
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public float voiceVolume;
        public int graphicsQuality;
        public bool enableAR;
        public bool enableVR;
        public bool enableVoiceChat;
        public bool enableMotionDetection;
        public string language;
        public bool autoSave;
        public float autoSaveInterval;
    }
}
