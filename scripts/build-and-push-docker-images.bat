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

REM Function to build and push Docker images
goto :main

:build_and_push_image
set dockerfile_path=%1
set language=%2
set type=%3

set image_name=%DOCKER_USERNAME%/%BASE_IMAGE_NAME%-%language%-%type%:latest

echo.
echo Building: !image_name!
echo Dockerfile path: %dockerfile_path%

REM Build the Docker image specifically for Ubuntu platform
docker build --platform linux/amd64 -t "!image_name!" -f "%dockerfile_path%\Dockerfile" "%dockerfile_path%"

if !errorlevel! equ 0 (
    echo Successfully built: !image_name!
    
    REM Push the image
    echo Pushing: !image_name!
    docker push "!image_name!"
    if !errorlevel! equ 0 (
        echo Successfully pushed: !image_name!
    ) else (
        echo Failed to push: !image_name!
        exit /b 1
    )
) else (
    echo Failed to build: !image_name!
    exit /b 1
)
goto :eof

:main
REM Build runner images
echo.
echo Processing runner images...

REM C# Runner
if exist ".\environments\runners\csharp" (
    call :build_and_push_image ".\environments\runners\csharp" "csharp" "runner"
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

REM C# Tester
if exist ".\environments\testers\csharp" (
    call :build_and_push_image ".\environments\testers\csharp" "csharp" "tester"
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

endlocal