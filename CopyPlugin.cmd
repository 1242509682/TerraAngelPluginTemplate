@echo off
chcp 65001 >nul

echo [DeployPlugin] Starting deployment...
echo Project: %cd%
echo Target: %1

if "%~1"=="" (
    echo ❌ Error: Missing target DLL parameter
    echo Usage: %0 [PathToDLL]
    exit /b 1
)

:: 使用相对路径参数
set "TARGET_DIR=%~dp0..\..\..\Documents\My Games\Terraria\TerraAngel\Plugins\"
set "JSON_FILE=%~dp0..\..\..\Documents\My Games\Terraria\TerraAngel\MyPlugin.json"

:: 删除旧的 JSON 文件
if exist "%JSON_FILE%" (
    echo Deleting JSON file: "%JSON_FILE%"
    del /F /Q "%JSON_FILE%"
    if %errorlevel% equ 0 (
        echo ✅ JSON file deleted
    ) else (
        echo ❌ Failed to delete JSON file
        exit /b 1
    )
)

:: 复制新的 DLL 文件
echo Copying plugin: "%~1"
echo To: "%TARGET_DIR%"

if not exist "%TARGET_DIR%" (
    echo Creating directory: %TARGET_DIR%
    mkdir "%TARGET_DIR%"
)

copy /Y "%~1" "%TARGET_DIR%"
if %errorlevel% equ 0 (
    echo ✅ Plugin copied successfully
    exit /b 0
) else (
    echo ❌ Failed to copy DLL
    echo Check:
    echo  1. Terraria is not running
    echo  2. Target directory exists
    exit /b 1
)