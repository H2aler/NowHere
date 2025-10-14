@echo off
echo ========================================
echo NowHere AR MMORPG APK 설치
echo ========================================

REM APK 파일 확인
if not exist "SimpleAPK\NowHere_AR_MMORPG_v0.1.0.apk" (
    echo ❌ APK 파일을 찾을 수 없습니다!
    echo 먼저 simple_apk_build.bat을 실행하여 APK를 빌드해주세요.
    pause
    exit /b 1
)

echo ✅ APK 파일 확인됨: SimpleAPK\NowHere_AR_MMORPG_v0.1.0.apk

REM ADB 확인
adb version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ ADB를 찾을 수 없습니다!
    echo.
    echo ADB를 설치하거나 PATH에 추가해주세요.
    echo Android Studio SDK의 platform-tools 폴더에 있습니다.
    echo.
    echo 예: C:\Users\%USERNAME%\AppData\Local\Android\Sdk\platform-tools
    echo.
    echo 또는 APK 파일을 수동으로 설치하세요:
    echo 1. SimpleAPK\NowHere_AR_MMORPG_v0.1.0.apk 파일을 안드로이드 기기로 복사
    echo 2. 기기에서 "알 수 없는 소스" 허용
    echo 3. APK 파일을 탭하여 설치
    echo.
    pause
    exit /b 1
)

echo ✅ ADB 확인됨

echo.
echo ========================================
echo 연결된 안드로이드 기기 확인
echo ========================================

REM 연결된 디바이스 확인
adb devices

echo.
echo 📱 연결된 기기가 표시되는지 확인하세요.
echo 기기가 연결되지 않았다면:
echo 1. USB 케이블로 안드로이드 기기 연결
echo 2. 기기에서 USB 디버깅 허용
echo 3. 기기에서 파일 전송 모드 선택
echo.

echo 🚀 APK를 설치하시겠습니까? (Y/N)
set /p install_apk="선택: "

if /i not "%install_apk%"=="Y" (
    echo.
    echo 📋 수동 설치 방법:
    echo 1. SimpleAPK\NowHere_AR_MMORPG_v0.1.0.apk 파일을 안드로이드 기기로 복사
    echo 2. 기기에서 "알 수 없는 소스" 허용
    echo 3. APK 파일을 탭하여 설치
    echo 4. 앱 실행하여 테스트
    echo.
    pause
    exit /b 0
)

echo.
echo ========================================
echo APK 설치 시작
echo ========================================

echo 📱 APK 설치 중...
adb install -r "SimpleAPK\NowHere_AR_MMORPG_v0.1.0.apk"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ APK 설치 성공!
    echo.
    echo 🎮 앱을 실행하여 테스트하세요.
    echo.
    echo 📋 테스트 방법:
    echo 1. 앱 실행
    echo 2. "NowHere AR MMORPG" 화면 확인
    echo 3. "테스트 앱이 성공적으로 빌드되었습니다!" 메시지 확인
    echo 4. "이 앱은 Unity 없이 직접 빌드된 테스트 APK입니다." 메시지 확인
    echo.
    
    echo 🚀 앱을 바로 실행하시겠습니까? (Y/N)
    set /p launch_app="선택: "
    
    if /i "%launch_app%"=="Y" (
        echo.
        echo 📱 앱 실행 중...
        adb shell am start -n com.nowhere.armmorpg/.MainActivity
        
        if %ERRORLEVEL% EQU 0 (
            echo ✅ 앱 실행 성공!
        ) else (
            echo ❌ 앱 실행 실패! 수동으로 실행해주세요.
        )
    )
    
) else (
    echo.
    echo ❌ APK 설치 실패!
    echo.
    echo 🔍 문제 해결 방법:
    echo 1. 안드로이드 기기가 USB로 연결되어 있는지 확인
    echo 2. USB 디버깅이 활성화되어 있는지 확인
    echo 3. 기기에서 설치 권한을 허용했는지 확인
    echo 4. 기존 앱을 제거한 후 다시 설치 시도
    echo.
    echo 기존 앱 제거 명령어:
    echo adb uninstall com.nowhere.armmorpg
    echo.
    echo 📋 수동 설치 방법:
    echo 1. SimpleAPK\NowHere_AR_MMORPG_v0.1.0.apk 파일을 안드로이드 기기로 복사
    echo 2. 기기에서 "알 수 없는 소스" 허용
    echo 3. APK 파일을 탭하여 설치
    echo 4. 앱 실행하여 테스트
    echo.
)

echo.
pause
