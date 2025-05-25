#!/bin/bash

# Script to run unit tests for AscendDev solution using Docker

# Set script to exit on error
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
TEST_RESULTS_DIR="$ROOT_DIR/TestResults"

# Create test results directory if it doesn't exist
mkdir -p "$TEST_RESULTS_DIR"

echo "Building unit tests Docker image..."
docker build -t ascenddev-unit-tests -f "$ROOT_DIR/unittests.dockerfile" "$ROOT_DIR"

echo "Running unit tests..."
docker run --rm \
  -v "$TEST_RESULTS_DIR:/app/TestResults" \
  ascenddev-unit-tests

# Check if any test results were generated
if [ -n "$(ls -A "$TEST_RESULTS_DIR")" ]; then
  echo "Test results saved to $TEST_RESULTS_DIR"
  
  # Count test results
  PASSED=$(grep -r "outcome=\"Passed\"" "$TEST_RESULTS_DIR" | wc -l)
  FAILED=$(grep -r "outcome=\"Failed\"" "$TEST_RESULTS_DIR" | wc -l)
  TOTAL=$((PASSED + FAILED))
  
  echo "Test summary: $PASSED/$TOTAL tests passed"
  
  # Return non-zero exit code if any tests failed
  if [ $FAILED -gt 0 ]; then
    echo "❌ $FAILED tests failed"
    exit 1
  else
    echo "✅ All tests passed!"
    exit 0
  fi
else
  echo "No test results were generated"
  exit 1
fi