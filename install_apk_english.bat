@echo off
chcp 65001 >nul
echo ========================================
echo NowHere AR MMORPG APK Installer
echo ========================================

REM Check APK file
if not exist "ProperAPK\NowHere_AR_MMORPG_Proper_v0.1.0.apk" (
    echo ERROR: APK file not found!
    echo Please run build_proper_apk.bat first.
    pause
    exit /b 1
)

echo APK file found: ProperAPK\NowHere_AR_MMORPG_Proper_v0.1.0.apk

REM Show APK file info
for %%A in ("ProperAPK\NowHere_AR_MMORPG_Proper_v0.1.0.apk") do (
    echo APK file size: %%~zA bytes
    echo Created: %%~tA
)

echo.
echo ========================================
echo Installation Options
echo ========================================

echo 1. Copy APK to Downloads folder
echo 2. Show APK file location
echo 3. Exit
echo.

set /p choice="Choose option (1-3): "

if "%choice%"=="1" goto copy_to_downloads
if "%choice%"=="2" goto show_location
if "%choice%"=="3" goto end
goto invalid_choice

:copy_to_downloads
echo.
echo ========================================
echo Copying APK to Downloads folder
echo ========================================

REM Check Downloads folder
if not exist "%USERPROFILE%\Downloads" (
    echo ERROR: Downloads folder not found!
    goto end
)

REM Copy APK file
copy "ProperAPK\NowHere_AR_MMORPG_Proper_v0.1.0.apk" "%USERPROFILE%\Downloads\"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo SUCCESS: APK file copied to Downloads folder!
    echo.
    echo Location: %USERPROFILE%\Downloads\NowHere_AR_MMORPG_Proper_v0.1.0.apk
    echo.
    echo Next steps:
    echo 1. Transfer this file to your Android device
    echo 2. Enable "Unknown sources" in device settings
    echo 3. Tap the APK file to install
    echo 4. Run the app to test
    echo.
) else (
    echo.
    echo ERROR: Failed to copy APK file!
    echo.
)
goto end

:show_location
echo.
echo ========================================
echo APK File Information
echo ========================================

echo APK file location: %CD%\ProperAPK\NowHere_AR_MMORPG_Proper_v0.1.0.apk
echo.
echo App information:
echo - Package name: com.nowhere.armmorpg
echo - Version: 0.1.0
echo - Size: 4506 bytes
echo - Built with: Android SDK AAPT
echo.
echo To install manually:
echo 1. Copy the APK file to your Android device
echo 2. Enable "Unknown sources" in device settings
echo 3. Tap the APK file to install
echo 4. Run the app to test
echo.
goto end

:invalid_choice
echo.
echo ERROR: Invalid choice. Please choose 1-3.
goto end

:end
echo.
pause
