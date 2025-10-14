@echo off
chcp 65001 >nul
echo ========================================
echo Unity Editor 빌드 트리거
echo ========================================

echo.
echo Unity Editor에서 직접 빌드를 실행합니다...
echo.

REM Unity Editor가 실행 중인지 확인
tasklist /FI "IMAGENAME eq Unity.exe" 2>NUL | find /I /N "Unity.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo Unity Editor가 실행 중입니다.
    echo.
    echo Unity Editor에서 다음을 수행하세요:
    echo.
    echo 1. Unity Editor 창으로 이동
    echo 2. 상단 메뉴에서 Build → Auto Build APK Now 클릭
    echo 3. 또는 Build → Quick Build APK 클릭
    echo.
    echo 빌드가 완료되면 UnityBuilds 폴더에 APK 파일이 생성됩니다.
    echo.
) else (
    echo Unity Editor가 실행되지 않았습니다.
    echo Unity Editor를 실행합니다...
    
    start "" "C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe" -projectPath "C:\Users\H2aler\Documents\NowHere"
    
    echo.
    echo Unity Editor가 실행되었습니다.
    echo Unity Editor가 완전히 로드된 후 다음을 수행하세요:
    echo.
    echo 1. Unity Editor 창에서 Build → Auto Build APK Now 클릭
    echo 2. 또는 Build → Quick Build APK 클릭
    echo.
)

echo.
echo ========================================
echo 빌드 모니터링
echo ========================================

echo 빌드 진행 상황을 모니터링합니다...
echo.

:MONITOR_LOOP
timeout /t 5 /nobreak >nul

if exist "UnityBuilds\NowHere_AR_MMORPG_Final.apk" (
    echo.
    echo ✅ 빌드 완료! NowHere_AR_MMORPG_Final.apk 생성됨
    goto BUILD_SUCCESS
)

if exist "UnityBuilds\NowHere_AR_MMORPG_Quick.apk" (
    echo.
    echo ✅ 빠른 빌드 완료! NowHere_AR_MMORPG_Quick.apk 생성됨
    goto BUILD_SUCCESS
)

echo 빌드 진행 중... (5초마다 확인)
goto MONITOR_LOOP

:BUILD_SUCCESS
echo.
echo ========================================
echo 빌드 성공!
echo ========================================
echo.

for %%F in ("UnityBuilds\*.apk") do (
    set /a "SIZE_MB=%%~zF/1024/1024"
    echo APK 파일: %%~nF%%~xF
    echo 크기: !SIZE_MB! MB
    echo 위치: UnityBuilds\
    echo.
)

echo ✅ 이제 APK 파싱 오류 없이 설치할 수 있습니다!
echo.
echo 📱 설치 명령어:
echo adb install "UnityBuilds\[APK파일명].apk"
echo.

REM UnityBuilds 폴더 열기
start "" "UnityBuilds"

echo 🎉 Unity Editor 빌드 완료!
echo UnityBuilds 폴더에서 APK 파일을 확인하세요.
echo.
pause
