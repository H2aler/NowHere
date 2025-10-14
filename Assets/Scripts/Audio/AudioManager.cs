using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;
using NowHere.Game;
using NowHere.XR;

namespace NowHere.Audio
{
    /// <summary>
    /// 통합 오디오 관리 시스템
    /// 배경음악, 효과음, 3D 공간 오디오, 음성 채팅을 통합 관리
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource uiSource;
        
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;
        [SerializeField] private AudioMixerGroup voiceMixerGroup;
        [SerializeField] private AudioMixerGroup ambientMixerGroup;
        [SerializeField] private AudioMixerGroup uiMixerGroup;
        
        [Header("Audio Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.8f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float voiceVolume = 1f;
        [SerializeField] private float ambientVolume = 0.6f;
        [SerializeField] private float uiVolume = 0.7f;
        
        [Header("3D Audio Settings")]
        [SerializeField] private bool enable3DAudio = true;
        [SerializeField] private float max3DDistance = 50f;
        [SerializeField] private AnimationCurve distanceFalloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private bool enableSpatialAudio = true;
        
        [Header("Voice Chat Settings")]
        [SerializeField] private bool enableVoiceChat = true;
        [SerializeField] private bool enablePushToTalk = true;
        [SerializeField] private KeyCode pushToTalkKey = KeyCode.V;
        [SerializeField] private float voiceDetectionThreshold = 0.01f;
        [SerializeField] private float voiceSensitivity = 1f;
        
        [Header("Audio Clips")]
        [SerializeField] private AudioClip[] backgroundMusic;
        [SerializeField] private AudioClip[] uiSounds;
        [SerializeField] private AudioClip[] combatSounds;
        [SerializeField] private AudioClip[] ambientSounds;
        [SerializeField] private AudioClip[] voiceCommands;
        
        // 오디오 상태 관리
        private bool isAudioInitialized = false;
        private bool isMusicPlaying = false;
        private bool isVoiceChatActive = false;
        private bool isPushToTalkPressed = false;
        private AudioClip currentMusic;
        private int currentMusicIndex = 0;
        
        // 3D 오디오 관리
        private Dictionary<GameObject, AudioSource> spatialAudioSources = new Dictionary<GameObject, AudioSource>();
        private List<AudioSource> active3DAudioSources = new List<AudioSource>();
        
        // 음성 채팅 관리
        private Dictionary<int, AudioSource> playerVoiceSources = new Dictionary<int, AudioSource>();
        private bool isRecording = false;
        private float[] audioBuffer = new float[1024];
        
        // 오디오 풀링
        private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
        private List<AudioSource> activeAudioSources = new List<AudioSource>();
        
        // 참조
        private GameManager gameManager;
        private XRGameManager xrGameManager;
        private Camera mainCamera;
        
        // 이벤트
        public System.Action<AudioClip> OnMusicChanged;
        public System.Action<float> OnVolumeChanged;
        public System.Action<bool> OnVoiceChatStateChanged;
        public System.Action<int, float[]> OnVoiceDataReceived;
        public System.Action<GameObject, AudioClip> On3DAudioPlayed;
        
        // 싱글톤 패턴
        public static AudioManager Instance { get; private set; }
        
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
            InitializeAudioManager();
        }
        
        private void Update()
        {
            UpdateAudio();
            HandleVoiceInput();
            Update3DAudio();
        }
        
        private void InitializeAudioManager()
        {
            Debug.Log("Audio Manager 초기화 시작...");
            
            // 컴포넌트 참조
            gameManager = FindObjectOfType<GameManager>();
            xrGameManager = FindObjectOfType<XRGameManager>();
            mainCamera = Camera.main;
            
            // 오디오 소스 초기화
            InitializeAudioSources();
            
            // 오디오 믹서 설정
            SetupAudioMixer();
            
            // 3D 오디오 초기화
            if (enable3DAudio)
            {
                Initialize3DAudio();
            }
            
            // 음성 채팅 초기화
            if (enableVoiceChat)
            {
                InitializeVoiceChat();
            }
            
            // 오디오 풀 초기화
            InitializeAudioPool();
            
            // 이벤트 구독
            SubscribeToEvents();
            
            isAudioInitialized = true;
            Debug.Log("Audio Manager 초기화 완료");
        }
        
        private void InitializeAudioSources()
        {
            // 메인 오디오 소스들 생성
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            
            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
                voiceSource.playOnAwake = false;
            }
            
            if (ambientSource == null)
            {
                ambientSource = gameObject.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }
            
            if (uiSource == null)
            {
                uiSource = gameObject.AddComponent<AudioSource>();
                uiSource.playOnAwake = false;
            }
        }
        
        private void SetupAudioMixer()
        {
            // 오디오 믹서 그룹 설정
            if (musicMixerGroup != null && musicSource != null)
                musicSource.outputAudioMixerGroup = musicMixerGroup;
            
            if (sfxMixerGroup != null && sfxSource != null)
                sfxSource.outputAudioMixerGroup = sfxMixerGroup;
            
            if (voiceMixerGroup != null && voiceSource != null)
                voiceSource.outputAudioMixerGroup = voiceMixerGroup;
            
            if (ambientMixerGroup != null && ambientSource != null)
                ambientSource.outputAudioMixerGroup = ambientMixerGroup;
            
            if (uiMixerGroup != null && uiSource != null)
                uiSource.outputAudioMixerGroup = uiMixerGroup;
        }
        
        private void Initialize3DAudio()
        {
            // 3D 오디오 초기화
            Debug.Log("3D 오디오 시스템 초기화");
        }
        
        private void InitializeVoiceChat()
        {
            // 음성 채팅 초기화
            Debug.Log("음성 채팅 시스템 초기화");
        }
        
        private void InitializeAudioPool()
        {
            // 오디오 소스 풀 생성
            for (int i = 0; i < 10; i++)
            {
                AudioSource pooledSource = gameObject.AddComponent<AudioSource>();
                pooledSource.playOnAwake = false;
                audioSourcePool.Enqueue(pooledSource);
            }
        }
        
        private void SubscribeToEvents()
        {
            // 게임 매니저 이벤트 구독
            if (gameManager != null)
            {
                // 게임 상태 변경 이벤트 구독
            }
            
            // XR 게임 매니저 이벤트 구독
            if (xrGameManager != null)
            {
                xrGameManager.OnXRModeSwitched += OnXRModeSwitched;
            }
        }
        
        private void UpdateAudio()
        {
            // 오디오 업데이트 로직
            UpdateActiveAudioSources();
        }
        
        private void UpdateActiveAudioSources()
        {
            // 활성 오디오 소스들 업데이트
            for (int i = activeAudioSources.Count - 1; i >= 0; i--)
            {
                AudioSource source = activeAudioSources[i];
                if (source != null && !source.isPlaying)
                {
                    // 재생이 끝난 소스를 풀로 반환
                    ReturnAudioSourceToPool(source);
                    activeAudioSources.RemoveAt(i);
                }
            }
        }
        
        private void HandleVoiceInput()
        {
            if (!enableVoiceChat) return;
            
            // 푸시 투 톡 처리
            if (enablePushToTalk)
            {
                bool isPressed = Input.GetKey(pushToTalkKey);
                if (isPressed != isPushToTalkPressed)
                {
                    isPushToTalkPressed = isPressed;
                    if (isPressed)
                    {
                        StartVoiceRecording();
                    }
                    else
                    {
                        StopVoiceRecording();
                    }
                }
            }
            else
            {
                // 자동 음성 감지
                HandleAutomaticVoiceDetection();
            }
        }
        
        private void HandleAutomaticVoiceDetection()
        {
            // 자동 음성 감지 로직
            // 실제 구현에서는 마이크 입력을 분석하여 음성 감지
        }
        
        private void StartVoiceRecording()
        {
            if (isRecording) return;
            
            isRecording = true;
            isVoiceChatActive = true;
            OnVoiceChatStateChanged?.Invoke(true);
            
            Debug.Log("음성 녹음 시작");
        }
        
        private void StopVoiceRecording()
        {
            if (!isRecording) return;
            
            isRecording = false;
            isVoiceChatActive = false;
            OnVoiceChatStateChanged?.Invoke(false);
            
            Debug.Log("음성 녹음 중지");
        }
        
        private void Update3DAudio()
        {
            if (!enable3DAudio || mainCamera == null) return;
            
            // 3D 오디오 업데이트
            foreach (var kvp in spatialAudioSources)
            {
                GameObject audioObject = kvp.Key;
                AudioSource audioSource = kvp.Value;
                
                if (audioObject != null && audioSource != null)
                {
                    // 거리 기반 볼륨 조절
                    float distance = Vector3.Distance(mainCamera.transform.position, audioObject.transform.position);
                    float normalizedDistance = Mathf.Clamp01(distance / max3DDistance);
                    float volume = distanceFalloff.Evaluate(normalizedDistance);
                    
                    audioSource.volume = volume * sfxVolume;
                }
            }
        }
        
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null || clip == null) return;
            
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
            
            currentMusic = clip;
            isMusicPlaying = true;
            OnMusicChanged?.Invoke(clip);
            
            Debug.Log($"배경음악 재생: {clip.name}");
        }
        
        public void PlayMusic(int index)
        {
            if (backgroundMusic == null || index < 0 || index >= backgroundMusic.Length) return;
            
            PlayMusic(backgroundMusic[index]);
            currentMusicIndex = index;
        }
        
        public void PlayNextMusic()
        {
            if (backgroundMusic == null || backgroundMusic.Length == 0) return;
            
            currentMusicIndex = (currentMusicIndex + 1) % backgroundMusic.Length;
            PlayMusic(currentMusicIndex);
        }
        
        public void PlayPreviousMusic()
        {
            if (backgroundMusic == null || backgroundMusic.Length == 0) return;
            
            currentMusicIndex = (currentMusicIndex - 1 + backgroundMusic.Length) % backgroundMusic.Length;
            PlayMusic(currentMusicIndex);
        }
        
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
                isMusicPlaying = false;
                currentMusic = null;
            }
        }
        
        public void PauseMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Pause();
            }
        }
        
        public void ResumeMusic()
        {
            if (musicSource != null && !musicSource.isPlaying)
            {
                musicSource.UnPause();
            }
        }
        
        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (sfxSource == null || clip == null) return;
            
            sfxSource.PlayOneShot(clip, volume * sfxVolume * masterVolume);
            
            Debug.Log($"효과음 재생: {clip.name}");
        }
        
        public void Play3DSFX(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;
            
            AudioSource source = GetPooledAudioSource();
            if (source != null)
            {
                source.clip = clip;
                source.transform.position = position;
                source.volume = volume * sfxVolume * masterVolume;
                source.spatialBlend = 1f; // 3D 오디오
                source.maxDistance = max3DDistance;
                source.Play();
                
                activeAudioSources.Add(source);
                On3DAudioPlayed?.Invoke(null, clip);
                
                Debug.Log($"3D 효과음 재생: {clip.name} at {position}");
            }
        }
        
        public void PlayUISound(AudioClip clip)
        {
            if (uiSource == null || clip == null) return;
            
            uiSource.PlayOneShot(clip, uiVolume * masterVolume);
        }
        
        public void PlayAmbientSound(AudioClip clip, bool loop = true)
        {
            if (ambientSource == null || clip == null) return;
            
            ambientSource.clip = clip;
            ambientSource.loop = loop;
            ambientSource.volume = ambientVolume * masterVolume;
            ambientSource.Play();
            
            Debug.Log($"환경음 재생: {clip.name}");
        }
        
        public void PlayVoiceCommand(AudioClip clip)
        {
            if (voiceSource == null || clip == null) return;
            
            voiceSource.PlayOneShot(clip, voiceVolume * masterVolume);
        }
        
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
            OnVolumeChanged?.Invoke(masterVolume);
        }
        
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume * masterVolume;
            }
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume * masterVolume;
            }
        }
        
        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
            if (voiceSource != null)
            {
                voiceSource.volume = voiceVolume * masterVolume;
            }
        }
        
        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            if (ambientSource != null)
            {
                ambientSource.volume = ambientVolume * masterVolume;
            }
        }
        
        public void SetUIVolume(float volume)
        {
            uiVolume = Mathf.Clamp01(volume);
            if (uiSource != null)
            {
                uiSource.volume = uiVolume * masterVolume;
            }
        }
        
        private void UpdateAllVolumes()
        {
            SetMusicVolume(musicVolume);
            SetSFXVolume(sfxVolume);
            SetVoiceVolume(voiceVolume);
            SetAmbientVolume(ambientVolume);
            SetUIVolume(uiVolume);
        }
        
        public void Register3DAudioSource(GameObject audioObject, AudioSource audioSource)
        {
            if (audioObject != null && audioSource != null)
            {
                spatialAudioSources[audioObject] = audioSource;
                active3DAudioSources.Add(audioSource);
            }
        }
        
        public void Unregister3DAudioSource(GameObject audioObject)
        {
            if (spatialAudioSources.ContainsKey(audioObject))
            {
                AudioSource audioSource = spatialAudioSources[audioObject];
                active3DAudioSources.Remove(audioSource);
                spatialAudioSources.Remove(audioObject);
            }
        }
        
        public void AddPlayerVoiceSource(int playerId, AudioSource voiceSource)
        {
            playerVoiceSources[playerId] = voiceSource;
        }
        
        public void RemovePlayerVoiceSource(int playerId)
        {
            if (playerVoiceSources.ContainsKey(playerId))
            {
                playerVoiceSources.Remove(playerId);
            }
        }
        
        private AudioSource GetPooledAudioSource()
        {
            if (audioSourcePool.Count > 0)
            {
                return audioSourcePool.Dequeue();
            }
            else
            {
                // 풀이 비어있으면 새로 생성
                return gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void ReturnAudioSourceToPool(AudioSource source)
        {
            if (source != null)
            {
                source.Stop();
                source.clip = null;
                audioSourcePool.Enqueue(source);
            }
        }
        
        // 이벤트 핸들러들
        private void OnXRModeSwitched(XRMode mode)
        {
            // XR 모드 변경 시 오디오 설정 조정
            switch (mode)
            {
                case XRMode.VR:
                    // VR 모드 오디오 설정
                    enableSpatialAudio = true;
                    break;
                case XRMode.AR:
                    // AR 모드 오디오 설정
                    enableSpatialAudio = true;
                    break;
                case XRMode.MR:
                    // MR 모드 오디오 설정
                    enableSpatialAudio = true;
                    break;
            }
        }
        
        // 공개 메서드들
        public bool IsAudioInitialized()
        {
            return isAudioInitialized;
        }
        
        public bool IsMusicPlaying()
        {
            return isMusicPlaying;
        }
        
        public bool IsVoiceChatActive()
        {
            return isVoiceChatActive;
        }
        
        public AudioClip GetCurrentMusic()
        {
            return currentMusic;
        }
        
        public float GetMasterVolume()
        {
            return masterVolume;
        }
        
        public float GetMusicVolume()
        {
            return musicVolume;
        }
        
        public float GetSFXVolume()
        {
            return sfxVolume;
        }
        
        public float GetVoiceVolume()
        {
            return voiceVolume;
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (xrGameManager != null)
            {
                xrGameManager.OnXRModeSwitched -= OnXRModeSwitched;
            }
        }
    }
}
