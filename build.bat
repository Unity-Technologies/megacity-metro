@echo off
REM This script:
REM - removes *empty* folders potential paid assets that are developed in the project,
REM - replaces dev dependencies with prod dependencies, and
REM - packages the Microgame as a template.
REM Supported arguments:
REM - %1: force -- force removal of submodule folders even if they are not empty

set UPM_CI=stable

where /q npm
if not %ERRORLEVEL%==0 (
    echo ERROR: npm not in PATH, make sure Node.js is installed and in PATH, https://nodejs.org/en/
    goto :EOF
)

REM Delete empty folders for submodules
if "%1"=="force" (
    rmdir /q /s Assets\AddOns
) else (
    rmdir Assets\AddOns
)

if exist Assets\AddOns.meta. del Assets\AddOns.meta

REM Replace dev dependencies with prod dependencies, stash potential current changes
git stash push -- Packages\manifest.json
copy /y manifest-prod.json Packages\manifest.json

REM Package as template
call npm install upm-ci-utils@%UPM_CI% -g --registry https://artifactory.prd.cds.internal.unity3d.com/artifactory/api/npm/upm-npm
call upm-ci template pack
REM Make sure we return the exit code of "upm-ci template pack" for Yamato
set RET=%ERRORLEVEL%

REM Restore manifest.json
git checkout -- Packages\manifest.json
git stash pop

exit /b %RET%
