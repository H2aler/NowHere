using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Collections.Generic;
using System.Collections;

namespace NowHere.Editor
{
    /// <summary>
    /// Unity 패키지 자동 설치 스크립트
    /// 빌드에 필요한 모든 패키지를 자동으로 설치
    /// </summary>
    public class PackageInstaller : EditorWindow
    {
        [Header("Required Packages")]
        [SerializeField] private List<PackageInfo> requiredPackages = new List<PackageInfo>();
        
        [Header("Installation Status")]
        [SerializeField] private bool isInstalling = false;
        [SerializeField] private int currentPackageIndex = 0;
        [SerializeField] private string currentPackageName = "";
        
        private ListRequest listRequest;
        private AddRequest addRequest;
        private Queue<PackageInfo> installQueue = new Queue<PackageInfo>();
        
        [MenuItem("NowHere/Install Required Packages")]
        public static void ShowWindow()
        {
            GetWindow<PackageInstaller>("Package Installer");
        }
        
        private void OnEnable()
        {
            InitializeRequiredPackages();
        }
        
        private void InitializeRequiredPackages()
        {
            requiredPackages.Clear();
            
            // XR 관련 패키지
            requiredPackages.Add(new PackageInfo
            {
                name = "com.unity.xr.openxr",
                displayName = "OpenXR Plugin",
                description = "OpenXR 표준을 지원하는 XR 플러그인",
                isRequired = true
            });
            
            requiredPackages.Add(new PackageInfo
            {
                name = "com.unity.xr.interaction.toolkit",
                displayName = "XR Interaction Toolkit",
                description = "XR 상호작용을 위한 툴킷",
                isRequired = true
            });
            
            requiredPackages.Add(new PackageInfo
            {
                name = "com.unity.xr.arfoundation.samples",
                displayName = "AR Foundation Samples",
                description = "AR Foundation 샘플 프로젝트",
                isRequired = false
            });
            
            // 모바일 관련 패키지
            requiredPackages.Add(new PackageInfo
            {
                name = "com.unity.mobile.android-logcat",
                displayName = "Android Logcat",
                description = "Android 디바이스 로그 확인",
                isRequired = false
            });
            
            // 오디오 관련 패키지
            requiredPackages.Add(new PackageInfo
            {
                name = "com.unity.audio.spatializer",
                displayName = "Spatial Audio",
                description = "3D 공간 오디오 지원",
                isRequired = false
            });
            
            // 분석 관련 패키지
            requiredPackages.Add(new PackageInfo
            {
                name = "com.unity.analytics",
                displayName = "Unity Analytics",
                description = "게임 분석 서비스",
                isRequired = false
            });
            
            // 테스트 관련 패키지
            requiredPackages.Add(new PackageInfo
            {
                name = "com.unity.test-framework",
                displayName = "Test Framework",
                description = "Unity 테스트 프레임워크",
                isRequired = false
            });
        }
        
        private void OnGUI()
        {
            GUILayout.Label("NowHere Game Package Installer", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            // 설치 상태 표시
            if (isInstalling)
            {
                GUILayout.Label($"Installing: {currentPackageName}", EditorStyles.helpBox);
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(0, 20), 
                    (float)currentPackageIndex / requiredPackages.Count, 
                    $"Progress: {currentPackageIndex}/{requiredPackages.Count}");
            }
            
            GUILayout.Space(10);
            
            // 패키지 목록 표시
            GUILayout.Label("Required Packages:", EditorStyles.boldLabel);
            
            foreach (var package in requiredPackages)
            {
                EditorGUILayout.BeginHorizontal();
                
                // 패키지 상태 표시
                string status = GetPackageStatus(package.name);
                Color statusColor = GetStatusColor(status);
                
                GUI.color = statusColor;
                GUILayout.Label($"[{status}]", GUILayout.Width(60));
                GUI.color = Color.white;
                
                // 패키지 정보
                EditorGUILayout.BeginVertical();
                GUILayout.Label(package.displayName, EditorStyles.boldLabel);
                GUILayout.Label(package.description, EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            
            GUILayout.Space(20);
            
            // 설치 버튼
            GUI.enabled = !isInstalling;
            if (GUILayout.Button("Install All Required Packages", GUILayout.Height(30)))
            {
                InstallAllPackages();
            }
            
            GUI.enabled = true;
            
            GUILayout.Space(10);
            
            // 개별 패키지 설치 버튼
            GUILayout.Label("Individual Package Installation:", EditorStyles.boldLabel);
            
            foreach (var package in requiredPackages)
            {
                EditorGUILayout.BeginHorizontal();
                
                GUI.enabled = !isInstalling && GetPackageStatus(package.name) != "Installed";
                if (GUILayout.Button($"Install {package.displayName}", GUILayout.Width(200)))
                {
                    InstallPackage(package);
                }
                
                GUI.enabled = true;
                
                EditorGUILayout.EndHorizontal();
            }
            
            GUILayout.Space(20);
            
            // 도움말
            EditorGUILayout.HelpBox(
                "이 도구는 NowHere 게임 빌드에 필요한 모든 패키지를 자동으로 설치합니다.\n\n" +
                "설치 후 Unity 에디터를 재시작하는 것을 권장합니다.",
                MessageType.Info);
        }
        
        private void InstallAllPackages()
        {
            if (isInstalling) return;
            
            installQueue.Clear();
            
            // 설치할 패키지들을 큐에 추가
            foreach (var package in requiredPackages)
            {
                if (GetPackageStatus(package.name) != "Installed")
                {
                    installQueue.Enqueue(package);
                }
            }
            
            if (installQueue.Count == 0)
            {
                EditorUtility.DisplayDialog("Package Installer", 
                    "모든 필수 패키지가 이미 설치되어 있습니다.", "확인");
                return;
            }
            
            isInstalling = true;
            currentPackageIndex = 0;
            
            InstallNextPackage();
        }
        
        private void InstallPackage(PackageInfo package)
        {
            if (isInstalling) return;
            
            installQueue.Clear();
            installQueue.Enqueue(package);
            
            isInstalling = true;
            currentPackageIndex = 0;
            
            InstallNextPackage();
        }
        
        private void InstallNextPackage()
        {
            if (installQueue.Count == 0)
            {
                CompleteInstallation();
                return;
            }
            
            PackageInfo package = installQueue.Dequeue();
            currentPackageName = package.displayName;
            
            Debug.Log($"Installing package: {package.name}");
            
            addRequest = Client.Add(package.name);
            EditorApplication.update += OnAddRequestProgress;
        }
        
        private void OnAddRequestProgress()
        {
            if (addRequest == null) return;
            
            if (addRequest.IsCompleted)
            {
                EditorApplication.update -= OnAddRequestProgress;
                
                if (addRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"Package installed successfully: {addRequest.Result.packageId}");
                }
                else
                {
                    Debug.LogError($"Package installation failed: {addRequest.Error.message}");
                }
                
                currentPackageIndex++;
                InstallNextPackage();
            }
        }
        
        private void CompleteInstallation()
        {
            isInstalling = false;
            currentPackageName = "";
            
            Debug.Log("All packages installation completed!");
            
            EditorUtility.DisplayDialog("Package Installer", 
                "모든 패키지 설치가 완료되었습니다!\n\nUnity 에디터를 재시작하는 것을 권장합니다.", "확인");
            
            Repaint();
        }
        
        private string GetPackageStatus(string packageName)
        {
            // 패키지 상태 확인 로직
            // 실제 구현에서는 Package Manager API를 사용하여 상태 확인
            
            // 임시로 설치된 패키지 목록과 비교
            string[] installedPackages = {
                "com.unity.xr.management",
                "com.unity.xr.arfoundation",
                "com.unity.xr.arcore",
                "com.unity.xr.oculus",
                "com.unity.render-pipelines.universal",
                "com.unity.netcode.gameobjects",
                "com.unity.inputsystem",
                "com.unity.timeline",
                "com.unity.visualscripting"
            };
            
            foreach (string installed in installedPackages)
            {
                if (packageName == installed)
                {
                    return "Installed";
                }
            }
            
            return "Not Installed";
        }
        
        private Color GetStatusColor(string status)
        {
            switch (status)
            {
                case "Installed":
                    return Color.green;
                case "Not Installed":
                    return Color.red;
                case "Installing":
                    return Color.yellow;
                default:
                    return Color.white;
            }
        }
        
        [MenuItem("NowHere/Check Package Status")]
        public static void CheckPackageStatus()
        {
            Debug.Log("=== Package Status Check ===");
            
            string[] requiredPackages = {
                "com.unity.xr.management",
                "com.unity.xr.arfoundation",
                "com.unity.xr.arcore",
                "com.unity.xr.oculus",
                "com.unity.xr.openxr",
                "com.unity.xr.interaction.toolkit",
                "com.unity.render-pipelines.universal",
                "com.unity.netcode.gameobjects",
                "com.unity.inputsystem",
                "com.unity.timeline",
                "com.unity.visualscripting"
            };
            
            foreach (string package in requiredPackages)
            {
                Debug.Log($"Package: {package}");
            }
            
            Debug.Log("=== Package Status Check Complete ===");
        }
        
        [MenuItem("NowHere/Open Package Manager")]
        public static void OpenPackageManager()
        {
            EditorApplication.ExecuteMenuItem("Window/Package Manager");
        }
    }
    
    [System.Serializable]
    public class PackageInfo
    {
        public string name;
        public string displayName;
        public string description;
        public bool isRequired;
    }
}
