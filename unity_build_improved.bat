@echo off
chcp 65001 >nul
echo ========================================
echo Unity 개선된 APK 빌드
echo ========================================

REM Unity Editor 경로 설정
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe"

REM 프로젝트 경로
set PROJECT_PATH="C:\Users\H2aler\Documents\NowHere"

REM 빌드 폴더 생성
if not exist "UnityBuilds" mkdir UnityBuilds

echo Unity Editor 경로: %UNITY_PATH%
echo 프로젝트 경로: %PROJECT_PATH%
echo.

REM Unity Editor가 존재하는지 확인
if not exist %UNITY_PATH% (
    echo Unity Editor를 찾을 수 없습니다!
    echo 경로를 확인해주세요: %UNITY_PATH%
    pause
    exit /b 1
)

echo Unity Editor 확인됨
echo.

echo ========================================
echo Unity Editor로 APK 빌드 시작...
echo ========================================

REM Unity Editor를 명령줄 모드로 실행하여 빌드
%UNITY_PATH% ^
    -batchmode ^
    -quit ^
    -projectPath %PROJECT_PATH% ^
    -executeMethod NowHere.Editor.AutoBuild.BuildAndroidAPKInternal ^
    -logFile "UnityBuilds\build_log.txt" ^
    -buildTarget Android

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Unity 빌드 성공!
    echo.
    echo 빌드 결과 확인:
    dir UnityBuilds\*.apk
    echo.
    echo APK 파일을 Android 기기에 설치하여 테스트하세요.
    echo.
) else (
    echo.
    echo Unity 빌드 실패!
    echo.
    echo 빌드 로그 확인:
    if exist "UnityBuilds\build_log.txt" (
        echo 마지막 20줄:
        powershell -command "Get-Content 'UnityBuilds\build_log.txt' | Select-Object -Last 20"
    )
    echo.
    echo 문제를 해결한 후 다시 시도해주세요.
)

echo.
echo ========================================
echo 빌드 완료
echo ========================================
pause
