using UnityEngine;
using System.Collections.Generic;
using NowHere.Sensors;
using NowHere.RPG;
using NowHere.AI;

namespace NowHere.AR
{
    /// <summary>
    /// AR 오브젝트 생성 및 관리를 담당하는 클래스
    /// 현실 세계에 가상 오브젝트를 배치하고 관리
    /// </summary>
    public class ARObjectManager : MonoBehaviour
    {
        [Header("AR Object Settings")]
        [SerializeField] private GameObject[] arObjectPrefabs;
        [SerializeField] private LayerMask arPlaneLayer = 1;
        [SerializeField] private float objectPlacementHeight = 0.1f;
        [SerializeField] private float objectInteractionRange = 2f;
        
        [Header("Object Spawning")]
        [SerializeField] private bool enableAutoSpawning = true;
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private int maxObjectsInScene = 20;
        [SerializeField] private float spawnRadius = 10f;
        
        [Header("Object Management")]
        [SerializeField] private bool enableObjectPersistence = true;
        [SerializeField] private float objectLifetime = 300f; // 5분
        [SerializeField] private bool enableObjectPhysics = true;
        [SerializeField] private bool enableObjectCollision = true;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject spawnEffect;
        [SerializeField] private GameObject despawnEffect;
        [SerializeField] private GameObject interactionEffect;
        [SerializeField] private AudioClip spawnSound;
        [SerializeField] private AudioClip despawnSound;
        [SerializeField] private AudioClip interactionSound;
        
        // AR 오브젝트 관리
        private List<ARObjectData> activeObjects = new List<ARObjectData>();
        private Dictionary<GameObject, ARObjectData> objectDataMap = new Dictionary<GameObject, ARObjectData>();
        private Queue<GameObject> objectPool = new Queue<GameObject>();
        
        // 스폰 관리
        private float lastSpawnTime;
        private Vector3 lastSpawnPosition;
        private int spawnCount = 0;
        
        // 참조
        private ARManager arManager;
        private MobileSensorManager sensorManager;
        private ItemSystem itemSystem;
        private Camera arCamera;
        
        // 이벤트
        public System.Action<ARObjectData> OnObjectSpawned;
        public System.Action<ARObjectData> OnObjectDespawned;
        public System.Action<ARObjectData> OnObjectInteracted;
        public System.Action<ARObjectData> OnObjectDestroyed;
        public System.Action<int> OnObjectCountChanged;
        
        private void Start()
        {
            InitializeARObjectManager();
        }
        
        private void Update()
        {
            UpdateARObjects();
            ProcessAutoSpawning();
            ProcessObjectInteractions();
        }
        
        private void InitializeARObjectManager()
        {
            // AR 매니저 참조
            arManager = FindObjectOfType<ARManager>();
            if (arManager != null)
            {
                arManager.OnVirtualWorldPlaced += OnVirtualWorldPlaced;
            }
            
            // 센서 매니저 참조
            sensorManager = FindObjectOfType<MobileSensorManager>();
            
            // 아이템 시스템 참조
            itemSystem = FindObjectOfType<ItemSystem>();
            
            // AR 카메라 참조
            arCamera = Camera.main;
            if (arCamera == null)
            {
                arCamera = FindObjectOfType<Camera>();
            }
            
            // 오브젝트 풀 초기화
            InitializeObjectPool();
            
            Debug.Log("AR 오브젝트 매니저가 초기화되었습니다.");
        }
        
        private void InitializeObjectPool()
        {
            // 오브젝트 풀 생성
            for (int i = 0; i < maxObjectsInScene; i++)
            {
                if (arObjectPrefabs != null && arObjectPrefabs.Length > 0)
                {
                    GameObject pooledObject = Instantiate(arObjectPrefabs[0]);
                    pooledObject.SetActive(false);
                    objectPool.Enqueue(pooledObject);
                }
            }
        }
        
        private void UpdateARObjects()
        {
            // 활성 오브젝트 업데이트
            for (int i = activeObjects.Count - 1; i >= 0; i--)
            {
                ARObjectData objectData = activeObjects[i];
                
                // 오브젝트 수명 체크
                if (enableObjectPersistence && Time.time - objectData.spawnTime > objectLifetime)
                {
                    DespawnObject(objectData);
                    continue;
                }
                
                // 오브젝트 상태 업데이트
                UpdateObjectState(objectData);
            }
        }
        
        private void ProcessAutoSpawning()
        {
            if (!enableAutoSpawning) return;
            if (activeObjects.Count >= maxObjectsInScene) return;
            if (Time.time - lastSpawnTime < spawnInterval) return;
            
            // 자동 스폰 실행
            SpawnRandomObject();
        }
        
        private void ProcessObjectInteractions()
        {
            // 플레이어와 오브젝트 간 상호작용 처리
            Vector3 playerPosition = transform.position;
            
            foreach (var objectData in activeObjects)
            {
                if (objectData.gameObject == null) continue;
                
                float distance = Vector3.Distance(playerPosition, objectData.gameObject.transform.position);
                
                if (distance <= objectInteractionRange)
                {
                    // 상호작용 가능한 오브젝트
                    if (!objectData.isInteractable)
                    {
                        // 구조체는 foreach에서 수정할 수 없으므로 별도 처리
                        var modifiedData = objectData;
                        modifiedData.isInteractable = true;
                        HighlightObject(modifiedData, true);
                    }
                }
                else
                {
                    // 상호작용 불가능한 오브젝트
                    if (objectData.isInteractable)
                    {
                        // 구조체는 foreach에서 수정할 수 없으므로 별도 처리
                        var modifiedData = objectData;
                        modifiedData.isInteractable = false;
                        HighlightObject(modifiedData, false);
                    }
                }
            }
        }
        
        public void SpawnObjectAtPosition(Vector3 position, GameObject prefab = null)
        {
            if (activeObjects.Count >= maxObjectsInScene)
            {
                Debug.LogWarning("최대 오브젝트 수에 도달했습니다.");
                return;
            }
            
            // AR 평면 위에 배치
            Vector3 spawnPosition = GetARPlanePosition(position);
            
            // 프리팹 선택
            GameObject objectPrefab = prefab;
            if (objectPrefab == null && arObjectPrefabs != null && arObjectPrefabs.Length > 0)
            {
                objectPrefab = arObjectPrefabs[Random.Range(0, arObjectPrefabs.Length)];
            }
            
            if (objectPrefab == null)
            {
                Debug.LogError("스폰할 오브젝트 프리팹이 없습니다.");
                return;
            }
            
            // 오브젝트 생성
            GameObject newObject = CreateARObject(objectPrefab, spawnPosition);
            
            // AR 오브젝트 데이터 생성
            ARObjectData objectData = new ARObjectData
            {
                id = spawnCount++,
                gameObject = newObject,
                prefab = objectPrefab,
                spawnPosition = spawnPosition,
                spawnTime = Time.time,
                isInteractable = false,
                objectType = GetObjectType(objectPrefab),
                health = 100,
                maxHealth = 100,
                isDestroyed = false
            };
            
            // 오브젝트 등록
            activeObjects.Add(objectData);
            objectDataMap[newObject] = objectData;
            
            // 스폰 효과
            PlaySpawnEffect(spawnPosition);
            
            // 이벤트 발생
            OnObjectSpawned?.Invoke(objectData);
            OnObjectCountChanged?.Invoke(activeObjects.Count);
            
            lastSpawnTime = Time.time;
            lastSpawnPosition = spawnPosition;
            
            Debug.Log($"AR 오브젝트가 스폰되었습니다: {spawnPosition}");
        }
        
        public void SpawnRandomObject()
        {
            // 랜덤 위치에 오브젝트 스폰
            Vector3 randomPosition = GetRandomSpawnPosition();
            SpawnObjectAtPosition(randomPosition);
        }
        
        public void SpawnItemObject(Item item, Vector3 position)
        {
            if (item == null) return;
            
            // 아이템 기반 오브젝트 생성
            GameObject itemObject = CreateItemObject(item);
            SpawnObjectAtPosition(position, itemObject);
        }
        
        private GameObject CreateARObject(GameObject prefab, Vector3 position)
        {
            GameObject newObject;
            
            // 오브젝트 풀에서 가져오기
            if (objectPool.Count > 0)
            {
                newObject = objectPool.Dequeue();
                newObject.SetActive(true);
                newObject.transform.position = position;
                newObject.transform.rotation = Quaternion.identity;
            }
            else
            {
                // 새로 생성
                newObject = Instantiate(prefab, position, Quaternion.identity);
            }
            
            // AR 오브젝트 컴포넌트 추가
            ARObject arObjectComponent = newObject.GetComponent<ARObject>();
            if (arObjectComponent == null)
            {
                arObjectComponent = newObject.AddComponent<ARObject>();
            }
            
            // 물리 설정
            if (enableObjectPhysics)
            {
                Rigidbody rb = newObject.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = newObject.AddComponent<Rigidbody>();
                }
                rb.useGravity = true;
                rb.mass = 1f;
            }
            
            // 콜라이더 설정
            if (enableObjectCollision)
            {
                Collider collider = newObject.GetComponent<Collider>();
                if (collider == null)
                {
                    collider = newObject.AddComponent<BoxCollider>();
                }
                collider.isTrigger = false;
            }
            
            return newObject;
        }
        
        private GameObject CreateItemObject(Item item)
        {
            // 아이템 기반 오브젝트 생성
            GameObject itemObject = new GameObject($"Item_{item.name}");
            
            // 아이템 시각적 표현
            MeshRenderer renderer = itemObject.AddComponent<MeshRenderer>();
            MeshFilter filter = itemObject.AddComponent<MeshFilter>();
            
            // 아이템 등급에 따른 색상
            Material material = new Material(Shader.Find("Standard"));
            switch (item.rarity)
            {
                case ItemRarity.Common:
                    material.color = Color.white;
                    break;
                case ItemRarity.Rare:
                    material.color = Color.blue;
                    break;
                case ItemRarity.Epic:
                    material.color = Color.magenta;
                    break;
                case ItemRarity.Legendary:
                    material.color = Color.yellow;
                    break;
            }
            renderer.material = material;
            
            // 기본 큐브 메시
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            
            // 아이템 픽업 컴포넌트
            ItemPickup pickup = itemObject.AddComponent<ItemPickup>();
            pickup.SetItem(item);
            
            return itemObject;
        }
        
        private Vector3 GetARPlanePosition(Vector3 position)
        {
            // AR 평면과의 교차점 계산
            Ray ray = new Ray(position + Vector3.up * 10f, Vector3.down);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, arPlaneLayer))
            {
                return hit.point + Vector3.up * objectPlacementHeight;
            }
            
            return position;
        }
        
        private Vector3 GetRandomSpawnPosition()
        {
            // 플레이어 주변 랜덤 위치
            Vector3 playerPosition = transform.position;
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPosition = playerPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            return randomPosition;
        }
        
        private ARObjectType GetObjectType(GameObject obj)
        {
            // 오브젝트 타입 결정
            if (obj.GetComponent<ItemPickup>() != null)
                return ARObjectType.Item;
            else if (obj.GetComponent<EnemyController>() != null)
                return ARObjectType.Enemy;
            else if (obj.GetComponent<Projectile>() != null)
                return ARObjectType.Projectile;
            else
                return ARObjectType.Environment;
        }
        
        private void UpdateObjectState(ARObjectData objectData)
        {
            if (objectData.gameObject == null) return;
            
            // 오브젝트 상태 업데이트
            // 예: 애니메이션, 파티클 효과 등
        }
        
        private void HighlightObject(ARObjectData objectData, bool highlight)
        {
            if (objectData.gameObject == null) return;
            
            Renderer renderer = objectData.gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (highlight)
                {
                    renderer.material.color = Color.green;
                }
                else
                {
                    renderer.material.color = Color.white;
                }
            }
        }
        
        public void InteractWithObject(GameObject obj)
        {
            if (!objectDataMap.ContainsKey(obj)) return;
            
            ARObjectData objectData = objectDataMap[obj];
            
            if (!objectData.isInteractable) return;
            
            // 상호작용 처리
            switch (objectData.objectType)
            {
                case ARObjectType.Item:
                    InteractWithItem(objectData);
                    break;
                case ARObjectType.Enemy:
                    InteractWithEnemy(objectData);
                    break;
                case ARObjectType.Environment:
                    InteractWithEnvironment(objectData);
                    break;
            }
            
            // 상호작용 효과
            PlayInteractionEffect(obj.transform.position);
            
            OnObjectInteracted?.Invoke(objectData);
        }
        
        private void InteractWithItem(ARObjectData objectData)
        {
            // 아이템 상호작용
            ItemPickup pickup = objectData.gameObject.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                // 아이템 획득 처리
                Debug.Log("아이템을 획득했습니다.");
            }
        }
        
        private void InteractWithEnemy(ARObjectData objectData)
        {
            // 적 상호작용 (전투 시작)
            EnemyController enemy = objectData.gameObject.GetComponent<EnemyController>();
            if (enemy != null)
            {
                // 전투 시작
                Debug.Log("전투가 시작되었습니다.");
            }
        }
        
        private void InteractWithEnvironment(ARObjectData objectData)
        {
            // 환경 오브젝트 상호작용
            Debug.Log("환경 오브젝트와 상호작용했습니다.");
        }
        
        public void DestroyObject(GameObject obj)
        {
            if (!objectDataMap.ContainsKey(obj)) return;
            
            ARObjectData objectData = objectDataMap[obj];
            DestroyObject(objectData);
        }
        
        public void DestroyObject(ARObjectData objectData)
        {
            if (objectData.isDestroyed) return;
            
            objectData.isDestroyed = true;
            
            // 파괴 효과
            PlayDespawnEffect(objectData.gameObject.transform.position);
            
            // 오브젝트 제거
            DespawnObject(objectData);
            
            OnObjectDestroyed?.Invoke(objectData);
        }
        
        private void DespawnObject(ARObjectData objectData)
        {
            if (objectData.gameObject == null) return;
            
            // 오브젝트 비활성화
            objectData.gameObject.SetActive(false);
            
            // 오브젝트 풀에 반환
            objectPool.Enqueue(objectData.gameObject);
            
            // 리스트에서 제거
            activeObjects.Remove(objectData);
            objectDataMap.Remove(objectData.gameObject);
            
            OnObjectDespawned?.Invoke(objectData);
            OnObjectCountChanged?.Invoke(activeObjects.Count);
        }
        
        private void PlaySpawnEffect(Vector3 position)
        {
            if (spawnEffect != null)
            {
                GameObject effect = Instantiate(spawnEffect, position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            if (spawnSound != null)
            {
                AudioSource.PlayClipAtPoint(spawnSound, position);
            }
        }
        
        private void PlayDespawnEffect(Vector3 position)
        {
            if (despawnEffect != null)
            {
                GameObject effect = Instantiate(despawnEffect, position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            if (despawnSound != null)
            {
                AudioSource.PlayClipAtPoint(despawnSound, position);
            }
        }
        
        private void PlayInteractionEffect(Vector3 position)
        {
            if (interactionEffect != null)
            {
                GameObject effect = Instantiate(interactionEffect, position, Quaternion.identity);
                Destroy(effect, 1f);
            }
            
            if (interactionSound != null)
            {
                AudioSource.PlayClipAtPoint(interactionSound, position);
            }
        }
        
        // 이벤트 핸들러
        private void OnVirtualWorldPlaced(Vector3 position, Quaternion rotation)
        {
            // 가상 세계 배치 시 초기 오브젝트 스폰
            for (int i = 0; i < 3; i++)
            {
                Vector3 spawnPos = position + Random.insideUnitSphere * 5f;
                spawnPos.y = position.y;
                SpawnRandomObject();
            }
        }
        
        // 공개 메서드들
        public List<ARObjectData> GetActiveObjects()
        {
            return new List<ARObjectData>(activeObjects);
        }
        
        public ARObjectData? GetObjectData(GameObject obj)
        {
            return objectDataMap.ContainsKey(obj) ? objectDataMap[obj] : null;
        }
        
        public int GetActiveObjectCount()
        {
            return activeObjects.Count;
        }
        
        public void ClearAllObjects()
        {
            for (int i = activeObjects.Count - 1; i >= 0; i--)
            {
                DespawnObject(activeObjects[i]);
            }
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (arManager != null)
            {
                arManager.OnVirtualWorldPlaced -= OnVirtualWorldPlaced;
            }
        }
    }
    
    [System.Serializable]
    public struct ARObjectData
    {
        public int id;
        public GameObject gameObject;
        public GameObject prefab;
        public Vector3 spawnPosition;
        public float spawnTime;
        public bool isInteractable;
        public ARObjectType objectType;
        public int health;
        public int maxHealth;
        public bool isDestroyed;
    }
    
    public enum ARObjectType
    {
        Item,
        Enemy,
        Projectile,
        Environment,
        Player,
        NPC
    }
}
