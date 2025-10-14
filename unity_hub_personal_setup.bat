@echo off
chcp 65001 >nul
echo ========================================
echo Unity Hub Personal 라이선스 설정
echo ========================================

echo.
echo Unity Hub에서 Personal 라이선스를 설정합니다...
echo.

REM Unity Hub 경로 확인
set "UNITY_HUB_PATH=C:\Program Files\Unity Hub\Unity Hub.exe"

if exist "%UNITY_HUB_PATH%" (
    echo Unity Hub를 실행합니다...
    start "" "%UNITY_HUB_PATH%"
    
    echo.
    echo ========================================
    echo Unity Hub Personal 라이선스 설정 가이드
    echo ========================================
    echo.
    echo Unity Hub가 실행되었습니다. 다음 단계를 따라하세요:
    echo.
    echo 1. Unity Hub에서 Settings (설정) 클릭
    echo 2. License Management (라이선스 관리) 선택
    echo 3. Personal 라이선스 선택
    echo 4. Activate (활성화) 클릭
    echo 5. Unity ID로 로그인 (또는 새 계정 생성)
    echo 6. Personal 라이선스 활성화 완료
    echo.
    echo 라이선스 활성화 후:
    echo 1. Projects 탭에서 "Open" 클릭
    echo 2. "C:\Users\H2aler\Documents\NowHere" 폴더 선택
    echo 3. 프로젝트가 로드되면 File → Build Settings
    echo 4. Platform: Android 선택
    echo 5. Switch Platform 클릭
    echo 6. Build 클릭하여 APK 생성
    echo.
    
) else (
    echo Unity Hub를 찾을 수 없습니다: %UNITY_HUB_PATH%
    echo.
    echo Unity Hub를 설치해주세요:
    echo https://unity.com/download
    echo.
)

echo.
echo ========================================
echo 대안: Unity Editor 직접 실행
echo ========================================

echo Unity Editor를 직접 실행하여 Personal 라이선스를 설정합니다...
echo.

set "UNITY_EDITOR_PATH=C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe"

if exist "%UNITY_EDITOR_PATH%" (
    echo Unity Editor를 실행합니다...
    start "" "%UNITY_EDITOR_PATH%" -projectPath "C:\Users\H2aler\Documents\NowHere"
    
    echo.
    echo Unity Editor가 실행되었습니다.
    echo Personal 라이선스 설정을 위해 다음을 수행하세요:
    echo.
    echo 1. Unity Editor에서 Help → Manage License 클릭
    echo 2. Personal 라이선스 선택
    echo 3. Unity ID로 로그인
    echo 4. Personal 라이선스 활성화
    echo 5. File → Build Settings
    echo 6. Platform: Android 선택
    echo 7. Build 클릭
    echo.
    
) else (
    echo Unity Editor를 찾을 수 없습니다: %UNITY_EDITOR_PATH%
    echo.
    echo Unity Hub에서 Unity Editor를 설치해주세요.
    echo.
)

echo.
echo ========================================
echo 완료
echo ========================================
echo.
echo Unity Personal 라이선스 설정이 완료되면 APK 빌드가 가능합니다.
echo.
pause
