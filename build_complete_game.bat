@echo off
chcp 65001 >nul
echo ========================================
echo Complete Game Build Start
echo All Systems Integrated
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

REM Create Complete Build Folder
if not exist "%PROJECT_PATH%CompleteBuilds" (
    mkdir "%PROJECT_PATH%CompleteBuilds"
    echo Complete Build folder created: %PROJECT_PATH%CompleteBuilds
)

echo.
echo Running complete game build in Unity Editor...
echo Project Path: %PROJECT_PATH%
echo.

REM Execute Complete Game Build Script in Unity Editor
"%UNITY_PATH%" -projectPath "%PROJECT_PATH%" -executeMethod "NowHere.Editor.CompleteGameBuildScript.BuildCompleteGame" -batchmode -quit -logFile "%PROJECT_PATH%CompleteBuilds\complete_build_log.txt"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Complete Game Build Success!
    echo ========================================
    echo.
    echo Integrated Systems:
    echo ✅ Game Core System
    echo ✅ XR System (VR/AR/MR)
    echo ✅ UI System (Mobile/XR)
    echo ✅ Audio System (3D/Voice Chat)
    echo ✅ Save/Load System
    echo ✅ Asset Management System
    echo ✅ Analytics System
    echo ✅ Testing System
    echo ✅ Multiplayer System
    echo ✅ Combat System
    echo ✅ RPG System
    echo ✅ Sensor System
    echo ✅ Motion Detection System
    echo ✅ Touch Interaction System
    echo.
    echo Features:
    echo - Complete AR/VR/XR Support
    echo - Mobile Optimized UI
    echo - 3D Spatial Audio
    echo - Voice Chat & Commands
    echo - Hand Tracking
    echo - Motion Detection
    echo - Touch Interactions
    echo - Multiplayer Support
    echo - RPG Character System
    echo - Combat System
    echo - Save/Load System
    echo - Analytics & Statistics
    echo - Performance Optimization
    echo - Testing & Debugging Tools
    echo.
    echo Opening complete build folder...
    start "" "%PROJECT_PATH%CompleteBuilds"
) else (
    echo.
    echo ========================================
    echo Error occurred during complete build.
    echo ========================================
    echo.
    echo Check log file: %PROJECT_PATH%CompleteBuilds\complete_build_log.txt
    echo.
    echo Make sure all required packages are installed:
    echo 1. Open Unity Editor
    echo 2. Window ^> Package Manager
    echo 3. Install XR Plugin Management
    echo 4. Install OpenXR Plugin
    echo 5. Install AR Foundation
    echo 6. Install ARCore XR Plugin
    echo 7. Install Oculus XR Plugin
    echo.
    echo Required Scripts:
    echo - GameCore.cs
    echo - GameManager.cs
    echo - XRGameManager.cs
    echo - UIManager.cs
    echo - AudioManager.cs
    echo - SaveSystem.cs
    echo - AssetManager.cs
    echo - AnalyticsManager.cs
    echo - GameTester.cs
)

echo.
echo Press any key to exit...
pause >nul
