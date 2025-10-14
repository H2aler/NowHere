using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using NowHere.Networking;

namespace NowHere.Audio
{
    /// <summary>
    /// 실시간 음성 채팅을 관리하는 클래스
    /// 마이크 입력, 음성 전송, 재생을 담당
    /// </summary>
    public class VoiceChatManager : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private int sampleRate = 44100;
        [SerializeField] private int bufferSize = 1024;
        [SerializeField] private int channels = 1;
        [SerializeField] private float voiceThreshold = 0.01f;
        [SerializeField] private float voiceGain = 1f;
        
        [Header("Network Settings")]
        [SerializeField] private float sendInterval = 0.1f;
        [SerializeField] private int maxAudioDataSize = 4096;
        [SerializeField] private bool enableCompression = true;
        
        [Header("Voice Chat Settings")]
        [SerializeField] private bool enableVoiceChat = true;
        [SerializeField] private bool pushToTalk = false;
        [SerializeField] private KeyCode pushToTalkKey = KeyCode.V;
        [SerializeField] private float voiceRange = 10f;
        [SerializeField] private bool enableSpatialAudio = true;
        
        [Header("Audio Effects")]
        [SerializeField] private bool enableEchoCancellation = true;
        [SerializeField] private bool enableNoiseReduction = true;
        [SerializeField] private float echoDelay = 0.1f;
        [SerializeField] private float noiseThreshold = 0.005f;
        
        // 오디오 컴포넌트
        private AudioSource audioSource;
        private AudioClip microphoneClip;
        private string selectedMicrophone;
        
        // 음성 데이터
        private List<float> audioBuffer = new List<float>();
        private List<VoiceData> receivedVoiceData = new List<VoiceData>();
        private Dictionary<ulong, AudioSource> playerAudioSources = new Dictionary<ulong, AudioSource>();
        
        // 상태
        private bool isRecording = false;
        private bool isVoiceChatEnabled = false;
        private bool isMicrophoneAvailable = false;
        private float lastSendTime;
        
        // 참조
        private NetworkManager networkManager;
        
        // 이벤트
        public System.Action<bool> OnVoiceChatToggled;
        public System.Action<bool> OnRecordingStateChanged;
        public System.Action<ulong, float[]> OnVoiceDataReceived;
        public System.Action<string> OnMicrophoneError;
        
        private void Start()
        {
            InitializeVoiceChat();
        }
        
        private void Update()
        {
            if (!isVoiceChatEnabled) return;
            
            HandleVoiceInput();
            ProcessReceivedVoiceData();
        }
        
        private void InitializeVoiceChat()
        {
            // 네트워크 매니저 참조
            networkManager = FindObjectOfType<NetworkManager>();
            
            // 오디오 소스 설정
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // 마이크 권한 요청
            StartCoroutine(RequestMicrophonePermission());
            
            // 마이크 초기화
            InitializeMicrophone();
            
            Debug.Log("음성 채팅 시스템이 초기화되었습니다.");
        }
        
        private System.Collections.IEnumerator RequestMicrophonePermission()
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            
            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                isMicrophoneAvailable = true;
                Debug.Log("마이크 권한이 허용되었습니다.");
            }
            else
            {
                isMicrophoneAvailable = false;
                OnMicrophoneError?.Invoke("마이크 권한이 거부되었습니다.");
                Debug.LogWarning("마이크 권한이 거부되었습니다.");
            }
        }
        
        private void InitializeMicrophone()
        {
            if (!isMicrophoneAvailable) return;
            
            // 사용 가능한 마이크 목록 확인
            string[] microphones = Microphone.devices;
            if (microphones.Length > 0)
            {
                selectedMicrophone = microphones[0];
                Debug.Log($"선택된 마이크: {selectedMicrophone}");
            }
            else
            {
                OnMicrophoneError?.Invoke("사용 가능한 마이크가 없습니다.");
                return;
            }
            
            // 마이크 클립 생성
            microphoneClip = Microphone.Start(selectedMicrophone, true, 1, sampleRate);
            
            // 오디오 소스 설정
            audioSource.clip = microphoneClip;
            audioSource.loop = true;
            audioSource.mute = true; // 마이크 입력은 재생하지 않음
        }
        
        private void HandleVoiceInput()
        {
            if (!isMicrophoneAvailable || microphoneClip == null) return;
            
            // 푸시 투 톡 확인
            bool shouldRecord = pushToTalk ? Input.GetKey(pushToTalkKey) : true;
            
            if (shouldRecord && !isRecording)
            {
                StartRecording();
            }
            else if (!shouldRecord && isRecording)
            {
                StopRecording();
            }
            
            // 음성 데이터 수집
            if (isRecording)
            {
                CollectAudioData();
            }
        }
        
        private void StartRecording()
        {
            isRecording = true;
            OnRecordingStateChanged?.Invoke(true);
            Debug.Log("음성 녹음 시작");
        }
        
        private void StopRecording()
        {
            isRecording = false;
            OnRecordingStateChanged?.Invoke(false);
            Debug.Log("음성 녹음 중지");
        }
        
        private void CollectAudioData()
        {
            if (microphoneClip == null) return;
            
            // 마이크에서 오디오 데이터 수집
            int position = Microphone.GetPosition(selectedMicrophone);
            float[] samples = new float[microphoneClip.samples * microphoneClip.channels];
            microphoneClip.GetData(samples, 0);
            
            // 최근 데이터만 처리
            int startIndex = Mathf.Max(0, position - bufferSize);
            int endIndex = position;
            
            if (startIndex < endIndex)
            {
                float[] recentSamples = new float[endIndex - startIndex];
                System.Array.Copy(samples, startIndex, recentSamples, 0, recentSamples.Length);
                
                // 음성 감지
                if (IsVoiceDetected(recentSamples))
                {
                    // 오디오 처리
                    float[] processedSamples = ProcessAudioData(recentSamples);
                    
                    // 네트워크로 전송
                    SendVoiceData(processedSamples);
                }
            }
        }
        
        private bool IsVoiceDetected(float[] samples)
        {
            // 음성 감지 알고리즘
            float rms = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                rms += samples[i] * samples[i];
            }
            rms = Mathf.Sqrt(rms / samples.Length);
            
            return rms > voiceThreshold;
        }
        
        private float[] ProcessAudioData(float[] samples)
        {
            float[] processedSamples = new float[samples.Length];
            System.Array.Copy(samples, processedSamples, samples.Length);
            
            // 게인 적용
            for (int i = 0; i < processedSamples.Length; i++)
            {
                processedSamples[i] *= voiceGain;
                processedSamples[i] = Mathf.Clamp(processedSamples[i], -1f, 1f);
            }
            
            // 노이즈 감소
            if (enableNoiseReduction)
            {
                processedSamples = ApplyNoiseReduction(processedSamples);
            }
            
            // 에코 제거
            if (enableEchoCancellation)
            {
                processedSamples = ApplyEchoCancellation(processedSamples);
            }
            
            return processedSamples;
        }
        
        private float[] ApplyNoiseReduction(float[] samples)
        {
            // 간단한 노이즈 감소 알고리즘
            for (int i = 0; i < samples.Length; i++)
            {
                if (Mathf.Abs(samples[i]) < noiseThreshold)
                {
                    samples[i] = 0f;
                }
            }
            return samples;
        }
        
        private float[] ApplyEchoCancellation(float[] samples)
        {
            // 간단한 에코 제거 알고리즘
            // 실제로는 더 복잡한 알고리즘이 필요
            return samples;
        }
        
        private void SendVoiceData(float[] audioData)
        {
            if (Time.time - lastSendTime < sendInterval) return;
            
            // 오디오 데이터 압축
            byte[] compressedData = CompressAudioData(audioData);
            
            // 네트워크로 전송
            if (networkManager != null && networkManager.IsConnected())
            {
                // 실제로는 네트워크 매니저를 통해 전송
                // networkManager.SendVoiceData(compressedData);
            }
            
            lastSendTime = Time.time;
        }
        
        private byte[] CompressAudioData(float[] audioData)
        {
            // 오디오 데이터를 바이트 배열로 변환
            byte[] data = new byte[audioData.Length * 4];
            System.Buffer.BlockCopy(audioData, 0, data, 0, data.Length);
            
            // 압축 (실제로는 더 효율적인 압축 알고리즘 사용)
            if (enableCompression)
            {
                // 간단한 압축 예시
                return data;
            }
            
            return data;
        }
        
        private void ProcessReceivedVoiceData()
        {
            // 수신된 음성 데이터 처리
            for (int i = receivedVoiceData.Count - 1; i >= 0; i--)
            {
                VoiceData voiceData = receivedVoiceData[i];
                PlayVoiceData(voiceData);
                receivedVoiceData.RemoveAt(i);
            }
        }
        
        private void PlayVoiceData(VoiceData voiceData)
        {
            // 플레이어별 오디오 소스 관리
            if (!playerAudioSources.ContainsKey(voiceData.playerId))
            {
                CreatePlayerAudioSource(voiceData.playerId);
            }
            
            AudioSource playerAudioSource = playerAudioSources[voiceData.playerId];
            
            // 오디오 클립 생성
            AudioClip voiceClip = AudioClip.Create(
                $"Voice_{voiceData.playerId}",
                voiceData.audioData.Length,
                1,
                sampleRate,
                false
            );
            
            voiceClip.SetData(voiceData.audioData, 0);
            
            // 3D 공간 오디오 설정
            if (enableSpatialAudio)
            {
                SetSpatialAudio(playerAudioSource, voiceData.playerId);
            }
            
            // 오디오 재생
            playerAudioSource.clip = voiceClip;
            playerAudioSource.Play();
            
            OnVoiceDataReceived?.Invoke(voiceData.playerId, voiceData.audioData);
        }
        
        private void CreatePlayerAudioSource(ulong playerId)
        {
            GameObject audioObject = new GameObject($"VoiceAudio_{playerId}");
            audioObject.transform.SetParent(transform);
            
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = enableSpatialAudio ? 1f : 0f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = voiceRange;
            audioSource.volume = 1f;
            
            playerAudioSources[playerId] = audioSource;
        }
        
        private void SetSpatialAudio(AudioSource audioSource, ulong playerId)
        {
            // 플레이어 위치에 따른 3D 오디오 설정
            // 실제로는 네트워크 매니저에서 플레이어 위치를 가져와야 함
            Vector3 playerPosition = Vector3.zero; // 임시
            
            audioSource.transform.position = playerPosition;
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = voiceRange;
        }
        
        // 공개 메서드들
        public void ToggleVoiceChat()
        {
            isVoiceChatEnabled = !isVoiceChatEnabled;
            OnVoiceChatToggled?.Invoke(isVoiceChatEnabled);
            
            if (!isVoiceChatEnabled)
            {
                StopRecording();
            }
            
            Debug.Log($"음성 채팅: {(isVoiceChatEnabled ? "활성화" : "비활성화")}");
        }
        
        public void SetPushToTalk(bool enabled)
        {
            pushToTalk = enabled;
        }
        
        public void SetVoiceRange(float range)
        {
            voiceRange = range;
        }
        
        public void SetVoiceGain(float gain)
        {
            voiceGain = gain;
        }
        
        public void SetVoiceThreshold(float threshold)
        {
            voiceThreshold = threshold;
        }
        
        public bool IsVoiceChatEnabled()
        {
            return isVoiceChatEnabled;
        }
        
        public bool IsRecording()
        {
            return isRecording;
        }
        
        public bool IsMicrophoneAvailable()
        {
            return isMicrophoneAvailable;
        }
        
        public void ReceiveVoiceData(ulong playerId, byte[] compressedData)
        {
            // 압축 해제
            float[] audioData = DecompressAudioData(compressedData);
            
            // 음성 데이터 추가
            VoiceData voiceData = new VoiceData
            {
                playerId = playerId,
                audioData = audioData,
                timestamp = Time.time
            };
            
            receivedVoiceData.Add(voiceData);
        }
        
        private float[] DecompressAudioData(byte[] compressedData)
        {
            // 압축 해제
            float[] audioData = new float[compressedData.Length / 4];
            System.Buffer.BlockCopy(compressedData, 0, audioData, 0, compressedData.Length);
            return audioData;
        }
        
        private void OnDestroy()
        {
            // 마이크 정리
            if (Microphone.IsRecording(selectedMicrophone))
            {
                Microphone.End(selectedMicrophone);
            }
        }
    }
    
    [System.Serializable]
    public struct VoiceData
    {
        public ulong playerId;
        public float[] audioData;
        public float timestamp;
    }
}
