@echo off
echo ========================================
echo NowHere AR MMORPG 빠른 빌드
echo ========================================

REM Unity 경로 (수정 필요)
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\2023.2.20f1\Editor\Unity.exe"

REM 프로젝트 경로
set PROJECT_PATH=%~dp0

echo Unity 경로: %UNITY_PATH%
echo 프로젝트 경로: %PROJECT_PATH%

REM Unity 존재 확인
if not exist %UNITY_PATH% (
    echo.
    echo ❌ Unity를 찾을 수 없습니다!
    echo.
    echo Unity Hub에서 Unity 2023.2.20f1을 설치하고
    echo 이 스크립트의 UNITY_PATH를 수정해주세요.
    echo.
    echo 현재 설정된 경로: %UNITY_PATH%
    echo.
    pause
    exit /b 1
)

echo ✅ Unity 경로 확인됨

REM 빌드 폴더 생성
if not exist "Builds\Android" mkdir "Builds\Android"

echo.
echo ========================================
echo 테스트 APK 빌드 시작...
echo ========================================

REM 테스트 APK 빌드
%UNITY_PATH% -batchmode -quit -projectPath "%PROJECT_PATH%" -buildTarget Android -executeMethod NowHere.Editor.BuildScript.BuildTestAPKFromCommandLine

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ 빌드 성공!
    echo.
    echo 📁 빌드 파일 위치: Builds\Android\
    echo.
    echo 📱 APK 파일을 안드로이드 기기에 설치하여 테스트하세요.
    echo.
    echo 📋 테스트 방법:
    echo 1. APK 파일을 안드로이드 기기로 복사
    echo 2. 기기에서 "알 수 없는 소스" 허용
    echo 3. APK 파일을 탭하여 설치
    echo 4. 앱 실행 후 T 키로 테스트 패널 열기
    echo.
    
    REM 생성된 파일 목록 표시
    echo 📄 생성된 파일들:
    dir /b "Builds\Android\*.apk" 2>nul
    if %ERRORLEVEL% NEQ 0 (
        echo "APK 파일이 생성되지 않았습니다."
    )
    
) else (
    echo.
    echo ❌ 빌드 실패!
    echo.
    echo Unity 로그를 확인해주세요:
    echo Windows: %%LOCALAPPDATA%%\Unity\Editor\Editor.log
    echo.
)

echo.
pause
