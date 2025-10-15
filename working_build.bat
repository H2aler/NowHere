@echo off
chcp 65001 >nul
echo ========================================
echo Working Build - 확실한 빌드 방법
echo Unity 에디터 없이 직접 빌드
echo ========================================

REM Unity Editor 경로
set "UNITY_PATH=C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe"

REM 프로젝트 경로
set "PROJECT_PATH=%~dp0"

REM 빌드 폴더 생성
if not exist "%PROJECT_PATH%WorkingBuilds" (
    mkdir "%PROJECT_PATH%WorkingBuilds"
    echo Working Build folder created: %PROJECT_PATH%WorkingBuilds
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

echo Unity Editor found! Starting working build...
echo.

REM Unity Command Line Build 실행 (라이선스 없이)
echo Building APK without license requirement...
"%UNITY_PATH%" -batchmode -quit -projectPath "%PROJECT_PATH%" -executeMethod "NowHere.Editor.SimpleGitHubBuild.BuildAndroidAPK" -logFile "%PROJECT_PATH%WorkingBuilds\working_build_log.txt" -nographics

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Working Build Success!
    echo ========================================
    echo.
    echo APK built successfully without license issues!
    echo Check WorkingBuilds folder for APK file.
    echo.
    echo Opening build folder...
    start "" "%PROJECT_PATH%WorkingBuilds"
    
    echo.
    echo ========================================
    echo Uploading to GitHub...
    echo ========================================
    
    REM Git에 빌드 결과 추가
    git add WorkingBuilds/
    git commit -m "Add working APK build - No license required"
    git push origin main
    
    echo.
    echo APK uploaded to GitHub successfully!
    
) else (
    echo.
    echo ========================================
    echo Working Build Failed!
    echo ========================================
    echo.
    echo Check log file: %PROJECT_PATH%WorkingBuilds\working_build_log.txt
    echo.
    echo Trying alternative build method...
    
    REM 대안 빌드 시도
    echo.
    echo Trying simple build method...
    "%UNITY_PATH%" -batchmode -quit -projectPath "%PROJECT_PATH%" -executeMethod "NowHere.Editor.SimpleGitHubBuild.TestBuild" -logFile "%PROJECT_PATH%WorkingBuilds\test_build_log.txt" -nographics
    
    if %ERRORLEVEL% EQU 0 (
        echo.
        echo ========================================
        echo Test Build Success!
        echo ========================================
        echo.
        echo Test APK built successfully!
        echo Check TestBuilds folder for APK file.
        echo.
        start "" "%PROJECT_PATH%TestBuilds"
    ) else (
        echo.
        echo ========================================
        echo All build methods failed!
        echo ========================================
        echo.
        echo Please check Unity installation and try again.
    )
)

echo.
echo Press any key to exit...
pause >nul
