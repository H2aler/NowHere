using UnityEngine;
using System.Collections.Generic;
using NowHere.Sensors;

namespace NowHere.Motion
{
    /// <summary>
    /// 모션 감지 및 회피 시스템을 관리하는 클래스
    /// 모바일 기기의 움직임을 감지하여 게임 내 회피 동작을 처리
    /// </summary>
    public class MotionDetectionManager : MonoBehaviour
    {
        [Header("Motion Detection Settings")]
        [SerializeField] private float motionSensitivity = 1f;
        [SerializeField] private float motionThreshold = 0.5f;
        [SerializeField] private float motionCooldown = 0.5f;
        [SerializeField] private bool enableMotionPrediction = true;
        
        [Header("Dodge Settings")]
        [SerializeField] private float dodgeSpeed = 5f;
        [SerializeField] private float dodgeDuration = 0.3f;
        [SerializeField] private float dodgeCooldown = 1f;
        [SerializeField] private float dodgeDistance = 2f;
        
        [Header("Projectile Avoidance")]
        [SerializeField] private float avoidanceRange = 3f;
        [SerializeField] private float avoidanceSpeed = 3f;
        [SerializeField] private float predictionTime = 0.5f;
        [SerializeField] private LayerMask projectileLayer = 1;
        
        [Header("Motion Types")]
        [SerializeField] private bool enableShakeDetection = true;
        [SerializeField] private bool enableTiltDetection = true;
        [SerializeField] private bool enableSwipeDetection = true;
        [SerializeField] private bool enableRotationDetection = true;
        
        // 모션 상태
        private bool isDodging = false;
        private bool canDodge = true;
        private float lastDodgeTime;
        private Vector3 dodgeDirection;
        
        // 모션 데이터
        private MotionData currentMotion;
        private MotionData previousMotion;
        private List<MotionData> motionHistory = new List<MotionData>();
        private Vector3 deviceRotation;
        private Vector3 deviceAcceleration;
        
        // 회피 관련
        private List<ProjectileData> incomingProjectiles = new List<ProjectileData>();
        private Vector3 avoidanceDirection;
        private bool isAvoiding = false;
        
        // 참조
        private MobileSensorManager sensorManager;
        private Transform playerTransform;
        private Rigidbody playerRigidbody;
        
        // 이벤트
        public System.Action<MotionType, Vector3> OnMotionDetected;
        public System.Action<Vector3> OnDodgePerformed;
        public System.Action<Vector3> OnAvoidancePerformed;
        public System.Action<ProjectileData> OnProjectileDetected;
        public System.Action<float> OnMotionIntensityChanged;
        
        private void Start()
        {
            InitializeMotionDetection();
        }
        
        public void Update()
        {
            UpdateMotionData();
            ProcessMotionInput();
            ProcessProjectileAvoidance();
        }
        
        private void InitializeMotionDetection()
        {
            // 센서 매니저 참조
            sensorManager = FindObjectOfType<MobileSensorManager>();
            if (sensorManager != null)
            {
                sensorManager.OnMotionDataUpdated += OnMotionDataUpdated;
                sensorManager.OnGyroDataUpdated += OnGyroDataUpdated;
            }
            
            // 플레이어 참조
            playerTransform = transform;
            playerRigidbody = GetComponent<Rigidbody>();
            
            // 초기 모션 데이터 설정
            currentMotion = new MotionData();
            previousMotion = new MotionData();
            
            Debug.Log("모션 감지 시스템이 초기화되었습니다.");
        }
        
        private void UpdateMotionData()
        {
            if (sensorManager == null) return;
            
            // 센서 데이터 업데이트
            var motionData = sensorManager.GetMotionData();
            var gyroData = sensorManager.GetGyroData();
            
            // 센서 데이터 사용 (isAvailable 필드가 없으므로 직접 사용)
            deviceAcceleration = motionData.acceleration;
            deviceRotation = gyroData.rotationRate;
        }
        
        private void ProcessMotionInput()
        {
            // 흔들기 감지
            if (enableShakeDetection)
            {
                DetectShake();
            }
            
            // 기울기 감지
            if (enableTiltDetection)
            {
                DetectTilt();
            }
            
            // 스와이프 감지
            if (enableSwipeDetection)
            {
                DetectSwipe();
            }
            
            // 회전 감지
            if (enableRotationDetection)
            {
                DetectRotation();
            }
        }
        
        private void DetectShake()
        {
            float accelerationMagnitude = deviceAcceleration.magnitude;
            
            if (accelerationMagnitude > motionThreshold * motionSensitivity)
            {
                // 흔들기 감지
                Vector3 shakeDirection = deviceAcceleration.normalized;
                OnMotionDetected?.Invoke(MotionType.Shake, shakeDirection);
                
                // 회피 동작 실행
                PerformDodge(shakeDirection);
            }
        }
        
        private void DetectTilt()
        {
            // 기울기 감지 (자이로스코프 데이터 사용)
            Vector3 tiltDirection = new Vector3(deviceRotation.x, 0, deviceRotation.z);
            
            if (tiltDirection.magnitude > motionThreshold * motionSensitivity)
            {
                OnMotionDetected?.Invoke(MotionType.Tilt, tiltDirection);
                
                // 기울기 방향으로 회피
                PerformDodge(tiltDirection.normalized);
            }
        }
        
        private void DetectSwipe()
        {
            // 터치 스와이프 감지 (터치 매니저와 연동)
            // 실제로는 터치 매니저에서 스와이프 데이터를 받아옴
        }
        
        private void DetectRotation()
        {
            // 회전 감지
            float rotationMagnitude = deviceRotation.magnitude;
            
            if (rotationMagnitude > motionThreshold * motionSensitivity)
            {
                Vector3 rotationDirection = deviceRotation.normalized;
                OnMotionDetected?.Invoke(MotionType.Rotation, rotationDirection);
                
                // 회전 방향으로 회피
                PerformDodge(rotationDirection);
            }
        }
        
        private void PerformDodge(Vector3 direction)
        {
            if (!canDodge || isDodging) return;
            
            // 회피 방향 계산
            dodgeDirection = direction.normalized;
            dodgeDirection.y = 0; // 수직 회피 방지
            
            // 회피 시작
            StartCoroutine(PerformDodgeCoroutine());
            
            OnDodgePerformed?.Invoke(dodgeDirection);
            Debug.Log($"회피 실행: {dodgeDirection}");
        }
        
        private System.Collections.IEnumerator PerformDodgeCoroutine()
        {
            isDodging = true;
            canDodge = false;
            lastDodgeTime = Time.time;
            
            // 회피 애니메이션 및 이동
            Vector3 startPosition = playerTransform.position;
            Vector3 targetPosition = startPosition + dodgeDirection * dodgeDistance;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < dodgeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / dodgeDuration;
                
                // 부드러운 회피 이동
                Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, progress);
                playerTransform.position = currentPosition;
                
                yield return null;
            }
            
            isDodging = false;
            
            // 회피 쿨다운
            yield return new WaitForSeconds(dodgeCooldown);
            canDodge = true;
        }
        
        private void ProcessProjectileAvoidance()
        {
            if (!enableMotionPrediction) return;
            
            // 주변 투사체 감지
            DetectIncomingProjectiles();
            
            // 회피 계산
            if (incomingProjectiles.Count > 0)
            {
                CalculateAvoidanceDirection();
                
                if (avoidanceDirection != Vector3.zero)
                {
                    PerformAvoidance(avoidanceDirection);
                }
            }
        }
        
        private void DetectIncomingProjectiles()
        {
            incomingProjectiles.Clear();
            
            // 주변 투사체 감지
            Collider[] projectiles = Physics.OverlapSphere(playerTransform.position, avoidanceRange, projectileLayer);
            
            foreach (var projectile in projectiles)
            {
                ProjectileData projectileData = new ProjectileData
                {
                    position = projectile.transform.position,
                    velocity = projectile.GetComponent<Rigidbody>()?.linearVelocity ?? Vector3.zero,
                    direction = projectile.transform.forward,
                    speed = projectile.GetComponent<Rigidbody>()?.linearVelocity.magnitude ?? 0f,
                    timeToImpact = CalculateTimeToImpact(projectile.transform.position, projectile.GetComponent<Rigidbody>()?.linearVelocity ?? Vector3.zero)
                };
                
                incomingProjectiles.Add(projectileData);
                OnProjectileDetected?.Invoke(projectileData);
            }
        }
        
        private float CalculateTimeToImpact(Vector3 projectilePosition, Vector3 projectileVelocity)
        {
            Vector3 directionToPlayer = (playerTransform.position - projectilePosition).normalized;
            float distanceToPlayer = Vector3.Distance(projectilePosition, playerTransform.position);
            
            // 투사체가 플레이어를 향해 움직이는지 확인
            float dotProduct = Vector3.Dot(projectileVelocity.normalized, directionToPlayer);
            
            if (dotProduct > 0)
            {
                return distanceToPlayer / projectileVelocity.magnitude;
            }
            
            return float.MaxValue; // 플레이어를 향하지 않음
        }
        
        private void CalculateAvoidanceDirection()
        {
            Vector3 totalAvoidanceDirection = Vector3.zero;
            int validProjectiles = 0;
            
            foreach (var projectile in incomingProjectiles)
            {
                if (projectile.timeToImpact < predictionTime)
                {
                    // 위험한 투사체
                    Vector3 directionFromProjectile = (playerTransform.position - projectile.position).normalized;
                    totalAvoidanceDirection += directionFromProjectile;
                    validProjectiles++;
                }
            }
            
            if (validProjectiles > 0)
            {
                avoidanceDirection = (totalAvoidanceDirection / validProjectiles).normalized;
            }
            else
            {
                avoidanceDirection = Vector3.zero;
            }
        }
        
        private void PerformAvoidance(Vector3 direction)
        {
            if (isAvoiding) return;
            
            StartCoroutine(PerformAvoidanceCoroutine(direction));
        }
        
        private System.Collections.IEnumerator PerformAvoidanceCoroutine(Vector3 direction)
        {
            isAvoiding = true;
            
            Vector3 startPosition = playerTransform.position;
            Vector3 targetPosition = startPosition + direction * avoidanceSpeed;
            
            float elapsedTime = 0f;
            float avoidanceDuration = 0.2f;
            
            while (elapsedTime < avoidanceDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / avoidanceDuration;
                
                Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, progress);
                playerTransform.position = currentPosition;
                
                yield return null;
            }
            
            isAvoiding = false;
            OnAvoidancePerformed?.Invoke(direction);
        }
        
        // 센서 이벤트 핸들러
        private void OnMotionDataUpdated(NowHere.Sensors.MotionData sensorMotionData)
        {
            previousMotion = currentMotion;
            
            // 센서 데이터를 로컬 MotionData로 변환
            currentMotion = new MotionData
            {
                isAvailable = true, // 센서 데이터가 전달되었다면 사용 가능
                acceleration = sensorMotionData.acceleration,
                rotationRate = Vector3.zero, // 기본값으로 설정
                accelerationMagnitude = sensorMotionData.acceleration.magnitude,
                timestamp = Time.time
            };
            
            // 모션 히스토리 업데이트
            motionHistory.Add(currentMotion);
            if (motionHistory.Count > 10)
            {
                motionHistory.RemoveAt(0);
            }
            
            // 모션 강도 변화 감지
            float intensityChange = Mathf.Abs(currentMotion.accelerationMagnitude - previousMotion.accelerationMagnitude);
            OnMotionIntensityChanged?.Invoke(intensityChange);
        }
        
        private void OnGyroDataUpdated(GyroscopeData gyroData)
        {
            deviceRotation = gyroData.rotationRate;
        }
        
        // 공개 메서드들
        public void SetMotionSensitivity(float sensitivity)
        {
            motionSensitivity = sensitivity;
        }
        
        public void SetDodgeSpeed(float speed)
        {
            dodgeSpeed = speed;
        }
        
        public void SetAvoidanceRange(float range)
        {
            avoidanceRange = range;
        }
        
        public bool IsDodging()
        {
            return isDodging;
        }
        
        public bool CanDodge()
        {
            return canDodge;
        }
        
        public Vector3 GetDodgeDirection()
        {
            return dodgeDirection;
        }
        
        public List<ProjectileData> GetIncomingProjectiles()
        {
            return new List<ProjectileData>(incomingProjectiles);
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (sensorManager != null)
            {
                sensorManager.OnMotionDataUpdated -= OnMotionDataUpdated;
                sensorManager.OnGyroDataUpdated -= OnGyroDataUpdated;
            }
        }
    }
    
    [System.Serializable]
    public struct MotionData
    {
        public bool isAvailable;
        public Vector3 acceleration;
        public Vector3 rotationRate;
        public float accelerationMagnitude;
        public float timestamp;
    }
    
    [System.Serializable]
    public struct ProjectileData
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 direction;
        public float speed;
        public float timeToImpact;
    }
    
    public enum MotionType
    {
        Shake,
        Tilt,
        Swipe,
        Rotation,
        Tap,
        LongPress
    }
}
