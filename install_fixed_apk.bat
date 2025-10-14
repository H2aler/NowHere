@echo off
echo ========================================
echo NowHere AR MMORPG 수정된 APK 설치
echo ========================================

REM APK 파일 확인
if not exist "FixedAPK\NowHere_AR_MMORPG_Fixed_v0.1.0.apk" (
    echo ❌ 수정된 APK 파일을 찾을 수 없습니다!
    echo 먼저 fix_apk_build.bat을 실행하여 APK를 빌드해주세요.
    pause
    exit /b 1
)

echo ✅ 수정된 APK 파일 확인됨: FixedAPK\NowHere_AR_MMORPG_Fixed_v0.1.0.apk

REM APK 파일 정보 표시
for %%A in ("FixedAPK\NowHere_AR_MMORPG_Fixed_v0.1.0.apk") do (
    echo 📊 APK 파일 크기: %%~zA bytes
    echo 📅 생성 날짜: %%~tA
)

echo.
echo ========================================
echo APK 파일 검증
echo ========================================

REM APK 파일이 ZIP 형식인지 확인
powershell -command "try { Add-Type -AssemblyName System.IO.Compression.FileSystem; [System.IO.Compression.ZipFile]::OpenRead('FixedAPK\NowHere_AR_MMORPG_Fixed_v0.1.0.apk') | Out-Null; Write-Host '✅ APK 파일 형식이 올바릅니다.' } catch { Write-Host '❌ APK 파일 형식에 문제가 있습니다.' }"

echo.
echo ========================================
echo 설치 방법 선택
echo ========================================

echo 1. ADB를 통한 자동 설치 (안드로이드 기기 연결 필요)
echo 2. 수동 설치 (APK 파일을 기기로 복사)
echo 3. APK 파일 내용 확인
echo.

set /p choice="선택하세요 (1-3): "

if "%choice%"=="1" goto adb_install
if "%choice%"=="2" goto manual_install
if "%choice%"=="3" goto show_content
goto invalid_choice

:adb_install
echo.
echo ========================================
echo ADB 자동 설치
echo ========================================

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
    goto manual_install
)

echo ✅ ADB 확인됨

REM 연결된 디바이스 확인
echo 📱 연결된 안드로이드 기기 확인 중...
adb devices

echo.
echo 🚀 APK를 설치하시겠습니까? (Y/N)
set /p install_apk="선택: "

if /i not "%install_apk%"=="Y" (
    goto manual_install
)

echo.
echo 📱 APK 설치 중...
adb install -r "FixedAPK\NowHere_AR_MMORPG_Fixed_v0.1.0.apk"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ APK 설치 성공!
    echo.
    echo 🎮 앱을 실행하여 테스트하세요.
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
)
goto end

:manual_install
echo.
echo ========================================
echo 수동 설치 방법
echo ========================================

echo 📋 수동 설치 단계:
echo.
echo 1. 📁 APK 파일 위치: FixedAPK\NowHere_AR_MMORPG_Fixed_v0.1.0.apk
echo.
echo 2. 📱 안드로이드 기기로 APK 파일 복사:
echo    - USB 케이블 연결
echo    - 파일 전송 모드 선택
echo    - APK 파일을 기기의 Downloads 폴더에 복사
echo.
echo 3. 🔧 기기 설정:
echo    - 설정 → 보안 → "알 수 없는 소스" 허용
echo    - 또는 설정 → 앱 → 특별 액세스 → 알 수 없는 앱 설치 허용
echo.
echo 4. 📲 APK 설치:
echo    - 파일 관리자에서 APK 파일 찾기
echo    - APK 파일을 탭하여 설치
echo    - 설치 완료 후 앱 실행
echo.
echo 5. 🧪 테스트:
echo    - "NowHere AR MMORPG" 앱 실행
echo    - "✅ 테스트 앱이 성공적으로 빌드되었습니다!" 메시지 확인
echo    - "이 앱은 Unity 없이 직접 빌드된 테스트 APK입니다." 메시지 확인
echo    - 버전 정보 확인
echo.
goto end

:show_content
echo.
echo ========================================
echo APK 파일 내용 확인
echo ========================================

echo 📦 APK 파일 구조:
echo.
echo FixedAPK\NowHere_AR_MMORPG_Fixed_v0.1.0.apk
echo ├── AndroidManifest.xml (앱 매니페스트)
echo ├── classes.dex (컴파일된 Java 바이트코드)
echo ├── resources.arsc (컴파일된 리소스)
echo ├── META-INF\ (메타데이터)
echo │   └── MANIFEST.MF (매니페스트 정보)
echo └── res\ (리소스 파일들)
echo     ├── layout\main.xml (메인 레이아웃)
echo     ├── values\strings.xml (문자열 리소스)
echo     └── mipmap-hdpi\ic_launcher.xml (앱 아이콘)
echo.
echo 📋 앱 정보:
echo - 패키지명: com.nowhere.armmorpg
echo - 버전: 0.1.0
echo - 최소 SDK: 21 (Android 5.0)
echo - 타겟 SDK: 33 (Android 13)
echo - 권한: 카메라, 마이크, 위치, 인터넷
echo.
goto end

:invalid_choice
echo.
echo ❌ 잘못된 선택입니다. 1-3 중에서 선택해주세요.
goto end

:end
echo.
pause
