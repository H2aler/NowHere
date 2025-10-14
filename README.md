# NowHere - Complete AR/VR/XR MMORPG

![Unity](https://img.shields.io/badge/Unity-2023.3.0f1-blue)
![Platform](https://img.shields.io/badge/Platform-Android-green)
![XR](https://img.shields.io/badge/XR-VR%20%7C%20AR%20%7C%20MR-purple)
![License](https://img.shields.io/badge/License-MIT-yellow)

## 🎮 프로젝트 개요

**NowHere**는 현실과 가상세계를 연결하는 혁신적인 AR/VR/XR MMORPG입니다. 모바일 기기에서 AR 기능을 활용하여 현실 공간에 가상의 게임 세계를 구현하고, VR 기기에서는 완전한 몰입형 게임 경험을 제공합니다.

## ✨ 주요 기능

### 🎯 **핵심 시스템**
- **완전한 모바일 게임**: Android APK로 실행 가능
- **AR 기능**: ARCore를 활용한 현실 공간 게임
- **VR 지원**: OpenXR 기반 VR 게임 경험
- **MR 통합**: AR과 VR을 결합한 혼합현실
- **멀티플레이어**: 실시간 네트워킹 지원
- **RPG 시스템**: 캐릭터, 아이템, 스킬 시스템
- **전투 시스템**: AR/VR 기반 몰입형 전투

### 📱 **모바일 기능**
- **터치 인터페이스**: 가상 조이스틱 및 터치 컨트롤
- **센서 활용**: 자이로스코프, 가속도계, GPS
- **음성 명령**: 음성 인식 기반 게임 조작
- **모션 감지**: 기기 움직임을 통한 게임 조작
- **카메라 AR**: 현실 공간에 가상 오브젝트 배치

### 🥽 **XR 기능**
- **VR 컨트롤러**: VR 컨트롤러를 활용한 상호작용
- **핸드 트래킹**: 손동작 인식 기반 조작
- **아이 트래킹**: 시선 추적 기능
- **3D UI**: VR 환경에 최적화된 UI
- **공간 오디오**: 3D 공간 음향 효과

## 🚀 빌드 방법

### **방법 1: Unity 에디터 빌드**
```bash
# Unity 에디터에서
Build > Complete Game Build (All Systems)
```

### **방법 2: Command Line 빌드**
```bash
# Unity 에디터 없이 직접 빌드
.\direct_unity_build.bat
```

### **방법 3: GitHub Actions 빌드**
```bash
# GitHub에 푸시하면 자동 빌드
git push origin main
```

### **방법 4: Android Studio 빌드**
```bash
# Unity에서 Android Studio 프로젝트로 Export
.\android_studio_build.bat
```

## 📦 설치된 패키지

- **XR Plugin Management**: 4.5.1
- **AR Foundation**: 6.2.0
- **ARCore XR Plugin**: 6.2.0
- **Oculus XR Plugin**: 4.5.2
- **Universal Render Pipeline**: 17.2.0
- **Multiplayer Netcode**: 1.14.1
- **Input System**: 1.14.2
- **Timeline**: 1.8.9
- **Visual Scripting**: 1.9.7

## 🎯 시스템 요구사항

### **개발 환경**
- Unity 2023.3.0f1 이상
- Android SDK API Level 24 이상
- JDK 8 이상

### **실행 환경**
- **Android**: 7.0 (API Level 24) 이상
- **AR 지원**: ARCore 지원 기기
- **VR 지원**: OpenXR 호환 VR 기기
- **권한**: 카메라, 마이크, 위치, 인터넷

## 📁 프로젝트 구조

```
NowHere/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/           # 핵심 시스템
│   │   ├── Game/           # 게임 관리
│   │   ├── XR/             # XR 시스템
│   │   ├── UI/             # UI 시스템
│   │   ├── Audio/          # 오디오 시스템
│   │   ├── Data/           # 데이터 관리
│   │   ├── Analytics/      # 분석 시스템
│   │   ├── Testing/        # 테스트 도구
│   │   └── Editor/         # 에디터 도구
│   ├── Scenes/             # 게임 씬
│   ├── Prefabs/            # 프리팹
│   ├── Models/             # 3D 모델
│   ├── Textures/           # 텍스처
│   ├── Audio/              # 오디오 파일
│   └── Shaders/            # 셰이더
├── Packages/               # Unity 패키지
├── ProjectSettings/        # 프로젝트 설정
└── Build Scripts/          # 빌드 스크립트
```

## 🔧 개발 도구

### **Unity 에디터 도구**
- **Package Installer**: 필수 패키지 자동 설치
- **Build Validator**: 빌드 환경 검증
- **Complete Game Build**: 완전한 게임 빌드
- **XR Build**: XR 전용 빌드

### **빌드 스크립트**
- `build_complete_game.bat`: 완전한 게임 빌드
- `build_xr_apk.bat`: XR APK 빌드
- `direct_unity_build.bat`: Unity 에디터 없이 빌드
- `android_studio_build.bat`: Android Studio 빌드

## 🎮 게임 플레이

### **모바일 모드**
1. **AR 게임**: 현실 공간에 가상 오브젝트 배치
2. **터치 조작**: 가상 조이스틱으로 캐릭터 이동
3. **센서 활용**: 기기 기울기로 카메라 조작
4. **음성 명령**: "공격", "방어" 등 음성으로 조작

### **VR 모드**
1. **VR 컨트롤러**: 컨트롤러로 무기 조작
2. **핸드 트래킹**: 손동작으로 아이템 조작
3. **3D UI**: VR 환경에 최적화된 인터페이스
4. **공간 오디오**: 3D 음향으로 몰입감 증대

## 🚀 GitHub Actions

이 프로젝트는 GitHub Actions를 통해 자동 빌드됩니다:

- **자동 빌드**: 코드 푸시 시 자동 APK 생성
- **Unity Cloud Build**: Unity 클라우드에서 빌드
- **Android APK**: 자동으로 APK 파일 생성
- **빌드 아티팩트**: 빌드 결과물 자동 저장

## 📊 분석 및 테스트

### **분석 시스템**
- **사용자 행동**: 게임 플레이 패턴 분석
- **성능 모니터링**: 프레임률, 메모리 사용량
- **XR 사용 패턴**: VR/AR 기능 사용 통계

### **테스트 도구**
- **자동 테스트**: 게임 기능 자동 검증
- **성능 테스트**: 프레임률 및 메모리 테스트
- **XR 테스트**: VR/AR 기능 테스트

## 🤝 기여하기

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 라이선스

이 프로젝트는 MIT 라이선스 하에 배포됩니다. 자세한 내용은 `LICENSE` 파일을 참조하세요.

## 📞 연락처

- **프로젝트 링크**: [https://github.com/H2aler/NowHere](https://github.com/H2aler/NowHere)
- **이슈 리포트**: [GitHub Issues](https://github.com/H2aler/NowHere/issues)

## 🙏 감사의 말

- Unity Technologies
- ARCore Team
- OpenXR Community
- VR/AR 개발자 커뮤니티

---

**NowHere** - 현실과 가상세계를 연결하는 혁신적인 게임 경험을 제공합니다! 🎮✨