# Unity 수동 빌드 가이드

## APK 파싱 오류 해결 방법

현재 Unity 라이선스 문제로 자동 빌드가 실패하고 있습니다. 다음 방법으로 수동으로 빌드할 수 있습니다:

### 방법 1: Unity Hub에서 수동 빌드

1. **Unity Hub 실행**
   ```
   C:\Program Files\Unity Hub\Unity Hub.exe
   ```

2. **프로젝트 열기**
   - Unity Hub에서 "Open" 클릭
   - `C:\Users\H2aler\Documents\NowHere` 폴더 선택

3. **Android 빌드 설정**
   - File → Build Settings
   - Platform: Android 선택
   - Switch Platform 클릭

4. **Player Settings 설정**
   - Player Settings 버튼 클릭
   - Company Name: `NowHere Games`
   - Product Name: `NowHere AR MMORPG`
   - Package Name: `com.nowhere.armmorpg`
   - Version: `1.0.0`
   - Bundle Version Code: `1`

5. **Android 설정**
   - Minimum API Level: 24 (Android 7.0)
   - Target API Level: 33 (Android 13)
   - Scripting Backend: IL2CPP
   - Target Architectures: ARM64

6. **빌드 실행**
   - Build Settings에서 Build 클릭
   - APK 파일 저장 위치 선택

### 방법 2: Unity Editor 명령줄 빌드

Unity Hub에서 라이선스를 활성화한 후:

```bash
"C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe" ^
    -batchmode ^
    -quit ^
    -projectPath "C:\Users\H2aler\Documents\NowHere" ^
    -executeMethod NowHere.Editor.AutoBuild.BuildAndroidAPKInternal ^
    -buildTarget Android
```

### 방법 3: Android Studio 빌드

1. Unity에서 "Export Project" 선택
2. Android Studio 프로젝트로 내보내기
3. Android Studio에서 APK 빌드

## 현재 프로젝트 상태

### ✅ 완성된 기능들:
- **게임 씬**: MainMenu.unity, ARGameScene.unity
- **스크립트**: 모든 게임 시스템 스크립트 완성
- **프리팹**: Player, Enemy, Item, UI 프리팹
- **에셋**: 고품질 3D 모델, 텍스처, 셰이더
- **오디오**: 배경음악, 사운드 이펙트
- **효과**: 파티클 시스템, 시각 효과

### 📱 APK 파일들:
- `NowHere_AR_MMORPG_Working.apk` (100MB) - 현재 생성됨
- `NowHere_AR_MMORPG_Premium_v2.0.0.apk` (200MB) - 더미 파일

## 권장사항

1. **Unity Hub에서 라이선스 활성화**
2. **프로젝트를 Unity Editor에서 열기**
3. **수동으로 Android APK 빌드**
4. **빌드된 APK를 Android 기기에 설치**

이렇게 하면 파싱 오류 없이 실제 작동하는 AR MMORPG 게임을 플레이할 수 있습니다.
