#!/bin/bash

echo "========================================"
echo "NowHere AR MMORPG 빠른 빌드"
echo "========================================"

# Unity 경로 (수정 필요)
UNITY_PATH="/Applications/Unity/Hub/Editor/2023.2.20f1/Unity.app/Contents/MacOS/Unity"

# 프로젝트 경로
PROJECT_PATH=$(pwd)

echo "Unity 경로: $UNITY_PATH"
echo "프로젝트 경로: $PROJECT_PATH"

# Unity 존재 확인
if [ ! -f "$UNITY_PATH" ]; then
    echo ""
    echo "❌ Unity를 찾을 수 없습니다!"
    echo ""
    echo "Unity Hub에서 Unity 2023.2.20f1을 설치하고"
    echo "이 스크립트의 UNITY_PATH를 수정해주세요."
    echo ""
    echo "현재 설정된 경로: $UNITY_PATH"
    echo ""
    read -p "계속하려면 Enter를 누르세요..."
    exit 1
fi

echo "✅ Unity 경로 확인됨"

# 빌드 폴더 생성
mkdir -p "Builds/Android"

echo ""
echo "========================================"
echo "테스트 APK 빌드 시작..."
echo "========================================"

# 테스트 APK 빌드
"$UNITY_PATH" -batchmode -quit -projectPath "$PROJECT_PATH" -buildTarget Android -executeMethod NowHere.Editor.BuildScript.BuildTestAPKFromCommandLine

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ 빌드 성공!"
    echo ""
    echo "📁 빌드 파일 위치: Builds/Android/"
    echo ""
    echo "📱 APK 파일을 안드로이드 기기에 설치하여 테스트하세요."
    echo ""
    echo "📋 테스트 방법:"
    echo "1. APK 파일을 안드로이드 기기로 복사"
    echo "2. 기기에서 '알 수 없는 소스' 허용"
    echo "3. APK 파일을 탭하여 설치"
    echo "4. 앱 실행 후 T 키로 테스트 패널 열기"
    echo ""
    
    # 생성된 파일 목록 표시
    echo "📄 생성된 파일들:"
    ls -la "Builds/Android"/*.apk 2>/dev/null
    if [ $? -ne 0 ]; then
        echo "APK 파일이 생성되지 않았습니다."
    fi
    
else
    echo ""
    echo "❌ 빌드 실패!"
    echo ""
    echo "Unity 로그를 확인해주세요:"
    echo "macOS: ~/Library/Logs/Unity/Editor.log"
    echo "Linux: ~/.config/unity3d/Editor.log"
    echo ""
fi

echo ""
read -p "계속하려면 Enter를 누르세요..."
