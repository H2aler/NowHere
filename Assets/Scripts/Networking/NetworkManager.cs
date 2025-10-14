using UnityEngine;
using System.Collections.Generic;
using NowHere.Game;

namespace NowHere.Networking
{
    /// <summary>
    /// 네트워킹 시스템을 관리하는 클래스
    /// 멀티플레이어 기능과 서버/클라이언트 통신을 담당
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        [Header("Network Settings")]
        [SerializeField] private string serverIP = "127.0.0.1";
        [SerializeField] private int serverPort = 7777;
        [SerializeField] private bool autoConnect = false;
        [SerializeField] private float connectionTimeout = 10f;
        
        [Header("Player Management")]
        [SerializeField] private int maxPlayers = 8;
        [SerializeField] private GameObject playerPrefab;
        
        // 네트워크 상태
        private bool isConnected = false;
        private bool isHost = false;
        private bool isServer = false;
        private float connectionStartTime;
        
        // 플레이어 관리
        private Dictionary<int, GameObject> connectedPlayers = new Dictionary<int, GameObject>();
        private int localPlayerId = -1;
        
        // 이벤트
        public System.Action<int> OnPlayerConnected;
        public System.Action<int> OnPlayerDisconnected;
        public System.Action<string> OnConnectionError;
        public System.Action OnServerStarted;
        public System.Action OnClientConnected;
        public System.Action OnClientDisconnected;
        
        // 싱글톤 패턴
        public static NetworkManager Instance { get; private set; }
        
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
            InitializeNetworkManager();
        }
        
        private void Update()
        {
            UpdateNetworkStatus();
        }
        
        private void InitializeNetworkManager()
        {
            Debug.Log("네트워크 매니저 초기화");
            
            if (autoConnect)
            {
                StartClient();
            }
        }
        
        private void UpdateNetworkStatus()
        {
            // 연결 타임아웃 체크
            if (!isConnected && Time.time - connectionStartTime > connectionTimeout)
            {
                OnConnectionError?.Invoke("연결 타임아웃");
            }
        }
        
        public void StartHost()
        {
            Debug.Log("호스트 시작 시도");
            
            // Unity 6000에서는 Netcode for GameObjects 사용
            // 실제 구현은 Unity Editor에서 설정 필요
            
            isHost = true;
            isServer = true;
            isConnected = true;
            
            OnServerStarted?.Invoke();
            Debug.Log("호스트로 시작되었습니다.");
        }
        
        public void StartClient()
        {
            Debug.Log($"클라이언트 시작 시도: {serverIP}:{serverPort}");
            
            connectionStartTime = Time.time;
            
            // Unity 6000에서는 Netcode for GameObjects 사용
            // 실제 구현은 Unity Editor에서 설정 필요
            
            isConnected = true;
            localPlayerId = Random.Range(1, 1000);
            
            OnClientConnected?.Invoke();
            Debug.Log("클라이언트로 연결되었습니다.");
        }
        
        public void StartServer()
        {
            Debug.Log("서버 시작 시도");
            
            // Unity 6000에서는 Netcode for GameObjects 사용
            // 실제 구현은 Unity Editor에서 설정 필요
            
            isServer = true;
            isConnected = true;
            
            OnServerStarted?.Invoke();
            Debug.Log("서버가 시작되었습니다.");
        }
        
        public void Disconnect()
        {
            Debug.Log("네트워크 연결 해제");
            
            isConnected = false;
            isHost = false;
            isServer = false;
            
            // 연결된 플레이어들 정리
            connectedPlayers.Clear();
            localPlayerId = -1;
            
            OnClientDisconnected?.Invoke();
        }
        
        public void SendMessage(string message)
        {
            if (!isConnected) return;
            
            Debug.Log($"메시지 전송: {message}");
            // 실제 메시지 전송 로직 구현
        }
        
        public void SendPlayerData(int playerId, object data)
        {
            if (!isConnected) return;
            
            Debug.Log($"플레이어 데이터 전송: Player {playerId}");
            // 실제 데이터 전송 로직 구현
        }
        
        private void OnPlayerConnectedInternal(int playerId)
        {
            Debug.Log($"플레이어 연결됨: {playerId}");
            
            // 플레이어 오브젝트 생성
            if (playerPrefab != null)
            {
                GameObject playerObject = Instantiate(playerPrefab);
                connectedPlayers[playerId] = playerObject;
            }
            
            OnPlayerConnected?.Invoke(playerId);
        }
        
        private void OnPlayerDisconnectedInternal(int playerId)
        {
            Debug.Log($"플레이어 연결 해제됨: {playerId}");
            
            // 플레이어 오브젝트 제거
            if (connectedPlayers.ContainsKey(playerId))
            {
                Destroy(connectedPlayers[playerId]);
                connectedPlayers.Remove(playerId);
            }
            
            OnPlayerDisconnected?.Invoke(playerId);
        }
        
        // 공개 메서드들
        public bool IsConnected() => isConnected;
        public bool IsHost() => isHost;
        public bool IsServer() => isServer;
        public int GetLocalPlayerId() => localPlayerId;
        public int GetConnectedPlayerCount() => connectedPlayers.Count;
        
        public void SetServerIP(string ip)
        {
            serverIP = ip;
        }
        
        public void SetServerPort(int port)
        {
            serverPort = port;
        }
        
        private void OnDestroy()
        {
            Disconnect();
        }
    }
}