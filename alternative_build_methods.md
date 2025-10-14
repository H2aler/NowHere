# Unity 에디터 없이 빌드하는 방법들

## 🚀 **대안 빌드 방법들**

### **1. Unity Cloud Build (권장)**
- Unity 에디터 없이 클라우드에서 빌드
- GitHub 연동으로 자동 빌드
- 무료 플랜 제공

### **2. Unity Command Line Build**
- Unity 에디터가 설치되어 있으면 명령줄로 빌드 가능
- 배치 파일로 자동화

### **3. Android Studio 직접 빌드**
- Unity에서 Android 프로젝트로 Export
- Android Studio에서 APK 빌드

### **4. GitHub Actions**
- GitHub에서 자동 빌드
- Unity Cloud Build와 연동

### **5. Jenkins CI/CD**
- 자동화된 빌드 파이프라인
- 정기적인 빌드 자동화

## 🔧 **즉시 사용 가능한 방법들**

### **방법 1: Unity Command Line Build**
```bash
# Unity 에디터 경로 찾기
where Unity.exe

# 직접 빌드 실행
"C:\Program Files\Unity\Hub\Editor\2023.3.0f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\USER\Documents\NowHere" -executeMethod "NowHere.Editor.CompleteGameBuildScript.BuildCompleteGame"
```

### **방법 2: Android Studio 빌드**
1. Unity에서 `File > Build Settings > Export Project`
2. Android Studio에서 프로젝트 열기
3. `Build > Build Bundle(s) / APK(s) > Build APK(s)`

### **방법 3: Unity Cloud Build**
1. Unity ID로 로그인
2. GitHub에 프로젝트 업로드
3. Unity Cloud Build에서 프로젝트 연결
4. 자동 빌드 설정

## 🎯 **가장 쉬운 방법: Unity Command Line**

Unity 에디터가 설치되어 있다면 명령줄로 바로 빌드할 수 있습니다!
