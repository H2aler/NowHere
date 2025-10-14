using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace NowHere.Testing
{
    /// <summary>
    /// 테스트용 빌드 매니저
    /// 개발 및 테스트를 위한 APK 빌드를 자동화
    /// </summary>
    public class TestBuildManager : MonoBehaviour
    {
        [Header("Test Build Settings")]
        [SerializeField] private string testBuildPath = "Builds/Test";
        [SerializeField] private string testApkName = "NowHere_Test";
        [SerializeField] private bool enableDevelopmentBuild = true;
        [SerializeField] private bool enableScriptDebugging = true;
        [SerializeField] private bool enableProfiler = true;
        
        [Header("Test Scenes")]
        [SerializeField] private string[] testScenes = {
            "Assets/Scenes/TestScene.unity"
        };
        
        [ContextMenu("Build Test APK")]
        public static void BuildTestAPK()
        {
            BuildTest(BuildTarget.Android, false);
        }
        
        [ContextMenu("Build Test AAB")]
        public static void BuildTestAAB()
        {
            BuildTest(BuildTarget.Android, true);
        }
        
        [ContextMenu("Build Development APK")]
        public static void BuildDevelopmentAPK()
        {
            BuildTest(BuildTarget.Android, false, true);
        }
        
        private static void BuildTest(BuildTarget target, bool buildAppBundle, bool isDevelopment = false)
        {
            // 빌드 설정
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            
            // 씬 설정
            buildPlayerOptions.scenes = new string[] {
                "Assets/Scenes/TestScene.unity"
            };
            
            // 빌드 경로 설정
            string buildPath = GetBuildPath(target, buildAppBundle, isDevelopment);
            buildPlayerOptions.locationPathName = buildPath;
            buildPlayerOptions.target = target;
            
            // 빌드 옵션 설정
            BuildOptions buildOptions = BuildOptions.None;
            
            if (isDevelopment)
            {
                buildOptions |= BuildOptions.Development;
                buildOptions |= BuildOptions.AllowDebugging;
                buildOptions |= BuildOptions.ConnectWithProfiler;
            }
            
            buildOptions |= BuildOptions.CompressWithLz4;
            
            buildPlayerOptions.options = buildOptions;
            
            // 안드로이드 설정
            if (target == BuildTarget.Android)
            {
                SetupAndroidSettings(isDevelopment);
            }
            
            // 빌드 실행
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            // 빌드 결과 출력
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"테스트 빌드 성공: {summary.totalSize} bytes");
                Debug.Log($"빌드 경로: {buildPath}");
                
                // 빌드 정보 저장
                SaveBuildInfo(summary, buildPath, isDevelopment);
            }
            else
            {
                Debug.LogError($"테스트 빌드 실패: {summary.result}");
                
                // 에러 로그 출력
                foreach (var step in report.steps)
                {
                    foreach (var message in step.messages)
                    {
                        if (message.type == LogType.Error)
                        {
                            Debug.LogError($"빌드 에러: {message.content}");
                        }
                    }
                }
            }
        }
        
        private static string GetBuildPath(BuildTarget target, bool buildAppBundle, bool isDevelopment)
        {
            string fileName = "NowHere_Test";
            
            if (isDevelopment)
            {
                fileName += "_Dev";
            }
            
            if (buildAppBundle)
            {
                fileName += ".aab";
            }
            else
            {
                fileName += ".apk";
            }
            
            string buildPath = Path.Combine("Builds", "Test", fileName);
            
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
                PlayerSettings.bundleVersion = "0.1.0-test";
                PlayerSettings.Android.bundleVersionCode = 2;
            }
            
            // 개발 빌드 설정
            if (isDevelopment)
            {
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.stripEngineCode = false;
            }
        }
        
        private static void SaveBuildInfo(BuildSummary summary, string buildPath, bool isDevelopment)
        {
            string buildInfoPath = Path.Combine(Path.GetDirectoryName(buildPath), "BuildInfo.txt");
            
            string buildInfo = $"=== 빌드 정보 ===\n";
            buildInfo += $"빌드 시간: {System.DateTime.Now}\n";
            buildInfo += $"빌드 타입: {(isDevelopment ? "개발" : "테스트")}\n";
            buildInfo += $"빌드 결과: {summary.result}\n";
            buildInfo += $"빌드 시간: {summary.totalTime}\n";
            buildInfo += $"빌드 크기: {summary.totalSize} bytes\n";
            buildInfo += $"빌드 경로: {buildPath}\n";
            buildInfo += $"Unity 버전: {Application.unityVersion}\n";
            buildInfo += $"플랫폼: {summary.platform}\n";
            
            File.WriteAllText(buildInfoPath, buildInfo);
            Debug.Log($"빌드 정보 저장됨: {buildInfoPath}");
        }
        
        [ContextMenu("Clean Test Builds")]
        public static void CleanTestBuilds()
        {
            string testBuildDir = Path.Combine("Builds", "Test");
            
            if (Directory.Exists(testBuildDir))
            {
                Directory.Delete(testBuildDir, true);
                Debug.Log("테스트 빌드 폴더가 정리되었습니다.");
            }
        }
        
        [ContextMenu("Validate Test Build")]
        public static void ValidateTestBuild()
        {
            Debug.Log("=== 테스트 빌드 유효성 검사 ===");
            
            bool isValid = true;
            
            // 씬 파일 확인
            string testScenePath = "Assets/Scenes/TestScene.unity";
            if (!File.Exists(testScenePath))
            {
                Debug.LogError($"테스트 씬 파일이 없습니다: {testScenePath}");
                isValid = false;
            }
            
            // 스크립트 컴파일 확인
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("스크립트가 컴파일 중입니다. 완료 후 다시 시도하세요.");
                isValid = false;
            }
            
            // 안드로이드 SDK 확인
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.LogWarning("안드로이드 빌드 타겟이 설정되지 않았습니다.");
            }
            
            // 결과 출력
            if (isValid)
            {
                Debug.Log("테스트 빌드 유효성 검사 통과");
            }
            else
            {
                Debug.LogError("테스트 빌드 유효성 검사 실패");
            }
        }
        
        [ContextMenu("Install Test APK")]
        public static void InstallTestAPK()
        {
            string apkPath = Path.Combine("Builds", "Test", "NowHere_Test.apk");
            
            if (!File.Exists(apkPath))
            {
                Debug.LogError($"APK 파일을 찾을 수 없습니다: {apkPath}");
                return;
            }
            
            // ADB를 통한 설치
            string adbCommand = $"adb install -r \"{apkPath}\"";
            
            Debug.Log($"APK 설치 명령: {adbCommand}");
            Debug.Log("수동으로 ADB 명령을 실행하거나 Unity에서 직접 설치하세요.");
        }
        
        [ContextMenu("Run Tests on Device")]
        public static void RunTestsOnDevice()
        {
            Debug.Log("=== 디바이스 테스트 실행 ===");
            Debug.Log("1. APK를 안드로이드 기기에 설치하세요");
            Debug.Log("2. 앱을 실행하세요");
            Debug.Log("3. 테스트 패널을 열어서 (T 키) 각 시스템을 테스트하세요");
            Debug.Log("4. 콘솔 로그를 확인하여 테스트 결과를 확인하세요");
        }
    }
}
