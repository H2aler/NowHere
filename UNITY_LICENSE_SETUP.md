# Unity 라이선스 GitHub Secrets 설정 가이드

## 🔑 Unity 라이선스 파일 생성 및 GitHub Secrets 추가

### 1단계: Unity 라이선스 파일 생성

#### 방법 1: Unity Hub에서 라이선스 생성
1. **Unity Hub 실행**
2. **Settings (설정) 클릭**
3. **License Management (라이선스 관리) 클릭**
4. **Save License (라이선스 저장) 클릭**
5. **라이선스 파일 저장** (예: `Unity_v2023.3.0f1.ulf`)

#### 방법 2: Unity 에디터에서 라이선스 생성
1. **Unity 에디터 실행**
2. **Help → Manage License 클릭**
3. **Save License 클릭**
4. **라이선스 파일 저장**

### 2단계: 라이선스 파일 내용 확인

생성된 라이선스 파일을 텍스트 에디터로 열어서 내용을 확인합니다:

```
<?xml version="1.0" encoding="utf-8"?>
<root>
  <Unity>
    <License>
      <Serial>XXXX-XXXX-XXXX-XXXX-XXXX</Serial>
      <UnityVersion>2023.3.0f1</UnityVersion>
      <LicenseType>Personal</LicenseType>
      <!-- 기타 라이선스 정보 -->
    </License>
  </Unity>
</root>
```

### 3단계: GitHub Secrets에 라이선스 추가

#### GitHub 웹사이트에서 설정:
1. **GitHub 저장소 방문**: [https://github.com/H2aler/NowHere](https://github.com/H2aler/NowHere)
2. **Settings 탭 클릭**
3. **왼쪽 메뉴에서 "Secrets and variables" → "Actions" 클릭**
4. **"New repository secret" 버튼 클릭**
5. **다음 정보 입력**:
   - **Name**: `UNITY_LICENSE`
   - **Secret**: 라이선스 파일의 전체 내용 (XML 형식)
6. **"Add secret" 버튼 클릭**

### 4단계: 라이선스 설정 확인

GitHub Actions 워크플로우에서 라이선스가 올바르게 설정되었는지 확인:

```yaml
- name: Activate Unity License
  uses: game-ci/unity-activate@v2
  with:
    unity-license: ${{ secrets.UNITY_LICENSE }}
```

## 🔧 대안: Unity Personal 라이선스 사용

Unity Personal 라이선스는 무료이지만 GitHub Actions에서 사용하기 위해 추가 설정이 필요할 수 있습니다.

### Personal 라이선스 설정:
1. **Unity Hub에서 Personal 라이선스 활성화**
2. **라이선스 파일 생성**
3. **GitHub Secrets에 추가**

## 🚀 빌드 테스트

라이선스 설정 완료 후:
1. **새로운 커밋 푸시**
2. **GitHub Actions 빌드 실행**
3. **빌드 로그에서 라이선스 활성화 확인**

## 📋 라이선스 파일 예시

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <Unity>
    <License>
      <Serial>XXXX-XXXX-XXXX-XXXX-XXXX</Serial>
      <UnityVersion>2023.3.0f1</UnityVersion>
      <LicenseType>Personal</LicenseType>
      <CompanyName>Your Company</CompanyName>
      <UserName>Your Name</UserName>
      <Email>your.email@example.com</Email>
      <ActivationDate>2024-01-01</ActivationDate>
      <ExpirationDate>2025-01-01</ExpirationDate>
    </License>
  </Unity>
</root>
```

## ⚠️ 주의사항

1. **라이선스 파일 보안**: GitHub Secrets에 저장된 라이선스는 안전하게 보호됩니다
2. **라이선스 만료**: 라이선스가 만료되면 새로운 라이선스로 업데이트해야 합니다
3. **개인정보**: 라이선스 파일에 포함된 개인정보를 확인하고 필요시 수정합니다

## 🔍 문제 해결

### 라이선스 활성화 실패 시:
1. **라이선스 파일 형식 확인**: XML 형식이 올바른지 확인
2. **Unity 버전 확인**: 라이선스 버전과 빌드 버전이 일치하는지 확인
3. **GitHub Secrets 확인**: 라이선스가 올바르게 저장되었는지 확인

### 빌드 로그 확인:
GitHub Actions → 해당 워크플로우 → 로그에서 라이선스 활성화 상태 확인
