#!/bin/bash

# Configuration - Replace with your Docker Hub username
DOCKER_USERNAME=${DOCKER_USERNAME:-jhypki}

# Base name for all images
BASE_IMAGE_NAME="ascenddev"

echo "Starting Docker image build and push process..."
echo "Using Docker username: $DOCKER_USERNAME"

# Get the script directory and navigate to project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR/.."
DOCKER_CONFIG_DIR="$PROJECT_ROOT/configuration/docker"

echo "Project root: $PROJECT_ROOT"
echo "Docker config directory: $DOCKER_CONFIG_DIR"

# Change to docker configuration directory
cd "$DOCKER_CONFIG_DIR" || exit 1

# Setup Docker buildx for multi-platform builds
echo "Setting up Docker buildx for multi-platform builds..."
docker buildx create --name multiplatform-builder --use --bootstrap 2>/dev/null || {
    echo "Using existing multiplatform-builder..."
    docker buildx use multiplatform-builder
}

# Function to build and push Docker images
build_and_push_image() {
    local dockerfile_path="$1"
    local language="$2"
    local type="$3"
    
    local image_name="$DOCKER_USERNAME/$BASE_IMAGE_NAME-$language-$type:latest"
    
    echo ""
    echo "Building multi-platform image: $image_name"
    echo "Dockerfile path: $dockerfile_path"
    echo "Platforms: linux/amd64,linux/arm64"
    
    # Build and push multi-platform Docker image using buildx
    docker buildx build --platform linux/amd64,linux/arm64 --push -t "$image_name" -f "$dockerfile_path/Dockerfile" "$dockerfile_path"
    
    if [ $? -eq 0 ]; then
        echo "Successfully built and pushed multi-platform image: $image_name"
    else
        echo "Failed to build multi-platform image: $image_name"
        exit 1
    fi
}

# Build runner images
echo ""
echo "Processing runner images..."

# Go Runner
if [ -d "./environments/runners/go" ]; then
    build_and_push_image "./environments/runners/go" "go" "runner"
fi

# JavaScript Runner
if [ -d "./environments/runners/javascript" ]; then
    build_and_push_image "./environments/runners/javascript" "javascript" "runner"
fi

# Python Runner
if [ -d "./environments/runners/python" ]; then
    build_and_push_image "./environments/runners/python" "python" "runner"
fi

# TypeScript Runner
if [ -d "./environments/runners/typescript" ]; then
    build_and_push_image "./environments/runners/typescript" "typescript" "runner"
fi

# Build tester images
echo ""
echo "Processing tester images..."

# Go Tester
if [ -d "./environments/testers/go" ]; then
    build_and_push_image "./environments/testers/go" "go" "tester"
fi

# Python Tester
if [ -d "./environments/testers/python" ]; then
    build_and_push_image "./environments/testers/python" "python" "tester"
fi

# TypeScript Tester
if [ -d "./environments/testers/typescript" ]; then
    build_and_push_image "./environments/testers/typescript" "typescript" "tester"
fi

echo ""
echo "Docker image build and push process completed!"
echo "Note: You need to be logged in to Docker Hub with 'docker login' for this script to work."

# Cleanup: Remove the buildx builder (optional - comment out if you want to keep it for future builds)
echo "Cleaning up buildx builder..."
docker buildx rm multiplatform-builder 2>/dev/null || echo "Builder already removed or doesn't exist."