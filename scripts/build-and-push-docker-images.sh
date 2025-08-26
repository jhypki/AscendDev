#!/bin/bash

# Configuration - Replace with your Docker Hub username
DOCKER_USERNAME="${DOCKER_USERNAME:-jhypki}"

# Base name for all images
BASE_IMAGE_NAME="ascenddev"

# Colors for better output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Starting Docker image build and push process...${NC}"
echo -e "${YELLOW}Using Docker username: ${DOCKER_USERNAME}${NC}"

# Function to build and push Docker images
build_and_push_image() {
    local dockerfile_path=$1
    local language=$2
    local type=$3 # runner or tester
    
    local image_name="${DOCKER_USERNAME}/${BASE_IMAGE_NAME}-${language}-${type}:latest"
    
    echo -e "\n${YELLOW}Building: ${image_name}${NC}"
    echo -e "Dockerfile path: ${dockerfile_path}"
    
    # Build the Docker image specifically for Ubuntu platform
    docker build --platform linux/amd64 -t "$image_name" -f "$dockerfile_path/Dockerfile" "$dockerfile_path"
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}Successfully built: ${image_name}${NC}"
        
        # Push the image
        echo -e "${YELLOW}Pushing: ${image_name}${NC}"
        docker push "$image_name"
        if [ $? -eq 0 ]; then
            echo -e "${GREEN}Successfully pushed: ${image_name}${NC}"
        else
            echo -e "${RED}Failed to push: ${image_name}${NC}"
            return 1
        fi
    else
        echo -e "${RED}Failed to build: ${image_name}${NC}"
        return 1
    fi
}

# Get the script directory and navigate to project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
DOCKER_CONFIG_DIR="$PROJECT_ROOT/configuration/docker"

echo -e "${YELLOW}Project root: ${PROJECT_ROOT}${NC}"
echo -e "${YELLOW}Docker config directory: ${DOCKER_CONFIG_DIR}${NC}"

# Change to docker configuration directory
cd "$DOCKER_CONFIG_DIR"

# Build runner images
echo -e "\n${YELLOW}Processing runner images...${NC}"

# C# Runner
if [ -d "./environments/runners/csharp" ]; then
    build_and_push_image "./environments/runners/csharp" "csharp" "runner"
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
echo -e "\n${YELLOW}Processing tester images...${NC}"

# C# Tester
if [ -d "./environments/testers/csharp" ]; then
    build_and_push_image "./environments/testers/csharp" "csharp" "tester"
fi

# Python Tester
if [ -d "./environments/testers/python" ]; then
    build_and_push_image "./environments/testers/python" "python" "tester"
fi

# TypeScript Tester
if [ -d "./environments/testers/typescript" ]; then
    build_and_push_image "./environments/testers/typescript" "typescript" "tester"
fi

echo -e "\n${GREEN}Docker image build and push process completed!${NC}"
echo -e "${YELLOW}Note: You need to be logged in to Docker Hub with 'docker login' for this script to work.${NC}"