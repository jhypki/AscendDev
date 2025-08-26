#!/bin/sh

# Change to code directory
cd /app/code

# Print current directory and list files for debugging
echo "Current directory: $(pwd)"
echo "Files in directory:"
ls -la

# Initialize go module if go.mod doesn't exist
if [ ! -f "go.mod" ]; then
  echo "Initializing Go module..."
  go mod init main
fi

# Check if we need to install dependencies
if [ -f "go.sum" ]; then
  echo "Installing dependencies..."
  go mod download
fi

# Run the Go code
echo "Running Go code..."
timeout 30s go run main.go
EXIT_CODE=$?

if [ $EXIT_CODE -eq 124 ]; then
  echo "Code execution timed out after 30 seconds"
  exit 124
elif [ $EXIT_CODE -ne 0 ]; then
  echo "Code execution failed with exit code $EXIT_CODE"
  exit $EXIT_CODE
else
  echo "Code execution completed successfully"
  exit 0
fi