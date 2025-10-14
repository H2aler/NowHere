using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace NowHere.Editor
{
    /// <summary>
    /// Unity Editor에서 자동으로 APK를 빌드하는 스크립트
    /// </summary>
    public class UnityAutoBuild
    {
        [MenuItem("Build/Auto Build APK Now")]
        public static void BuildAPKNow()
        {
            Debug.Log("=== Unity 자동 빌드 시작 ===");
            
            // 빌드 설정
            string buildPath = Path.Combine(Application.dataPath, "..", "UnityBuilds");
            string apkName = "NowHere_AR_MMORPG_Final.apk";
            string fullPath = Path.Combine(buildPath, apkName);
            
            // 빌드 폴더 생성
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
                Debug.Log($"빌드 폴더 생성: {buildPath}");
            }
            
            // 씬 목록 설정 (실제 존재하는 씬들)
            string[] scenes = {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/ARGameScene.unity",
                "Assets/Scenes/TestScene.unity"
            };
            
            // 빌드 설정에 씬 추가
            var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (string scenePath in scenes)
            {
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }
            EditorBuildSettings.scenes = buildScenes.ToArray();
            
            // 빌드 설정
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = fullPath;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;
            
            // Android 빌드 타겟 확인 및 설정
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.Log("Android 빌드 타겟으로 전환 중...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            
            // Android 설정
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            
            // Player Settings 설정
            PlayerSettings.companyName = "NowHere Games";
            PlayerSettings.productName = "NowHere AR MMORPG";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.nowhere.armmorpg");
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            
            // Android 설정
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            // XR 설정 비활성화
            PlayerSettings.SetVirtualRealitySupported(BuildTargetGroup.Android, false);
            
            Debug.Log("Android 설정 완료");
            Debug.Log($"Company: {PlayerSettings.companyName}");
            Debug.Log($"Product: {PlayerSettings.productName}");
            Debug.Log($"Package: {PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)}");
            
            // 빌드 실행
            Debug.Log("=== APK 빌드 시작 ===");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"=== 빌드 성공! ===");
                Debug.Log($"APK 파일: {fullPath}");
                Debug.Log($"파일 크기: {new FileInfo(fullPath).Length / 1024 / 1024} MB");
                Debug.Log($"빌드 시간: {summary.totalTime}");
                
                // 빌드 성공 알림
                EditorUtility.DisplayDialog("빌드 완료", 
                    $"APK 빌드가 성공적으로 완료되었습니다!\n\n파일: {apkName}\n경로: {buildPath}\n크기: {new FileInfo(fullPath).Length / 1024 / 1024} MB", 
                    "확인");
                
                // 빌드 폴더 열기
                EditorUtility.RevealInFinder(buildPath);
            }
            else
            {
                Debug.LogError($"=== 빌드 실패! ===");
                Debug.LogError($"오류: {summary.result}");
                
                // 빌드 실패 알림
                EditorUtility.DisplayDialog("빌드 실패", 
                    $"APK 빌드가 실패했습니다.\n\n오류: {summary.result}\n\nConsole 창에서 자세한 오류를 확인하세요.", 
                    "확인");
            }
            
            Debug.Log("=== Unity 자동 빌드 완료 ===");
        }
        
        [MenuItem("Build/Quick Build APK")]
        public static void QuickBuildAPK()
        {
            Debug.Log("빠른 APK 빌드 시작...");
            
            // 현재 씬만 빌드
            string[] scenes = { EditorSceneManager.GetActiveScene().path };
            
            string buildPath = Path.Combine(Application.dataPath, "..", "UnityBuilds");
            string apkName = "NowHere_AR_MMORPG_Quick.apk";
            string fullPath = Path.Combine(buildPath, apkName);
            
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }
            
            // 빠른 빌드 설정
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = fullPath;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.Development;
            
            // Android 설정
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            PlayerSettings.companyName = "NowHere Games";
            PlayerSettings.productName = "NowHere AR MMORPG";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.nowhere.armmorpg");
            
            // 빌드 실행
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("빠른 빌드 성공!");
                EditorUtility.DisplayDialog("빌드 완료", "빠른 APK 빌드가 완료되었습니다!", "확인");
                EditorUtility.RevealInFinder(buildPath);
            }
            else
            {
                Debug.LogError("빠른 빌드 실패!");
                EditorUtility.DisplayDialog("빌드 실패", "빠른 APK 빌드가 실패했습니다.", "확인");
            }
        }
    }
}
