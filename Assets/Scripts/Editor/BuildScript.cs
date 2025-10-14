using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace NowHere.Editor
{
    /// <summary>
    /// Unity 에디터에서 실행할 수 있는 빌드 스크립트
    /// 명령줄에서도 실행 가능
    /// </summary>
    public class BuildScript
    {
        [MenuItem("Build/Android APK")]
        public static void BuildAndroidAPK()
        {
            BuildAndroid(false);
        }
        
        [MenuItem("Build/Android AAB")]
        public static void BuildAndroidAAB()
        {
            BuildAndroid(true);
        }
        
        [MenuItem("Build/Test APK")]
        public static void BuildTestAPK()
        {
            BuildTest(false);
        }
        
        [MenuItem("Build/Development APK")]
        public static void BuildDevelopmentAPK()
        {
            BuildDevelopment(false);
        }
        
        [MenuItem("Build/All Builds")]
        public static void BuildAll()
        {
            Debug.Log("=== 모든 빌드 시작 ===");
            
            // 개발 빌드
            Debug.Log("개발 빌드 시작...");
            BuildDevelopment(false);
            
            // 테스트 빌드
            Debug.Log("테스트 빌드 시작...");
            BuildTest(false);
            
            // 릴리즈 빌드
            Debug.Log("릴리즈 빌드 시작...");
            BuildAndroid(false);
            
            Debug.Log("=== 모든 빌드 완료 ===");
        }
        
        private static void BuildAndroid(bool buildAppBundle)
        {
            // 빌드 설정
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            
            // 씬 설정
            buildPlayerOptions.scenes = GetBuildScenes();
            
            // 빌드 경로 설정
            string buildPath = GetBuildPath("Release", buildAppBundle);
            buildPlayerOptions.locationPathName = buildPath;
            buildPlayerOptions.target = BuildTarget.Android;
            
            // 빌드 옵션
            buildPlayerOptions.options = BuildOptions.None;
            
            // 안드로이드 설정
            SetupAndroidSettings(false);
            
            // 빌드 실행
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"릴리즈 빌드 성공: {summary.totalSize} bytes");
                Debug.Log($"빌드 경로: {buildPath}");
            }
            else
            {
                Debug.LogError($"릴리즈 빌드 실패: {summary.result}");
            }
        }
        
        private static void BuildTest(bool buildAppBundle)
        {
            // 빌드 설정
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            
            // 테스트 씬만 포함
            buildPlayerOptions.scenes = new string[] {
                "Assets/Scenes/TestScene.unity"
            };
            
            // 빌드 경로 설정
            string buildPath = GetBuildPath("Test", buildAppBundle);
            buildPlayerOptions.locationPathName = buildPath;
            buildPlayerOptions.target = BuildTarget.Android;
            
            // 빌드 옵션
            buildPlayerOptions.options = BuildOptions.CompressWithLz4;
            
            // 안드로이드 설정
            SetupAndroidSettings(false);
            
            // 빌드 실행
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"테스트 빌드 성공: {summary.totalSize} bytes");
                Debug.Log($"빌드 경로: {buildPath}");
            }
            else
            {
                Debug.LogError($"테스트 빌드 실패: {summary.result}");
            }
        }
        
        private static void BuildDevelopment(bool buildAppBundle)
        {
            // 빌드 설정
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            
            // 씬 설정
            buildPlayerOptions.scenes = GetBuildScenes();
            
            // 빌드 경로 설정
            string buildPath = GetBuildPath("Development", buildAppBundle);
            buildPlayerOptions.locationPathName = buildPath;
            buildPlayerOptions.target = BuildTarget.Android;
            
            // 개발 빌드 옵션
            buildPlayerOptions.options = BuildOptions.Development | 
                                       BuildOptions.AllowDebugging | 
                                       BuildOptions.ConnectWithProfiler;
            
            // 안드로이드 설정
            SetupAndroidSettings(true);
            
            // 빌드 실행
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"개발 빌드 성공: {summary.totalSize} bytes");
                Debug.Log($"빌드 경로: {buildPath}");
            }
            else
            {
                Debug.LogError($"개발 빌드 실패: {summary.result}");
            }
        }
        
        private static string[] GetBuildScenes()
        {
            return new string[] {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/GameWorld.unity",
                "Assets/Scenes/ARWorld.unity",
                "Assets/Scenes/TestScene.unity"
            };
        }
        
        private static string GetBuildPath(string buildType, bool buildAppBundle)
        {
            string fileName = $"NowHere_AR_MMORPG_{buildType}";
            
            if (buildAppBundle)
            {
                fileName += ".aab";
            }
            else
            {
                fileName += ".apk";
            }
            
            string buildPath = Path.Combine("Builds", "Android", fileName);
            
            // 디렉토리 생성
            Directory.CreateDirectory(Path.GetDirectoryName(buildPath));
            
            return buildPath;
        }
        
        private static void SetupAndroidSettings(bool isDevelopment)
        {
            // 안드로이드 빌드 설정
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            
            // 플레이어 설정
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            // 버전 정보
            if (isDevelopment)
            {
                PlayerSettings.bundleVersion = "0.1.0-dev";
                PlayerSettings.Android.bundleVersionCode = 1;
            }
            else
            {
                PlayerSettings.bundleVersion = "0.1.0";
                PlayerSettings.Android.bundleVersionCode = 1;
            }
            
            // 개발 빌드 설정
            if (isDevelopment)
            {
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.stripEngineCode = false;
            }
        }
        
        // 명령줄에서 실행할 수 있는 메서드들
        public static void BuildAndroidAPKFromCommandLine()
        {
            BuildAndroid(false);
        }
        
        public static void BuildAndroidAABFromCommandLine()
        {
            BuildAndroid(true);
        }
        
        public static void BuildTestAPKFromCommandLine()
        {
            BuildTest(false);
        }
        
        public static void BuildDevelopmentAPKFromCommandLine()
        {
            BuildDevelopment(false);
        }
    }
}
