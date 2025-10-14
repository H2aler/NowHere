using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using NowHere.XR;

namespace NowHere.XR
{
    /// <summary>
    /// VR 최적화 UI 시스템을 관리하는 클래스
    /// VR 환경에 최적화된 UI 요소들을 생성하고 관리
    /// </summary>
    public class VRUIManager : MonoBehaviour
    {
        [Header("VR UI Settings")]
        [SerializeField] private float uiDistance = 2f;
        [SerializeField] private float uiScale = 1f;
        [SerializeField] private bool followHead = true;
        [SerializeField] private bool enableUIHover = true;
        [SerializeField] private float hoverDistance = 0.5f;
        
        [Header("VR UI Prefabs")]
        [SerializeField] private GameObject vrMenuPrefab;
        [SerializeField] private GameObject vrButtonPrefab;
        [SerializeField] private GameObject vrSliderPrefab;
        [SerializeField] private GameObject vrTogglePrefab;
        [SerializeField] private GameObject vrTextPrefab;
        [SerializeField] private GameObject vrPanelPrefab;
        
        [Header("VR UI Canvas")]
        [SerializeField] private Canvas vrCanvas;
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private GraphicRaycaster graphicRaycaster;
        
        [Header("VR UI Interaction")]
        [SerializeField] private XRRayInteractor leftRayInteractor;
        [SerializeField] private XRRayInteractor rightRayInteractor;
        [SerializeField] private XRInteractorLineVisual leftLineVisual;
        [SerializeField] private XRInteractorLineVisual rightLineVisual;
        
        [Header("VR UI Effects")]
        [SerializeField] private Material uiHoverMaterial;
        [SerializeField] private Color uiHoverColor = Color.cyan;
        [SerializeField] private float uiHoverScale = 1.1f;
        [SerializeField] private AudioClip uiHoverSound;
        [SerializeField] private AudioClip uiClickSound;
        
        // VR UI 상태
        private bool isVRUIInitialized = false;
        private bool isUIVisible = false;
        private GameObject currentUIPanel = null;
        
        // VR UI 요소들
        private Dictionary<string, GameObject> vrUIElements = new Dictionary<string, GameObject>();
        private Dictionary<GameObject, VRUIElement> vrUIElementData = new Dictionary<GameObject, VRUIElement>();
        
        // VR UI 상호작용
        private GameObject hoveredUIElement = null;
        private GameObject selectedUIElement = null;
        private Vector3 lastHeadPosition;
        private Quaternion lastHeadRotation;
        
        // 참조
        private XRManager xrManager;
        private Camera xrCamera;
        private AudioSource audioSource;
        
        // 이벤트
        public System.Action<GameObject> OnUIElementHovered;
        public System.Action<GameObject> OnUIElementUnhovered;
        public System.Action<GameObject> OnUIElementSelected;
        public System.Action<GameObject> OnUIElementDeselected;
        public System.Action<string> OnUIElementClicked;
        public System.Action<bool> OnUIVisibilityChanged;
        
        // 싱글톤 패턴
        public static VRUIManager Instance { get; private set; }
        
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
            InitializeVRUI();
        }
        
        private void Update()
        {
            if (!isVRUIInitialized) return;
            
            UpdateVRUIPosition();
            UpdateVRUIInteraction();
            UpdateVRUIHover();
        }
        
        private void InitializeVRUI()
        {
            Debug.Log("VR UI 시스템 초기화 시작...");
            
            // 컴포넌트 참조
            xrManager = FindObjectOfType<XRManager>();
            xrCamera = Camera.main;
            if (xrCamera == null)
            {
                xrCamera = FindObjectOfType<Camera>();
            }
            
            // 오디오 소스 초기화
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // VR Canvas 초기화
            InitializeVRCanvas();
            
            // VR UI 상호작용 초기화
            InitializeVRUIInteraction();
            
            // 기본 VR UI 생성
            CreateDefaultVRUI();
            
            isVRUIInitialized = true;
            Debug.Log("VR UI 시스템이 초기화되었습니다.");
        }
        
        private void InitializeVRCanvas()
        {
            // VR Canvas 설정
            if (vrCanvas == null)
            {
                GameObject canvasObject = new GameObject("VR Canvas");
                vrCanvas = canvasObject.AddComponent<Canvas>();
                canvasObject.transform.SetParent(transform);
            }
            
            // Canvas 설정
            vrCanvas.renderMode = RenderMode.WorldSpace;
            vrCanvas.worldCamera = xrCamera;
            
            // Canvas Scaler 설정
            if (canvasScaler == null)
            {
                canvasScaler = vrCanvas.gameObject.AddComponent<CanvasScaler>();
            }
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
            
            // Graphic Raycaster 설정
            if (graphicRaycaster == null)
            {
                graphicRaycaster = vrCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }
            
            // Canvas 크기 및 위치 설정
            RectTransform canvasRect = vrCanvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(4f, 2.25f); // 16:9 비율
            canvasRect.localScale = Vector3.one * uiScale;
        }
        
        private void InitializeVRUIInteraction()
        {
            // XR Ray Interactor 설정
            if (leftRayInteractor == null)
            {
                leftRayInteractor = FindObjectOfType<XRRayInteractor>();
            }
            
            if (rightRayInteractor == null)
            {
                rightRayInteractor = FindObjectOfType<XRRayInteractor>();
            }
            
            // XR Interactor Line Visual 설정
            if (leftLineVisual == null && leftRayInteractor != null)
            {
                leftLineVisual = leftRayInteractor.GetComponent<XRInteractorLineVisual>();
            }
            
            if (rightLineVisual == null && rightRayInteractor != null)
            {
                rightLineVisual = rightRayInteractor.GetComponent<XRInteractorLineVisual>();
            }
        }
        
        private void CreateDefaultVRUI()
        {
            // 기본 VR 메뉴 생성
            if (vrMenuPrefab != null)
            {
                CreateVRMenu("MainMenu", "메인 메뉴");
            }
        }
        
        private void UpdateVRUIPosition()
        {
            if (!followHead || xrCamera == null) return;
            
            // 헤드 위치 추적
            Vector3 currentHeadPosition = xrCamera.transform.position;
            Quaternion currentHeadRotation = xrCamera.transform.rotation;
            
            // 헤드 움직임 감지
            if (Vector3.Distance(currentHeadPosition, lastHeadPosition) > 0.1f ||
                Quaternion.Angle(currentHeadRotation, lastHeadRotation) > 5f)
            {
                // UI 위치 업데이트
                UpdateUIPosition(currentHeadPosition, currentHeadRotation);
                
                lastHeadPosition = currentHeadPosition;
                lastHeadRotation = currentHeadRotation;
            }
        }
        
        private void UpdateUIPosition(Vector3 headPosition, Quaternion headRotation)
        {
            if (vrCanvas == null) return;
            
            // UI를 플레이어 앞에 배치
            Vector3 uiPosition = headPosition + headRotation * Vector3.forward * uiDistance;
            uiPosition.y = headPosition.y; // 높이는 플레이어와 동일하게
            
            vrCanvas.transform.position = uiPosition;
            vrCanvas.transform.rotation = headRotation;
        }
        
        private void UpdateVRUIInteraction()
        {
            if (!enableUIHover) return;
            
            // VR UI 상호작용 업데이트
            // 실제 구현에서는 XR Ray Interactor와의 상호작용 처리
        }
        
        private void UpdateVRUIHover()
        {
            // UI 호버 효과 업데이트
            // 실제 구현에서는 레이캐스팅을 통한 호버 감지
        }
        
        public GameObject CreateVRMenu(string menuName, string title)
        {
            GameObject menuObject = new GameObject(menuName);
            menuObject.transform.SetParent(vrCanvas.transform);
            
            // VR Panel 추가
            if (vrPanelPrefab != null)
            {
                GameObject panel = Instantiate(vrPanelPrefab, menuObject.transform);
                panel.name = "Panel";
            }
            
            // 제목 텍스트 추가
            if (vrTextPrefab != null)
            {
                GameObject titleText = Instantiate(vrTextPrefab, menuObject.transform);
                titleText.name = "Title";
                Text textComponent = titleText.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = title;
                    textComponent.fontSize = 48;
                    textComponent.color = Color.white;
                }
            }
            
            // VR UI 요소로 등록
            vrUIElements[menuName] = menuObject;
            
            // VR UI 요소 데이터 생성
            VRUIElement elementData = new VRUIElement
            {
                elementType = VRUIElementType.Menu,
                elementName = menuName,
                isInteractable = true,
                isVisible = true
            };
            
            vrUIElementData[menuObject] = elementData;
            
            Debug.Log($"VR 메뉴가 생성되었습니다: {menuName}");
            return menuObject;
        }
        
        public GameObject CreateVRButton(string buttonName, string buttonText, System.Action onClick)
        {
            GameObject buttonObject = new GameObject(buttonName);
            buttonObject.transform.SetParent(vrCanvas.transform);
            
            // VR Button 추가
            if (vrButtonPrefab != null)
            {
                GameObject button = Instantiate(vrButtonPrefab, buttonObject.transform);
                button.name = "Button";
                
                // 버튼 텍스트 설정
                Text buttonTextComponent = button.GetComponentInChildren<Text>();
                if (buttonTextComponent != null)
                {
                    buttonTextComponent.text = buttonText;
                }
                
                // 버튼 클릭 이벤트 설정
                Button buttonComponent = button.GetComponent<Button>();
                if (buttonComponent != null)
                {
                    buttonComponent.onClick.AddListener(() => {
                        OnVRButtonClicked(buttonName, onClick);
                    });
                }
            }
            
            // VR UI 요소로 등록
            vrUIElements[buttonName] = buttonObject;
            
            // VR UI 요소 데이터 생성
            VRUIElement elementData = new VRUIElement
            {
                elementType = VRUIElementType.Button,
                elementName = buttonName,
                isInteractable = true,
                isVisible = true,
                onClickAction = onClick
            };
            
            vrUIElementData[buttonObject] = elementData;
            
            Debug.Log($"VR 버튼이 생성되었습니다: {buttonName}");
            return buttonObject;
        }
        
        public GameObject CreateVRSlider(string sliderName, float minValue, float maxValue, float currentValue, System.Action<float> onValueChanged)
        {
            GameObject sliderObject = new GameObject(sliderName);
            sliderObject.transform.SetParent(vrCanvas.transform);
            
            // VR Slider 추가
            if (vrSliderPrefab != null)
            {
                GameObject slider = Instantiate(vrSliderPrefab, sliderObject.transform);
                slider.name = "Slider";
                
                // 슬라이더 설정
                Slider sliderComponent = slider.GetComponent<Slider>();
                if (sliderComponent != null)
                {
                    sliderComponent.minValue = minValue;
                    sliderComponent.maxValue = maxValue;
                    sliderComponent.value = currentValue;
                    sliderComponent.onValueChanged.AddListener((value) => {
                        OnVRSliderValueChanged(sliderName, value, onValueChanged);
                    });
                }
            }
            
            // VR UI 요소로 등록
            vrUIElements[sliderName] = sliderObject;
            
            // VR UI 요소 데이터 생성
            VRUIElement elementData = new VRUIElement
            {
                elementType = VRUIElementType.Slider,
                elementName = sliderName,
                isInteractable = true,
                isVisible = true,
                onValueChangedAction = onValueChanged
            };
            
            vrUIElementData[sliderObject] = elementData;
            
            Debug.Log($"VR 슬라이더가 생성되었습니다: {sliderName}");
            return sliderObject;
        }
        
        public GameObject CreateVRToggle(string toggleName, string toggleText, bool isOn, System.Action<bool> onToggle)
        {
            GameObject toggleObject = new GameObject(toggleName);
            toggleObject.transform.SetParent(vrCanvas.transform);
            
            // VR Toggle 추가
            if (vrTogglePrefab != null)
            {
                GameObject toggle = Instantiate(vrTogglePrefab, toggleObject.transform);
                toggle.name = "Toggle";
                
                // 토글 설정
                Toggle toggleComponent = toggle.GetComponent<Toggle>();
                if (toggleComponent != null)
                {
                    toggleComponent.isOn = isOn;
                    toggleComponent.onValueChanged.AddListener((value) => {
                        OnVRToggleValueChanged(toggleName, value, onToggle);
                    });
                }
                
                // 토글 텍스트 설정
                Text toggleTextComponent = toggle.GetComponentInChildren<Text>();
                if (toggleTextComponent != null)
                {
                    toggleTextComponent.text = toggleText;
                }
            }
            
            // VR UI 요소로 등록
            vrUIElements[toggleName] = toggleObject;
            
            // VR UI 요소 데이터 생성
            VRUIElement elementData = new VRUIElement
            {
                elementType = VRUIElementType.Toggle,
                elementName = toggleName,
                isInteractable = true,
                isVisible = true,
                onToggleAction = onToggle
            };
            
            vrUIElementData[toggleObject] = elementData;
            
            Debug.Log($"VR 토글이 생성되었습니다: {toggleName}");
            return toggleObject;
        }
        
        private void OnVRButtonClicked(string buttonName, System.Action onClick)
        {
            // 버튼 클릭 처리
            PlayUIAudio(uiClickSound);
            
            // 클릭 이벤트 발생
            OnUIElementClicked?.Invoke(buttonName);
            
            // 액션 실행
            onClick?.Invoke();
            
            Debug.Log($"VR 버튼이 클릭되었습니다: {buttonName}");
        }
        
        private void OnVRSliderValueChanged(string sliderName, float value, System.Action<float> onValueChanged)
        {
            // 슬라이더 값 변경 처리
            onValueChanged?.Invoke(value);
            
            Debug.Log($"VR 슬라이더 값이 변경되었습니다: {sliderName} = {value}");
        }
        
        private void OnVRToggleValueChanged(string toggleName, bool value, System.Action<bool> onToggle)
        {
            // 토글 값 변경 처리
            PlayUIAudio(uiClickSound);
            
            onToggle?.Invoke(value);
            
            Debug.Log($"VR 토글 값이 변경되었습니다: {toggleName} = {value}");
        }
        
        private void PlayUIAudio(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        public void ShowVRUI()
        {
            if (vrCanvas != null)
            {
                vrCanvas.gameObject.SetActive(true);
                isUIVisible = true;
                OnUIVisibilityChanged?.Invoke(true);
                Debug.Log("VR UI가 표시되었습니다.");
            }
        }
        
        public void HideVRUI()
        {
            if (vrCanvas != null)
            {
                vrCanvas.gameObject.SetActive(false);
                isUIVisible = false;
                OnUIVisibilityChanged?.Invoke(false);
                Debug.Log("VR UI가 숨겨졌습니다.");
            }
        }
        
        public void ToggleVRUI()
        {
            if (isUIVisible)
            {
                HideVRUI();
            }
            else
            {
                ShowVRUI();
            }
        }
        
        public GameObject GetVRUIElement(string elementName)
        {
            return vrUIElements.ContainsKey(elementName) ? vrUIElements[elementName] : null;
        }
        
        public bool IsUIVisible()
        {
            return isUIVisible;
        }
        
        public void SetUIScale(float scale)
        {
            uiScale = scale;
            if (vrCanvas != null)
            {
                vrCanvas.transform.localScale = Vector3.one * uiScale;
            }
        }
        
        public void SetUIDistance(float distance)
        {
            uiDistance = distance;
        }
        
        public void SetFollowHead(bool follow)
        {
            followHead = follow;
        }
        
        private void OnDestroy()
        {
            // VR UI 요소들 정리
            foreach (var element in vrUIElements.Values)
            {
                if (element != null)
                {
                    Destroy(element);
                }
            }
            
            vrUIElements.Clear();
            vrUIElementData.Clear();
        }
    }
    
    // VR UI 데이터 구조체들
    [System.Serializable]
    public struct VRUIElement
    {
        public VRUIElementType elementType;
        public string elementName;
        public bool isInteractable;
        public bool isVisible;
        public System.Action onClickAction;
        public System.Action<float> onValueChangedAction;
        public System.Action<bool> onToggleAction;
    }
    
    public enum VRUIElementType
    {
        Menu,
        Button,
        Slider,
        Toggle,
        Text,
        Panel,
        Image
    }
}
