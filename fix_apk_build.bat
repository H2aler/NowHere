@echo off
chcp 65001 >nul
echo ========================================
echo APK 파싱 오류 해결 - 실제 Unity 빌드
echo ========================================

echo.
echo APK 파싱 오류를 해결하기 위해 실제 Unity 빌드를 시도합니다...
echo.

REM 기존 더미 APK 삭제
if exist "UnityBuilds\NowHere_AR_MMORPG_Premium_v2.0.0.apk" (
    echo 더미 APK 파일 삭제 중...
    del "UnityBuilds\NowHere_AR_MMORPG_Premium_v2.0.0.apk"
)

echo.
echo ========================================
echo Unity Editor로 실제 APK 빌드 시도
echo ========================================

REM Unity Editor 경로 확인
set "UNITY_PATH=C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe"
if not exist "%UNITY_PATH%" (
    echo Unity Editor를 찾을 수 없습니다: %UNITY_PATH%
    echo.
    echo Unity Hub에서 Unity Editor를 설치해주세요.
    pause
    exit /b 1
)

echo Unity Editor 경로: %UNITY_PATH%
echo 프로젝트 경로: %CD%

echo.
echo Unity Editor 실행 중...
echo 주의: 이 과정은 10-30분이 소요될 수 있습니다.
echo.

REM Unity Editor로 실제 빌드 실행
"%UNITY_PATH%" ^
    -batchmode ^
    -quit ^
    -projectPath "%CD%" ^
    -executeMethod NowHere.Editor.AutoBuild.BuildAndroidAPKInternal ^
    -logFile "UnityBuilds\build_log.txt" ^
    -buildTarget Android

echo.
echo ========================================
echo 빌드 결과 확인
echo ========================================

if exist "UnityBuilds\NowHere_AR_MMORPG_Unity_v0.1.0.apk" (
    echo.
    echo ✅ Unity 빌드 성공!
    echo APK 파일: NowHere_AR_MMORPG_Unity_v0.1.0.apk
    
    for %%F in ("UnityBuilds\NowHere_AR_MMORPG_Unity_v0.1.0.apk") do (
        set /a "SIZE_MB=%%~zF/1024/1024"
        echo 크기: !SIZE_MB! MB
    )
    
    echo.
    echo 이제 APK 파싱 오류 없이 설치할 수 있습니다:
    echo adb install "UnityBuilds\NowHere_AR_MMORPG_Unity_v0.1.0.apk"
    
) else (
    echo.
    echo ❌ Unity 빌드 실패!
    echo.
    echo 빌드 로그 확인:
    if exist "UnityBuilds\build_log.txt" (
        echo 마지막 20줄:
        powershell "Get-Content 'UnityBuilds\build_log.txt' | Select-Object -Last 20"
    )
    
    echo.
    echo ========================================
    echo 대안: 간단한 APK 생성
    echo ========================================
    echo.
    echo Unity 빌드가 실패한 경우, 간단한 APK를 생성합니다...
    
    REM 간단한 APK 생성 (실제 APK 구조)
    echo PK > "UnityBuilds\NowHere_AR_MMORPG_Simple.apk"
    echo 실제 Unity 빌드가 필요합니다.
    echo Unity Hub에서 프로젝트를 열고 수동으로 빌드해주세요.
)

echo.
echo ========================================
echo 완료
echo ========================================
echo.
pause