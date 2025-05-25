#!/bin/sh

# Change to test directory
cd /app/test

# Debug: List files in current directory
echo "Files in test directory:"
ls -la

# Check if we need to install dependencies
if [ -f "install-deps.sh" ]; then
  echo "Installing additional dependencies..."
  sh install-deps.sh
fi

# Create empty results.json in case tests fail to run
echo "{\"numTotalTests\": 0, \"numPassedTests\": 0, \"numFailedTests\": 0, \"testResults\": [], \"success\": false}" > results.json

# Check if there's a custom jest.config.js in the test directory
if [ -f "jest.config.js" ]; then
  echo "Using custom Jest configuration from test directory"
  npx jest --json --outputFile=results.json || true
else
  echo "Using default Jest configuration"
  npx jest --config=/app/jest.config.js --json --outputFile=results.json || true
fi

# Ensure results.json exists and has content
if [ ! -s "results.json" ]; then
  echo "Warning: results.json is empty or doesn't exist. Creating default results."
  echo "{\"numTotalTests\": 0, \"numPassedTests\": 0, \"numFailedTests\": 1, \"testResults\": [{\"assertionResults\": [{\"status\": \"failed\", \"title\": \"Test Execution\", \"failureMessages\": [\"Tests failed to execute properly\"]}], \"name\": \"default\"}], \"success\": false}" > results.json
fi

# Debug: Show results.json content
echo "Results.json content:"
cat results.json