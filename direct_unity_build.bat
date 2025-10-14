@echo off
chcp 65001 >nul
echo ========================================
echo Direct Unity Command Line Build
echo Unity Editor 없이 직접 빌드
echo ========================================

REM Unity Editor 경로
set "UNITY_PATH=C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe"

REM 프로젝트 경로
set "PROJECT_PATH=%~dp0"

REM 빌드 폴더 생성
if not exist "%PROJECT_PATH%DirectBuilds" (
    mkdir "%PROJECT_PATH%DirectBuilds"
    echo Direct Build folder created: %PROJECT_PATH%DirectBuilds
)

echo.
echo Unity Editor Path: %UNITY_PATH%
echo Project Path: %PROJECT_PATH%
echo.

REM Unity 에디터가 존재하는지 확인
if not exist "%UNITY_PATH%" (
    echo Unity Editor not found at: %UNITY_PATH%
    echo Please check Unity installation path.
    pause
    exit /b 1
)

echo Unity Editor found! Starting build...
echo.

REM Unity Command Line Build 실행 (간단한 빌드 스크립트 사용)
"%UNITY_PATH%" -batchmode -quit -projectPath "%PROJECT_PATH%" -executeMethod "NowHere.Editor.SimpleDirectBuild.BuildAPK" -logFile "%PROJECT_PATH%DirectBuilds\direct_build_log.txt"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Direct Build Success!
    echo ========================================
    echo.
    echo Build completed without Unity Editor GUI!
    echo Check DirectBuilds folder for APK file.
    echo.
    echo Opening build folder...
    start "" "%PROJECT_PATH%DirectBuilds"
) else (
    echo.
    echo ========================================
    echo Direct Build Failed!
    echo ========================================
    echo.
    echo Check log file: %PROJECT_PATH%DirectBuilds\direct_build_log.txt
    echo.
    echo Possible issues:
    echo - Unity license not activated
    echo - Missing packages
    echo - Build script errors
)

echo.
echo Press any key to exit...
pause >nul