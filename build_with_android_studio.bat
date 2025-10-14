@echo off
echo ========================================
echo Android Studio로 APK 빌드
echo ========================================

REM Android Studio 경로 설정
set ANDROID_STUDIO_PATH="C:\Program Files\Android\Android Studio\bin\studio64.exe"

REM 프로젝트 경로
set PROJECT_PATH=%~dp0AndroidStudioProject

echo Android Studio 경로: %ANDROID_STUDIO_PATH%
echo 프로젝트 경로: %PROJECT_PATH%

REM Android Studio 존재 확인
if not exist %ANDROID_STUDIO_PATH% (
    echo.
    echo ❌ Android Studio를 찾을 수 없습니다!
    echo.
    echo 경로를 확인해주세요: %ANDROID_STUDIO_PATH%
    echo.
    pause
    exit /b 1
)

echo ✅ Android Studio 경로 확인됨

REM 프로젝트 폴더 확인
if not exist "%PROJECT_PATH%" (
    echo.
    echo ❌ Android Studio 프로젝트 폴더를 찾을 수 없습니다!
    echo.
    echo 경로를 확인해주세요: %PROJECT_PATH%
    echo.
    pause
    exit /b 1
)

echo ✅ 프로젝트 폴더 확인됨

echo.
echo ========================================
echo Android Studio 실행 중...
echo ========================================

REM Android Studio 실행 (프로젝트 열기)
start "" %ANDROID_STUDIO_PATH% "%PROJECT_PATH%"

echo.
echo ✅ Android Studio가 실행되었습니다!
echo.
echo 📋 다음 단계:
echo 1. Android Studio에서 프로젝트가 로드될 때까지 대기
echo 2. Gradle 동기화 완료 대기
echo 3. Build → Build Bundle(s) / APK(s) → Build APK(s) 클릭
echo 4. 빌드 완료 후 APK 파일 확인
echo.
echo 📁 APK 파일 위치: app\build\outputs\apk\debug\app-debug.apk
echo.

echo 🚀 Android Studio에서 빌드를 진행하세요!
echo.

pause
