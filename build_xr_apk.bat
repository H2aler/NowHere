@echo off
chcp 65001 >nul
echo ========================================
echo XR Android APK Build Start
echo VR/AR/MR Support
echo ========================================

REM Find Unity Editor Path
set "UNITY_PATH="
for /f "tokens=*" %%i in ('where Unity.exe 2^>nul') do (
    set "UNITY_PATH=%%i"
    goto :found_unity
)

echo Unity.exe not found.
echo Please run Unity Editor through Unity Hub first.
pause
exit /b 1

:found_unity
echo Unity Editor Found: %UNITY_PATH%

REM Project Path
set "PROJECT_PATH=%~dp0"

REM Create XR Build Folder
if not exist "%PROJECT_PATH%XRBuilds" (
    mkdir "%PROJECT_PATH%XRBuilds"
    echo XR Build folder created: %PROJECT_PATH%XRBuilds
)

echo.
echo Running XR build in Unity Editor...
echo Project Path: %PROJECT_PATH%
echo.

REM Execute XR Build Script in Unity Editor
"%UNITY_PATH%" -projectPath "%PROJECT_PATH%" -executeMethod "NowHere.Editor.XRBuildScript.BuildXRAndroidAPK" -batchmode -quit -logFile "%PROJECT_PATH%XRBuilds\xr_build_log.txt"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo XR Build completed successfully!
    echo ========================================
    echo.
    echo XR Features:
    echo - VR (Virtual Reality) Support
    echo - AR (Augmented Reality) Support  
    echo - MR (Mixed Reality) Support
    echo - Multiplayer Support
    echo - XR Combat System
    echo - Hand Tracking
    echo - Voice Commands
    echo - Performance Optimization
    echo.
    echo Opening XR build folder...
    start "" "%PROJECT_PATH%XRBuilds"
) else (
    echo.
    echo ========================================
    echo Error occurred during XR build.
    echo ========================================
    echo.
    echo Check log file: %PROJECT_PATH%XRBuilds\xr_build_log.txt
    echo.
    echo Make sure XR Plugin Management is installed:
    echo 1. Open Unity Editor
    echo 2. Window ^> Package Manager
    echo 3. Install XR Plugin Management
    echo 4. Install OpenXR Plugin
    echo 5. Install AR Foundation
)

echo.
echo Press any key to exit...
pause >nul
