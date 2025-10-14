@echo off
chcp 65001 >nul
echo ========================================
echo Android Studio Build Method
echo Unity Export + Android Studio Build
echo ========================================

REM Unity Editor 경로
set "UNITY_PATH=C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe"

REM 프로젝트 경로
set "PROJECT_PATH=%~dp0"

REM Android Studio 프로젝트 경로
set "ANDROID_PROJECT_PATH=%PROJECT_PATH%AndroidStudioProject"

echo.
echo Step 1: Export Unity Project to Android Studio
echo Unity Editor Path: %UNITY_PATH%
echo Project Path: %PROJECT_PATH%
echo Android Project Path: %ANDROID_PROJECT_PATH%
echo.

REM Android Studio 프로젝트 폴더 생성
if not exist "%ANDROID_PROJECT_PATH%" (
    mkdir "%ANDROID_PROJECT_PATH%"
    echo Android Studio project folder created: %ANDROID_PROJECT_PATH%
)

echo.
echo Exporting Unity project to Android Studio format...
echo This will create an Android Studio project that can be built without Unity Editor.
echo.

REM Unity에서 Android Studio 프로젝트로 Export
"%UNITY_PATH%" -batchmode -quit -projectPath "%PROJECT_PATH%" -buildTarget Android -exportPackage "%ANDROID_PROJECT_PATH%" -logFile "%PROJECT_PATH%android_export_log.txt"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Unity Export Success!
    echo ========================================
    echo.
    echo Unity project exported to: %ANDROID_PROJECT_PATH%
    echo.
    echo Next steps:
    echo 1. Open Android Studio
    echo 2. Open the exported project: %ANDROID_PROJECT_PATH%
    echo 3. Build APK in Android Studio
    echo.
    echo Opening Android Studio project folder...
    start "" "%ANDROID_PROJECT_PATH%"
) else (
    echo.
    echo ========================================
    echo Unity Export Failed!
    echo ========================================
    echo.
    echo Check log file: %PROJECT_PATH%android_export_log.txt
    echo.
    echo Alternative: Manual export in Unity Editor
    echo File ^> Build Settings ^> Export Project
)

echo.
echo Press any key to exit...
pause >nul