using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace NowHere.Utils
{
    /// <summary>
    /// 빌드 관리를 담당하는 클래스
    /// 안드로이드 APK 빌드 및 배포를 자동화
    /// </summary>
    public class BuildManager : MonoBehaviour
    {
        [Header("Build Settings")]
        [SerializeField] private string buildPath = "Builds/Android";
        [SerializeField] private string apkName = "NowHere_AR_MMORPG";
        [SerializeField] private BuildTarget buildTarget = BuildTarget.Android;
        
        [Header("Android Settings")]
        [SerializeField] private AndroidSdkVersions minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        [SerializeField] private AndroidSdkVersions targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
        [SerializeField] private AndroidArchitecture targetArchitecture = AndroidArchitecture.ARM64;
        
        [Header("Build Options")]
        [SerializeField] private bool developmentBuild = false;
        [SerializeField] private bool allowDebugging = false;
        [SerializeField] private bool compressFilesInPackage = true;
        [SerializeField] private bool createSymbolsZip = false;
        
        [Header("Version Info")]
        [SerializeField] private string version = "1.0.0";
        [SerializeField] private int buildNumber = 1;
        
        private void Start()
        {
            // 빌드 설정 초기화
            InitializeBuildSettings();
        }
        
        private void InitializeBuildSettings()
        {
            // 안드로이드 빌드 설정
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            
            // 플레이어 설정
            PlayerSettings.Android.minSdkVersion = minSdkVersion;
            PlayerSettings.Android.targetSdkVersion = targetSdkVersion;
            PlayerSettings.Android.targetArchitectures = targetArchitecture;
            
            // 버전 정보 설정
            PlayerSettings.bundleVersion = version;
            PlayerSettings.Android.bundleVersionCode = buildNumber;
            
            // 아이콘 및 스플래시 설정
            SetupAndroidIcons();
            SetupAndroidSplash();
            
            Debug.Log("빌드 설정이 초기화되었습니다.");
        }
        
        private void SetupAndroidIcons()
        {
            // 안드로이드 아이콘 설정
            // 실제로는 아이콘 텍스처를 할당해야 함
            // PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);
        }
        
        private void SetupAndroidSplash()
        {
            // 안드로이드 스플래시 화면 설정
            // PlayerSettings.Android.splashScreen = splashScreen;
        }
        
        [ContextMenu("Build APK")]
        public void BuildAPK()
        {
            BuildGame(BuildTarget.Android, false);
        }
        
        [ContextMenu("Build AAB")]
        public void BuildAAB()
        {
            BuildGame(BuildTarget.Android, true);
        }
        
        [ContextMenu("Build Development")]
        public void BuildDevelopment()
        {
            developmentBuild = true;
            allowDebugging = true;
            BuildGame(BuildTarget.Android, false);
        }
        
        private void BuildGame(BuildTarget target, bool buildAppBundle)
        {
            // 빌드 전 준비
            PrepareBuild();
            
            // 빌드 옵션 설정
            BuildOptions buildOptions = BuildOptions.None;
            
            if (developmentBuild)
                buildOptions |= BuildOptions.Development;
            
            if (allowDebugging)
                buildOptions |= BuildOptions.AllowDebugging;
            
            if (compressFilesInPackage)
                buildOptions |= BuildOptions.CompressWithLz4;
            
            // Unity 6000에서는 CreateSymbolsZip이 제거됨
            // if (createSymbolsZip)
            //     buildOptions |= BuildOptions.CreateSymbolsZip;
            
            // 빌드 경로 설정
            string buildPath = GetBuildPath(target, buildAppBundle);
            
            // 씬 목록 가져오기
            string[] scenes = GetBuildScenes();
            
            // 빌드 실행
            BuildPipeline.BuildPlayer(scenes, buildPath, target, buildOptions);
            
            // 빌드 완료 후 처리
            PostBuildProcess(buildPath);
        }
        
        private void PrepareBuild()
        {
            // 빌드 전 준비 작업
            Debug.Log("빌드 준비 중...");
            
            // 씬 저장
            EditorSceneManager.SaveOpenScenes();
            
            // 에셋 정리
            AssetDatabase.Refresh();
            
            // 빌드 번호 증가
            buildNumber++;
            PlayerSettings.Android.bundleVersionCode = buildNumber;
            
            Debug.Log($"빌드 번호: {buildNumber}");
        }
        
        private string GetBuildPath(BuildTarget target, bool buildAppBundle)
        {
            string fileName = apkName;
            
            if (buildAppBundle)
            {
                fileName += ".aab";
            }
            else
            {
                fileName += ".apk";
            }
            
            if (developmentBuild)
            {
                fileName = fileName.Replace(".apk", "_Dev.apk");
                fileName = fileName.Replace(".aab", "_Dev.aab");
            }
            
            string fullPath = Path.Combine(buildPath, fileName);
            
            // 디렉토리 생성
            Directory.CreateDirectory(buildPath);
            
            return fullPath;
        }
        
        private string[] GetBuildScenes()
        {
            List<string> scenes = new List<string>();
            
            // 빌드에 포함할 씬들
            scenes.Add("Assets/Scenes/MainMenu.unity");
            scenes.Add("Assets/Scenes/GameWorld.unity");
            scenes.Add("Assets/Scenes/ARWorld.unity");
            
            return scenes.ToArray();
        }
        
        private void PostBuildProcess(string buildPath)
        {
            Debug.Log($"빌드 완료: {buildPath}");
            
            // 빌드 정보 출력
            FileInfo buildFile = new FileInfo(buildPath);
            if (buildFile.Exists)
            {
                Debug.Log($"빌드 파일 크기: {buildFile.Length / (1024 * 1024)} MB");
            }
            
            // 빌드 후 처리
            if (developmentBuild)
            {
                Debug.Log("개발 빌드가 완료되었습니다.");
            }
            else
            {
                Debug.Log("릴리즈 빌드가 완료되었습니다.");
            }
        }
        
        [ContextMenu("Clean Build")]
        public void CleanBuild()
        {
            // 빌드 폴더 정리
            if (Directory.Exists(buildPath))
            {
                Directory.Delete(buildPath, true);
                Debug.Log("빌드 폴더가 정리되었습니다.");
            }
            
            // 에셋 정리
            AssetDatabase.Refresh();
        }
        
        [ContextMenu("Validate Build")]
        public void ValidateBuild()
        {
            // 빌드 유효성 검사
            bool isValid = true;
            List<string> errors = new List<string>();
            
            // 씬 검사
            string[] scenes = GetBuildScenes();
            foreach (string scene in scenes)
            {
                if (!File.Exists(scene))
                {
                    errors.Add($"씬 파일이 존재하지 않습니다: {scene}");
                    isValid = false;
                }
            }
            
            // 설정 검사
            if (string.IsNullOrEmpty(PlayerSettings.productName))
            {
                errors.Add("제품 이름이 설정되지 않았습니다.");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(PlayerSettings.applicationIdentifier))
            {
                errors.Add("번들 식별자가 설정되지 않았습니다.");
                isValid = false;
            }
            
            // 결과 출력
            if (isValid)
            {
                Debug.Log("빌드 유효성 검사 통과");
            }
            else
            {
                Debug.LogError("빌드 유효성 검사 실패:");
                foreach (string error in errors)
                {
                    Debug.LogError($"- {error}");
                }
            }
        }
        
        public void SetVersion(string newVersion)
        {
            version = newVersion;
            PlayerSettings.bundleVersion = version;
        }
        
        public void SetBuildNumber(int newBuildNumber)
        {
            buildNumber = newBuildNumber;
            PlayerSettings.Android.bundleVersionCode = buildNumber;
        }
        
        public string GetVersion()
        {
            return version;
        }
        
        public int GetBuildNumber()
        {
            return buildNumber;
        }
    }
}
