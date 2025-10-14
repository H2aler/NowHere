using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Linq;

namespace NowHere.Editor
{
    /// <summary>
    /// Unity 자동 빌드 스크립트
    /// 명령줄에서 Unity Editor를 실행하여 자동으로 APK를 빌드
    /// </summary>
    public class AutoBuild
    {
        [MenuItem("Build/Auto Build Android APK")]
        public static void BuildAndroidAPK()
        {
            BuildAndroidAPKInternal();
        }
        
        public static void BuildAndroidAPKInternal()
        {
            Debug.Log("=== Unity 자동 빌드 시작 ===");
            
            // 빌드 설정
            string buildPath = Path.Combine(Application.dataPath, "..", "UnityBuilds");
            string apkName = "NowHere_AR_MMORPG_Unity_v0.1.0.apk";
            string fullPath = Path.Combine(buildPath, apkName);
            
            // 빌드 폴더 생성
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
                Debug.Log($"빌드 폴더 생성: {buildPath}");
            }
            
            // 완전한 게임 씬들 설정
            string[] scenes = {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/ARGameScene.unity"
            };
            
            // 빌드 설정에 씬 추가
            System.Collections.Generic.List<EditorBuildSettingsScene> buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (string scenePath in scenes)
            {
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }
            EditorBuildSettings.scenes = buildScenes.ToArray();
            
            Debug.Log($"게임 씬 설정 완료: {scenes.Length}개 씬");
            
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
            
            Debug.Log($"빌드 대상: {buildPlayerOptions.target}");
            Debug.Log($"빌드 경로: {fullPath}");
            Debug.Log($"씬 개수: {scenes.Length}");
            
            // Android 설정
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            
            // Player Settings 설정
            PlayerSettings.companyName = "NowHere Games";
            PlayerSettings.productName = "NowHere AR MMORPG";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.nowhere.armmorpg");
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            
            // Android 설정
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            // XR 설정 비활성화 (OpenXR 문제 해결)
            PlayerSettings.SetVirtualRealitySupported(BuildTargetGroup.Android, false);
            
            // OpenXR 설정 완전 비활성화 (Unity 6000에서는 다른 방식 사용)
            // EditorUserBuildSettings.enableOpenXRSupport = false;
            
            // 권한 설정 (Unity 6000에서는 다른 방식 사용)
            // PlayerSettings.Android.usesPermission.Add("android.permission.CAMERA");
            // PlayerSettings.Android.usesPermission.Add("android.permission.RECORD_AUDIO");
            // PlayerSettings.Android.usesPermission.Add("android.permission.ACCESS_FINE_LOCATION");
            // PlayerSettings.Android.usesPermission.Add("android.permission.INTERNET");
            
            Debug.Log("Android 설정 완료");
            Debug.Log($"Company: {PlayerSettings.companyName}");
            Debug.Log($"Product: {PlayerSettings.productName}");
            Debug.Log($"Package: {PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)}");
            Debug.Log($"Min SDK: {PlayerSettings.Android.minSdkVersion}");
            Debug.Log($"Target SDK: {PlayerSettings.Android.targetSdkVersion}");
            
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
                    $"APK 빌드가 성공적으로 완료되었습니다!\n\n파일: {apkName}\n경로: {buildPath}", 
                    "확인");
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
