#!/bin/bash

# Configuration - Replace with your Docker Hub username
DOCKER_USERNAME="${DOCKER_USERNAME:-jhypki}"

# Base name for all images
BASE_IMAGE_NAME="ascenddev"

# Colors for better output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Starting multi-architecture Docker image build process for testers...${NC}"
echo -e "${YELLOW}Using Docker username: ${DOCKER_USERNAME}${NC}"

# Make sure we're in the docker directory
cd "$(dirname "$0")"

# Enable Docker BuildKit for multi-platform builds
export DOCKER_BUILDKIT=1

# Set up Docker buildx
echo -e "\n${YELLOW}Setting up Docker buildx...${NC}"
docker buildx create --name multiarch-builder --use || docker buildx use multiarch-builder
docker buildx inspect --bootstrap

# Build and push TypeScript tester image for multiple architectures
echo -e "\n${YELLOW}Building and pushing TypeScript tester image for multiple architectures...${NC}"
docker buildx build --platform linux/amd64,linux/arm64 \
  -t "${DOCKER_USERNAME}/${BASE_IMAGE_NAME}-typescript-tester:latest" \
  -f typescript.tester.dockerfile \
  --push .

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Successfully built and pushed TypeScript tester image for multiple architectures${NC}"
else
    echo -e "\033[0;31mFailed to build and push TypeScript tester image\033[0m"
fi

# Build and push C# tester image for multiple architectures
echo -e "\n${YELLOW}Building and pushing C# tester image for multiple architectures...${NC}"
docker buildx build --platform linux/amd64,linux/arm64 \
  -t "${DOCKER_USERNAME}/${BASE_IMAGE_NAME}-csharp-tester:latest" \
  -f csharp.tester.dockerfile \
  --push .

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Successfully built and pushed C# tester image for multiple architectures${NC}"
else
    echo -e "\033[0;31mFailed to build and push C# tester image\033[0m"
fi

# Build and push Python tester image for multiple architectures
echo -e "\n${YELLOW}Building and pushing Python tester image for multiple architectures...${NC}"
docker buildx build --platform linux/amd64,linux/arm64 \
  -t "${DOCKER_USERNAME}/${BASE_IMAGE_NAME}-python-tester:latest" \
  -f python.tester.dockerfile \
  --push .

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Successfully built and pushed Python tester image for multiple architectures${NC}"
else
    echo -e "\033[0;31mFailed to build and push Python tester image\033[0m"
fi

echo -e "\n${GREEN}Multi-architecture Docker image build and push process completed!${NC}"
echo -e "${YELLOW}Note: You need to be logged in to Docker Hub with 'docker login' for this script to work.${NC}"