#!/bin/sh

# This script:
# - removes *empty* folders potential paid assets that are developed in the project,
# - replaces dev dependencies with prod dependencies, and
# - packages the Microgame as a template.
# Supported arguments:
# - %1: force -- force removal of submodule folders even if they are not empty

UPM_CI=stable

npm
ERRORLEVEL=$?
if [ $ERRORLEVEL -eq 0 ]
then
    echo ERROR: npm not in PATH, make sure Node.js is installed and in PATH, https://nodejs.org/en/
    exit $ERRORLEVEL
fi

# Delete empty folders for submodules
if ["$1"=="force"] 
then
    rm -rf Assets/AddOns
else 
    rm -r -i Assets/AddOns
fi

rm -f Assets/AddOns.meta

# Replace dev dependencies with prod dependencies, stash potential current changes
git stash push -- Packages/manifest.json
\cp -rf manifest-prod.json Packages/manifest.json

# Package as template
npm install upm-ci-utils@$UPM_CI -g --registry https://artifactory.prd.cds.internal.unity3d.com/artifactory/api/npm/upm-npm
upm-ci template pack
RET=$?
# Make sure we return the exit code of "upm-ci template pack" for Yamato


# Restore manifest.json
git checkout -- Packages/manifest.json
git stash pop

exit $RET
