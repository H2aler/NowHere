using UnityEngine;
using System.Collections.Generic;
using NowHere.Sensors;
using NowHere.RPG;

namespace NowHere.Interaction
{
    /// <summary>
    /// 터치 인터랙션을 관리하는 클래스
    /// 터치를 통한 오브젝트 생성, 파괴, 조작 기능
    /// </summary>
    public class TouchInteractionManager : MonoBehaviour
    {
        [Header("Touch Settings")]
        [SerializeField] private float touchSensitivity = 1f;
        [SerializeField] private float longPressTime = 1f;
        [SerializeField] private float doubleTapTime = 0.3f;
        [SerializeField] private float pinchSensitivity = 1f;
        
        [Header("Object Creation")]
        [SerializeField] private GameObject[] creatableObjects;
        [SerializeField] private LayerMask groundLayer = 1;
        [SerializeField] private float creationHeight = 0.1f;
        
        [Header("Destruction")]
        [SerializeField] private LayerMask destructibleLayer = 1;
        [SerializeField] private float destructionRange = 2f;
        [SerializeField] private GameObject destructionEffect;
        
        [Header("Manipulation")]
        [SerializeField] private float manipulationSpeed = 2f;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float scaleSpeed = 0.5f;
        
        // 터치 상태
        private List<TouchData> activeTouches = new List<TouchData>();
        private TouchData lastTouch;
        private float lastTapTime;
        private bool isManipulating = false;
        private GameObject selectedObject;
        
        // 참조
        private Camera arCamera;
        private MobileSensorManager sensorManager;
        private ItemSystem itemSystem;
        
        // 이벤트
        public System.Action<Vector3> OnObjectCreated;
        public System.Action<GameObject> OnObjectDestroyed;
        public System.Action<GameObject> OnObjectSelected;
        public System.Action<GameObject> OnObjectManipulated;
        public System.Action<string> OnChatMessageSent;
        
        private void Start()
        {
            InitializeTouchSystem();
        }
        
        private void Update()
        {
            HandleTouchInput();
        }
        
        private void InitializeTouchSystem()
        {
            // AR 카메라 참조
            arCamera = Camera.main;
            if (arCamera == null)
            {
                arCamera = FindObjectOfType<Camera>();
            }
            
            // 센서 매니저 참조
            sensorManager = FindObjectOfType<MobileSensorManager>();
            
            // 아이템 시스템 참조
            itemSystem = FindObjectOfType<ItemSystem>();
            
            Debug.Log("터치 인터랙션 시스템이 초기화되었습니다.");
        }
        
        private void HandleTouchInput()
        {
            if (Input.touchCount == 0)
            {
                // 터치가 없을 때
                if (activeTouches.Count > 0)
                {
                    ProcessTouchEnd();
                }
                return;
            }
            
            // 현재 터치들 업데이트
            UpdateActiveTouches();
            
            // 터치 제스처 처리
            ProcessTouchGestures();
        }
        
        private void UpdateActiveTouches()
        {
            activeTouches.Clear();
            
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                TouchData touchData = new TouchData
                {
                    fingerId = touch.fingerId,
                    position = touch.position,
                    deltaPosition = touch.deltaPosition,
                    phase = touch.phase,
                    startTime = Time.time,
                    startPosition = touch.position
                };
                
                activeTouches.Add(touchData);
            }
        }
        
        private void ProcessTouchGestures()
        {
            if (activeTouches.Count == 1)
            {
                ProcessSingleTouch(activeTouches[0]);
            }
            else if (activeTouches.Count == 2)
            {
                ProcessTwoFingerTouch(activeTouches[0], activeTouches[1]);
            }
            else if (activeTouches.Count > 2)
            {
                ProcessMultiTouch();
            }
        }
        
        private void ProcessSingleTouch(TouchData touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnTouchBegan(touch);
                    break;
                case TouchPhase.Moved:
                    OnTouchMoved(touch);
                    break;
                case TouchPhase.Ended:
                    OnTouchEnded(touch);
                    break;
                case TouchPhase.Canceled:
                    OnTouchCanceled(touch);
                    break;
            }
        }
        
        private void ProcessTwoFingerTouch(TouchData touch1, TouchData touch2)
        {
            // 핀치 제스처 처리
            if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                ProcessPinchGesture(touch1, touch2);
            }
            
            // 회전 제스처 처리
            ProcessRotationGesture(touch1, touch2);
        }
        
        private void ProcessMultiTouch()
        {
            // 멀티 터치 처리 (복잡한 제스처)
            Debug.Log("멀티 터치 감지");
        }
        
        private void OnTouchBegan(TouchData touch)
        {
            lastTouch = touch;
            
            // 오브젝트 선택 확인
            GameObject hitObject = GetObjectAtPosition(touch.position);
            if (hitObject != null)
            {
                SelectObject(hitObject);
            }
        }
        
        private void OnTouchMoved(TouchData touch)
        {
            if (selectedObject != null && isManipulating)
            {
                // 오브젝트 조작
                ManipulateObject(touch);
            }
        }
        
        private void OnTouchEnded(TouchData touch)
        {
            float touchDuration = Time.time - touch.startTime;
            
            // 더블 탭 확인
            if (Time.time - lastTapTime < doubleTapTime)
            {
                OnDoubleTap(touch);
            }
            else
            {
                // 싱글 탭 처리
                if (touchDuration < 0.2f)
                {
                    OnSingleTap(touch);
                }
                // 롱 프레스 확인
                else if (touchDuration >= longPressTime)
                {
                    OnLongPress(touch);
                }
            }
            
            lastTapTime = Time.time;
            isManipulating = false;
        }
        
        private void OnTouchCanceled(TouchData touch)
        {
            isManipulating = false;
            selectedObject = null;
        }
        
        private void ProcessTouchEnd()
        {
            isManipulating = false;
            selectedObject = null;
        }
        
        private void OnSingleTap(TouchData touch)
        {
            Vector3 worldPosition = GetWorldPosition(touch.position);
            
            // 오브젝트 생성
            CreateObjectAtPosition(worldPosition);
        }
        
        private void OnDoubleTap(TouchData touch)
        {
            GameObject hitObject = GetObjectAtPosition(touch.position);
            if (hitObject != null)
            {
                // 오브젝트 파괴
                DestroyObject(hitObject);
            }
        }
        
        private void OnLongPress(TouchData touch)
        {
            GameObject hitObject = GetObjectAtPosition(touch.position);
            if (hitObject != null)
            {
                // 오브젝트 선택 및 조작 모드 시작
                SelectObject(hitObject);
                isManipulating = true;
            }
        }
        
        private void ProcessPinchGesture(TouchData touch1, TouchData touch2)
        {
            if (selectedObject == null) return;
            
            // 핀치 거리 계산
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            float previousDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
            
            float scaleFactor = currentDistance / previousDistance;
            
            // 오브젝트 스케일 조정
            Vector3 newScale = selectedObject.transform.localScale * scaleFactor;
            newScale = Vector3.Max(newScale, Vector3.one * 0.1f); // 최소 크기 제한
            newScale = Vector3.Min(newScale, Vector3.one * 5f); // 최대 크기 제한
            
            selectedObject.transform.localScale = newScale;
            
            OnObjectManipulated?.Invoke(selectedObject);
        }
        
        private void ProcessRotationGesture(TouchData touch1, TouchData touch2)
        {
            if (selectedObject == null) return;
            
            // 회전 각도 계산
            Vector2 currentVector = touch2.position - touch1.position;
            Vector2 previousVector = (touch2.position - touch2.deltaPosition) - (touch1.position - touch1.deltaPosition);
            
            float angle = Vector2.SignedAngle(previousVector, currentVector);
            
            // 오브젝트 회전
            selectedObject.transform.Rotate(0, angle, 0);
            
            OnObjectManipulated?.Invoke(selectedObject);
        }
        
        private void ManipulateObject(TouchData touch)
        {
            if (selectedObject == null) return;
            
            // 터치 위치를 월드 좌표로 변환
            Vector3 worldPosition = GetWorldPosition(touch.position);
            
            // 오브젝트 위치 업데이트
            selectedObject.transform.position = worldPosition;
            
            OnObjectManipulated?.Invoke(selectedObject);
        }
        
        private Vector3 GetWorldPosition(Vector2 screenPosition)
        {
            // AR 평면과의 교차점 계산
            Ray ray = arCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                return hit.point + Vector3.up * creationHeight;
            }
            
            // 평면이 없으면 카메라 앞쪽에 생성
            return ray.GetPoint(2f);
        }
        
        private GameObject GetObjectAtPosition(Vector2 screenPosition)
        {
            Ray ray = arCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                return hit.collider.gameObject;
            }
            
            return null;
        }
        
        private void CreateObjectAtPosition(Vector3 position)
        {
            if (creatableObjects == null || creatableObjects.Length == 0) return;
            
            // 랜덤 오브젝트 선택
            GameObject objectToCreate = creatableObjects[Random.Range(0, creatableObjects.Length)];
            
            // 오브젝트 생성
            GameObject newObject = Instantiate(objectToCreate, position, Quaternion.identity);
            
            // 생성 효과
            PlayCreationEffect(position);
            
            OnObjectCreated?.Invoke(position);
            Debug.Log($"오브젝트가 생성되었습니다: {position}");
        }
        
        private void DestroyObject(GameObject objectToDestroy)
        {
            if (objectToDestroy == null) return;
            
            // 파괴 효과
            PlayDestructionEffect(objectToDestroy.transform.position);
            
            // 오브젝트 제거
            Destroy(objectToDestroy);
            
            OnObjectDestroyed?.Invoke(objectToDestroy);
            Debug.Log("오브젝트가 파괴되었습니다.");
        }
        
        private void SelectObject(GameObject obj)
        {
            selectedObject = obj;
            OnObjectSelected?.Invoke(obj);
            
            // 선택 효과 (하이라이트 등)
            HighlightObject(obj, true);
        }
        
        private void HighlightObject(GameObject obj, bool highlight)
        {
            // 오브젝트 하이라이트 효과
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (highlight)
                {
                    renderer.material.color = Color.yellow;
                }
                else
                {
                    renderer.material.color = Color.white;
                }
            }
        }
        
        private void PlayCreationEffect(Vector3 position)
        {
            // 생성 파티클 효과
            if (destructionEffect != null)
            {
                GameObject effect = Instantiate(destructionEffect, position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        
        private void PlayDestructionEffect(Vector3 position)
        {
            // 파괴 파티클 효과
            if (destructionEffect != null)
            {
                GameObject effect = Instantiate(destructionEffect, position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        
        // 공개 메서드들
        public void SetCreatableObjects(GameObject[] objects)
        {
            creatableObjects = objects;
        }
        
        public GameObject GetSelectedObject()
        {
            return selectedObject;
        }
        
        public bool IsManipulating()
        {
            return isManipulating;
        }
        
        public void ClearSelection()
        {
            if (selectedObject != null)
            {
                HighlightObject(selectedObject, false);
                selectedObject = null;
            }
        }
        
        public void SpawnObjectAtPosition(Vector3 position, GameObject prefab = null)
        {
            // 위치에 오브젝트 생성
            GameObject objectToSpawn = prefab;
            if (objectToSpawn == null && creatableObjects != null && creatableObjects.Length > 0)
            {
                objectToSpawn = creatableObjects[Random.Range(0, creatableObjects.Length)];
            }
            
            if (objectToSpawn != null)
            {
                Vector3 spawnPosition = position + Vector3.up * creationHeight;
                GameObject newObject = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
                
                OnObjectCreated?.Invoke(spawnPosition);
                Debug.Log($"오브젝트 생성됨: {spawnPosition}");
            }
        }
    }
    
    [System.Serializable]
    public struct TouchData
    {
        public int fingerId;
        public Vector2 position;
        public Vector2 deltaPosition;
        public TouchPhase phase;
        public float startTime;
        public Vector2 startPosition;
    }
}
