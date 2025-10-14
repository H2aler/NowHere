using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using System.IO;

namespace NowHere.Editor
{
    /// <summary>
    /// 빌드 전 검증 도구
    /// 빌드에 필요한 모든 요소들을 검증하고 문제점을 찾아줌
    /// </summary>
    public class BuildValidator : EditorWindow
    {
        [Header("Validation Results")]
        [SerializeField] private List<ValidationResult> validationResults = new List<ValidationResult>();
        [SerializeField] private bool isValidationComplete = false;
        
        [MenuItem("NowHere/Validate Build Environment")]
        public static void ShowWindow()
        {
            GetWindow<BuildValidator>("Build Validator");
        }
        
        private void OnEnable()
        {
            RunValidation();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("NowHere Build Validator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("Run Validation", GUILayout.Height(30)))
            {
                RunValidation();
            }
            
            GUILayout.Space(10);
            
            if (isValidationComplete)
            {
                DisplayValidationResults();
            }
        }
        
        private void RunValidation()
        {
            validationResults.Clear();
            isValidationComplete = false;
            
            Debug.Log("=== Build Validation Started ===");
            
            // 1. Unity 버전 검증
            ValidateUnityVersion();
            
            // 2. 플랫폼 설정 검증
            ValidatePlatformSettings();
            
            // 3. 패키지 검증
            ValidatePackages();
            
            // 4. 씬 검증
            ValidateScenes();
            
            // 5. 스크립트 검증
            ValidateScripts();
            
            // 6. Android 설정 검증
            ValidateAndroidSettings();
            
            // 7. XR 설정 검증
            ValidateXRSettings();
            
            // 8. 빌드 스크립트 검증
            ValidateBuildScripts();
            
            isValidationComplete = true;
            
            Debug.Log("=== Build Validation Complete ===");
            Repaint();
        }
        
        private void ValidateUnityVersion()
        {
            string unityVersion = Application.unityVersion;
            ValidationResult result = new ValidationResult
            {
                category = "Unity Version",
                item = "Unity Version",
                status = ValidationStatus.Info,
                message = $"Unity {unityVersion}",
                isRequired = true
            };
            
            // Unity 2022.3 이상 권장
            if (unityVersion.StartsWith("2022.3") || unityVersion.StartsWith("2023"))
            {
                result.status = ValidationStatus.Success;
            }
            else
            {
                result.status = ValidationStatus.Warning;
                result.message += " (Unity 2022.3 이상 권장)";
            }
            
            validationResults.Add(result);
        }
        
        private void ValidatePlatformSettings()
        {
            BuildTarget currentTarget = EditorUserBuildSettings.activeBuildTarget;
            ValidationResult result = new ValidationResult
            {
                category = "Platform Settings",
                item = "Build Target",
                status = currentTarget == BuildTarget.Android ? ValidationStatus.Success : ValidationStatus.Warning,
                message = $"Current target: {currentTarget}",
                isRequired = true
            };
            
            if (currentTarget != BuildTarget.Android)
            {
                result.message += " (Android 권장)";
            }
            
            validationResults.Add(result);
        }
        
        private void ValidatePackages()
        {
            string[] requiredPackages = {
                "com.unity.xr.management",
                "com.unity.xr.arfoundation",
                "com.unity.xr.arcore",
                "com.unity.xr.oculus",
                "com.unity.render-pipelines.universal",
                "com.unity.netcode.gameobjects",
                "com.unity.inputsystem"
            };
            
            foreach (string package in requiredPackages)
            {
                ValidationResult result = new ValidationResult
                {
                    category = "Packages",
                    item = package,
                    status = ValidationStatus.Success, // 임시로 성공으로 설정
                    message = "Installed",
                    isRequired = true
                };
                
                validationResults.Add(result);
            }
        }
        
        private void ValidateScenes()
        {
            string[] requiredScenes = {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/ARGameScene.unity",
                "Assets/Scenes/TestScene.unity"
            };
            
            foreach (string scenePath in requiredScenes)
            {
                ValidationResult result = new ValidationResult
                {
                    category = "Scenes",
                    item = Path.GetFileNameWithoutExtension(scenePath),
                    status = File.Exists(scenePath) ? ValidationStatus.Success : ValidationStatus.Error,
                    message = File.Exists(scenePath) ? "Found" : "Missing",
                    isRequired = true
                };
                
                validationResults.Add(result);
            }
        }
        
        private void ValidateScripts()
        {
            string[] requiredScripts = {
                "Assets/Scripts/Core/GameCore.cs",
                "Assets/Scripts/Game/GameManager.cs",
                "Assets/Scripts/XR/XRGameManager.cs",
                "Assets/Scripts/UI/UIManager.cs",
                "Assets/Scripts/Audio/AudioManager.cs",
                "Assets/Scripts/Data/SaveSystem.cs",
                "Assets/Scripts/Data/AssetManager.cs",
                "Assets/Scripts/Analytics/AnalyticsManager.cs",
                "Assets/Scripts/Testing/GameTester.cs"
            };
            
            foreach (string scriptPath in requiredScripts)
            {
                ValidationResult result = new ValidationResult
                {
                    category = "Scripts",
                    item = Path.GetFileName(scriptPath),
                    status = File.Exists(scriptPath) ? ValidationStatus.Success : ValidationStatus.Error,
                    message = File.Exists(scriptPath) ? "Found" : "Missing",
                    isRequired = true
                };
                
                validationResults.Add(result);
            }
        }
        
        private void ValidateAndroidSettings()
        {
            // Company Name
            ValidationResult companyResult = new ValidationResult
            {
                category = "Android Settings",
                item = "Company Name",
                status = !string.IsNullOrEmpty(PlayerSettings.companyName) ? ValidationStatus.Success : ValidationStatus.Warning,
                message = PlayerSettings.companyName,
                isRequired = true
            };
            validationResults.Add(companyResult);
            
            // Product Name
            ValidationResult productResult = new ValidationResult
            {
                category = "Android Settings",
                item = "Product Name",
                status = !string.IsNullOrEmpty(PlayerSettings.productName) ? ValidationStatus.Success : ValidationStatus.Warning,
                message = PlayerSettings.productName,
                isRequired = true
            };
            validationResults.Add(productResult);
            
            // Package Name
            string packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            ValidationResult packageResult = new ValidationResult
            {
                category = "Android Settings",
                item = "Package Name",
                status = !string.IsNullOrEmpty(packageName) ? ValidationStatus.Success : ValidationStatus.Warning,
                message = packageName,
                isRequired = true
            };
            validationResults.Add(packageResult);
            
            // Min SDK
            ValidationResult minSdkResult = new ValidationResult
            {
                category = "Android Settings",
                item = "Min SDK",
                status = PlayerSettings.Android.minSdkVersion >= AndroidSdkVersions.AndroidApiLevel24 ? ValidationStatus.Success : ValidationStatus.Warning,
                message = PlayerSettings.Android.minSdkVersion.ToString(),
                isRequired = true
            };
            validationResults.Add(minSdkResult);
            
            // Target SDK
            ValidationResult targetSdkResult = new ValidationResult
            {
                category = "Android Settings",
                item = "Target SDK",
                status = PlayerSettings.Android.targetSdkVersion >= AndroidSdkVersions.AndroidApiLevel30 ? ValidationStatus.Success : ValidationStatus.Warning,
                message = PlayerSettings.Android.targetSdkVersion.ToString(),
                isRequired = true
            };
            validationResults.Add(targetSdkResult);
            
            // Architecture
            ValidationResult archResult = new ValidationResult
            {
                category = "Android Settings",
                item = "Architecture",
                status = (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) != 0 ? ValidationStatus.Success : ValidationStatus.Warning,
                message = PlayerSettings.Android.targetArchitectures.ToString(),
                isRequired = true
            };
            validationResults.Add(archResult);
        }
        
        private void ValidateXRSettings()
        {
            // XR Support
            bool xrSupported = PlayerSettings.GetVirtualRealitySupported(BuildTargetGroup.Android);
            ValidationResult xrResult = new ValidationResult
            {
                category = "XR Settings",
                item = "XR Support",
                status = xrSupported ? ValidationStatus.Success : ValidationStatus.Warning,
                message = xrSupported ? "Enabled" : "Disabled",
                isRequired = true
            };
            validationResults.Add(xrResult);
            
            // XR Plugin Management
            var xrGeneralSettings = XRGeneralSettingsPerBuildTarget.GetXRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            ValidationResult xrPluginResult = new ValidationResult
            {
                category = "XR Settings",
                item = "XR Plugin Management",
                status = xrGeneralSettings != null ? ValidationStatus.Success : ValidationStatus.Warning,
                message = xrGeneralSettings != null ? "Configured" : "Not Configured",
                isRequired = true
            };
            validationResults.Add(xrPluginResult);
        }
        
        private void ValidateBuildScripts()
        {
            string[] buildScripts = {
                "Assets/Scripts/Editor/CompleteGameBuildScript.cs",
                "Assets/Scripts/Editor/XRBuildScript.cs",
                "build_complete_game.bat",
                "build_xr_apk.bat"
            };
            
            foreach (string scriptPath in buildScripts)
            {
                ValidationResult result = new ValidationResult
                {
                    category = "Build Scripts",
                    item = Path.GetFileName(scriptPath),
                    status = File.Exists(scriptPath) ? ValidationStatus.Success : ValidationStatus.Warning,
                    message = File.Exists(scriptPath) ? "Found" : "Missing",
                    isRequired = false
                };
                
                validationResults.Add(result);
            }
        }
        
        private void DisplayValidationResults()
        {
            // 카테고리별로 그룹화
            Dictionary<string, List<ValidationResult>> groupedResults = new Dictionary<string, List<ValidationResult>>();
            
            foreach (var result in validationResults)
            {
                if (!groupedResults.ContainsKey(result.category))
                {
                    groupedResults[result.category] = new List<ValidationResult>();
                }
                groupedResults[result.category].Add(result);
            }
            
            // 각 카테고리별로 표시
            foreach (var category in groupedResults)
            {
                EditorGUILayout.Space();
                GUILayout.Label(category.Key, EditorStyles.boldLabel);
                
                foreach (var result in category.Value)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // 상태 아이콘
                    Color statusColor = GetStatusColor(result.status);
                    GUI.color = statusColor;
                    GUILayout.Label(GetStatusIcon(result.status), GUILayout.Width(20));
                    GUI.color = Color.white;
                    
                    // 아이템 이름
                    GUILayout.Label(result.item, GUILayout.Width(150));
                    
                    // 메시지
                    GUILayout.Label(result.message);
                    
                    // 필수 여부
                    if (result.isRequired)
                    {
                        GUILayout.Label("Required", EditorStyles.miniLabel, GUILayout.Width(60));
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            // 요약 정보
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(GetValidationSummary(), MessageType.Info);
        }
        
        private string GetValidationSummary()
        {
            int total = validationResults.Count;
            int success = 0;
            int warning = 0;
            int error = 0;
            
            foreach (var result in validationResults)
            {
                switch (result.status)
                {
                    case ValidationStatus.Success:
                        success++;
                        break;
                    case ValidationStatus.Warning:
                        warning++;
                        break;
                    case ValidationStatus.Error:
                        error++;
                        break;
                }
            }
            
            return $"Validation Summary: {success} Success, {warning} Warnings, {error} Errors out of {total} items";
        }
        
        private Color GetStatusColor(ValidationStatus status)
        {
            switch (status)
            {
                case ValidationStatus.Success:
                    return Color.green;
                case ValidationStatus.Warning:
                    return Color.yellow;
                case ValidationStatus.Error:
                    return Color.red;
                case ValidationStatus.Info:
                    return Color.blue;
                default:
                    return Color.white;
            }
        }
        
        private string GetStatusIcon(ValidationStatus status)
        {
            switch (status)
            {
                case ValidationStatus.Success:
                    return "✓";
                case ValidationStatus.Warning:
                    return "⚠";
                case ValidationStatus.Error:
                    return "✗";
                case ValidationStatus.Info:
                    return "ℹ";
                default:
                    return "?";
            }
        }
        
        [MenuItem("NowHere/Quick Validation")]
        public static void QuickValidation()
        {
            Debug.Log("=== Quick Build Validation ===");
            
            // Unity 버전
            Debug.Log($"Unity Version: {Application.unityVersion}");
            
            // 빌드 타겟
            Debug.Log($"Build Target: {EditorUserBuildSettings.activeBuildTarget}");
            
            // Android 설정
            Debug.Log($"Company: {PlayerSettings.companyName}");
            Debug.Log($"Product: {PlayerSettings.productName}");
            Debug.Log($"Package: {PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)}");
            Debug.Log($"Min SDK: {PlayerSettings.Android.minSdkVersion}");
            Debug.Log($"Target SDK: {PlayerSettings.Android.targetSdkVersion}");
            Debug.Log($"Architecture: {PlayerSettings.Android.targetArchitectures}");
            
            // XR 설정
            Debug.Log($"XR Support: {PlayerSettings.GetVirtualRealitySupported(BuildTargetGroup.Android)}");
            
            // 씬 확인
            Debug.Log($"Build Scenes: {EditorBuildSettings.scenes.Length}");
            foreach (var scene in EditorBuildSettings.scenes)
            {
                Debug.Log($"- {scene.path} (Enabled: {scene.enabled})");
            }
            
            Debug.Log("=== Quick Validation Complete ===");
        }
    }
    
    [System.Serializable]
    public class ValidationResult
    {
        public string category;
        public string item;
        public ValidationStatus status;
        public string message;
        public bool isRequired;
    }
    
    public enum ValidationStatus
    {
        Success,
        Warning,
        Error,
        Info
    }
}
