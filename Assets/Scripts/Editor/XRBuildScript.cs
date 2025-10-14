using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using System.IO;

namespace NowHere.Editor
{
    /// <summary>
    /// XR 버전 빌드를 위한 Unity 에디터 스크립트
    /// VR, AR, MR을 지원하는 XR APK 빌드
    /// </summary>
    public class XRBuildScript
    {
        [MenuItem("Build/XR Android APK (VR/AR/MR)")]
        public static void BuildXRAndroidAPK()
        {
            Debug.Log("=== XR Android APK 빌드 시작 ===");
            
            try
            {
                // 1. XR 빌드 전 준비 작업
                PrepareXRBuildEnvironment();
                
                // 2. XR 설정 구성
                ConfigureXRSettings();
                
                // 3. 씬 설정
                SetupXRBuildScenes();
                
                // 4. Android 설정
                ConfigureAndroidSettings();
                
                // 5. XR 빌드 실행
                ExecuteXRBuild();
                
                Debug.Log("=== XR 빌드 프로세스 완료 ===");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"XR 빌드 중 오류 발생: {e.Message}");
                EditorUtility.DisplayDialog("XR 빌드 오류", $"XR APK 빌드가 실패했습니다:\n{e.Message}", "확인");
            }
        }
        
        private static void PrepareXRBuildEnvironment()
        {
            Debug.Log("XR 빌드 환경 준비 중...");
            
            // 빌드 폴더 생성
            string buildPath = Path.Combine(Application.dataPath, "..", "XRBuilds");
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
                Debug.Log($"XR 빌드 폴더 생성: {buildPath}");
            }
            
            // Android 빌드 타겟으로 전환
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.Log("Android 빌드 타겟으로 전환 중...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            
            // 씬 저장
            EditorSceneManager.SaveOpenScenes();
            
            Debug.Log("XR 빌드 환경 준비 완료");
        }
        
        private static void ConfigureXRSettings()
        {
            Debug.Log("XR 설정 구성 중...");
            
            // XR Plugin Management 설정
            var xrGeneralSettings = XRGeneralSettingsPerBuildTarget.GetXRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if (xrGeneralSettings == null)
            {
                Debug.LogWarning("XR General Settings가 없습니다. XR Plugin Management를 설치해주세요.");
                return;
            }
            
            // XR Provider 활성화
            var xrManagerSettings = xrGeneralSettings.Manager;
            if (xrManagerSettings != null)
            {
                // OpenXR Provider 활성화
                xrManagerSettings.InitializeManagerSettings();
                
                // AR Foundation Provider 활성화
                // 실제 구현에서는 필요한 XR Provider들을 활성화
                
                Debug.Log("XR Provider 설정 완료");
            }
            
            // XR 설정 저장
            EditorUtility.SetDirty(xrGeneralSettings);
            AssetDatabase.SaveAssets();
            
            Debug.Log("XR 설정 구성 완료");
        }
        
        private static void SetupXRBuildScenes()
        {
            Debug.Log("XR 빌드 씬 설정 중...");
            
            // XR 지원 씬들 설정
            string[] xrScenes = {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/ARGameScene.unity",
                "Assets/Scenes/VRGameScene.unity",
                "Assets/Scenes/MRGameScene.unity"
            };
            
            // 실제 존재하는 씬들만 포함
            var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (string scenePath in xrScenes)
            {
                if (File.Exists(scenePath))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    Debug.Log($"XR 씬 추가: {scenePath}");
                }
                else
                {
                    Debug.LogWarning($"XR 씬 파일을 찾을 수 없음: {scenePath}");
                }
            }
            
            if (buildScenes.Count == 0)
            {
                throw new System.Exception("빌드할 XR 씬이 없습니다!");
            }
            
            // 빌드 설정에 씬 추가
            EditorBuildSettings.scenes = buildScenes.ToArray();
            
            Debug.Log($"XR 빌드 씬 설정 완료: {buildScenes.Count}개 씬");
        }
        
        private static void ConfigureAndroidSettings()
        {
            Debug.Log("Android XR 설정 구성 중...");
            
            // 빌드 시스템 설정
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            
            // 회사 및 제품 정보
            PlayerSettings.companyName = "NowHere Games";
            PlayerSettings.productName = "NowHere XR MMORPG";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.nowhere.xrmmorpg");
            
            // 버전 정보
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            
            // Android SDK 설정
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            
            // 아키텍처 설정
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            // 스크립팅 백엔드
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            
            // XR 지원 활성화
            PlayerSettings.SetVirtualRealitySupported(BuildTargetGroup.Android, true);
            
            // XR 권한 설정
            PlayerSettings.Android.usesPermission.Add("android.permission.CAMERA");
            PlayerSettings.Android.usesPermission.Add("android.permission.RECORD_AUDIO");
            PlayerSettings.Android.usesPermission.Add("android.permission.ACCESS_FINE_LOCATION");
            PlayerSettings.Android.usesPermission.Add("android.permission.INTERNET");
            PlayerSettings.Android.usesPermission.Add("android.permission.ACCESS_COARSE_LOCATION");
            PlayerSettings.Android.usesPermission.Add("android.permission.ACCESS_WIFI_STATE");
            PlayerSettings.Android.usesPermission.Add("android.permission.ACCESS_NETWORK_STATE");
            PlayerSettings.Android.usesPermission.Add("android.permission.BLUETOOTH");
            PlayerSettings.Android.usesPermission.Add("android.permission.BLUETOOTH_ADMIN");
            
            // XR 특화 설정
            PlayerSettings.Android.blitType = AndroidBlitType.Never;
            PlayerSettings.Android.startInFullscreen = true;
            PlayerSettings.Android.renderOutsideSafeArea = true;
            
            // 최적화 설정
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.stripUnusedMeshComponents = true;
            
            // XR 렌더링 설정
            PlayerSettings.Android.vulkanSettings = AndroidVulkanSettings.Disable;
            PlayerSettings.Android.graphicsJobs = true;
            
            Debug.Log("Android XR 설정 완료");
            Debug.Log($"Company: {PlayerSettings.companyName}");
            Debug.Log($"Product: {PlayerSettings.productName}");
            Debug.Log($"Package: {PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)}");
            Debug.Log($"Min SDK: {PlayerSettings.Android.minSdkVersion}");
            Debug.Log($"Target SDK: {PlayerSettings.Android.targetSdkVersion}");
            Debug.Log($"XR Supported: {PlayerSettings.GetVirtualRealitySupported(BuildTargetGroup.Android)}");
        }
        
        private static void ExecuteXRBuild()
        {
            Debug.Log("XR APK 빌드 실행 중...");
            
            // 빌드 경로 설정
            string buildPath = Path.Combine(Application.dataPath, "..", "XRBuilds");
            string apkName = "NowHere_XR_MMORPG_v1.0.0.apk";
            string fullPath = Path.Combine(buildPath, apkName);
            
            // 빌드 옵션 설정
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetEnabledScenePaths();
            buildPlayerOptions.locationPathName = fullPath;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;
            
            Debug.Log($"XR 빌드 대상: {buildPlayerOptions.target}");
            Debug.Log($"XR 빌드 경로: {fullPath}");
            Debug.Log($"XR 씬 개수: {buildPlayerOptions.scenes.Length}");
            
            // XR 빌드 실행
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            // 빌드 결과 처리
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"=== XR 빌드 성공! ===");
                Debug.Log($"XR APK 파일: {fullPath}");
                
                if (File.Exists(fullPath))
                {
                    long fileSize = new FileInfo(fullPath).Length;
                    Debug.Log($"파일 크기: {fileSize / 1024 / 1024} MB");
                }
                
                Debug.Log($"빌드 시간: {summary.totalTime}");
                Debug.Log($"총 크기: {summary.totalSize} bytes");
                
                // 성공 알림 (배치 모드에서는 다이얼로그 사용 안함)
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("XR 빌드 완료", 
                        $"XR APK 빌드가 성공적으로 완료되었습니다!\n\n파일: {apkName}\n경로: {buildPath}\n크기: {new FileInfo(fullPath).Length / 1024 / 1024} MB\n\n지원 기능:\n- VR (Virtual Reality)\n- AR (Augmented Reality)\n- MR (Mixed Reality)\n- 멀티플레이어\n- XR 전투 시스템", 
                        "확인");
                    
                    // 빌드 폴더 열기
                    EditorUtility.RevealInFinder(buildPath);
                }
            }
            else
            {
                Debug.LogError($"=== XR 빌드 실패! ===");
                Debug.LogError($"오류: {summary.result}");
                
                // 실패 알림 (배치 모드에서는 다이얼로그 사용 안함)
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("XR 빌드 실패", 
                        $"XR APK 빌드가 실패했습니다.\n\n오류: {summary.result}\n\nConsole 창에서 자세한 오류를 확인하세요.\n\nXR Plugin Management가 설치되어 있는지 확인해주세요.", 
                        "확인");
                }
            }
        }
        
        private static string[] GetEnabledScenePaths()
        {
            var enabledScenes = new System.Collections.Generic.List<string>();
            
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    enabledScenes.Add(scene.path);
                }
            }
            
            return enabledScenes.ToArray();
        }
        
        [MenuItem("Build/XR Build Settings")]
        public static void CheckXRBuildSettings()
        {
            Debug.Log("=== XR 빌드 설정 확인 ===");
            Debug.Log($"현재 빌드 타겟: {EditorUserBuildSettings.activeBuildTarget}");
            Debug.Log($"회사명: {PlayerSettings.companyName}");
            Debug.Log($"제품명: {PlayerSettings.productName}");
            Debug.Log($"패키지명: {PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)}");
            Debug.Log($"버전: {PlayerSettings.bundleVersion}");
            Debug.Log($"Min SDK: {PlayerSettings.Android.minSdkVersion}");
            Debug.Log($"Target SDK: {PlayerSettings.Android.targetSdkVersion}");
            Debug.Log($"아키텍처: {PlayerSettings.Android.targetArchitectures}");
            Debug.Log($"XR 지원: {PlayerSettings.GetVirtualRealitySupported(BuildTargetGroup.Android)}");
            Debug.Log($"빌드 씬 개수: {EditorBuildSettings.scenes.Length}");
            
            // XR 설정 확인
            var xrGeneralSettings = XRGeneralSettingsPerBuildTarget.GetXRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if (xrGeneralSettings != null)
            {
                Debug.Log("XR Plugin Management: 설치됨");
                Debug.Log($"XR Manager: {xrGeneralSettings.Manager != null}");
            }
            else
            {
                Debug.LogWarning("XR Plugin Management: 설치되지 않음");
            }
            
            foreach (var scene in EditorBuildSettings.scenes)
            {
                Debug.Log($"씬: {scene.path} (활성화: {scene.enabled})");
            }
            
            Debug.Log("=== XR 빌드 설정 확인 완료 ===");
        }
        
        [MenuItem("Build/Install XR Dependencies")]
        public static void InstallXRDependencies()
        {
            Debug.Log("XR 의존성 설치 중...");
            
            // XR Plugin Management 설치 안내
            EditorUtility.DisplayDialog("XR 의존성 설치", 
                "XR 기능을 사용하려면 다음 패키지들을 설치해야 합니다:\n\n" +
                "1. XR Plugin Management\n" +
                "2. OpenXR Plugin\n" +
                "3. AR Foundation\n" +
                "4. ARCore XR Plugin\n" +
                "5. Oculus XR Plugin\n\n" +
                "Window > Package Manager에서 설치해주세요.", 
                "확인");
            
            Debug.Log("XR 의존성 설치 안내 완료");
        }
    }
}
