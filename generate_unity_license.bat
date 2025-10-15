@echo off
chcp 65001 >nul
echo ========================================
echo Unity License Generator
echo Unity 라이선스 파일 생성
echo ========================================

REM Unity Editor 경로
set "UNITY_PATH=C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe"

echo.
echo Unity Editor Path: %UNITY_PATH%
echo.

REM Unity 에디터가 존재하는지 확인
if not exist "%UNITY_PATH%" (
    echo Unity Editor not found at: %UNITY_PATH%
    echo Please check Unity installation path.
    pause
    exit /b 1
)

echo Unity Editor found!
echo.

REM 라이선스 파일 생성
echo Generating Unity license file...
echo.

REM Unity에서 라이선스 파일 생성
"%UNITY_PATH%" -batchmode -quit -createManualActivationFile -logFile "unity_license_log.txt"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo License file generation completed!
    echo ========================================
    echo.
    echo License file should be created in Unity Editor directory.
    echo.
    echo Next steps:
    echo 1. Find the generated .alf file
    echo 2. Upload it to Unity website for activation
    echo 3. Download the .ulf file
    echo 4. Add the .ulf content to GitHub Secrets
    echo.
    echo Opening Unity Editor directory...
    start "" "C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor"
) else (
    echo.
    echo ========================================
    echo License file generation failed!
    echo ========================================
    echo.
    echo Check log file: unity_license_log.txt
    echo.
    echo Alternative: Use Unity Hub to manage license
    echo 1. Open Unity Hub
    echo 2. Go to Settings
    echo 3. License Management
    echo 4. Save License
)

echo.
echo Press any key to exit...
pause >nul
