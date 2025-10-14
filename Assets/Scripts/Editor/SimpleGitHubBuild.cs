using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace NowHere.Editor
{
    /// <summary>
    /// GitHub Actions용 간단한 빌드 스크립트
    /// Unity 라이선스 문제를 해결하기 위한 최소한의 빌드
    /// </summary>
    public class SimpleGitHubBuild
    {
        [MenuItem("NowHere/GitHub Build/Simple Android Build")]
        public static void BuildAndroidAPK()
        {
            Debug.Log("=== Simple GitHub Build Start ===");
            
            try
            {
                // 빌드 경로 설정
                string buildPath = Path.Combine(Application.dataPath, "..", "Builds");
                if (!Directory.Exists(buildPath))
                {
                    Directory.CreateDirectory(buildPath);
                }
                
                string apkName = "NowHere_Simple.apk";
                string fullPath = Path.Combine(buildPath, apkName);
                
                // 기존 APK 파일 삭제
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                
                // 씬 목록 설정 (최소한의 씬만)
                string[] scenes = {
                    "Assets/Scenes/MainMenu.unity"
                };
                
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
                PlayerSettings.productName = "NowHere Simple";
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.nowhere.simple");
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                
                // 빌드 실행
                BuildReport report = BuildPipeline.BuildPlayer(scenes, fullPath, BuildTarget.Android, BuildOptions.None);
                
                if (report.summary.result == BuildResult.Succeeded)
                {
                    Debug.Log($"Simple Build Success! APK: {fullPath}");
                    Debug.Log($"Build Size: {new FileInfo(fullPath).Length / 1024 / 1024} MB");
                    
                    // 성공 알림
                    if (!Application.isBatchMode)
                    {
                        EditorUtility.DisplayDialog("Simple Build Success", 
                            $"APK 빌드가 성공적으로 완료되었습니다!\n\n파일: {apkName}\n경로: {buildPath}", 
                            "확인");
                        
                        EditorUtility.RevealInFinder(buildPath);
                    }
                }
                else
                {
                    Debug.LogError($"Simple Build Failed! Errors: {report.summary.totalErrors}");
                    Debug.LogError($"Warnings: {report.summary.totalWarnings}");
                    
                    if (!Application.isBatchMode)
                    {
                        EditorUtility.DisplayDialog("Simple Build Failed", 
                            $"빌드가 실패했습니다.\n\n오류 수: {report.summary.totalErrors}\n경고 수: {report.summary.totalWarnings}", 
                            "확인");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Simple Build Error: {e.Message}");
                Debug.LogError($"Stack Trace: {e.StackTrace}");
                
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Simple Build Error", 
                        $"빌드 중 오류가 발생했습니다:\n{e.Message}", 
                        "확인");
                }
            }
            
            Debug.Log("=== Simple GitHub Build Complete ===");
        }
        
        [MenuItem("NowHere/GitHub Build/Test Build")]
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
            string[] scenes = { UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path };
            
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
            }
            else
            {
                Debug.LogError($"Test Build Failed! Errors: {report.summary.totalErrors}");
            }
            
            Debug.Log("=== Test Build Complete ===");
        }
    }
}
