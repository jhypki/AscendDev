#!/bin/bash

# Configuration - Replace with your Docker Hub username
DOCKER_USERNAME="${DOCKER_USERNAME:-jhypki}"

# Base name for all images
BASE_IMAGE_NAME="ascenddev"

# Colors for better output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Starting Docker image build process...${NC}"
echo -e "${YELLOW}Using Docker username: ${DOCKER_USERNAME}${NC}"

# Function to extract language from Dockerfile
extract_language() {
    local dockerfile=$1
    local filename=$(basename "$dockerfile")
    
    # Extract language from filename pattern (e.g., typescript.tester.dockerfile -> typescript)
    local language=$(echo "$filename" | cut -d'.' -f1)
    echo "$language"
}

# Function to build Docker images
build_image() {
    local dockerfile=$1
    local type=$2 # runner or tester
    
    local language=$(extract_language "$dockerfile")
    local image_name="${DOCKER_USERNAME}/${BASE_IMAGE_NAME}-${language}-${type}:latest"
    
    echo -e "\n${YELLOW}Building: ${image_name}${NC}"
    echo -e "Dockerfile: ${dockerfile}"
    
    # Build the Docker image specifically for Ubuntu platform
    docker build --platform linux/amd64 -t "$image_name" -f "$dockerfile" .
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}Successfully built: ${image_name}${NC}"
        
        # Always push the image
        echo -e "${YELLOW}Pushing: ${image_name}${NC}"
        docker push "$image_name"
        if [ $? -eq 0 ]; then
            echo -e "${GREEN}Successfully pushed: ${image_name}${NC}"
        else
            echo -e "\033[0;31mFailed to push: ${image_name}\033[0m"
        fi
    else
        echo -e "\033[0;31mFailed to build: ${image_name}\033[0m"
    fi
}

# Make sure we're in the docker directory
cd "$(dirname "$0")"

# Build runner images
echo -e "\n${YELLOW}Processing runner Dockerfiles...${NC}"
for dockerfile in ./environments/runners/*.dockerfile; do
    if [ -f "$dockerfile" ]; then
        build_image "$dockerfile" "runner"
    fi
done

# Build tester images
echo -e "\n${YELLOW}Processing tester Dockerfiles...${NC}"
for dockerfile in ./environments/testers/*.dockerfile; do
    if [ -f "$dockerfile" ]; then
        build_image "$dockerfile" "tester"
    fi
done

# Handle special case for the typescript.tester.dockerfile in the root
if [ -f "typescript.tester.dockerfile" ]; then
    echo -e "\n${YELLOW}Processing special case Dockerfile...${NC}"
    build_image "typescript.tester.dockerfile" "tester"
fi

echo -e "\n${GREEN}Docker image build and push process completed!${NC}"