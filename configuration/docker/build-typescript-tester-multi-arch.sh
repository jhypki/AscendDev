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

echo -e "${YELLOW}Starting multi-architecture Docker image build process for TypeScript tester...${NC}"
echo -e "${YELLOW}Using Docker username: ${DOCKER_USERNAME}${NC}"

# Make sure we're in the docker directory
cd "$(dirname "$0")"

# Enable Docker BuildKit for multi-platform builds
export DOCKER_BUILDKIT=1

# Set up Docker buildx
echo -e "\n${YELLOW}Setting up Docker buildx...${NC}"
docker buildx create --name multiarch-builder --use || docker buildx use multiarch-builder
docker buildx inspect --bootstrap

# First, build a local version for testing
echo -e "\n${YELLOW}Building local version of TypeScript tester image for testing...${NC}"
docker build -t "${DOCKER_USERNAME}/${BASE_IMAGE_NAME}-typescript-tester:latest" -f typescript.tester.dockerfile .

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Successfully built local TypeScript tester image${NC}"
    
    # Test the image by running a simple verification
    echo -e "\n${YELLOW}Testing the TypeScript tester image...${NC}"
    docker run --rm "${DOCKER_USERNAME}/${BASE_IMAGE_NAME}-typescript-tester:latest" node -e "console.log('TypeScript tester image is working')"
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}TypeScript tester image test successful${NC}"
        
        # Build and push TypeScript tester image for multiple architectures
        echo -e "\n${YELLOW}Building and pushing TypeScript tester image for multiple architectures...${NC}"
        docker buildx build --platform linux/amd64,linux/arm64 \
          -t "${DOCKER_USERNAME}/${BASE_IMAGE_NAME}-typescript-tester:latest" \
          -f typescript.tester.dockerfile \
          --push .
        
        if [ $? -eq 0 ]; then
            echo -e "${GREEN}Successfully built and pushed TypeScript tester image for multiple architectures${NC}"
        else
            echo -e "${RED}Failed to build and push TypeScript tester image for multiple architectures${NC}"
            exit 1
        fi
    else
        echo -e "${RED}TypeScript tester image test failed${NC}"
        exit 1
    fi
else
    echo -e "${RED}Failed to build local TypeScript tester image${NC}"
    exit 1
fi

echo -e "\n${GREEN}Multi-architecture Docker image build and push process completed!${NC}"
echo -e "${YELLOW}Note: You need to be logged in to Docker Hub with 'docker login' for this script to work.${NC}"