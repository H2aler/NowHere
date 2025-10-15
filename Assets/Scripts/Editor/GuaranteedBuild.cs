using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace NowHere.Editor
{
    /// <summary>
    /// 확실한 빌드 스크립트
    /// 라이선스 문제 없이 반드시 빌드되는 스크립트
    /// </summary>
    public class GuaranteedBuild
    {
        [MenuItem("NowHere/Guaranteed Build/Android APK")]
        public static void BuildAndroidAPK()
        {
            Debug.Log("=== Guaranteed Android Build Start ===");
            
            try
            {
                // 빌드 경로 설정
                string buildPath = Path.Combine(Application.dataPath, "..", "GuaranteedBuilds");
                if (!Directory.Exists(buildPath))
                {
                    Directory.CreateDirectory(buildPath);
                }
                
                string apkName = "NowHere_Guaranteed.apk";
                string fullPath = Path.Combine(buildPath, apkName);
                
                // 기존 APK 파일 삭제
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                
                // 최소한의 씬만 빌드
                string[] scenes = {
                    "Assets/Scenes/MainMenu.unity"
                };
                
                // 씬 존재 확인
                foreach (string scene in scenes)
                {
                    if (!File.Exists(scene))
                    {
                        Debug.LogError($"Scene not found: {scene}");
                        return;
                    }
                }
                
                // 빌드 설정
                EditorBuildSettings.scenes = new EditorBuildSettingsScene[scenes.Length];
                for (int i = 0; i < scenes.Length; i++)
                {
                    EditorBuildSettings.scenes[i] = new EditorBuildSettingsScene(scenes[i], true);
                }
                
                // Android 설정
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                
                // 최소한의 Player Settings
                PlayerSettings.companyName = "NowHere Games";
                PlayerSettings.productName = "NowHere Guaranteed";
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.nowhere.guaranteed");
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                
                // 빌드 옵션 (최소한의 설정)
                BuildOptions buildOptions = BuildOptions.None;
                
                // 빌드 실행
                BuildReport report = BuildPipeline.BuildPlayer(scenes, fullPath, BuildTarget.Android, buildOptions);
                
                if (report.summary.result == BuildResult.Succeeded)
                {
                    Debug.Log($"Guaranteed Build Success! APK: {fullPath}");
                    
                    if (File.Exists(fullPath))
                    {
                        long fileSize = new FileInfo(fullPath).Length;
                        Debug.Log($"Build Size: {fileSize / 1024 / 1024} MB");
                        
                        // 성공 알림
                        if (!Application.isBatchMode)
                        {
                            EditorUtility.DisplayDialog("Guaranteed Build Success", 
                                $"APK 빌드가 성공적으로 완료되었습니다!\n\n파일: {apkName}\n크기: {fileSize / 1024 / 1024} MB\n경로: {buildPath}", 
                                "확인");
                            
                            EditorUtility.RevealInFinder(buildPath);
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Guaranteed Build Failed! Errors: {report.summary.totalErrors}");
                    Debug.LogError($"Warnings: {report.summary.totalWarnings}");
                    
                    // 실패 시 상세 로그
                    foreach (var step in report.steps)
                    {
                        if (step.messages.Length > 0)
                        {
                            foreach (var message in step.messages)
                            {
                                if (message.type == LogType.Error)
                                {
                                    Debug.LogError($"Build Error: {message.content}");
                                }
                                else if (message.type == LogType.Warning)
                                {
                                    Debug.LogWarning($"Build Warning: {message.content}");
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Guaranteed Build Error: {e.Message}");
                Debug.LogError($"Stack Trace: {e.StackTrace}");
            }
            
            Debug.Log("=== Guaranteed Android Build Complete ===");
        }
        
        [MenuItem("NowHere/Guaranteed Build/Test Build")]
        public static void TestBuild()
        {
            Debug.Log("=== Test Build Start ===");
            
            // 빌드 경로 설정
            string buildPath = Path.Combine(Application.dataPath, "..", "TestBuilds");
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }
            
            string apkName = "NowHere_Test.apk";
            string fullPath = Path.Combine(buildPath, apkName);
            
            // 현재 씬만 빌드
            string currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(currentScene))
            {
                Debug.LogError("No active scene found!");
                return;
            }
            
            string[] scenes = { currentScene };
            
            // Android 설정
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            
            // 기본 Player Settings
            PlayerSettings.companyName = "NowHere Games";
            PlayerSettings.productName = "NowHere Test";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.nowhere.test");
            
            // 빌드 실행
            BuildReport report = BuildPipeline.BuildPlayer(scenes, fullPath, BuildTarget.Android, BuildOptions.None);
            
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Test Build Success! APK: {fullPath}");
                
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Test Build Success", 
                        $"테스트 빌드가 성공했습니다!\n\n파일: {apkName}\n경로: {buildPath}", 
                        "확인");
                    
                    EditorUtility.RevealInFinder(buildPath);
                }
            }
            else
            {
                Debug.LogError($"Test Build Failed! Errors: {report.summary.totalErrors}");
            }
            
            Debug.Log("=== Test Build Complete ===");
        }
        
        [MenuItem("NowHere/Guaranteed Build/Check Build Environment")]
        public static void CheckBuildEnvironment()
        {
            Debug.Log("=== Build Environment Check ===");
            
            // Unity 버전
            Debug.Log($"Unity Version: {Application.unityVersion}");
            
            // 플랫폼 설정
            Debug.Log($"Current Platform: {EditorUserBuildSettings.activeBuildTarget}");
            Debug.Log($"Android Min SDK: {PlayerSettings.Android.minSdkVersion}");
            Debug.Log($"Android Target SDK: {PlayerSettings.Android.targetSdkVersion}");
            Debug.Log($"Android Architecture: {PlayerSettings.Android.targetArchitectures}");
            
            // 씬 확인
            var scenes = EditorBuildSettings.scenes;
            Debug.Log($"Build Scenes Count: {scenes.Length}");
            foreach (var scene in scenes)
            {
                Debug.Log($"Scene: {scene.path} (Enabled: {scene.enabled})");
            }
            
            // Player Settings
            Debug.Log($"Company: {PlayerSettings.companyName}");
            Debug.Log($"Product: {PlayerSettings.productName}");
            Debug.Log($"Package: {PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)}");
            
            Debug.Log("=== Build Environment Check Complete ===");
        }
    }
}
