@echo off
chcp 65001 >nul
echo ========================================
echo Unity License Activation
echo Unity 라이선스 활성화
echo ========================================

REM Unity Editor 경로
set "UNITY_PATH=C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe"

echo.
echo Unity Editor Path: %UNITY_PATH%
echo.

REM Unity 라이선스 활성화
echo Activating Unity license...
"%UNITY_PATH%" -batchmode -quit -serial SB-XXXX-XXXX-XXXX-XXXX-XXXX -username "your-email@example.com" -password "your-password"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Unity License Activated!
    echo ========================================
    echo.
    echo License activation successful!
    echo You can now build without Unity Editor GUI.
    echo.
    echo Try building again with: .\direct_unity_build.bat
) else (
    echo.
    echo ========================================
    echo License Activation Failed!
    echo ========================================
    echo.
    echo Please activate Unity license manually:
    echo 1. Open Unity Hub
    echo 2. Go to Settings
    echo 3. Activate Unity Personal License
    echo.
    echo Or use Unity Editor once to activate license.
)

echo.
echo Press any key to exit...
pause >nul
