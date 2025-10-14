#!/bin/bash

echo "========================================"
echo "NowHere AR MMORPG APK 빌드 스크립트"
echo "========================================"

# Unity 설치 경로 확인 (macOS/Linux)
UNITY_PATH="/Applications/Unity/Hub/Editor/2023.2.20f1/Unity.app/Contents/MacOS/Unity"
if [ ! -f "$UNITY_PATH" ]; then
    echo "Unity를 찾을 수 없습니다. 경로를 확인해주세요."
    echo "기본 경로: $UNITY_PATH"
    echo ""
    echo "Unity Hub에서 설치된 Unity 버전의 경로를 확인하고"
    echo "이 스크립트의 UNITY_PATH 변수를 수정해주세요."
    read -p "계속하려면 Enter를 누르세요..."
    exit 1
fi

echo "Unity 경로 확인됨: $UNITY_PATH"

# 프로젝트 경로 설정
PROJECT_PATH=$(pwd)
echo "프로젝트 경로: $PROJECT_PATH"

# 빌드 출력 경로
BUILD_PATH="$PROJECT_PATH/Builds/Android"
mkdir -p "$BUILD_PATH"

echo ""
echo "========================================"
echo "빌드 옵션을 선택하세요:"
echo "========================================"
echo "1. 개발 빌드 (디버깅 활성화)"
echo "2. 테스트 빌드 (안정성 테스트)"
echo "3. 릴리즈 빌드 (최종 배포)"
echo "4. 자동 빌드 (모든 옵션)"
echo ""
read -p "선택 (1-4): " choice

case $choice in
    1)
        echo ""
        echo "========================================"
        echo "개발 빌드 시작..."
        echo "========================================"
        "$UNITY_PATH" -batchmode -quit -projectPath "$PROJECT_PATH" -buildTarget Android -executeMethod NowHere.Testing.TestBuildManager.BuildDevelopmentAPK
        ;;
    2)
        echo ""
        echo "========================================"
        echo "테스트 빌드 시작..."
        echo "========================================"
        "$UNITY_PATH" -batchmode -quit -projectPath "$PROJECT_PATH" -buildTarget Android -executeMethod NowHere.Testing.TestBuildManager.BuildTestAPK
        ;;
    3)
        echo ""
        echo "========================================"
        echo "릴리즈 빌드 시작..."
        echo "========================================"
        "$UNITY_PATH" -batchmode -quit -projectPath "$PROJECT_PATH" -buildTarget Android -executeMethod NowHere.Utils.BuildManager.BuildAPK
        ;;
    4)
        echo ""
        echo "========================================"
        echo "자동 빌드 시작 (모든 옵션)..."
        echo "========================================"
        echo "개발 빌드 중..."
        "$UNITY_PATH" -batchmode -quit -projectPath "$PROJECT_PATH" -buildTarget Android -executeMethod NowHere.Testing.TestBuildManager.BuildDevelopmentAPK
        echo ""
        echo "테스트 빌드 중..."
        "$UNITY_PATH" -batchmode -quit -projectPath "$PROJECT_PATH" -buildTarget Android -executeMethod NowHere.Testing.TestBuildManager.BuildTestAPK
        echo ""
        echo "릴리즈 빌드 중..."
        "$UNITY_PATH" -batchmode -quit -projectPath "$PROJECT_PATH" -buildTarget Android -executeMethod NowHere.Utils.BuildManager.BuildAPK
        ;;
    *)
        echo "잘못된 선택입니다. 1-4 중에서 선택해주세요."
        exit 1
        ;;
esac

echo ""
echo "========================================"
echo "빌드 완료!"
echo "========================================"
echo "빌드 파일 위치: $BUILD_PATH"
echo ""
echo "생성된 파일들:"
ls -la "$BUILD_PATH"/*.apk 2>/dev/null
ls -la "$BUILD_PATH"/*.aab 2>/dev/null
echo ""
echo "APK 파일을 안드로이드 기기에 설치하여 테스트하세요."
echo ""
echo "설치 방법:"
echo "1. APK 파일을 안드로이드 기기로 복사"
echo "2. 기기에서 '알 수 없는 소스' 허용"
echo "3. APK 파일을 탭하여 설치"
echo ""
read -p "계속하려면 Enter를 누르세요..."
