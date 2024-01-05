@echo off
REM This script exports add-on packages (.unitypackages) of this Microgame.
REM Each subfolder in ASSETPATH represents an asset pack and is packaged separately.
REM Pass in path for Unity.exe as the first argument, e.g. "C:\Program Files\Unity\Hub\Editor\2018.4.14f1\Editor\Unity.exe"

setlocal EnableDelayedExpansion

set UNITYPATH=%~1
REM Default to what Yamato has if no argument provided
if "%UNITYPATH%"=="" set UNITYPATH=.Editor\Unity.exe
set PROJECTPATH=%CD%
set ASSETPATH=Assets\AddOns
set OUTPUTDIR=addons

if not exist "%UNITYPATH%". (
    echo Unity.exe not found from "%UNITYPATH%", set UNITYPATH accordingly. Aborting.
    exit /b 1
)

if not exist "%ASSETPATH%". (
    echo "%PROJECTPATH%\%ASSETPATH%" does not exist, aborting.
    exit /b 1
)

if not exist "%OUTPUTDIR%". mkdir "%OUTPUTDIR%"

for /f "delims=" %%i in ('dir %ASSETPATH% /ad /b') do (
    set INPUT=!ASSETPATH!\%%i
    set OUTPUT=!OUTPUTDIR!\%%i.unitypackage
    echo Exporting !OUTPUT! from "!INPUT!"...
    "%UNITYPATH%" -quit -batchmode -projectPath "%PROJECTPATH%" -exportPackage !INPUT! !OUTPUT!
    if not ERRORLEVEL 0 exit /b %ERRORLEVEL%
)
