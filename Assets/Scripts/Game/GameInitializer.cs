using UnityEngine;
using UnityEngine.SceneManagement;
using NowHere.AR;
using NowHere.Networking;
using NowHere.UI;
using NowHere.Sensors;
using NowHere.Interaction;
using NowHere.Audio;
using NowHere.Motion;
using NowHere.Combat;
using NowHere.RPG;
using NowHere.Player;
using NowHere.AI;

namespace NowHere.Game
{
    /// <summary>
    /// 게임 초기화 및 완전한 게임 시스템 관리
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("게임 설정")]
        public bool enableAR = true;
        public bool enableMultiplayer = true;
        public bool enableVoiceChat = true;
        public bool enableMotionDetection = true;
        public bool enableCombat = true;
        public bool enableRPG = true;
        
        [Header("게임 오브젝트")]
        public GameObject playerPrefab;
        public GameObject enemyPrefab;
        public GameObject itemPrefab;
        public GameObject uiCanvasPrefab;
        
        [Header("매니저들")]
        public GameManager gameManager;
        public ARManager arManager;
        public NetworkManager networkManager;
        public UIManager uiManager;
        public MobileSensorManager sensorManager;
        public TouchInteractionManager touchManager;
        public VoiceChatManager voiceManager;
        public MotionDetectionManager motionManager;
        public ARCombatSystem combatSystem;
        
        private bool isInitialized = false;
        
        void Start()
        {
            InitializeGame();
        }
        
        /// <summary>
        /// 완전한 게임 시스템 초기화
        /// </summary>
        public void InitializeGame()
        {
            if (isInitialized) return;
            
            Debug.Log("=== NowHere AR MMORPG 초기화 시작 ===");
            
            // 1. 기본 시스템 초기화
            InitializeBasicSystems();
            
            // 2. AR 시스템 초기화
            if (enableAR)
            {
                InitializeARSystem();
            }
            
            // 3. 멀티플레이어 시스템 초기화
            if (enableMultiplayer)
            {
                InitializeMultiplayerSystem();
            }
            
            // 4. 센서 시스템 초기화
            InitializeSensorSystem();
            
            // 5. 상호작용 시스템 초기화
            InitializeInteractionSystem();
            
            // 6. 오디오 시스템 초기화
            if (enableVoiceChat)
            {
                InitializeAudioSystem();
            }
            
            // 7. 모션 감지 시스템 초기화
            if (enableMotionDetection)
            {
                InitializeMotionSystem();
            }
            
            // 8. 전투 시스템 초기화
            if (enableCombat)
            {
                InitializeCombatSystem();
            }
            
            // 9. RPG 시스템 초기화
            if (enableRPG)
            {
                InitializeRPGSystem();
            }
            
            // 10. UI 시스템 초기화
            InitializeUISystem();
            
            // 11. 게임 오브젝트 생성
            CreateGameObjects();
            
            isInitialized = true;
            Debug.Log("=== NowHere AR MMORPG 초기화 완료 ===");
            
            // 게임 시작
            StartGame();
        }
        
        private void InitializeBasicSystems()
        {
            Debug.Log("기본 시스템 초기화...");
            
            // 게임 매니저 초기화
            if (gameManager == null)
            {
                GameObject gameManagerObj = new GameObject("GameManager");
                gameManager = gameManagerObj.AddComponent<GameManager>();
            }
        }
        
        private void InitializeARSystem()
        {
            Debug.Log("AR 시스템 초기화...");
            
            if (arManager == null)
            {
                GameObject arManagerObj = new GameObject("ARManager");
                arManager = arManagerObj.AddComponent<ARManager>();
            }
            
            // AR 카메라 설정
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.name = "AR Camera";
                // AR 카메라 컴포넌트 추가
                if (mainCamera.GetComponent<ARObjectManager>() == null)
                {
                    mainCamera.gameObject.AddComponent<ARObjectManager>();
                }
            }
        }
        
        private void InitializeMultiplayerSystem()
        {
            Debug.Log("멀티플레이어 시스템 초기화...");
            
            if (networkManager == null)
            {
                GameObject networkManagerObj = new GameObject("NetworkManager");
                networkManager = networkManagerObj.AddComponent<NetworkManager>();
            }
        }
        
        private void InitializeSensorSystem()
        {
            Debug.Log("센서 시스템 초기화...");
            
            if (sensorManager == null)
            {
                GameObject sensorManagerObj = new GameObject("MobileSensorManager");
                sensorManager = sensorManagerObj.AddComponent<MobileSensorManager>();
            }
        }
        
        private void InitializeInteractionSystem()
        {
            Debug.Log("상호작용 시스템 초기화...");
            
            if (touchManager == null)
            {
                GameObject touchManagerObj = new GameObject("TouchInteractionManager");
                touchManager = touchManagerObj.AddComponent<TouchInteractionManager>();
            }
        }
        
        private void InitializeAudioSystem()
        {
            Debug.Log("오디오 시스템 초기화...");
            
            if (voiceManager == null)
            {
                GameObject voiceManagerObj = new GameObject("VoiceChatManager");
                voiceManager = voiceManagerObj.AddComponent<VoiceChatManager>();
            }
        }
        
        private void InitializeMotionSystem()
        {
            Debug.Log("모션 감지 시스템 초기화...");
            
            if (motionManager == null)
            {
                GameObject motionManagerObj = new GameObject("MotionDetectionManager");
                motionManager = motionManagerObj.AddComponent<MotionDetectionManager>();
            }
        }
        
        private void InitializeCombatSystem()
        {
            Debug.Log("전투 시스템 초기화...");
            
            if (combatSystem == null)
            {
                GameObject combatSystemObj = new GameObject("ARCombatSystem");
                combatSystem = combatSystemObj.AddComponent<ARCombatSystem>();
            }
        }
        
        private void InitializeRPGSystem()
        {
            Debug.Log("RPG 시스템 초기화...");
            
            // 캐릭터 시스템 초기화
            GameObject characterSystemObj = new GameObject("CharacterSystem");
            characterSystemObj.AddComponent<CharacterSystem>();
            
            // 아이템 시스템 초기화
            GameObject itemSystemObj = new GameObject("ItemSystem");
            itemSystemObj.AddComponent<ItemSystem>();
        }
        
        private void InitializeUISystem()
        {
            Debug.Log("UI 시스템 초기화...");
            
            if (uiManager == null)
            {
                GameObject uiManagerObj = new GameObject("UIManager");
                uiManager = uiManagerObj.AddComponent<UIManager>();
            }
            
            // UI 캔버스 생성
            if (uiCanvasPrefab != null)
            {
                Instantiate(uiCanvasPrefab);
            }
            else
            {
                CreateDefaultUI();
            }
        }
        
        private void CreateDefaultUI()
        {
            // 기본 UI 캔버스 생성
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // 이벤트 시스템 생성
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            // 기본 UI 패널들 생성
            CreateMainMenuUI(canvasObj);
            CreateGameUI(canvasObj);
            CreateInventoryUI(canvasObj);
        }
        
        private void CreateMainMenuUI(GameObject parent)
        {
            GameObject mainMenuPanel = new GameObject("MainMenuPanel");
            mainMenuPanel.transform.SetParent(parent.transform, false);
            
            RectTransform rectTransform = mainMenuPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            // 배경
            UnityEngine.UI.Image background = mainMenuPanel.AddComponent<UnityEngine.UI.Image>();
            background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // 제목
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(mainMenuPanel.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.8f);
            titleRect.sizeDelta = new Vector2(400, 100);
            titleRect.anchoredPosition = Vector2.zero;
            
            UnityEngine.UI.Text titleText = titleObj.AddComponent<UnityEngine.UI.Text>();
            titleText.text = "NowHere AR MMORPG";
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 36;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            
            // 시작 버튼
            CreateButton(mainMenuPanel, "StartButton", "게임 시작", new Vector2(0.5f, 0.6f), () => {
                Debug.Log("게임 시작!");
                StartGame();
            });
            
            // 설정 버튼
            CreateButton(mainMenuPanel, "SettingsButton", "설정", new Vector2(0.5f, 0.5f), () => {
                Debug.Log("설정 메뉴 열기");
            });
            
            // 종료 버튼
            CreateButton(mainMenuPanel, "ExitButton", "종료", new Vector2(0.5f, 0.4f), () => {
                Application.Quit();
            });
        }
        
        private void CreateGameUI(GameObject parent)
        {
            GameObject gamePanel = new GameObject("GamePanel");
            gamePanel.transform.SetParent(parent.transform, false);
            
            RectTransform rectTransform = gamePanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            // 플레이어 정보 패널
            GameObject playerInfoPanel = new GameObject("PlayerInfoPanel");
            playerInfoPanel.transform.SetParent(gamePanel.transform, false);
            
            RectTransform playerInfoRect = playerInfoPanel.AddComponent<RectTransform>();
            playerInfoRect.anchorMin = new Vector2(0, 1);
            playerInfoRect.anchorMax = new Vector2(0.3f, 1);
            playerInfoRect.sizeDelta = new Vector2(300, 150);
            playerInfoRect.anchoredPosition = new Vector2(150, -75);
            
            UnityEngine.UI.Image playerInfoBg = playerInfoPanel.AddComponent<UnityEngine.UI.Image>();
            playerInfoBg.color = new Color(0, 0, 0, 0.5f);
            
            // 체력 바
            CreateHealthBar(playerInfoPanel);
            
            // 마나 바
            CreateManaBar(playerInfoPanel);
            
            // 레벨 표시
            CreateLevelDisplay(playerInfoPanel);
        }
        
        private void CreateInventoryUI(GameObject parent)
        {
            GameObject inventoryPanel = new GameObject("InventoryPanel");
            inventoryPanel.transform.SetParent(parent.transform, false);
            
            RectTransform rectTransform = inventoryPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(400, 300);
            rectTransform.anchoredPosition = Vector2.zero;
            
            UnityEngine.UI.Image background = inventoryPanel.AddComponent<UnityEngine.UI.Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            // 인벤토리 제목
            GameObject titleObj = new GameObject("InventoryTitle");
            titleObj.transform.SetParent(inventoryPanel.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.sizeDelta = new Vector2(200, 50);
            titleRect.anchoredPosition = new Vector2(0, -25);
            
            UnityEngine.UI.Text titleText = titleObj.AddComponent<UnityEngine.UI.Text>();
            titleText.text = "인벤토리";
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 24;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            
            // 닫기 버튼
            CreateButton(inventoryPanel, "CloseButton", "닫기", new Vector2(1, 1), () => {
                inventoryPanel.SetActive(false);
            });
        }
        
        private void CreateButton(GameObject parent, string name, string text, Vector2 anchorPosition, System.Action onClick)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);
            
            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchorPosition;
            rectTransform.anchorMax = anchorPosition;
            rectTransform.sizeDelta = new Vector2(200, 50);
            rectTransform.anchoredPosition = Vector2.zero;
            
            UnityEngine.UI.Image buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = new Color(0.2f, 0.5f, 0.8f, 1f);
            
            UnityEngine.UI.Button button = buttonObj.AddComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() => onClick());
            
            // 버튼 텍스트
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            UnityEngine.UI.Text buttonText = textObj.AddComponent<UnityEngine.UI.Text>();
            buttonText.text = text;
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 18;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateHealthBar(GameObject parent)
        {
            GameObject healthBarObj = new GameObject("HealthBar");
            healthBarObj.transform.SetParent(parent.transform, false);
            
            RectTransform rectTransform = healthBarObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.1f, 0.7f);
            rectTransform.anchorMax = new Vector2(0.9f, 0.8f);
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            UnityEngine.UI.Image healthBar = healthBarObj.AddComponent<UnityEngine.UI.Image>();
            healthBar.color = Color.red;
        }
        
        private void CreateManaBar(GameObject parent)
        {
            GameObject manaBarObj = new GameObject("ManaBar");
            manaBarObj.transform.SetParent(parent.transform, false);
            
            RectTransform rectTransform = manaBarObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.1f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.9f, 0.6f);
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            UnityEngine.UI.Image manaBar = manaBarObj.AddComponent<UnityEngine.UI.Image>();
            manaBar.color = Color.blue;
        }
        
        private void CreateLevelDisplay(GameObject parent)
        {
            GameObject levelObj = new GameObject("LevelDisplay");
            levelObj.transform.SetParent(parent.transform, false);
            
            RectTransform rectTransform = levelObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.1f, 0.3f);
            rectTransform.anchorMax = new Vector2(0.9f, 0.4f);
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            UnityEngine.UI.Text levelText = levelObj.AddComponent<UnityEngine.UI.Text>();
            levelText.text = "레벨: 1";
            levelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            levelText.fontSize = 16;
            levelText.color = Color.white;
            levelText.alignment = TextAnchor.MiddleLeft;
        }
        
        private void CreateGameObjects()
        {
            Debug.Log("게임 오브젝트 생성...");
            
            // 플레이어 생성
            if (playerPrefab != null)
            {
                GameObject player = Instantiate(playerPrefab);
                player.name = "Player";
                player.transform.position = Vector3.zero;
            }
            else
            {
                CreateDefaultPlayer();
            }
            
            // 적 생성
            if (enemyPrefab != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    GameObject enemy = Instantiate(enemyPrefab);
                    enemy.name = $"Enemy_{i}";
                    enemy.transform.position = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
                }
            }
            else
            {
                CreateDefaultEnemies();
            }
            
            // 아이템 생성
            if (itemPrefab != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    GameObject item = Instantiate(itemPrefab);
                    item.name = $"Item_{i}";
                    item.transform.position = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
                }
            }
            else
            {
                CreateDefaultItems();
            }
        }
        
        private void CreateDefaultPlayer()
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            player.transform.position = Vector3.zero;
            player.transform.localScale = new Vector3(1, 2, 1);
            
            // 플레이어 컨트롤러 추가
            if (player.GetComponent<PlayerController>() == null)
            {
                player.AddComponent<PlayerController>();
            }
            
            // 머티리얼 설정
            Renderer renderer = player.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material playerMaterial = new Material(Shader.Find("Standard"));
                playerMaterial.color = Color.blue;
                renderer.material = playerMaterial;
            }
        }
        
        private void CreateDefaultEnemies()
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
                enemy.name = $"Enemy_{i}";
                enemy.tag = "Enemy";
                enemy.transform.position = new Vector3(Random.Range(-5f, 5f), 0.5f, Random.Range(-5f, 5f));
                enemy.transform.localScale = new Vector3(1, 1, 1);
                
                // 적 컨트롤러 추가
                if (enemy.GetComponent<EnemyController>() == null)
                {
                    enemy.AddComponent<EnemyController>();
                }
                
                // 머티리얼 설정
                Renderer renderer = enemy.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material enemyMaterial = new Material(Shader.Find("Standard"));
                    enemyMaterial.color = Color.red;
                    renderer.material = enemyMaterial;
                }
            }
        }
        
        private void CreateDefaultItems()
        {
            for (int i = 0; i < 5; i++)
            {
                GameObject item = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                item.name = $"Item_{i}";
                item.tag = "Item";
                item.transform.position = new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f));
                item.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                
                // 아이템 픽업 컴포넌트 추가
                if (item.GetComponent<ItemPickup>() == null)
                {
                    item.AddComponent<ItemPickup>();
                }
                
                // 머티리얼 설정
                Renderer renderer = item.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material itemMaterial = new Material(Shader.Find("Standard"));
                    itemMaterial.color = Color.green;
                    renderer.material = itemMaterial;
                }
                
                // 회전 애니메이션
                item.AddComponent<ItemRotator>();
            }
        }
        
        private void StartGame()
        {
            Debug.Log("=== NowHere AR MMORPG 게임 시작! ===");
            
            // 게임 시작 이벤트 발생
            if (gameManager != null)
            {
                gameManager.StartGame();
            }
            
            // UI 업데이트 시작
            if (uiManager != null)
            {
                uiManager.ShowMainMenu();
            }
        }
        
        void Update()
        {
            // 게임 루프 업데이트
            if (isInitialized)
            {
                UpdateGameSystems();
            }
        }
        
        private void UpdateGameSystems()
        {
            // 센서 데이터 업데이트
            if (sensorManager != null)
            {
                sensorManager.Update();
            }
            
            // 모션 감지 업데이트
            if (motionManager != null)
            {
                motionManager.Update();
            }
            
            // AR 시스템 업데이트
            if (arManager != null)
            {
                arManager.Update();
            }
            
            // 전투 시스템 업데이트
            if (combatSystem != null)
            {
                combatSystem.Update();
            }
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Debug.Log("게임 일시정지");
                // 게임 일시정지 처리
            }
            else
            {
                Debug.Log("게임 재개");
                // 게임 재개 처리
            }
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Debug.Log("게임 포커스 해제");
                // 게임 포커스 해제 처리
            }
            else
            {
                Debug.Log("게임 포커스 획득");
                // 게임 포커스 획득 처리
            }
        }
    }
    
    /// <summary>
    /// 아이템 회전 애니메이션
    /// </summary>
    public class ItemRotator : MonoBehaviour
    {
        public float rotationSpeed = 50f;
        
        void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
