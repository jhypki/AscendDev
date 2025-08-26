# Docker Build Scripts

This directory contains scripts for building and pushing Docker images for the AscendDev project.

## Structure

The Docker configuration has been reorganized with the following structure:

```
configuration/docker/
└── environments/
    ├── runners/
    │   ├── csharp/
    │   │   └── Dockerfile
    │   ├── javascript/
    │   │   └── Dockerfile
    │   ├── python/
    │   │   └── Dockerfile
    │   └── typescript/
    │       └── Dockerfile
    └── testers/
        ├── csharp/
        │   └── Dockerfile
        ├── python/
        │   └── Dockerfile
        └── typescript/
            └── Dockerfile
```

## Scripts

### build-and-push-docker-images.sh / build-and-push-docker-images.bat

Main scripts for building and pushing all Docker images. These scripts:

- Build all runner images (csharp, javascript, python, typescript)
- Build all tester images (csharp, python, typescript)
- Follow the naming convention: `jhypki/ascenddev-{language}-{type}:latest`
- Push images to Docker Hub after successful builds

**Usage (Linux/macOS):**
```bash
# Make sure you're logged in to Docker Hub first
docker login

# Run the build script
bash scripts/build-and-push-docker-images.sh
```

**Usage (Windows):**
```cmd
# Make sure you're logged in to Docker Hub first
docker login

# Run the build script
scripts\build-and-push-docker-images.bat
```

**Environment Variables:**
- `DOCKER_USERNAME`: Docker Hub username (defaults to "jhypki")

### verify-docker-structure.ps1

PowerShell script to verify that all Dockerfiles are in their correct locations.

**Usage:**
```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify-docker-structure.ps1
```

## Image Naming Convention

Images are named following the pattern defined in `AscendDev.Core/Constants/DockerImages.cs`:

- **Runners**: `jhypki/ascenddev-{language}-runner:latest`
  - `jhypki/ascenddev-csharp-runner:latest`
  - `jhypki/ascenddev-javascript-runner:latest`
  - `jhypki/ascenddev-python-runner:latest`
  - `jhypki/ascenddev-typescript-runner:latest`

- **Testers**: `jhypki/ascenddev-{language}-tester:latest`
  - `jhypki/ascenddev-csharp-tester:latest`
  - `jhypki/ascenddev-python-tester:latest`
  - `jhypki/ascenddev-typescript-tester:latest`

## Migration Notes

This new structure replaces the previous setup where:
- Dockerfiles were in the root of `configuration/docker/`
- Multiple build scripts existed for different purposes
- File naming used the pattern `{language}.{type}.dockerfile`

All legacy files have been removed and consolidated into the single unified build script.