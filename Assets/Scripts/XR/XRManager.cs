using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

namespace NowHere.XR
{
    /// <summary>
    /// XR 시스템을 관리하는 메인 매니저 클래스
    /// VR, AR, MR을 통합하여 관리하는 핵심 컴포넌트
    /// </summary>
    public class XRManager : MonoBehaviour
    {
        [Header("XR Components")]
        [SerializeField] private XROrigin xrOrigin;
        [SerializeField] private XRInteractionManager xrInteractionManager;
        [SerializeField] private XRRayInteractor leftRayInteractor;
        [SerializeField] private XRRayInteractor rightRayInteractor;
        [SerializeField] private XRDirectInteractor leftDirectInteractor;
        [SerializeField] private XRDirectInteractor rightDirectInteractor;
        
        [Header("XR Settings")]
        [SerializeField] private XRMode currentXRMode = XRMode.VR;
        [SerializeField] private bool enableHandTracking = true;
        [SerializeField] private bool enableEyeTracking = false;
        [SerializeField] private bool enableVoiceCommands = true;
        [SerializeField] private float interactionDistance = 10f;
        
        [Header("VR Controllers")]
        [SerializeField] private GameObject leftControllerPrefab;
        [SerializeField] private GameObject rightControllerPrefab;
        [SerializeField] private GameObject handPrefab;
        
        [Header("XR UI")]
        [SerializeField] private Canvas xrUICanvas;
        [SerializeField] private GameObject vrMenuPrefab;
        [SerializeField] private GameObject arMenuPrefab;
        
        // XR 상태 관리
        private bool isXRInitialized = false;
        private bool isHandTrackingActive = false;
        private bool isEyeTrackingActive = false;
        private XRMode previousXRMode;
        
        // 컨트롤러 참조
        private GameObject leftController;
        private GameObject rightController;
        private GameObject leftHand;
        private GameObject rightHand;
        
        // XR 입력 데이터
        private XRInputData leftControllerData;
        private XRInputData rightControllerData;
        private HandTrackingData leftHandData;
        private HandTrackingData rightHandData;
        private EyeTrackingData eyeTrackingData;
        
        // 이벤트
        public System.Action<XRMode> OnXRModeChanged;
        public System.Action<XRInputData> OnControllerInput;
        public System.Action<HandTrackingData> OnHandTrackingData;
        public System.Action<EyeTrackingData> OnEyeTrackingData;
        public System.Action<string> OnVoiceCommand;
        public System.Action OnXRInitialized;
        
        // 싱글톤 패턴
        public static XRManager Instance { get; private set; }
        
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
            InitializeXR();
        }
        
        private void Update()
        {
            if (!isXRInitialized) return;
            
            UpdateXRInput();
            UpdateHandTracking();
            UpdateEyeTracking();
            UpdateVoiceCommands();
        }
        
        private void InitializeXR()
        {
            Debug.Log("XR 시스템 초기화 시작...");
            
            // XR 기기 확인
            if (!XRSettings.isDeviceActive)
            {
                Debug.LogWarning("XR 기기가 감지되지 않았습니다. 시뮬레이션 모드로 실행됩니다.");
            }
            
            // XR Origin 설정
            if (xrOrigin == null)
            {
                xrOrigin = FindObjectOfType<XROrigin>();
            }
            
            // XR Interaction Manager 설정
            if (xrInteractionManager == null)
            {
                xrInteractionManager = FindObjectOfType<XRInteractionManager>();
            }
            
            // 컨트롤러 초기화
            InitializeControllers();
            
            // 핸드 트래킹 초기화
            if (enableHandTracking)
            {
                InitializeHandTracking();
            }
            
            // 아이 트래킹 초기화
            if (enableEyeTracking)
            {
                InitializeEyeTracking();
            }
            
            // XR UI 초기화
            InitializeXRUI();
            
            // XR 모드 설정
            SetXRMode(currentXRMode);
            
            isXRInitialized = true;
            OnXRInitialized?.Invoke();
            
            Debug.Log($"XR 시스템이 초기화되었습니다. 모드: {currentXRMode}");
        }
        
        private void InitializeControllers()
        {
            // 왼쪽 컨트롤러
            if (leftControllerPrefab != null)
            {
                leftController = Instantiate(leftControllerPrefab);
                leftController.transform.SetParent(xrOrigin.CameraFloorOffsetObject.transform);
                leftController.name = "Left Controller";
            }
            
            // 오른쪽 컨트롤러
            if (rightControllerPrefab != null)
            {
                rightController = Instantiate(rightControllerPrefab);
                rightController.transform.SetParent(xrOrigin.CameraFloorOffsetObject.transform);
                rightController.name = "Right Controller";
            }
            
            // 컨트롤러 데이터 초기화
            leftControllerData = new XRInputData();
            rightControllerData = new XRInputData();
        }
        
        private void InitializeHandTracking()
        {
            // 핸드 트래킹 초기화
            if (handPrefab != null)
            {
                leftHand = Instantiate(handPrefab);
                rightHand = Instantiate(handPrefab);
                
                leftHand.transform.SetParent(xrOrigin.CameraFloorOffsetObject.transform);
                rightHand.transform.SetParent(xrOrigin.CameraFloorOffsetObject.transform);
                
                leftHand.name = "Left Hand";
                rightHand.name = "Right Hand";
            }
            
            // 핸드 데이터 초기화
            leftHandData = new HandTrackingData();
            rightHandData = new HandTrackingData();
            
            isHandTrackingActive = true;
            Debug.Log("핸드 트래킹이 활성화되었습니다.");
        }
        
        private void InitializeEyeTracking()
        {
            // 아이 트래킹 초기화 (지원되는 기기에서만)
            if (XRSettings.supportedDevices.Contains("oculus") || 
                XRSettings.supportedDevices.Contains("openxr"))
            {
                eyeTrackingData = new EyeTrackingData();
                isEyeTrackingActive = true;
                Debug.Log("아이 트래킹이 활성화되었습니다.");
            }
            else
            {
                Debug.LogWarning("현재 기기에서 아이 트래킹을 지원하지 않습니다.");
            }
        }
        
        private void InitializeXRUI()
        {
            // XR UI 캔버스 설정
            if (xrUICanvas != null)
            {
                xrUICanvas.renderMode = RenderMode.WorldSpace;
                xrUICanvas.worldCamera = xrOrigin.Camera;
                
                // VR/AR에 따른 UI 설정
                UpdateXRUI();
            }
        }
        
        private void UpdateXRInput()
        {
            // 왼쪽 컨트롤러 입력
            UpdateControllerInput(InputDeviceRole.LeftHanded, ref leftControllerData);
            
            // 오른쪽 컨트롤러 입력
            UpdateControllerInput(InputDeviceRole.RightHanded, ref rightControllerData);
            
            // 컨트롤러 입력 이벤트 발생
            OnControllerInput?.Invoke(leftControllerData);
            OnControllerInput?.Invoke(rightControllerData);
        }
        
        private void UpdateControllerInput(InputDeviceRole role, ref XRInputData data)
        {
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesWithRole(role, devices);
            
            if (devices.Count > 0)
            {
                InputDevice device = devices[0];
                
                // 트리거 입력
                if (device.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
                {
                    data.triggerValue = triggerValue;
                }
                
                // 그립 입력
                if (device.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
                {
                    data.gripValue = gripValue;
                }
                
                // 버튼 입력
                if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton))
                {
                    data.primaryButton = primaryButton;
                }
                
                if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButton))
                {
                    data.secondaryButton = secondaryButton;
                }
                
                // 썸스틱 입력
                if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstick))
                {
                    data.thumbstick = thumbstick;
                }
                
                // 포지션 및 로테이션
                if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
                {
                    data.position = position;
                }
                
                if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
                {
                    data.rotation = rotation;
                }
                
                data.isConnected = true;
            }
            else
            {
                data.isConnected = false;
            }
        }
        
        private void UpdateHandTracking()
        {
            if (!isHandTrackingActive) return;
            
            // 핸드 트래킹 데이터 업데이트
            // 실제 구현에서는 OpenXR Hand Tracking API 사용
            // 여기서는 시뮬레이션 데이터 사용
            
            leftHandData.isTracked = true;
            rightHandData.isTracked = true;
            
            // 핸드 트래킹 데이터 이벤트 발생
            OnHandTrackingData?.Invoke(leftHandData);
            OnHandTrackingData?.Invoke(rightHandData);
        }
        
        private void UpdateEyeTracking()
        {
            if (!isEyeTrackingActive) return;
            
            // 아이 트래킹 데이터 업데이트
            // 실제 구현에서는 OpenXR Eye Tracking API 사용
            
            eyeTrackingData.isTracked = true;
            eyeTrackingData.gazeDirection = xrOrigin.Camera.transform.forward;
            eyeTrackingData.gazePosition = xrOrigin.Camera.transform.position + eyeTrackingData.gazeDirection * 5f;
            
            // 아이 트래킹 데이터 이벤트 발생
            OnEyeTrackingData?.Invoke(eyeTrackingData);
        }
        
        private void UpdateVoiceCommands()
        {
            if (!enableVoiceCommands) return;
            
            // 음성 명령 처리
            // 실제 구현에서는 음성 인식 API 사용
            // 여기서는 키보드 입력으로 시뮬레이션
            
            if (Input.GetKeyDown(KeyCode.V))
            {
                OnVoiceCommand?.Invoke("attack");
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                OnVoiceCommand?.Invoke("defend");
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                OnVoiceCommand?.Invoke("skill");
            }
        }
        
        public void SetXRMode(XRMode mode)
        {
            if (currentXRMode == mode) return;
            
            previousXRMode = currentXRMode;
            currentXRMode = mode;
            
            switch (mode)
            {
                case XRMode.VR:
                    SetVRMode();
                    break;
                case XRMode.AR:
                    SetARMode();
                    break;
                case XRMode.MR:
                    SetMRMode();
                    break;
            }
            
            UpdateXRUI();
            OnXRModeChanged?.Invoke(mode);
            
            Debug.Log($"XR 모드가 변경되었습니다: {previousXRMode} -> {mode}");
        }
        
        private void SetVRMode()
        {
            // VR 모드 설정
            xrOrigin.Camera.clearFlags = CameraClearFlags.Skybox;
            
            // 컨트롤러 활성화
            if (leftController != null) leftController.SetActive(true);
            if (rightController != null) rightController.SetActive(true);
            
            // 핸드 트래킹 비활성화 (컨트롤러 우선)
            if (leftHand != null) leftHand.SetActive(false);
            if (rightHand != null) rightHand.SetActive(false);
        }
        
        private void SetARMode()
        {
            // AR 모드 설정
            xrOrigin.Camera.clearFlags = CameraClearFlags.SolidColor;
            xrOrigin.Camera.backgroundColor = Color.clear;
            
            // 컨트롤러 비활성화
            if (leftController != null) leftController.SetActive(false);
            if (rightController != null) rightController.SetActive(false);
            
            // 핸드 트래킹 활성화
            if (leftHand != null) leftHand.SetActive(true);
            if (rightHand != null) rightHand.SetActive(true);
        }
        
        private void SetMRMode()
        {
            // MR 모드 설정 (VR + AR 혼합)
            xrOrigin.Camera.clearFlags = CameraClearFlags.Skybox;
            
            // 모든 입력 방식 활성화
            if (leftController != null) leftController.SetActive(true);
            if (rightController != null) rightController.SetActive(true);
            if (leftHand != null) leftHand.SetActive(true);
            if (rightHand != null) rightHand.SetActive(true);
        }
        
        private void UpdateXRUI()
        {
            if (xrUICanvas == null) return;
            
            // XR 모드에 따른 UI 업데이트
            switch (currentXRMode)
            {
                case XRMode.VR:
                    if (vrMenuPrefab != null)
                    {
                        // VR UI 활성화
                    }
                    break;
                case XRMode.AR:
                    if (arMenuPrefab != null)
                    {
                        // AR UI 활성화
                    }
                    break;
                case XRMode.MR:
                    // MR UI 활성화
                    break;
            }
        }
        
        public XRMode GetCurrentXRMode()
        {
            return currentXRMode;
        }
        
        public bool IsXRInitialized()
        {
            return isXRInitialized;
        }
        
        public bool IsHandTrackingActive()
        {
            return isHandTrackingActive;
        }
        
        public bool IsEyeTrackingActive()
        {
            return isEyeTrackingActive;
        }
        
        public XRInputData GetLeftControllerData()
        {
            return leftControllerData;
        }
        
        public XRInputData GetRightControllerData()
        {
            return rightControllerData;
        }
        
        public HandTrackingData GetLeftHandData()
        {
            return leftHandData;
        }
        
        public HandTrackingData GetRightHandData()
        {
            return rightHandData;
        }
        
        public EyeTrackingData GetEyeTrackingData()
        {
            return eyeTrackingData;
        }
    }
    
    // XR 데이터 구조체들
    [System.Serializable]
    public struct XRInputData
    {
        public bool isConnected;
        public float triggerValue;
        public float gripValue;
        public bool primaryButton;
        public bool secondaryButton;
        public Vector2 thumbstick;
        public Vector3 position;
        public Quaternion rotation;
    }
    
    [System.Serializable]
    public struct HandTrackingData
    {
        public bool isTracked;
        public Vector3 position;
        public Quaternion rotation;
        public float[] fingerBends; // 5개 손가락의 굽힘 정도
        public bool isPointing;
        public bool isGrabbing;
    }
    
    [System.Serializable]
    public struct EyeTrackingData
    {
        public bool isTracked;
        public Vector3 gazeDirection;
        public Vector3 gazePosition;
        public float eyeOpenness;
        public Vector2 pupilPosition;
    }
    
    public enum XRMode
    {
        VR,     // 가상 현실
        AR,     // 증강 현실
        MR      // 혼합 현실
    }
}
