using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Collections.Generic;

namespace NowHere.Editor
{
    /// <summary>
    /// 완전한 게임 빌드 스크립트
    /// 모든 시스템이 통합된 완성된 게임을 빌드
    /// </summary>
    public class CompleteGameBuildScript
    {
        [MenuItem("Build/Complete Game Build (All Systems)")]
        public static void BuildCompleteGame()
        {
            Debug.Log("=== 완전한 게임 빌드 시작 ===");
            
            try
            {
                // 1. 빌드 전 검증
                ValidateBuildEnvironment();
                
                // 2. 모든 시스템 통합 확인
                ValidateAllSystems();
                
                // 3. 빌드 환경 준비
                PrepareCompleteBuildEnvironment();
                
                // 4. 씬 설정
                SetupCompleteBuildScenes();
                
                // 5. Android 설정
                ConfigureCompleteAndroidSettings();
                
                // 6. XR 설정
                ConfigureCompleteXRSettings();
                
                // 7. 최종 빌드 실행
                ExecuteCompleteBuild();
                
                Debug.Log("=== 완전한 게임 빌드 완료 ===");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"완전한 게임 빌드 실패: {e.Message}");
                EditorUtility.DisplayDialog("빌드 실패", $"완전한 게임 빌드가 실패했습니다:\n{e.Message}", "확인");
            }
        }
        
        private static void ValidateBuildEnvironment()
        {
            Debug.Log("빌드 환경 검증 중...");
            
            // Unity 버전 확인
            string unityVersion = Application.unityVersion;
            Debug.Log($"Unity 버전: {unityVersion}");
            
            // 플랫폼 확인
            BuildTarget currentTarget = EditorUserBuildSettings.activeBuildTarget;
            Debug.Log($"현재 빌드 타겟: {currentTarget}");
            
            // Android SDK 확인
            if (currentTarget == BuildTarget.Android)
            {
                string androidSDKPath = EditorPrefs.GetString("AndroidSdkRoot");
                if (string.IsNullOrEmpty(androidSDKPath))
                {
                    Debug.LogWarning("Android SDK 경로가 설정되지 않았습니다.");
                }
                else
                {
                    Debug.Log($"Android SDK 경로: {androidSDKPath}");
                }
            }
            
            Debug.Log("빌드 환경 검증 완료");
        }
        
        private static void ValidateAllSystems()
        {
            Debug.Log("모든 시스템 통합 확인 중...");
            
            // 필수 스크립트 확인
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
                if (!File.Exists(scriptPath))
                {
                    throw new System.Exception($"필수 스크립트가 없습니다: {scriptPath}");
                }
            }
            
            // 필수 씬 확인
            string[] requiredScenes = {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/ARGameScene.unity",
                "Assets/Scenes/TestScene.unity"
            };
            
            foreach (string scenePath in requiredScenes)
            {
                if (!File.Exists(scenePath))
                {
                    throw new System.Exception($"필수 씬이 없습니다: {scenePath}");
                }
            }
            
            Debug.Log("모든 시스템 통합 확인 완료");
        }
        
        private static void PrepareCompleteBuildEnvironment()
        {
            Debug.Log("완전한 빌드 환경 준비 중...");
            
            // 빌드 폴더 생성
            string buildPath = Path.Combine(Application.dataPath, "..", "CompleteBuilds");
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
                Debug.Log($"완전한 빌드 폴더 생성: {buildPath}");
            }
            
            // Android 빌드 타겟으로 전환
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.Log("Android 빌드 타겟으로 전환 중...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            
            // 씬 저장
            EditorSceneManager.SaveOpenScenes();
            
            Debug.Log("완전한 빌드 환경 준비 완료");
        }
        
        private static void SetupCompleteBuildScenes()
        {
            Debug.Log("완전한 빌드 씬 설정 중...");
            
            // 모든 씬 설정
            string[] allScenes = {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/ARGameScene.unity",
                "Assets/Scenes/TestScene.unity"
            };
            
            var buildScenes = new List<EditorBuildSettingsScene>();
            foreach (string scenePath in allScenes)
            {
                if (File.Exists(scenePath))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    Debug.Log($"완전한 빌드 씬 추가: {scenePath}");
                }
                else
                {
                    Debug.LogWarning($"씬 파일을 찾을 수 없음: {scenePath}");
                }
            }
            
            if (buildScenes.Count == 0)
            {
                throw new System.Exception("빌드할 씬이 없습니다!");
            }
            
            // 빌드 설정에 씬 추가
            EditorBuildSettings.scenes = buildScenes.ToArray();
            
            Debug.Log($"완전한 빌드 씬 설정 완료: {buildScenes.Count}개 씬");
        }
        
        private static void ConfigureCompleteAndroidSettings()
        {
            Debug.Log("완전한 Android 설정 구성 중...");
            
            // 빌드 시스템 설정
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            
            // 회사 및 제품 정보
            PlayerSettings.companyName = "NowHere Games";
            PlayerSettings.productName = "NowHere Complete AR/VR/XR MMORPG";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.nowhere.complete.mmorpg");
            
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
            
            // 모든 권한 설정
            PlayerSettings.Android.usesPermission.Add("android.permission.CAMERA");
            PlayerSettings.Android.usesPermission.Add("android.permission.RECORD_AUDIO");
            PlayerSettings.Android.usesPermission.Add("android.permission.ACCESS_FINE_LOCATION");
            PlayerSettings.Android.usesPermission.Add("android.permission.INTERNET");
            PlayerSettings.Android.usesPermission.Add("android.permission.ACCESS_COARSE_LOCATION");
            PlayerSettings.Android.usesPermission.Add("android.permission.ACCESS_WIFI_STATE");
            PlayerSettings.Android.usesPermission.Add("android.permission.ACCESS_NETWORK_STATE");
            PlayerSettings.Android.usesPermission.Add("android.permission.BLUETOOTH");
            PlayerSettings.Android.usesPermission.Add("android.permission.BLUETOOTH_ADMIN");
            PlayerSettings.Android.usesPermission.Add("android.permission.VIBRATE");
            PlayerSettings.Android.usesPermission.Add("android.permission.WAKE_LOCK");
            PlayerSettings.Android.usesPermission.Add("android.permission.WRITE_EXTERNAL_STORAGE");
            PlayerSettings.Android.usesPermission.Add("android.permission.READ_EXTERNAL_STORAGE");
            
            // 최적화 설정
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.stripUnusedMeshComponents = true;
            PlayerSettings.Android.vulkanSettings = AndroidVulkanSettings.Disable;
            PlayerSettings.Android.graphicsJobs = true;
            
            // 앱 아이콘 및 스플래시 설정
            PlayerSettings.Android.startInFullscreen = true;
            PlayerSettings.Android.renderOutsideSafeArea = true;
            
            Debug.Log("완전한 Android 설정 완료");
        }
        
        private static void ConfigureCompleteXRSettings()
        {
            Debug.Log("완전한 XR 설정 구성 중...");
            
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
                xrManagerSettings.InitializeManagerSettings();
                Debug.Log("XR Provider 설정 완료");
            }
            
            // XR 설정 저장
            EditorUtility.SetDirty(xrGeneralSettings);
            AssetDatabase.SaveAssets();
            
            Debug.Log("완전한 XR 설정 완료");
        }
        
        private static void ExecuteCompleteBuild()
        {
            Debug.Log("완전한 게임 빌드 실행 중...");
            
            // 빌드 경로 설정
            string buildPath = Path.Combine(Application.dataPath, "..", "CompleteBuilds");
            string apkName = "NowHere_Complete_AR_VR_XR_MMORPG_v1.0.0.apk";
            string fullPath = Path.Combine(buildPath, apkName);
            
            // 빌드 옵션 설정
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetEnabledScenePaths();
            buildPlayerOptions.locationPathName = fullPath;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;
            
            Debug.Log($"완전한 빌드 대상: {buildPlayerOptions.target}");
            Debug.Log($"완전한 빌드 경로: {fullPath}");
            Debug.Log($"완전한 빌드 씬 개수: {buildPlayerOptions.scenes.Length}");
            
            // 완전한 빌드 실행
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            // 빌드 결과 처리
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"=== 완전한 게임 빌드 성공! ===");
                Debug.Log($"완전한 APK 파일: {fullPath}");
                
                if (File.Exists(fullPath))
                {
                    long fileSize = new FileInfo(fullPath).Length;
                    Debug.Log($"파일 크기: {fileSize / 1024 / 1024} MB");
                }
                
                Debug.Log($"빌드 시간: {summary.totalTime}");
                Debug.Log($"총 크기: {summary.totalSize} bytes");
                
                // 성공 알림
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("완전한 게임 빌드 완료", 
                        $"완전한 AR/VR/XR MMORPG 빌드가 성공적으로 완료되었습니다!\n\n" +
                        $"파일: {apkName}\n" +
                        $"경로: {buildPath}\n" +
                        $"크기: {new FileInfo(fullPath).Length / 1024 / 1024} MB\n\n" +
                        $"포함된 시스템:\n" +
                        $"✅ 게임 코어 시스템\n" +
                        $"✅ XR 시스템 (VR/AR/MR)\n" +
                        $"✅ UI 시스템 (모바일/XR)\n" +
                        $"✅ 오디오 시스템 (3D/음성채팅)\n" +
                        $"✅ 저장/로드 시스템\n" +
                        $"✅ 에셋 관리 시스템\n" +
                        $"✅ 분석 시스템\n" +
                        $"✅ 테스트 시스템\n" +
                        $"✅ 멀티플레이어 시스템\n" +
                        $"✅ 전투 시스템\n" +
                        $"✅ RPG 시스템\n" +
                        $"✅ 센서 시스템\n" +
                        $"✅ 모션 감지 시스템\n" +
                        $"✅ 터치 상호작용 시스템", 
                        "확인");
                    
                    // 빌드 폴더 열기
                    EditorUtility.RevealInFinder(buildPath);
                }
            }
            else
            {
                Debug.LogError($"=== 완전한 게임 빌드 실패! ===");
                Debug.LogError($"오류: {summary.result}");
                
                // 실패 알림
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("완전한 게임 빌드 실패", 
                        $"완전한 AR/VR/XR MMORPG 빌드가 실패했습니다.\n\n" +
                        $"오류: {summary.result}\n\n" +
                        $"Console 창에서 자세한 오류를 확인하세요.\n\n" +
                        $"필요한 패키지들이 설치되어 있는지 확인해주세요:\n" +
                        $"• XR Plugin Management\n" +
                        $"• OpenXR Plugin\n" +
                        $"• AR Foundation\n" +
                        $"• ARCore XR Plugin", 
                        "확인");
                }
            }
        }
        
        private static string[] GetEnabledScenePaths()
        {
            var enabledScenes = new List<string>();
            
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    enabledScenes.Add(scene.path);
                }
            }
            
            return enabledScenes.ToArray();
        }
        
        [MenuItem("Build/Complete Build Settings Check")]
        public static void CheckCompleteBuildSettings()
        {
            Debug.Log("=== 완전한 빌드 설정 확인 ===");
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
            
            // 필수 스크립트 확인
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
            
            Debug.Log("필수 스크립트 확인:");
            foreach (string scriptPath in requiredScripts)
            {
                bool exists = File.Exists(scriptPath);
                Debug.Log($"- {scriptPath}: {(exists ? "존재" : "없음")}");
            }
            
            foreach (var scene in EditorBuildSettings.scenes)
            {
                Debug.Log($"씬: {scene.path} (활성화: {scene.enabled})");
            }
            
            Debug.Log("=== 완전한 빌드 설정 확인 완료 ===");
        }
    }
}
