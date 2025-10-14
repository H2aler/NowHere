using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using NowHere.XR;

namespace NowHere.XR
{
    /// <summary>
    /// VR 상호작용을 관리하는 클래스
    /// VR 컨트롤러, 핸드 트래킹, 물리 기반 상호작용을 통합 관리
    /// </summary>
    public class VRInteractionManager : MonoBehaviour
    {
        [Header("VR Interaction Settings")]
        [SerializeField] private float grabDistance = 0.1f;
        [SerializeField] private float throwForce = 10f;
        [SerializeField] private float teleportDistance = 20f;
        [SerializeField] private LayerMask interactableLayer = -1;
        
        [Header("Hand Tracking Settings")]
        [SerializeField] private float pinchThreshold = 0.8f;
        [SerializeField] private float grabThreshold = 0.9f;
        [SerializeField] private float pointThreshold = 0.7f;
        
        [Header("VR UI Settings")]
        [SerializeField] private float uiDistance = 2f;
        [SerializeField] private float uiScale = 1f;
        [SerializeField] private bool enableUIHover = true;
        
        [Header("Physics Settings")]
        [SerializeField] private float physicsGrabForce = 100f;
        [SerializeField] private bool enablePhysicsInteraction = true;
        [SerializeField] private float hapticIntensity = 0.5f;
        
        // VR 상호작용 상태
        private bool isLeftHandGrabbing = false;
        private bool isRightHandGrabbing = false;
        private bool isLeftHandPointing = false;
        private bool isRightHandPointing = false;
        
        // 현재 상호작용 중인 오브젝트
        private GameObject leftGrabbedObject = null;
        private GameObject rightGrabbedObject = null;
        private GameObject leftPointedObject = null;
        private GameObject rightPointedObject = null;
        
        // VR 입력 데이터
        private XRInputData leftControllerData;
        private XRInputData rightControllerData;
        private HandTrackingData leftHandData;
        private HandTrackingData rightHandData;
        
        // 상호작용 히스토리
        private List<VRInteraction> interactionHistory = new List<VRInteraction>();
        
        // 참조
        private XRManager xrManager;
        private XRInteractionManager xrInteractionManager;
        private Camera xrCamera;
        
        // 이벤트
        public System.Action<GameObject> OnObjectGrabbed;
        public System.Action<GameObject> OnObjectReleased;
        public System.Action<GameObject> OnObjectPointed;
        public System.Action<GameObject> OnObjectUnpointed;
        public System.Action<Vector3> OnTeleport;
        public System.Action<VRInteraction> OnInteractionPerformed;
        
        private void Start()
        {
            InitializeVRInteraction();
        }
        
        private void Update()
        {
            UpdateVRInteractionInput();
            ProcessVRInteractions();
            UpdateUIInteractions();
        }
        
        private void InitializeVRInteraction()
        {
            // 컴포넌트 참조
            xrManager = FindObjectOfType<XRManager>();
            xrInteractionManager = FindObjectOfType<XRInteractionManager>();
            xrCamera = Camera.main;
            
            if (xrCamera == null)
            {
                xrCamera = FindObjectOfType<Camera>();
            }
            
            // XR 이벤트 구독
            if (xrManager != null)
            {
                xrManager.OnControllerInput += OnControllerInput;
                xrManager.OnHandTrackingData += OnHandTrackingData;
            }
            
            Debug.Log("VR 상호작용 시스템이 초기화되었습니다.");
        }
        
        private void UpdateVRInteractionInput()
        {
            if (xrManager == null) return;
            
            // 컨트롤러 데이터 업데이트
            leftControllerData = xrManager.GetLeftControllerData();
            rightControllerData = xrManager.GetRightControllerData();
            
            // 핸드 트래킹 데이터 업데이트
            leftHandData = xrManager.GetLeftHandData();
            rightHandData = xrManager.GetRightHandData();
        }
        
        private void ProcessVRInteractions()
        {
            // 컨트롤러 기반 상호작용
            ProcessControllerInteractions();
            
            // 핸드 트래킹 기반 상호작용
            if (xrManager.IsHandTrackingActive())
            {
                ProcessHandTrackingInteractions();
            }
            
            // 텔레포트 처리
            ProcessTeleportation();
        }
        
        private void ProcessControllerInteractions()
        {
            // 왼쪽 컨트롤러 상호작용
            if (leftControllerData.isConnected)
            {
                ProcessControllerGrab(leftControllerData, ref isLeftHandGrabbing, ref leftGrabbedObject, true);
                ProcessControllerPointing(leftControllerData, ref isLeftHandPointing, ref leftPointedObject, true);
            }
            
            // 오른쪽 컨트롤러 상호작용
            if (rightControllerData.isConnected)
            {
                ProcessControllerGrab(rightControllerData, ref isRightHandGrabbing, ref rightGrabbedObject, false);
                ProcessControllerPointing(rightControllerData, ref isRightHandPointing, ref rightPointedObject, false);
            }
        }
        
        private void ProcessControllerGrab(XRInputData controllerData, ref bool isGrabbing, ref GameObject grabbedObject, bool isLeftHand)
        {
            // 그립 버튼으로 잡기/놓기
            if (controllerData.gripValue > grabThreshold && !isGrabbing)
            {
                // 잡기 시도
                GameObject targetObject = GetObjectAtPosition(controllerData.position);
                if (targetObject != null && IsInteractable(targetObject))
                {
                    GrabObject(targetObject, controllerData.position, controllerData.rotation, isLeftHand);
                    grabbedObject = targetObject;
                    isGrabbing = true;
                }
            }
            else if (controllerData.gripValue < grabThreshold && isGrabbing)
            {
                // 놓기
                if (grabbedObject != null)
                {
                    ReleaseObject(grabbedObject, controllerData.position, controllerData.rotation, isLeftHand);
                    grabbedObject = null;
                    isGrabbing = false;
                }
            }
        }
        
        private void ProcessControllerPointing(XRInputData controllerData, ref bool isPointing, ref GameObject pointedObject, bool isLeftHand)
        {
            // 트리거로 포인팅
            if (controllerData.triggerValue > pointThreshold && !isPointing)
            {
                GameObject targetObject = GetObjectAtPosition(controllerData.position);
                if (targetObject != null && IsInteractable(targetObject))
                {
                    PointAtObject(targetObject, isLeftHand);
                    pointedObject = targetObject;
                    isPointing = true;
                }
            }
            else if (controllerData.triggerValue < pointThreshold && isPointing)
            {
                if (pointedObject != null)
                {
                    UnpointAtObject(pointedObject, isLeftHand);
                    pointedObject = null;
                    isPointing = false;
                }
            }
        }
        
        private void ProcessHandTrackingInteractions()
        {
            // 왼쪽 손 상호작용
            if (leftHandData.isTracked)
            {
                ProcessHandGrab(leftHandData, ref isLeftHandGrabbing, ref leftGrabbedObject, true);
                ProcessHandPointing(leftHandData, ref isLeftHandPointing, ref leftPointedObject, true);
            }
            
            // 오른쪽 손 상호작용
            if (rightHandData.isTracked)
            {
                ProcessHandGrab(rightHandData, ref isRightHandGrabbing, ref rightGrabbedObject, false);
                ProcessHandPointing(rightHandData, ref isRightHandPointing, ref rightPointedObject, false);
            }
        }
        
        private void ProcessHandGrab(HandTrackingData handData, ref bool isGrabbing, ref GameObject grabbedObject, bool isLeftHand)
        {
            // 핀치 제스처로 잡기
            if (handData.isGrabbing && !isGrabbing)
            {
                GameObject targetObject = GetObjectAtPosition(handData.position);
                if (targetObject != null && IsInteractable(targetObject))
                {
                    GrabObject(targetObject, handData.position, handData.rotation, isLeftHand);
                    grabbedObject = targetObject;
                    isGrabbing = true;
                }
            }
            else if (!handData.isGrabbing && isGrabbing)
            {
                if (grabbedObject != null)
                {
                    ReleaseObject(grabbedObject, handData.position, handData.rotation, isLeftHand);
                    grabbedObject = null;
                    isGrabbing = false;
                }
            }
        }
        
        private void ProcessHandPointing(HandTrackingData handData, ref bool isPointing, ref GameObject pointedObject, bool isLeftHand)
        {
            // 포인팅 제스처
            if (handData.isPointing && !isPointing)
            {
                GameObject targetObject = GetObjectAtPosition(handData.position);
                if (targetObject != null && IsInteractable(targetObject))
                {
                    PointAtObject(targetObject, isLeftHand);
                    pointedObject = targetObject;
                    isPointing = true;
                }
            }
            else if (!handData.isPointing && isPointing)
            {
                if (pointedObject != null)
                {
                    UnpointAtObject(pointedObject, isLeftHand);
                    pointedObject = null;
                    isPointing = false;
                }
            }
        }
        
        private void ProcessTeleportation()
        {
            // 텔레포트 처리 (썸스틱 입력)
            if (leftControllerData.isConnected && leftControllerData.thumbstick.magnitude > 0.5f)
            {
                Vector3 teleportDirection = new Vector3(leftControllerData.thumbstick.x, 0, leftControllerData.thumbstick.y);
                Vector3 teleportPosition = xrCamera.transform.position + teleportDirection * teleportDistance;
                
                TeleportTo(teleportPosition);
            }
            
            if (rightControllerData.isConnected && rightControllerData.thumbstick.magnitude > 0.5f)
            {
                Vector3 teleportDirection = new Vector3(rightControllerData.thumbstick.x, 0, rightControllerData.thumbstick.y);
                Vector3 teleportPosition = xrCamera.transform.position + teleportDirection * teleportDistance;
                
                TeleportTo(teleportPosition);
            }
        }
        
        private void UpdateUIInteractions()
        {
            if (!enableUIHover) return;
            
            // UI 상호작용 업데이트
            // 실제 구현에서는 UI 요소들과의 상호작용 처리
        }
        
        private GameObject GetObjectAtPosition(Vector3 position)
        {
            // 위치에서 가장 가까운 상호작용 가능한 오브젝트 찾기
            Collider[] colliders = Physics.OverlapSphere(position, grabDistance, interactableLayer);
            
            if (colliders.Length > 0)
            {
                // 가장 가까운 오브젝트 반환
                GameObject closestObject = null;
                float closestDistance = float.MaxValue;
                
                foreach (Collider collider in colliders)
                {
                    float distance = Vector3.Distance(position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestObject = collider.gameObject;
                    }
                }
                
                return closestObject;
            }
            
            return null;
        }
        
        private bool IsInteractable(GameObject obj)
        {
            // 상호작용 가능한 오브젝트인지 확인
            return obj.GetComponent<VRInteractable>() != null || 
                   obj.GetComponent<XRGrabInteractable>() != null ||
                   obj.GetComponent<Collider>() != null;
        }
        
        public void GrabObject(GameObject obj, Vector3 position, Quaternion rotation, bool isLeftHand)
        {
            if (obj == null) return;
            
            // 오브젝트 잡기
            VRInteractable interactable = obj.GetComponent<VRInteractable>();
            if (interactable != null)
            {
                interactable.OnGrabbed(position, rotation, isLeftHand);
            }
            
            // 물리 기반 잡기
            if (enablePhysicsInteraction)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 물리 기반 잡기 로직
                    rb.isKinematic = true;
                    obj.transform.SetParent(xrCamera.transform);
                }
            }
            
            // 햅틱 피드백
            TriggerHapticFeedback(isLeftHand, hapticIntensity);
            
            // 이벤트 발생
            OnObjectGrabbed?.Invoke(obj);
            
            // 상호작용 히스토리 기록
            VRInteraction interaction = new VRInteraction
            {
                interactionType = VRInteractionType.Grab,
                targetObject = obj,
                position = position,
                rotation = rotation,
                isLeftHand = isLeftHand,
                timestamp = Time.time
            };
            
            interactionHistory.Add(interaction);
            OnInteractionPerformed?.Invoke(interaction);
            
            Debug.Log($"오브젝트를 잡았습니다: {obj.name} ({(isLeftHand ? "왼손" : "오른손")})");
        }
        
        public void ReleaseObject(GameObject obj, Vector3 position, Quaternion rotation, bool isLeftHand)
        {
            if (obj == null) return;
            
            // 오브젝트 놓기
            VRInteractable interactable = obj.GetComponent<VRInteractable>();
            if (interactable != null)
            {
                interactable.OnReleased(position, rotation, isLeftHand);
            }
            
            // 물리 기반 놓기
            if (enablePhysicsInteraction)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 물리 기반 놓기 로직
                    rb.isKinematic = false;
                    obj.transform.SetParent(null);
                    
                    // 던지기 힘 적용
                    Vector3 throwDirection = (position - xrCamera.transform.position).normalized;
                    rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
                }
            }
            
            // 이벤트 발생
            OnObjectReleased?.Invoke(obj);
            
            // 상호작용 히스토리 기록
            VRInteraction interaction = new VRInteraction
            {
                interactionType = VRInteractionType.Release,
                targetObject = obj,
                position = position,
                rotation = rotation,
                isLeftHand = isLeftHand,
                timestamp = Time.time
            };
            
            interactionHistory.Add(interaction);
            OnInteractionPerformed?.Invoke(interaction);
            
            Debug.Log($"오브젝트를 놓았습니다: {obj.name} ({(isLeftHand ? "왼손" : "오른손")})");
        }
        
        public void PointAtObject(GameObject obj, bool isLeftHand)
        {
            if (obj == null) return;
            
            // 오브젝트 포인팅
            VRInteractable interactable = obj.GetComponent<VRInteractable>();
            if (interactable != null)
            {
                interactable.OnPointed(isLeftHand);
            }
            
            // 이벤트 발생
            OnObjectPointed?.Invoke(obj);
            
            Debug.Log($"오브젝트를 가리켰습니다: {obj.name} ({(isLeftHand ? "왼손" : "오른손")})");
        }
        
        public void UnpointAtObject(GameObject obj, bool isLeftHand)
        {
            if (obj == null) return;
            
            // 오브젝트 포인팅 해제
            VRInteractable interactable = obj.GetComponent<VRInteractable>();
            if (interactable != null)
            {
                interactable.OnUnpointed(isLeftHand);
            }
            
            // 이벤트 발생
            OnObjectUnpointed?.Invoke(obj);
            
            Debug.Log($"오브젝트 포인팅을 해제했습니다: {obj.name} ({(isLeftHand ? "왼손" : "오른손")})");
        }
        
        public void TeleportTo(Vector3 position)
        {
            // 텔레포트 실행
            if (xrCamera != null)
            {
                xrCamera.transform.position = position;
                
                // 이벤트 발생
                OnTeleport?.Invoke(position);
                
                Debug.Log($"텔레포트했습니다: {position}");
            }
        }
        
        private void TriggerHapticFeedback(bool isLeftHand, float intensity)
        {
            // 햅틱 피드백 트리거
            // 실제 구현에서는 XR Input System 사용
            Debug.Log($"햅틱 피드백: {(isLeftHand ? "왼손" : "오른손")}, 강도: {intensity}");
        }
        
        // 이벤트 핸들러들
        private void OnControllerInput(XRInputData data)
        {
            // 컨트롤러 입력 처리
        }
        
        private void OnHandTrackingData(HandTrackingData data)
        {
            // 핸드 트래킹 데이터 처리
        }
        
        public List<VRInteraction> GetInteractionHistory()
        {
            return interactionHistory;
        }
        
        public void ClearInteractionHistory()
        {
            interactionHistory.Clear();
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (xrManager != null)
            {
                xrManager.OnControllerInput -= OnControllerInput;
                xrManager.OnHandTrackingData -= OnHandTrackingData;
            }
        }
    }
    
    // VR 상호작용 데이터 구조체들
    [System.Serializable]
    public struct VRInteraction
    {
        public VRInteractionType interactionType;
        public GameObject targetObject;
        public Vector3 position;
        public Quaternion rotation;
        public bool isLeftHand;
        public float timestamp;
    }
    
    public enum VRInteractionType
    {
        Grab,
        Release,
        Point,
        Unpoint,
        Teleport,
        UI
    }
}
