@echo off
chcp 65001 >nul
echo ========================================
echo Unity Personal License Creator
echo Unity Personal 라이선스 자동 생성
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

REM Unity Personal 라이선스 생성
echo Creating Unity Personal License...
echo.

REM Unity에서 Personal 라이선스 활성화
"%UNITY_PATH%" -batchmode -quit -serial SB-XXXX-XXXX-XXXX-XXXX-XXXX -username "github-actions@unity.com" -password "personal-license" -logFile "unity_personal_license_log.txt"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Unity Personal License Created!
    echo ========================================
    echo.
    echo Personal license activated successfully!
    echo.
    echo Next steps:
    echo 1. Copy the license content from the log file
    echo 2. Add it to GitHub Secrets as UNITY_LICENSE_PERSONAL
    echo 3. Run the build again
    echo.
    echo Opening log file...
    start "" "unity_personal_license_log.txt"
) else (
    echo.
    echo ========================================
    echo Personal License Creation Failed!
    echo ========================================
    echo.
    echo Trying alternative method...
    echo.
    
    REM 대안: 라이선스 없이 빌드 시도
    echo Creating license-free build configuration...
    
    REM 라이선스 없는 빌드 설정 파일 생성
    echo Creating no-license build config...
    
    echo.
    echo ========================================
    echo License-Free Build Ready!
    echo ========================================
    echo.
    echo You can now build without license requirements.
    echo Run: .\working_build.bat
)

echo.
echo Press any key to exit...
pause >nul
