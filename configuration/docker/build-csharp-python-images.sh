#!/bin/bash

# Configuration - Replace with your Docker Hub username
DOCKER_USERNAME="${DOCKER_USERNAME:-jhypki}"

# Base name for all images
BASE_IMAGE_NAME="ascenddev"

# Colors for better output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Starting Docker image build process for C# and Python...${NC}"
echo -e "${YELLOW}Using Docker username: ${DOCKER_USERNAME}${NC}"

# Make sure we're in the docker directory
cd "$(dirname "$0")"

# Build C# tester image
echo -e "\n${YELLOW}Building C# tester image...${NC}"
docker build --platform linux/amd64 -t "${DOCKER_USERNAME}/${BASE_IMAGE_NAME}-csharp-tester:latest" -f csharp.tester.dockerfile .

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Successfully built C# tester image${NC}"
    
    # Push the image
    echo -e "${YELLOW}Pushing C# tester image...${NC}"
    docker push "${DOCKER_USERNAME}/${BASE_IMAGE_NAME}-csharp-tester:latest"
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}Successfully pushed C# tester image${NC}"
    else
        echo -e "\033[0;31mFailed to push C# tester image\033[0m"
    fi
else
    echo -e "\033[0;31mFailed to build C# tester image\033[0m"
fi

# Build Python tester image
echo -e "\n${YELLOW}Building Python tester image...${NC}"
docker build --platform linux/amd64 -t "${DOCKER_USERNAME}/${BASE_IMAGE_NAME}-python-tester:latest" -f python.tester.dockerfile .

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Successfully built Python tester image${NC}"
    
    # Push the image
    echo -e "${YELLOW}Pushing Python tester image...${NC}"
    docker push "${DOCKER_USERNAME}/${BASE_IMAGE_NAME}-python-tester:latest"
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}Successfully pushed Python tester image${NC}"
    else
        echo -e "\033[0;31mFailed to push Python tester image\033[0m"
    fi
else
    echo -e "\033[0;31mFailed to build Python tester image\033[0m"
fi

echo -e "\n${GREEN}Docker image build and push process completed!${NC}"