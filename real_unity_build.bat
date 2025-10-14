@echo off
chcp 65001 >nul
echo ========================================
echo 실제 Unity 빌드 - 실행 가능한 APK 생성
echo ========================================

echo.
echo 더미 파일이 아닌 실제 실행 가능한 APK를 Unity Editor로 빌드합니다...
echo.

REM 기존 더미 파일들 정리
if exist "UnityBuilds" (
    echo 기존 더미 파일들 정리 중...
    rmdir /s /q "UnityBuilds"
)

REM 새로운 빌드 폴더 생성
mkdir "UnityBuilds"

echo.
echo ========================================
echo Unity Editor 실행 (실제 빌드)
echo ========================================

set "UNITY_PATH=C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe"
set "PROJECT_PATH=%CD%"

echo Unity Editor 경로: %UNITY_PATH%
echo 프로젝트 경로: %PROJECT_PATH%

echo.
echo Unity Editor를 실행하여 실제 APK를 빌드합니다...
echo.

REM Unity Editor를 실행하여 실제 빌드
start "" "%UNITY_PATH%" -projectPath "%PROJECT_PATH%" -batchmode -quit -executeMethod NowHere.Editor.AutoBuild.BuildAndroidAPKInternal -logFile "UnityBuilds\build_log.txt" -buildTarget Android

echo Unity Editor가 실행되었습니다.
echo.

echo ========================================
echo 빌드 모니터링
echo ========================================

echo 실제 빌드 진행 상황을 모니터링합니다...
echo.

set /a "WAIT_TIME=0"
set /a "MAX_WAIT=1800"

:WAIT_LOOP
timeout /t 20 /nobreak >nul
set /a "WAIT_TIME+=20"

echo 빌드 진행 중... (%WAIT_TIME%초 경과)

REM 실제 빌드 완료 확인
if exist "UnityBuilds\NowHere_AR_MMORPG_Unity_v0.1.0.apk" (
    echo.
    echo ✅ 실제 Unity 빌드 성공!
    goto BUILD_SUCCESS
)

if %WAIT_TIME% GEQ %MAX_WAIT% (
    echo.
    echo ⏰ 빌드 시간 초과 (30분)
    goto BUILD_TIMEOUT
)

goto WAIT_LOOP

:BUILD_SUCCESS
echo.
echo ========================================
echo 실제 빌드 성공!
echo ========================================
echo.
echo APK 파일: NowHere_AR_MMORPG_Unity_v0.1.0.apk

for %%F in ("UnityBuilds\NowHere_AR_MMORPG_Unity_v0.1.0.apk") do (
    set /a "SIZE_MB=%%~zF/1024/1024"
    echo 크기: !SIZE_MB! MB
)

echo.
echo ✅ 이것은 더미 파일이 아닌 실제 Unity로 빌드된 APK입니다!
echo.
echo 📱 설치 명령어:
echo adb install "UnityBuilds\NowHere_AR_MMORPG_Unity_v0.1.0.apk"
echo.
goto END

:BUILD_TIMEOUT
echo.
echo ========================================
echo 빌드 시간 초과
echo ========================================
echo.
echo Unity 빌드가 30분 내에 완료되지 않았습니다.
echo.
echo 빌드 로그 확인:
if exist "UnityBuilds\build_log.txt" (
    echo 마지막 30줄:
    powershell "Get-Content 'UnityBuilds\build_log.txt' | Select-Object -Last 30"
)
echo.
echo ========================================
echo Unity Hub에서 수동 빌드
echo ========================================
echo.
echo Unity Hub에서 수동으로 빌드해보세요:
echo.
echo 1. Unity Hub 실행
echo 2. 프로젝트 열기: %PROJECT_PATH%
echo 3. File → Build Settings
echo 4. Platform: Android 선택
echo 5. Build 클릭
echo.

:END
echo.
echo ========================================
echo 완료
echo ========================================
echo.

REM UnityBuilds 폴더 열기
start "" "UnityBuilds"

echo 🎉 실제 Unity 빌드 프로세스 완료!
echo UnityBuilds 폴더에서 결과를 확인하세요.
echo.
pause
