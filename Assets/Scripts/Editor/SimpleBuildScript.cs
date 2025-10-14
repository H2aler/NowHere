using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace NowHere.Editor
{
    /// <summary>
    /// 간단한 Unity Android APK 빌드 스크립트
    /// Unity 에디터에서 바로 실행 가능
    /// </summary>
    public class SimpleBuildScript
    {
        [MenuItem("Build/Simple Android APK")]
        public static void BuildSimpleAPK()
        {
            Debug.Log("=== Simple Android APK Build Start ===");
            
            // Set build path
            string buildPath = Path.Combine(Application.dataPath, "..", "UnityBuilds");
            string apkName = "NowHere_AR_MMORPG_Simple.apk";
            string fullPath = Path.Combine(buildPath, apkName);
            
            // Create build folder
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
                Debug.Log($"Build folder created: {buildPath}");
            }
            
            // Get current scenes
            string[] scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                scenes[i] = EditorBuildSettings.scenes[i].path;
            }
            
            // Switch to Android build target
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.Log("Switching to Android build target...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            
            // Basic Android settings
            PlayerSettings.companyName = "NowHere Games";
            PlayerSettings.productName = "NowHere AR MMORPG";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.nowhere.armmorpg");
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            // Set build options
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = fullPath;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;
            
            Debug.Log($"Build start: {fullPath}");
            Debug.Log($"Scene count: {scenes.Length}");
            
            // Execute build
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("=== Build Success! ===");
                Debug.Log($"APK file: {fullPath}");
                
                if (File.Exists(fullPath))
                {
                    long fileSize = new FileInfo(fullPath).Length;
                    Debug.Log($"File size: {fileSize / 1024 / 1024} MB");
                }
                
                EditorUtility.DisplayDialog("Build Complete", 
                    $"APK build completed successfully!\n\nFile: {apkName}\nPath: {buildPath}", 
                    "OK");
                
                // Open build folder
                EditorUtility.RevealInFinder(buildPath);
            }
            else
            {
                Debug.LogError("=== Build Failed! ===");
                Debug.LogError($"Error: {summary.result}");
                
                EditorUtility.DisplayDialog("Build Failed", 
                    $"APK build failed.\n\nError: {summary.result}\n\nCheck Console window for detailed error.", 
                    "OK");
            }
            
            Debug.Log("=== Build Complete ===");
        }
        
        [MenuItem("Build/Check Build Settings")]
        public static void CheckBuildSettings()
        {
            Debug.Log("=== Check Build Settings ===");
            Debug.Log($"Current build target: {EditorUserBuildSettings.activeBuildTarget}");
            Debug.Log($"Company name: {PlayerSettings.companyName}");
            Debug.Log($"Product name: {PlayerSettings.productName}");
            Debug.Log($"Package name: {PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)}");
            Debug.Log($"Version: {PlayerSettings.bundleVersion}");
            Debug.Log($"Min SDK: {PlayerSettings.Android.minSdkVersion}");
            Debug.Log($"Target SDK: {PlayerSettings.Android.targetSdkVersion}");
            Debug.Log($"Architecture: {PlayerSettings.Android.targetArchitectures}");
            Debug.Log($"Build scene count: {EditorBuildSettings.scenes.Length}");
            
            foreach (var scene in EditorBuildSettings.scenes)
            {
                Debug.Log($"Scene: {scene.path} (Enabled: {scene.enabled})");
            }
            
            Debug.Log("=== Build Settings Check Complete ===");
        }
    }
}
