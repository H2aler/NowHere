using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace NowHere.Editor
{
    /// <summary>
    /// Unity 에디터 없이 직접 빌드하는 간단한 스크립트
    /// </summary>
    public class SimpleDirectBuild
    {
        [MenuItem("NowHere/Direct Build APK")]
        public static void BuildAPK()
        {
            Debug.Log("=== Direct APK Build Start ===");
            
            try
            {
                // 빌드 경로 설정
                string buildPath = Path.Combine(Application.dataPath, "..", "DirectBuilds");
                if (!Directory.Exists(buildPath))
                {
                    Directory.CreateDirectory(buildPath);
                }
                
                string apkName = "NowHere_Direct_Build.apk";
                string fullPath = Path.Combine(buildPath, apkName);
                
                // 기존 APK 파일 삭제
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                
                // 씬 목록 설정
                string[] scenes = {
                    "Assets/Scenes/MainMenu.unity",
                    "Assets/Scenes/ARGameScene.unity",
                    "Assets/Scenes/TestScene.unity"
                };
                
                // 빌드 설정
                EditorBuildSettings.scenes = new EditorBuildSettingsScene[scenes.Length];
                for (int i = 0; i < scenes.Length; i++)
                {
                    EditorBuildSettings.scenes[i] = new EditorBuildSettingsScene(scenes[i], true);
                }
                
                // Android 설정
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                
                // Player Settings
                PlayerSettings.companyName = "NowHere Games";
                PlayerSettings.productName = "NowHere Direct Build";
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.nowhere.direct.build");
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                
                // 빌드 실행
                BuildReport report = BuildPipeline.BuildPlayer(scenes, fullPath, BuildTarget.Android, BuildOptions.None);
                
                if (report.summary.result == BuildResult.Succeeded)
                {
                    Debug.Log($"Direct Build Success! APK: {fullPath}");
                    Debug.Log($"Build Size: {new FileInfo(fullPath).Length / 1024 / 1024} MB");
                    
                    // 성공 알림
                    if (!Application.isBatchMode)
                    {
                        EditorUtility.DisplayDialog("Direct Build Success", 
                            $"APK 빌드가 성공적으로 완료되었습니다!\n\n파일: {apkName}\n경로: {buildPath}", 
                            "확인");
                        
                        EditorUtility.RevealInFinder(buildPath);
                    }
                }
                else
                {
                    Debug.LogError($"Direct Build Failed! Errors: {report.summary.totalErrors}");
                    
                    if (!Application.isBatchMode)
                    {
                        EditorUtility.DisplayDialog("Direct Build Failed", 
                            $"빌드가 실패했습니다.\n\n오류 수: {report.summary.totalErrors}\n경고 수: {report.summary.totalWarnings}", 
                            "확인");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Direct Build Error: {e.Message}");
                
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Direct Build Error", 
                        $"빌드 중 오류가 발생했습니다:\n{e.Message}", 
                        "확인");
                }
            }
            
            Debug.Log("=== Direct APK Build Complete ===");
        }
        
        [MenuItem("NowHere/Quick Direct Build")]
        public static void QuickBuild()
        {
            Debug.Log("=== Quick Direct Build Start ===");
            
            // 최소한의 설정으로 빠른 빌드
            string buildPath = Path.Combine(Application.dataPath, "..", "DirectBuilds");
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }
            
            string apkName = "NowHere_Quick_Build.apk";
            string fullPath = Path.Combine(buildPath, apkName);
            
            // 현재 씬만 빌드
            string[] scenes = { UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path };
            
            // Android 설정
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            
            // 기본 Player Settings
            PlayerSettings.companyName = "NowHere Games";
            PlayerSettings.productName = "NowHere Quick Build";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.nowhere.quick.build");
            
            // 빌드 실행
            BuildReport report = BuildPipeline.BuildPlayer(scenes, fullPath, BuildTarget.Android, BuildOptions.None);
            
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Quick Build Success! APK: {fullPath}");
            }
            else
            {
                Debug.LogError($"Quick Build Failed! Errors: {report.summary.totalErrors}");
            }
            
            Debug.Log("=== Quick Direct Build Complete ===");
        }
    }
}
