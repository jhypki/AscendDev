#!/bin/sh

# Change to test directory
cd /app/test

# Check if we need to install dependencies
if [ -f "install-deps.sh" ]; then
  echo "Installing additional dependencies..."
  sh install-deps.sh
fi

# Check if there's a custom jest.config.js in the test directory
if [ -f "jest.config.js" ]; then
  echo "Using custom Jest configuration from test directory"
  npx jest --json --outputFile=results.json || (cat results.json && exit 1)
else
  echo "Using default Jest configuration"
  npx jest --config=/app/jest.config.js --json --outputFile=results.json || (cat results.json && exit 1)
fi