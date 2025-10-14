# GitHub Actions Unity 빌드 설정 가이드

## 🔧 Unity 라이선스 설정

GitHub Actions에서 Unity 빌드를 위해 라이선스 설정이 필요합니다.

### 1. Unity 라이선스 파일 생성
```bash
# Unity 에디터에서 라이선스 파일 생성
# Help > Manage License > Save License
```

### 2. GitHub Secrets 설정
1. GitHub 저장소 → Settings → Secrets and variables → Actions
2. New repository secret 추가:
   - **Name**: `UNITY_LICENSE`
   - **Value**: Unity 라이선스 파일 내용

### 3. 대안: Unity Personal 라이선스 사용
```yaml
- name: Activate Unity License
  uses: game-ci/unity-activate@v2
  with:
    unity-license: ${{ secrets.UNITY_LICENSE }}
    # 또는 Personal 라이선스 사용
    # unity-license: ${{ secrets.UNITY_LICENSE_PERSONAL }}
```

## 🚀 빌드 스크립트 수정

현재 빌드 스크립트에 문제가 있을 수 있습니다. 간단한 빌드 스크립트로 수정:

```csharp
[MenuItem("NowHere/Simple Build")]
public static void SimpleBuild()
{
    string buildPath = Path.Combine(Application.dataPath, "..", "Builds");
    if (!Directory.Exists(buildPath))
        Directory.CreateDirectory(buildPath);
    
    string apkPath = Path.Combine(buildPath, "NowHere.apk");
    
    BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
    buildPlayerOptions.scenes = new[] { "Assets/Scenes/MainMenu.unity" };
    buildPlayerOptions.locationPathName = apkPath;
    buildPlayerOptions.target = BuildTarget.Android;
    buildPlayerOptions.options = BuildOptions.None;
    
    BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
    
    if (report.summary.result == BuildResult.Succeeded)
    {
        Debug.Log("Build succeeded!");
    }
    else
    {
        Debug.LogError("Build failed!");
    }
}
```

## 📱 Android 설정 확인

### 1. Android SDK 설정
```yaml
- name: Setup Android SDK
  uses: android-actions/setup-android@v2
  with:
    api-level: 33
    build-tools: 33.0.0
```

### 2. Unity Android 설정
- Min SDK: API Level 24
- Target SDK: API Level 33
- Architecture: ARM64
- Scripting Backend: IL2CPP

## 🔄 빌드 재시도

설정 완료 후:
1. GitHub Secrets 설정
2. 빌드 스크립트 수정
3. 새로운 커밋 푸시
4. GitHub Actions 재실행

## 📊 빌드 로그 확인

GitHub Actions → 해당 워크플로우 → 로그 확인:
- Unity 라이선스 활성화 로그
- 빌드 스크립트 실행 로그
- Android 빌드 로그
- 오류 메시지 확인
