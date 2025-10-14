using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace NowHere.Editor
{
    /// <summary>
    /// 완전한 Unity Android APK 빌드 스크립트
    /// 모든 설정을 자동으로 구성하고 안정적인 빌드를 수행
    /// </summary>
    public class CompleteBuildScript
    {
        [MenuItem("Build/Complete Android APK Build")]
        public static void CompleteAndroidBuild()
        {
            Debug.Log("=== Complete Android APK Build Start ===");
            
            try
            {
                // 1. Prepare build environment
                PrepareBuildEnvironment();
                
                // 2. Setup scenes
                SetupBuildScenes();
                
                // 3. Configure Android settings
                ConfigureAndroidSettings();
                
                // 4. Execute build
                ExecuteBuild();
                
                Debug.Log("=== Build Process Complete ===");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Build Error: {e.Message}");
                EditorUtility.DisplayDialog("Build Error", $"An error occurred during build:\n{e.Message}", "OK");
            }
        }
        
        private static void PrepareBuildEnvironment()
        {
            Debug.Log("Preparing build environment...");
            
            // Create build folder
            string buildPath = Path.Combine(Application.dataPath, "..", "UnityBuilds");
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
                Debug.Log($"Build folder created: {buildPath}");
            }
            
            // Switch to Android build target
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.Log("Switching to Android build target...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            
            // Save scenes
            EditorSceneManager.SaveOpenScenes();
            
            Debug.Log("Build environment preparation complete");
        }
        
        private static void SetupBuildScenes()
        {
            Debug.Log("빌드 씬 설정 중...");
            
            // 실제 존재하는 씬들만 포함
            List<string> availableScenes = new List<string>();
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
            
            string[] scenePaths = {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/ARGameScene.unity",
                "Assets/Scenes/TestScene.unity"
            };
            
            foreach (string scenePath in scenePaths)
            {
                if (File.Exists(scenePath))
                {
                    availableScenes.Add(scenePath);
                    buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    Debug.Log($"씬 추가: {scenePath}");
                }
                else
                {
                    Debug.LogWarning($"씬 파일을 찾을 수 없음: {scenePath}");
                }
            }
            
            if (availableScenes.Count == 0)
            {
                throw new System.Exception("빌드할 씬이 없습니다!");
            }
            
            // 빌드 설정에 씬 추가
            EditorBuildSettings.scenes = buildScenes.ToArray();
            
            Debug.Log($"빌드 씬 설정 완료: {availableScenes.Count}개 씬");
        }
        
        private static void ConfigureAndroidSettings()
        {
            Debug.Log("Android 설정 구성 중...");
            
            // 빌드 시스템 설정
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            
            // 회사 및 제품 정보
            PlayerSettings.companyName = "NowHere Games";
            PlayerSettings.productName = "NowHere AR MMORPG";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.nowhere.armmorpg");
            
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
            
            // XR 설정 비활성화 (AR 기능이 있다면 필요시 활성화)
            PlayerSettings.SetVirtualRealitySupported(BuildTargetGroup.Android, false);
            
            // 권한 설정 (AR 게임에 필요한 권한들)
            // Unity 6000에서는 다른 방식으로 권한을 설정해야 할 수 있습니다.
            
            // 그래픽 설정
            PlayerSettings.Android.blitType = AndroidBlitType.Never;
            PlayerSettings.Android.startInFullscreen = true;
            PlayerSettings.Android.renderOutsideSafeArea = true;
            
            // 최적화 설정
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.stripUnusedMeshComponents = true;
            
            Debug.Log("Android 설정 완료");
            Debug.Log($"Company: {PlayerSettings.companyName}");
            Debug.Log($"Product: {PlayerSettings.productName}");
            Debug.Log($"Package: {PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)}");
            Debug.Log($"Min SDK: {PlayerSettings.Android.minSdkVersion}");
            Debug.Log($"Target SDK: {PlayerSettings.Android.targetSdkVersion}");
        }
        
        private static void ExecuteBuild()
        {
            Debug.Log("APK 빌드 실행 중...");
            
            // 빌드 경로 설정
            string buildPath = Path.Combine(Application.dataPath, "..", "UnityBuilds");
            string apkName = "NowHere_AR_MMORPG_Complete_v1.0.0.apk";
            string fullPath = Path.Combine(buildPath, apkName);
            
            // 빌드 옵션 설정
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetEnabledScenePaths();
            buildPlayerOptions.locationPathName = fullPath;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;
            
            Debug.Log($"빌드 대상: {buildPlayerOptions.target}");
            Debug.Log($"빌드 경로: {fullPath}");
            Debug.Log($"씬 개수: {buildPlayerOptions.scenes.Length}");
            
            // 빌드 실행
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            // 빌드 결과 처리
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"=== 빌드 성공! ===");
                Debug.Log($"APK 파일: {fullPath}");
                
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
                    EditorUtility.DisplayDialog("빌드 완료", 
                        $"APK 빌드가 성공적으로 완료되었습니다!\n\n파일: {apkName}\n경로: {buildPath}\n크기: {new FileInfo(fullPath).Length / 1024 / 1024} MB", 
                        "확인");
                    
                    // 빌드 폴더 열기
                    EditorUtility.RevealInFinder(buildPath);
                }
            }
            else
            {
                Debug.LogError($"=== 빌드 실패! ===");
                Debug.LogError($"오류: {summary.result}");
                
                // 실패 알림 (배치 모드에서는 다이얼로그 사용 안함)
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("빌드 실패", 
                        $"APK 빌드가 실패했습니다.\n\n오류: {summary.result}\n\nConsole 창에서 자세한 오류를 확인하세요.", 
                        "확인");
                }
            }
        }
        
        private static string[] GetEnabledScenePaths()
        {
            List<string> enabledScenes = new List<string>();
            
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    enabledScenes.Add(scene.path);
                }
            }
            
            return enabledScenes.ToArray();
        }
        
        [MenuItem("Build/Quick Test Build")]
        public static void QuickTestBuild()
        {
            Debug.Log("빠른 테스트 빌드 시작...");
            
            // 현재 씬만 빌드
            string currentScene = EditorSceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(currentScene))
            {
                EditorUtility.DisplayDialog("오류", "현재 열린 씬이 없습니다.", "확인");
                return;
            }
            
            string buildPath = Path.Combine(Application.dataPath, "..", "UnityBuilds");
            string apkName = "NowHere_AR_MMORPG_QuickTest.apk";
            string fullPath = Path.Combine(buildPath, apkName);
            
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }
            
            // Android 설정
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            PlayerSettings.companyName = "NowHere Games";
            PlayerSettings.productName = "NowHere AR MMORPG";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.nowhere.armmorpg");
            
            // 빌드 실행
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new string[] { currentScene };
            buildPlayerOptions.locationPathName = fullPath;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.Development;
            
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("빠른 테스트 빌드 성공!");
                EditorUtility.DisplayDialog("빌드 완료", "빠른 테스트 APK 빌드가 완료되었습니다!", "확인");
                EditorUtility.RevealInFinder(buildPath);
            }
            else
            {
                Debug.LogError("빠른 테스트 빌드 실패!");
                EditorUtility.DisplayDialog("빌드 실패", "빠른 테스트 APK 빌드가 실패했습니다.", "확인");
            }
        }
        
        [MenuItem("Build/Open Build Folder")]
        public static void OpenBuildFolder()
        {
            string buildPath = Path.Combine(Application.dataPath, "..", "UnityBuilds");
            if (Directory.Exists(buildPath))
            {
                EditorUtility.RevealInFinder(buildPath);
            }
            else
            {
                EditorUtility.DisplayDialog("폴더 없음", "빌드 폴더가 존재하지 않습니다.", "확인");
            }
        }
        
        [MenuItem("Build/Clean Build Folder")]
        public static void CleanBuildFolder()
        {
            string buildPath = Path.Combine(Application.dataPath, "..", "UnityBuilds");
            if (Directory.Exists(buildPath))
            {
                if (EditorUtility.DisplayDialog("빌드 폴더 정리", 
                    "빌드 폴더의 모든 파일을 삭제하시겠습니까?", "삭제", "취소"))
                {
                    Directory.Delete(buildPath, true);
                    Debug.Log("빌드 폴더가 정리되었습니다.");
                }
            }
        }
    }
}
