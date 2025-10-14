# 📱 APK 빌드 가이드

Unity 에디터 없이 APK를 빌드하는 방법들을 제공합니다.

## 🚀 방법 1: 배치 스크립트 사용 (Windows)

### **Windows 배치 파일 실행**
```bash
# 프로젝트 폴더에서 실행
build_apk.bat
```

### **빌드 옵션**
1. **개발 빌드**: 디버깅 및 프로파일링 활성화
2. **테스트 빌드**: 안정성 테스트용
3. **릴리즈 빌드**: 최종 배포용
4. **자동 빌드**: 모든 옵션을 순차적으로 빌드

### **Unity 경로 설정**
스크립트에서 Unity 설치 경로를 확인하고 수정:
```batch
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\2023.2.20f1\Editor\Unity.exe"
```

## 🐧 방법 2: 셸 스크립트 사용 (macOS/Linux)

### **셸 스크립트 실행**
```bash
# 실행 권한 부여
chmod +x build_apk.sh

# 스크립트 실행
./build_apk.sh
```

### **Unity 경로 설정**
스크립트에서 Unity 설치 경로를 확인하고 수정:
```bash
UNITY_PATH="/Applications/Unity/Hub/Editor/2023.2.20f1/Unity.app/Contents/MacOS/Unity"
```

## ⚙️ 방법 3: Unity 명령줄 직접 실행

### **Windows 명령줄**
```cmd
"C:\Program Files\Unity\Hub\Editor\2023.2.20f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\path\to\project" -buildTarget Android -executeMethod NowHere.Editor.BuildScript.BuildTestAPKFromCommandLine
```

### **macOS/Linux 터미널**
```bash
/Applications/Unity/Hub/Editor/2023.2.20f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath "/path/to/project" -buildTarget Android -executeMethod NowHere.Editor.BuildScript.BuildTestAPKFromCommandLine
```

## 🎯 빌드 타입별 명령어

### **테스트 APK 빌드**
```bash
# Windows
Unity.exe -batchmode -quit -projectPath "프로젝트경로" -buildTarget Android -executeMethod NowHere.Editor.BuildScript.BuildTestAPKFromCommandLine

# macOS/Linux
Unity -batchmode -quit -projectPath "프로젝트경로" -buildTarget Android -executeMethod NowHere.Editor.BuildScript.BuildTestAPKFromCommandLine
```

### **개발 APK 빌드**
```bash
# Windows
Unity.exe -batchmode -quit -projectPath "프로젝트경로" -buildTarget Android -executeMethod NowHere.Editor.BuildScript.BuildDevelopmentAPKFromCommandLine

# macOS/Linux
Unity -batchmode -quit -projectPath "프로젝트경로" -buildTarget Android -executeMethod NowHere.Editor.BuildScript.BuildDevelopmentAPKFromCommandLine
```

### **릴리즈 APK 빌드**
```bash
# Windows
Unity.exe -batchmode -quit -projectPath "프로젝트경로" -buildTarget Android -executeMethod NowHere.Editor.BuildScript.BuildAndroidAPKFromCommandLine

# macOS/Linux
Unity -batchmode -quit -projectPath "프로젝트경로" -buildTarget Android -executeMethod NowHere.Editor.BuildScript.BuildAndroidAPKFromCommandLine
```

## 📁 빌드 결과

### **빌드 파일 위치**
```
Builds/
└── Android/
    ├── NowHere_AR_MMORPG_Test.apk
    ├── NowHere_AR_MMORPG_Development.apk
    ├── NowHere_AR_MMORPG_Release.apk
    └── BuildInfo.txt
```

### **빌드 정보 파일**
각 빌드마다 `BuildInfo.txt` 파일이 생성되어 다음 정보를 포함:
- 빌드 시간
- 빌드 타입
- 빌드 결과
- 빌드 크기
- Unity 버전
- 플랫폼 정보

## 📱 APK 설치 및 테스트

### **APK 설치 방법**
1. **파일 전송**: APK 파일을 안드로이드 기기로 복사
2. **권한 설정**: 기기에서 "알 수 없는 소스" 허용
3. **설치**: APK 파일을 탭하여 설치

### **설치 위치별 방법**
- **USB 케이블**: 컴퓨터에서 기기로 직접 복사
- **클라우드**: Google Drive, Dropbox 등 사용
- **이메일**: APK를 이메일로 전송
- **ADB**: `adb install -r app.apk` 명령어 사용

### **권한 설정**
```
설정 > 보안 > 알 수 없는 소스 허용
또는
설정 > 앱 > 특별 액세스 > 알 수 없는 앱 설치 허용
```

## 🧪 테스트 실행

### **앱 실행 후 테스트**
1. **앱 실행**: 설치된 앱을 실행
2. **권한 승인**: 카메라, 마이크, 위치 권한 허용
3. **테스트 패널**: T 키 또는 화면 터치로 테스트 패널 열기
4. **시스템 테스트**: 각 시스템별 테스트 버튼 클릭

### **테스트 시나리오**
- **센서 테스트**: 기기를 움직여서 센서 데이터 확인
- **터치 테스트**: 화면을 터치하여 오브젝트 생성/조작
- **음성 테스트**: 마이크에 말해서 음성 채팅 테스트
- **모션 테스트**: 기기를 흔들어서 회피 동작 확인
- **AR 테스트**: 실제 환경에서 AR 오브젝트 배치

## 🐛 문제 해결

### **빌드 실패 시**
1. **Unity 경로 확인**: Unity가 올바른 경로에 설치되어 있는지 확인
2. **프로젝트 경로 확인**: 프로젝트 경로가 올바른지 확인
3. **권한 확인**: 빌드 폴더에 쓰기 권한이 있는지 확인
4. **로그 확인**: Unity 로그 파일에서 에러 메시지 확인

### **Unity 로그 위치**
- **Windows**: `%LOCALAPPDATA%\Unity\Editor\Editor.log`
- **macOS**: `~/Library/Logs/Unity/Editor.log`
- **Linux**: `~/.config/unity3d/Editor.log`

### **일반적인 에러**
```
에러: Unity를 찾을 수 없습니다
해결: Unity 설치 경로를 스크립트에서 수정

에러: 프로젝트를 열 수 없습니다
해결: 프로젝트 경로가 올바른지 확인

에러: 빌드 실패
해결: Unity 로그 파일에서 상세 에러 확인
```

## 🔧 고급 설정

### **빌드 옵션 커스터마이징**
`Assets/Scripts/Editor/BuildScript.cs` 파일을 수정하여:
- 빌드 씬 목록 변경
- 빌드 옵션 수정
- 안드로이드 설정 변경
- 버전 정보 수정

### **자동화 스크립트**
```bash
# 매일 자동 빌드 (cron job)
0 2 * * * /path/to/build_apk.sh

# CI/CD 파이프라인에 통합
# GitHub Actions, Jenkins 등에서 사용 가능
```

## 📊 빌드 최적화

### **빌드 시간 단축**
- 불필요한 씬 제거
- 에셋 최적화
- 스크립트 컴파일 최적화

### **APK 크기 최적화**
- 텍스처 압축
- 오디오 압축
- 코드 스트리핑
- 에셋 번들 사용

---

**Happy Building! 🚀📱**
