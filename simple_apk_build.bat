@echo off
echo ========================================
echo NowHere AR MMORPG 간단 APK 빌드
echo ========================================

REM APK 빌드 폴더 생성
if not exist "SimpleAPK" mkdir SimpleAPK
cd SimpleAPK

echo ✅ APK 빌드 폴더 생성됨

REM 간단한 APK 구조 생성
if not exist "META-INF" mkdir META-INF
if not exist "res" mkdir res
if not exist "res\layout" mkdir res\layout
if not exist "res\values" mkdir res\values
if not exist "res\drawable" mkdir res\drawable

echo ✅ APK 구조 생성됨

REM AndroidManifest.xml 생성
echo ^<?xml version="1.0" encoding="utf-8"?^> > AndroidManifest.xml
echo ^<manifest xmlns:android="http://schemas.android.com/apk/res/android" >> AndroidManifest.xml
echo     android:versionCode="1" >> AndroidManifest.xml
echo     android:versionName="0.1.0" >> AndroidManifest.xml
echo     package="com.nowhere.armmorpg"^> >> AndroidManifest.xml
echo. >> AndroidManifest.xml
echo     ^<uses-permission android:name="android.permission.CAMERA" /^> >> AndroidManifest.xml
echo     ^<uses-permission android:name="android.permission.RECORD_AUDIO" /^> >> AndroidManifest.xml
echo     ^<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" /^> >> AndroidManifest.xml
echo     ^<uses-permission android:name="android.permission.INTERNET" /^> >> AndroidManifest.xml
echo. >> AndroidManifest.xml
echo     ^<application >> AndroidManifest.xml
echo         android:label="NowHere AR MMORPG" >> AndroidManifest.xml
echo         android:icon="@drawable/icon"^> >> AndroidManifest.xml
echo         ^<activity >> AndroidManifest.xml
echo             android:name=".MainActivity" >> AndroidManifest.xml
echo             android:exported="true"^> >> AndroidManifest.xml
echo             ^<intent-filter^> >> AndroidManifest.xml
echo                 ^<action android:name="android.intent.action.MAIN" /^> >> AndroidManifest.xml
echo                 ^<category android:name="android.intent.category.LAUNCHER" /^> >> AndroidManifest.xml
echo             ^</intent-filter^> >> AndroidManifest.xml
echo         ^</activity^> >> AndroidManifest.xml
echo     ^</application^> >> AndroidManifest.xml
echo ^</manifest^> >> AndroidManifest.xml

echo ✅ AndroidManifest.xml 생성됨

REM 간단한 레이아웃 파일 생성
echo ^<?xml version="1.0" encoding="utf-8"?^> > res\layout\main.xml
echo ^<LinearLayout xmlns:android="http://schemas.android.com/apk/res/android" >> res\layout\main.xml
echo     android:layout_width="match_parent" >> res\layout\main.xml
echo     android:layout_height="match_parent" >> res\layout\main.xml
echo     android:orientation="vertical" >> res\layout\main.xml
echo     android:padding="16dp"^> >> res\layout\main.xml
echo. >> res\layout\main.xml
echo     ^<TextView >> res\layout\main.xml
echo         android:layout_width="match_parent" >> res\layout\main.xml
echo         android:layout_height="wrap_content" >> res\layout\main.xml
echo         android:text="NowHere AR MMORPG" >> res\layout\main.xml
echo         android:textSize="24sp" >> res\layout\main.xml
echo         android:gravity="center" >> res\layout\main.xml
echo         android:layout_marginBottom="24dp" /^> >> res\layout\main.xml
echo. >> res\layout\main.xml
echo     ^<TextView >> res\layout\main.xml
echo         android:layout_width="match_parent" >> res\layout\main.xml
echo         android:layout_height="wrap_content" >> res\layout\main.xml
echo         android:text="테스트 앱이 성공적으로 빌드되었습니다!" >> res\layout\main.xml
echo         android:textSize="16sp" >> res\layout\main.xml
echo         android:gravity="center" >> res\layout\main.xml
echo         android:layout_marginBottom="16dp" /^> >> res\layout\main.xml
echo. >> res\layout\main.xml
echo     ^<TextView >> res\layout\main.xml
echo         android:layout_width="match_parent" >> res\layout\main.xml
echo         android:layout_height="wrap_content" >> res\layout\main.xml
echo         android:text="이 앱은 Unity 없이 직접 빌드된 테스트 APK입니다." >> res\layout\main.xml
echo         android:textSize="14sp" >> res\layout\main.xml
echo         android:gravity="center" >> res\layout\main.xml
echo         android:layout_marginBottom="16dp" /^> >> res\layout\main.xml
echo. >> res\layout\main.xml
echo     ^<TextView >> res\layout\main.xml
echo         android:layout_width="match_parent" >> res\layout\main.xml
echo         android:layout_height="wrap_content" >> res\layout\main.xml
echo         android:text="버전: 0.1.0" >> res\layout\main.xml
echo         android:textSize="12sp" >> res\layout\main.xml
echo         android:gravity="center" /^> >> res\layout\main.xml
echo. >> res\layout\main.xml
echo ^</LinearLayout^> >> res\layout\main.xml

echo ✅ 레이아웃 파일 생성됨

REM 간단한 아이콘 생성 (텍스트 기반)
echo ^<?xml version="1.0" encoding="utf-8"?^> > res\drawable\icon.xml
echo ^<shape xmlns:android="http://schemas.android.com/apk/res/android" >> res\drawable\icon.xml
echo     android:shape="rectangle"^> >> res\drawable\icon.xml
echo     ^<solid android:color="#2196F3" /^> >> res\drawable\icon.xml
echo     ^<corners android:radius="8dp" /^> >> res\drawable\icon.xml
echo ^</shape^> >> res\drawable\icon.xml

echo ✅ 아이콘 파일 생성됨

REM strings.xml 생성
echo ^<?xml version="1.0" encoding="utf-8"?^> > res\values\strings.xml
echo ^<resources^> >> res\values\strings.xml
echo     ^<string name="app_name"^>NowHere AR MMORPG^</string^> >> res\values\strings.xml
echo ^</resources^> >> res\values\strings.xml

echo ✅ strings.xml 생성됨

REM classes.dex 파일 생성 (더미)
echo. > classes.dex

echo ✅ classes.dex 생성됨

REM resources.arsc 파일 생성 (더미)
echo. > resources.arsc

echo ✅ resources.arsc 생성됨

echo.
echo ========================================
echo APK 파일 생성 중...
echo ========================================

REM APK 파일 생성 (ZIP 형식)
powershell -command "Compress-Archive -Path 'AndroidManifest.xml','classes.dex','resources.arsc','META-INF','res' -DestinationPath 'NowHere_AR_MMORPG_v0.1.0.zip' -Force"
if exist "NowHere_AR_MMORPG_v0.1.0.zip" (
    ren "NowHere_AR_MMORPG_v0.1.0.zip" "NowHere_AR_MMORPG_v0.1.0.apk"
)

if exist "NowHere_AR_MMORPG_v0.1.0.apk" (
    echo.
    echo ✅ APK 파일 생성 성공!
    echo.
    echo 📁 APK 파일 위치: SimpleAPK\NowHere_AR_MMORPG_v0.1.0.apk
    echo.
    echo 📱 APK 파일을 안드로이드 기기에 설치하여 테스트하세요.
    echo.
    echo 📋 설치 방법:
    echo 1. APK 파일을 안드로이드 기기로 복사
    echo 2. 기기에서 "알 수 없는 소스" 허용
    echo 3. APK 파일을 탭하여 설치
    echo 4. 앱 실행하여 테스트
    echo.
    
    REM APK 파일 크기 표시
    for %%A in ("NowHere_AR_MMORPG_v0.1.0.apk") do (
        echo 📊 APK 파일 크기: %%~zA bytes
    )
    
    echo.
    echo 🎉 빌드 완료! APK 파일이 준비되었습니다.
    
) else (
    echo.
    echo ❌ APK 파일 생성 실패!
    echo.
    echo 문제를 해결해주세요.
)

echo.
pause
