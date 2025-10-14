using UnityEngine;
using System.Collections.Generic;
using System;

namespace NowHere.Sensors
{
    /// <summary>
    /// 모바일 센서를 통합 관리하는 클래스
    /// 카메라, 위치, 가속도, 자이로스코프, 나침반 센서를 활용
    /// </summary>
    public class MobileSensorManager : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private bool enableCameraTracking = true;
        [SerializeField] private float cameraUpdateInterval = 0.1f;
        [SerializeField] private int cameraWidth = 1920;
        [SerializeField] private int cameraHeight = 1080;
        
        [Header("Location Settings")]
        [SerializeField] private bool enableLocationTracking = true;
        [SerializeField] private float locationUpdateInterval = 1f;
        [SerializeField] private float locationAccuracy = 1f;
        
        [Header("Motion Settings")]
        [SerializeField] private bool enableMotionTracking = true;
        [SerializeField] private float motionUpdateInterval = 0.05f;
        [SerializeField] private float motionSensitivity = 1f;
        
        [Header("Gyroscope Settings")]
        [SerializeField] private bool enableGyroscope = true;
        [SerializeField] private float gyroSensitivity = 1f;
        [SerializeField] private bool enableGyroCalibration = true;
        
        [Header("Compass Settings")]
        [SerializeField] private bool enableCompass = true;
        [SerializeField] private float compassUpdateInterval = 0.1f;
        
        // 센서 데이터
        private CameraData cameraData;
        private LocationData locationData;
        private MotionData motionData;
        private GyroscopeData gyroData;
        private CompassData compassData;
        
        // 센서 상태
        private bool isInitialized = false;
        private float lastCameraUpdate;
        private float lastLocationUpdate;
        private float lastMotionUpdate;
        private float lastCompassUpdate;
        
        // 이벤트
        public event Action<CameraData> OnCameraDataUpdated;
        public event Action<LocationData> OnLocationDataUpdated;
        public event Action<MotionData> OnMotionDataUpdated;
        public event Action<GyroscopeData> OnGyroDataUpdated;
        public event Action<CompassData> OnCompassDataUpdated;
        public event Action<SensorData> OnSensorDataUpdated;
        
        private void Start()
        {
            InitializeSensors();
        }
        
        public void Update()
        {
            if (!isInitialized) return;
            
            UpdateSensors();
        }
        
        private void InitializeSensors()
        {
            // 센서 초기화
            InitializeCamera();
            InitializeLocation();
            InitializeMotion();
            InitializeGyroscope();
            InitializeCompass();
            
            isInitialized = true;
            Debug.Log("모바일 센서 시스템이 초기화되었습니다.");
        }
        
        private void InitializeCamera()
        {
            if (!enableCameraTracking) return;
            
            // 카메라 권한 요청
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                StartCoroutine(RequestCameraPermission());
            }
            
            cameraData = new CameraData();
        }
        
        private void InitializeLocation()
        {
            if (!enableLocationTracking) return;
            
            // 위치 권한 요청
            if (!Input.location.isEnabledByUser)
            {
                StartCoroutine(RequestLocationPermission());
            }
            
            // 위치 서비스 시작
            Input.location.Start(locationAccuracy, locationAccuracy);
            
            locationData = new LocationData();
        }
        
        private void InitializeMotion()
        {
            if (!enableMotionTracking) return;
            
            // 가속도계 활성화
            Input.acceleration.ToString(); // 가속도계 초기화
            
            motionData = new MotionData();
        }
        
        private void InitializeGyroscope()
        {
            if (!enableGyroscope) return;
            
            // 자이로스코프 활성화
            if (SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = true;
                Input.gyro.updateInterval = motionUpdateInterval;
                
                if (enableGyroCalibration)
                {
                    Input.gyro.enabled = false;
                    Input.gyro.enabled = true;
                }
            }
            
            gyroData = new GyroscopeData();
        }
        
        private void InitializeCompass()
        {
            if (!enableCompass) return;
            
            // 나침반 활성화
            Input.compass.enabled = true;
            // Unity 6000에서는 updateInterval이 제거됨
            // Input.compass.updateInterval = compassUpdateInterval;
            
            compassData = new CompassData();
        }
        
        private void UpdateSensors()
        {
            // 카메라 데이터 업데이트
            if (enableCameraTracking && Time.time - lastCameraUpdate >= cameraUpdateInterval)
            {
                UpdateCameraData();
                lastCameraUpdate = Time.time;
            }
            
            // 위치 데이터 업데이트
            if (enableLocationTracking && Time.time - lastLocationUpdate >= locationUpdateInterval)
            {
                UpdateLocationData();
                lastLocationUpdate = Time.time;
            }
            
            // 모션 데이터 업데이트
            if (enableMotionTracking && Time.time - lastMotionUpdate >= motionUpdateInterval)
            {
                UpdateMotionData();
                lastMotionUpdate = Time.time;
            }
            
            // 자이로스코프 데이터 업데이트
            if (enableGyroscope && SystemInfo.supportsGyroscope)
            {
                UpdateGyroData();
            }
            
            // 나침반 데이터 업데이트
            if (enableCompass && Time.time - lastCompassUpdate >= compassUpdateInterval)
            {
                UpdateCompassData();
                lastCompassUpdate = Time.time;
            }
        }
        
        private void UpdateCameraData()
        {
            // 카메라 데이터 업데이트
            cameraData.timestamp = Time.time;
            cameraData.isAvailable = Application.HasUserAuthorization(UserAuthorization.WebCam);
            
            // 실제 카메라 피드가 있다면 추가 처리
            // WebCamTexture 등을 활용하여 실시간 이미지 처리 가능
            
            OnCameraDataUpdated?.Invoke(cameraData);
        }
        
        private void UpdateLocationData()
        {
            if (Input.location.status == LocationServiceStatus.Running)
            {
                locationData.timestamp = Time.time;
                locationData.latitude = Input.location.lastData.latitude;
                locationData.longitude = Input.location.lastData.longitude;
                locationData.altitude = Input.location.lastData.altitude;
                locationData.horizontalAccuracy = Input.location.lastData.horizontalAccuracy;
                locationData.verticalAccuracy = Input.location.lastData.verticalAccuracy;
                locationData.isAvailable = true;
                
                OnLocationDataUpdated?.Invoke(locationData);
            }
        }
        
        private void UpdateMotionData()
        {
            motionData.timestamp = Time.time;
            motionData.acceleration = Input.acceleration * motionSensitivity;
            motionData.accelerationMagnitude = motionData.acceleration.magnitude;
            motionData.isMoving = motionData.accelerationMagnitude > 0.1f;
            
            // 움직임 방향 계산
            motionData.movementDirection = motionData.acceleration.normalized;
            
            OnMotionDataUpdated?.Invoke(motionData);
        }
        
        private void UpdateGyroData()
        {
            if (SystemInfo.supportsGyroscope && Input.gyro.enabled)
            {
                gyroData.timestamp = Time.time;
                gyroData.rotationRate = Input.gyro.rotationRate * gyroSensitivity;
                gyroData.attitude = Input.gyro.attitude;
                gyroData.gravity = Input.gyro.gravity;
                gyroData.userAcceleration = Input.gyro.userAcceleration;
                gyroData.isAvailable = true;
                
                OnGyroDataUpdated?.Invoke(gyroData);
            }
        }
        
        private void UpdateCompassData()
        {
            if (Input.compass.enabled)
            {
                compassData.timestamp = Time.time;
                compassData.magneticHeading = Input.compass.magneticHeading;
                compassData.trueHeading = Input.compass.trueHeading;
                compassData.headingAccuracy = Input.compass.headingAccuracy;
                compassData.rawVector = Input.compass.rawVector;
                compassData.isAvailable = true;
                
                OnCompassDataUpdated?.Invoke(compassData);
            }
        }
        
        private System.Collections.IEnumerator RequestCameraPermission()
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        }
        
        private System.Collections.IEnumerator RequestLocationPermission()
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        }
        
        // 공개 메서드들
        public CameraData GetCameraData() => cameraData;
        public LocationData GetLocationData() => locationData;
        public MotionData GetMotionData() => motionData;
        public GyroscopeData GetGyroData() => gyroData;
        public CompassData GetCompassData() => compassData;
        
        public bool IsInitialized() => isInitialized;
        public bool IsCameraAvailable() => cameraData.isAvailable;
        public bool IsLocationAvailable() => locationData.isAvailable;
        public bool IsGyroAvailable() => gyroData.isAvailable;
        public bool IsCompassAvailable() => compassData.isAvailable;
        
        private void OnDestroy()
        {
            // 센서 정리
            if (Input.location.status == LocationServiceStatus.Running)
            {
                Input.location.Stop();
            }
            
            if (SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = false;
            }
            
            Input.compass.enabled = false;
        }
    }
    
    // 센서 데이터 구조체들
    [System.Serializable]
    public struct CameraData
    {
        public float timestamp;
        public bool isAvailable;
        public Vector2 resolution;
        public float fieldOfView;
    }
    
    [System.Serializable]
    public struct LocationData
    {
        public float timestamp;
        public double latitude;
        public double longitude;
        public double altitude;
        public float horizontalAccuracy;
        public float verticalAccuracy;
        public bool isAvailable;
    }
    
    [System.Serializable]
    public struct MotionData
    {
        public float timestamp;
        public Vector3 acceleration;
        public float accelerationMagnitude;
        public bool isMoving;
        public Vector3 movementDirection;
    }
    
    [System.Serializable]
    public struct GyroscopeData
    {
        public float timestamp;
        public Vector3 rotationRate;
        public Quaternion attitude;
        public Vector3 gravity;
        public Vector3 userAcceleration;
        public bool isAvailable;
    }
    
    [System.Serializable]
    public struct CompassData
    {
        public float timestamp;
        public float magneticHeading;
        public float trueHeading;
        public float headingAccuracy;
        public Vector3 rawVector;
        public bool isAvailable;
    }
    
    [System.Serializable]
    public struct SensorData
    {
        public CameraData camera;
        public LocationData location;
        public MotionData motion;
        public GyroscopeData gyro;
        public CompassData compass;
        public float timestamp;
    }
}
