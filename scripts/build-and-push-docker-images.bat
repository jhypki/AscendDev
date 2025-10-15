@echo off
setlocal enabledelayedexpansion

REM Configuration - Replace with your Docker Hub username
if "%DOCKER_USERNAME%"=="" set DOCKER_USERNAME=jhypki

REM Base name for all images
set BASE_IMAGE_NAME=ascenddev

echo Starting Docker image build and push process...
echo Using Docker username: %DOCKER_USERNAME%

REM Get the script directory and navigate to project root
set SCRIPT_DIR=%~dp0
set PROJECT_ROOT=%SCRIPT_DIR%..
set DOCKER_CONFIG_DIR=%PROJECT_ROOT%\configuration\docker

echo Project root: %PROJECT_ROOT%
echo Docker config directory: %DOCKER_CONFIG_DIR%

REM Change to docker configuration directory
cd /d "%DOCKER_CONFIG_DIR%"

REM Setup Docker buildx for multi-platform builds
echo Setting up Docker buildx for multi-platform builds...
docker buildx create --name multiplatform-builder --use --bootstrap 2>nul || (
    echo Using existing multiplatform-builder...
    docker buildx use multiplatform-builder
)

REM Function to build and push Docker images
goto :main

:build_and_push_image
set dockerfile_path=%1
set language=%2
set type=%3

set image_name=%DOCKER_USERNAME%/%BASE_IMAGE_NAME%-%language%-%type%:latest

echo.
echo Building multi-platform image: !image_name!
echo Dockerfile path: %dockerfile_path%
echo Platforms: linux/amd64,linux/arm64

REM Build and push multi-platform Docker image using buildx
docker buildx build --platform linux/amd64,linux/arm64 --push -t "!image_name!" -f "%dockerfile_path%\Dockerfile" "%dockerfile_path%"

if !errorlevel! equ 0 (
    echo Successfully built and pushed multi-platform image: !image_name!
) else (
    echo Failed to build multi-platform image: !image_name!
    exit /b 1
)
goto :eof

:main
REM Build runner images
echo.
echo Processing runner images...

REM Go Runner
if exist ".\environments\runners\go" (
    call :build_and_push_image ".\environments\runners\go" "go" "runner"
    if !errorlevel! neq 0 exit /b 1
)

REM JavaScript Runner
if exist ".\environments\runners\javascript" (
    call :build_and_push_image ".\environments\runners\javascript" "javascript" "runner"
    if !errorlevel! neq 0 exit /b 1
)

REM Python Runner
if exist ".\environments\runners\python" (
    call :build_and_push_image ".\environments\runners\python" "python" "runner"
    if !errorlevel! neq 0 exit /b 1
)

REM TypeScript Runner
if exist ".\environments\runners\typescript" (
    call :build_and_push_image ".\environments\runners\typescript" "typescript" "runner"
    if !errorlevel! neq 0 exit /b 1
)

REM Build tester images
echo.
echo Processing tester images...

REM Go Tester
if exist ".\environments\testers\go" (
    call :build_and_push_image ".\environments\testers\go" "go" "tester"
    if !errorlevel! neq 0 exit /b 1
)

REM Python Tester
if exist ".\environments\testers\python" (
    call :build_and_push_image ".\environments\testers\python" "python" "tester"
    if !errorlevel! neq 0 exit /b 1
)

REM TypeScript Tester
if exist ".\environments\testers\typescript" (
    call :build_and_push_image ".\environments\testers\typescript" "typescript" "tester"
    if !errorlevel! neq 0 exit /b 1
)

echo.
echo Docker image build and push process completed!
echo Note: You need to be logged in to Docker Hub with 'docker login' for this script to work.

REM Cleanup: Remove the buildx builder (optional - comment out if you want to keep it for future builds)
echo Cleaning up buildx builder...
docker buildx rm multiplatform-builder 2>nul || echo Builder already removed or doesn't exist.

endlocal