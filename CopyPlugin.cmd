@echo off
chcp 65001 >nul

:: Request admin rights
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Requesting administrator privileges...
    powershell start-process "%~f0" -verb runas
    exit /b
)

echo [Plugin Deployment Script] Current path: %cd%
echo.

set "targetPath=D:\Documents\My Games\Terraria\TerraAngel\Plugins\MyPlugin.TAPlugin.dll"
set "sourcePath=D:\游戏\Terraria\服务器\插件源码\TAPlugin\bin\Release\net8.0\MyPlugin.TAPlugin.dll"
set "pluginDir=D:\Documents\My Games\Terraria\TerraAngel\Plugins"
set "jsonFile=D:\Documents\My Games\Terraria\TerraAngel\MyPlugin.json"

:: Delete old DLL file
if exist "%targetPath%" (
    echo Deleting old plugin: "%targetPath%"
    del /F /Q "%targetPath%"
    if %ERRORLEVEL% == 0 (
        echo ✅ Deleted successfully.
    ) else (
        echo ❌ Failed to delete, error code: %ERRORLEVEL%
        goto pause_and_exit
    )
) else (
    echo ⚠️ Old plugin not found, skipping deletion.
)

:: Delete JSON file if exists
if exist "%jsonFile%" (
    echo Deleting JSON file: "%jsonFile%"
    del /F /Q "%jsonFile%"
    if %ERRORLEVEL% == 0 (
        echo ✅ JSON file deleted successfully.
    ) else (
        echo ❌ Failed to delete JSON file, error code: %ERRORLEVEL%
        goto pause_and_exit
    )
) else (
    echo ⚠️ JSON file not found, skipping deletion.
)

:: Copy new DLL file
if exist "%sourcePath%" (
    echo Copying new plugin: "%sourcePath%"
    copy /Y "%sourcePath%" "%pluginDir%\"
    if %ERRORLEVEL% == 0 (
        echo ✅ Copied successfully.
    ) else (
        echo ❌ Failed to copy, error code: %ERRORLEVEL%
        goto pause_and_exit
    )
) else (
    echo ❌ Source file does not exist. Please make sure the plugin was built.
    goto pause_and_exit
)

echo.
echo ✅ Plugin deployed successfully!

:pause_and_exit
exit /b 1