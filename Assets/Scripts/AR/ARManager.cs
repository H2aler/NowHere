using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

namespace NowHere.AR
{
    /// <summary>
    /// AR 기능을 관리하는 메인 매니저 클래스
    /// 현실 세계와 가상 세계를 연결하는 핵심 컴포넌트
    /// </summary>
    public class ARManager : MonoBehaviour
    {
        [Header("AR Components")]
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARSessionOrigin arSessionOrigin;
        [SerializeField] private ARPlaneManager arPlaneManager;
        [SerializeField] private ARPointCloudManager arPointCloudManager;
        [SerializeField] private ARRaycastManager arRaycastManager;
        
        [Header("AR Settings")]
        [SerializeField] private bool enablePlaneDetection = true;
        [SerializeField] private bool enablePointCloud = true;
        [SerializeField] private float planeDetectionDistance = 10f;
        
        [Header("Game Integration")]
        [SerializeField] private GameObject virtualWorldPrefab;
        [SerializeField] private Transform playerSpawnPoint;
        
        // AR 상태 관리
        private bool isARInitialized = false;
        private bool isPlaneDetected = false;
        private Vector3 detectedPlanePosition;
        private Quaternion detectedPlaneRotation;
        
        // 이벤트
        public System.Action OnARInitialized;
        public System.Action OnPlaneDetected;
        public System.Action<Vector3, Quaternion> OnVirtualWorldPlaced;
        
        private void Start()
        {
            InitializeAR();
        }
        
        private void InitializeAR()
        {
            if (arSession == null)
            {
                Debug.LogError("AR Session이 설정되지 않았습니다!");
                return;
            }
            
            // AR 세션 시작
            arSession.enabled = true;
            
            // 평면 감지 설정
            if (enablePlaneDetection && arPlaneManager != null)
            {
                arPlaneManager.enabled = true;
                arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
            }
            
            // 포인트 클라우드 설정
            if (enablePointCloud && arPointCloudManager != null)
            {
                arPointCloudManager.enabled = true;
            }
            
            isARInitialized = true;
            OnARInitialized?.Invoke();
            
            Debug.Log("AR 시스템이 초기화되었습니다.");
        }
        
        public void Update()
        {
            if (!isARInitialized) return;
            
            // 평면 감지 확인
            CheckPlaneDetection();
            
            // 터치 입력 처리
            HandleTouchInput();
        }
        
        private void CheckPlaneDetection()
        {
            if (arPlaneManager == null || isPlaneDetected) return;
            
            // 감지된 평면이 있는지 확인
            if (arPlaneManager.trackables.count > 0)
            {
                var plane = arPlaneManager.trackables.GetEnumerator().Current;
                if (plane != null)
                {
                    detectedPlanePosition = plane.transform.position;
                    detectedPlaneRotation = plane.transform.rotation;
                    isPlaneDetected = true;
                    
                    OnPlaneDetected?.Invoke();
                    Debug.Log($"평면이 감지되었습니다: {detectedPlanePosition}");
                }
            }
        }
        
        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Began)
                {
                    HandleTouch(touch.position);
                }
            }
        }
        
        private void HandleTouch(Vector2 screenPosition)
        {
            if (arRaycastManager == null) return;
            
            // 화면 터치 지점에서 AR 레이캐스트 수행
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (arRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                var hit = hits[0];
                Vector3 worldPosition = hit.pose.position;
                Quaternion worldRotation = hit.pose.rotation;
                
                // 가상 세계 배치
                PlaceVirtualWorld(worldPosition, worldRotation);
            }
        }
        
        public void PlaceVirtualWorld(Vector3 position, Quaternion rotation)
        {
            if (virtualWorldPrefab == null)
            {
                Debug.LogWarning("가상 세계 프리팹이 설정되지 않았습니다!");
                return;
            }
            
            // 가상 세계 인스턴스 생성
            GameObject virtualWorld = Instantiate(virtualWorldPrefab, position, rotation);
            
            // 플레이어 스폰 포인트 설정
            if (playerSpawnPoint != null)
            {
                playerSpawnPoint.position = position;
                playerSpawnPoint.rotation = rotation;
            }
            
            OnVirtualWorldPlaced?.Invoke(position, rotation);
            Debug.Log($"가상 세계가 배치되었습니다: {position}");
        }
        
        public void TogglePlaneDetection(bool enable)
        {
            enablePlaneDetection = enable;
            if (arPlaneManager != null)
            {
                arPlaneManager.enabled = enable;
            }
        }
        
        public void TogglePointCloud(bool enable)
        {
            enablePointCloud = enable;
            if (arPointCloudManager != null)
            {
                arPointCloudManager.enabled = enable;
            }
        }
        
        public bool IsARReady()
        {
            return isARInitialized && isPlaneDetected;
        }
        
        public Vector3 GetDetectedPlanePosition()
        {
            return detectedPlanePosition;
        }
        
        public Quaternion GetDetectedPlaneRotation()
        {
            return detectedPlaneRotation;
        }
    }
}
