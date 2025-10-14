# 📱 Android Studio 빌드 가이드

Unity 프로젝트를 Android Studio로 내보내서 APK를 빌드하는 방법을 안내합니다.

## 🚀 빠른 시작

### **1단계: Android Studio 프로젝트 내보내기**
```bash
# 프로젝트 폴더에서 실행
android_studio_build.bat
```

### **2단계: Android Studio에서 빌드**
1. Android Studio 실행
2. `AndroidStudioProject` 폴더 열기
3. Gradle 동기화 완료 대기
4. Build → Build Bundle(s) / APK(s) → Build APK(s) 실행

### **3단계: 명령줄에서 빌드 (선택사항)**
```bash
# AndroidStudioProject 폴더에서 실행
build_apk.bat          # 디버그 APK 빌드
build_release.bat      # 릴리즈 APK 빌드
install_apk.bat        # APK 설치
```

## 🛠️ 상세 과정

### **Unity에서 Android Studio 프로젝트 내보내기**

#### **자동 내보내기 (추천)**
```bash
# Windows 배치 파일 실행
android_studio_build.bat
```

#### **수동 내보내기**
1. Unity 에디터에서 `Build → Export to Android Studio` 메뉴 실행
2. 또는 `Assets/Scripts/Editor/AndroidStudioExport.cs` 스크립트 사용

### **Android Studio 설정**

#### **필수 요구사항**
- **Android Studio**: 최신 버전 설치
- **JDK**: Java Development Kit 8 이상
- **Android SDK**: API Level 24 이상
- **NDK**: Native Development Kit (Unity에서 자동 설정)

#### **SDK 구성 요소**
```
Android SDK Platform 33
Android SDK Build-Tools 33.0.0
Android SDK Platform-Tools
Android SDK Tools
NDK (Side by side)
```

### **프로젝트 구조**
```
AndroidStudioProject/
├── app/
│   ├── build.gradle
│   ├── src/
│   │   └── main/
│   │       ├── AndroidManifest.xml
│   │       ├── java/
│   │       └── res/
│   └── proguard-rules.pro
├── gradle/
├── gradlew.bat
├── build.gradle
├── settings.gradle
├── build_apk.bat
├── build_release.bat
└── install_apk.bat
```

## 🔧 빌드 옵션

### **디버그 APK 빌드**
```bash
# Android Studio에서
Build → Build Bundle(s) / APK(s) → Build APK(s)

# 명령줄에서
gradlew assembleDebug
```

**특징:**
- 디버깅 정보 포함
- 빠른 빌드 시간
- 개발 및 테스트용

### **릴리즈 APK 빌드**
```bash
# Android Studio에서
Build → Generate Signed Bundle / APK → APK

# 명령줄에서
gradlew assembleRelease
```

**특징:**
- 코드 난독화
- 최적화된 성능
- 작은 파일 크기
- 배포용

## 📱 APK 설치 및 테스트

### **설치 방법**

#### **1. ADB를 통한 설치 (추천)**
```bash
# AndroidStudioProject 폴더에서 실행
install_apk.bat
```

#### **2. 수동 설치**
1. APK 파일을 안드로이드 기기로 복사
2. 기기에서 "알 수 없는 소스" 허용
3. APK 파일을 탭하여 설치

#### **3. Android Studio에서 직접 설치**
1. Run → Run 'app' 클릭
2. 연결된 기기 선택
3. 자동으로 빌드 및 설치

### **테스트 실행**
1. **앱 실행**: 설치된 앱을 실행
2. **권한 승인**: 카메라, 마이크, 위치 권한 허용
3. **테스트 패널**: T 키 또는 화면 터치로 테스트 패널 열기
4. **시스템 테스트**: 각 시스템별 테스트 버튼 클릭

## 🐛 문제 해결

### **일반적인 문제들**

#### **Unity 내보내기 실패**
```
문제: Unity에서 Android Studio 프로젝트 내보내기 실패
해결: 
1. Unity 버전 확인 (2023.2.20f1 권장)
2. Android SDK 설정 확인
3. 프로젝트 경로에 특수문자 없는지 확인
```

#### **Gradle 동기화 실패**
```
문제: Android Studio에서 Gradle 동기화 실패
해결:
1. Android Studio 최신 버전으로 업데이트
2. Gradle Wrapper 버전 확인
3. 인터넷 연결 확인
4. Proxy 설정 확인
```

#### **빌드 실패**
```
문제: APK 빌드 실패
해결:
1. JDK 버전 확인 (JDK 8 이상)
2. Android SDK 구성 요소 확인
3. NDK 설치 확인
4. 메모리 부족 시 Gradle 힙 크기 증가
```

#### **APK 설치 실패**
```
문제: APK 설치 실패
해결:
1. USB 디버깅 활성화 확인
2. 기기에서 설치 권한 허용
3. 기존 앱 제거 후 재설치
4. ADB 드라이버 설치 확인
```

### **성능 최적화**

#### **빌드 시간 단축**
```gradle
// gradle.properties
org.gradle.parallel=true
org.gradle.daemon=true
org.gradle.configureondemand=true
```

#### **메모리 사용량 최적화**
```gradle
// gradle.properties
org.gradle.jvmargs=-Xmx4096m -XX:MaxPermSize=512m
```

## 🔧 고급 설정

### **ProGuard 설정**
```proguard
# app/proguard-rules.pro
-keep class com.unity3d.** { *; }
-keep class com.unity.** { *; }
-dontwarn com.unity3d.**
-dontwarn com.unity.**
```

### **빌드 변형 (Build Variants)**
```gradle
// app/build.gradle
android {
    buildTypes {
        debug {
            debuggable true
            minifyEnabled false
        }
        release {
            debuggable false
            minifyEnabled true
            proguardFiles getDefaultProguardFile('proguard-android.txt'), 'proguard-rules.pro'
        }
    }
}
```

### **서명 설정**
```gradle
// app/build.gradle
android {
    signingConfigs {
        release {
            storeFile file('keystore.jks')
            storePassword 'password'
            keyAlias 'key'
            keyPassword 'password'
        }
    }
}
```

## 📊 빌드 결과

### **APK 파일 위치**
```
app/build/outputs/apk/
├── debug/
│   └── app-debug.apk
└── release/
    └── app-release.apk
```

### **빌드 정보**
- **패키지명**: com.nowhere.armmorpg
- **최소 SDK**: API 24 (Android 7.0)
- **타겟 SDK**: API 33 (Android 13)
- **아키텍처**: ARM64

## 🎯 테스트 시나리오

### **기본 기능 테스트**
1. 앱 실행 및 로딩
2. 권한 요청 및 승인
3. 테스트 패널 열기/닫기
4. 각 시스템 개별 테스트

### **고급 기능 테스트**
1. **센서 테스트**: 기기 움직임, 회전, 위치
2. **터치 테스트**: 오브젝트 생성, 파괴, 조작
3. **음성 테스트**: 마이크, 음성 채팅
4. **모션 테스트**: 흔들기, 기울기, 회피
5. **AR 테스트**: 가상 오브젝트 배치
6. **전투 테스트**: 제스처, 모션, 터치 전투

## 📞 지원

문제가 발생하면:
1. Unity 로그 확인
2. Android Studio 로그 확인
3. Gradle 빌드 로그 확인
4. ADB 로그 확인

---

**Happy Building with Android Studio! 🚀📱**
