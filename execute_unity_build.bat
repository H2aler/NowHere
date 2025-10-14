@echo off
chcp 65001 >nul
echo ========================================
echo Unity Android APK Build Start
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

REM Create Build Folder
if not exist "%PROJECT_PATH%UnityBuilds" (
    mkdir "%PROJECT_PATH%UnityBuilds"
    echo Build folder created: %PROJECT_PATH%UnityBuilds
)

echo.
echo Running build in Unity Editor...
echo Project Path: %PROJECT_PATH%
echo.

REM Execute Build Script in Unity Editor
"%UNITY_PATH%" -projectPath "%PROJECT_PATH%" -executeMethod "NowHere.Editor.CompleteBuildScript.CompleteAndroidBuild" -batchmode -quit -logFile "%PROJECT_PATH%UnityBuilds\build_log.txt"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Build completed successfully!
    echo ========================================
    echo.
    echo Opening build folder...
    start "" "%PROJECT_PATH%UnityBuilds"
) else (
    echo.
    echo ========================================
    echo Error occurred during build.
    echo ========================================
    echo.
    echo Check log file: %PROJECT_PATH%UnityBuilds\build_log.txt
)

echo.
echo Press any key to exit...
pause >nul
