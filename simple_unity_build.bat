@echo off
chcp 65001 >nul
echo Unity Android APK Build Start...

REM Check if Unity Editor is running
tasklist /FI "IMAGENAME eq Unity.exe" 2>NUL | find /I /N "Unity.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo Unity Editor is running.
    echo Please select the following menu in Unity Editor:
    echo Build ^> Complete Android APK Build
    echo.
    echo OR
    echo Build ^> Simple Android APK
    echo.
    echo OR
    echo Build ^> Auto Build APK Now
    echo.
    pause
) else (
    echo Unity Editor is not running.
    echo Please run Unity Editor through Unity Hub first.
    pause
)
