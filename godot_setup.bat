@echo off
chcp 65001 >nul
echo ========================================
echo Godot Engine Setup
echo Unity 대신 Godot으로 게임 개발
echo ========================================

echo.
echo Godot Engine을 사용하여 NowHere 게임을 개발합니다.
echo Unity의 라이선스 문제를 피하고 완전 무료로 개발할 수 있습니다.
echo.

REM Godot 다운로드 URL
set "GODOT_URL=https://downloads.tuxfamily.org/godotengine/4.2.2/Godot_v4.2.2-stable_win64.exe.zip"

echo.
echo ========================================
echo Godot Engine 다운로드
echo ========================================
echo.
echo Godot 4.2.2 다운로드 중...
echo URL: %GODOT_URL%
echo.

REM Godot 다운로드
echo Downloading Godot Engine...
powershell -Command "Invoke-WebRequest -Uri '%GODOT_URL%' -OutFile 'Godot_v4.2.2-stable_win64.exe.zip'"

if exist "Godot_v4.2.2-stable_win64.exe.zip" (
    echo.
    echo ========================================
    echo Godot 다운로드 완료!
    echo ========================================
    echo.
    echo 압축 해제 중...
    
    REM 압축 해제
    powershell -Command "Expand-Archive -Path 'Godot_v4.2.2-stable_win64.exe.zip' -DestinationPath 'Godot' -Force"
    
    if exist "Godot\Godot_v4.2.2-stable_win64.exe" (
        echo.
        echo ========================================
        echo Godot 설치 완료!
        echo ========================================
        echo.
        echo Godot Engine이 성공적으로 설치되었습니다!
        echo.
        echo 다음 단계:
        echo 1. Godot 실행
        echo 2. 새 프로젝트 생성
        echo 3. NowHere 게임 개발 시작
        echo.
        echo Godot 실행 중...
        start "" "Godot\Godot_v4.2.2-stable_win64.exe"
        
        echo.
        echo ========================================
        echo NowHere Godot 프로젝트 생성
        echo ========================================
        echo.
        
        REM NowHere Godot 프로젝트 폴더 생성
        if not exist "NowHere_Godot" (
            mkdir "NowHere_Godot"
            echo NowHere_Godot 프로젝트 폴더 생성됨
        )
        
        REM Godot 프로젝트 파일 생성
        echo Creating Godot project file...
        echo [application] > "NowHere_Godot\project.godot"
        echo config/name="NowHere AR/VR/XR MMORPG" >> "NowHere_Godot\project.godot"
        echo config/description="Complete AR/VR/XR MMORPG built with Godot" >> "NowHere_Godot\project.godot"
        echo config/version="1.0.0" >> "NowHere_Godot\project.godot"
        echo config/features=PackedStringArray("4.2", "Mobile") >> "NowHere_Godot\project.godot"
        echo. >> "NowHere_Godot\project.godot"
        echo [rendering] >> "NowHere_Godot\project.godot"
        echo renderer/rendering_method="gl_compatibility" >> "NowHere_Godot\project.godot"
        echo renderer/rendering_method.mobile="gl_compatibility" >> "NowHere_Godot\project.godot"
        
        echo.
        echo ========================================
        echo Godot 프로젝트 생성 완료!
        echo ========================================
        echo.
        echo 프로젝트 위치: %CD%\NowHere_Godot
        echo.
        echo Godot에서 프로젝트를 열어서 게임 개발을 시작하세요!
        echo.
        echo 프로젝트 폴더 열기...
        start "" "NowHere_Godot"
        
    ) else (
        echo.
        echo ========================================
        echo Godot 설치 실패!
        echo ========================================
        echo.
        echo 압축 해제에 실패했습니다.
        echo 수동으로 다운로드하세요: https://godotengine.org/download
    )
    
    REM 임시 파일 정리
    del "Godot_v4.2.2-stable_win64.exe.zip"
    
) else (
    echo.
    echo ========================================
    echo Godot 다운로드 실패!
    echo ========================================
    echo.
    echo 인터넷 연결을 확인하고 다시 시도하세요.
    echo.
    echo 수동 다운로드: https://godotengine.org/download
    echo.
    echo Godot 4.2.2 다운로드 후:
    echo 1. 압축 해제
    echo 2. Godot 실행
    echo 3. 새 프로젝트 생성
    echo 4. NowHere 게임 개발 시작
)

echo.
echo ========================================
echo Godot Engine 장점
echo ========================================
echo.
echo ✅ 완전 무료 (라이선스 문제 없음)
echo ✅ 가벼움 (50MB 정도)
echo ✅ 빠른 빌드
echo ✅ AR/VR 지원 (OpenXR, ARCore)
echo ✅ Android APK 직접 빌드
echo ✅ GitHub Actions 자동 빌드
echo ✅ GDScript (Python-like 언어)
echo ✅ C# 지원
echo.

echo Press any key to exit...
pause >nul
