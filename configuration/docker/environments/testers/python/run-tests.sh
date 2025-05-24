#!/bin/sh

# Change to test directory
cd /app/test

# Check if we need to install dependencies
if [ -f "requirements.txt" ]; then
  echo "Installing additional dependencies..."
  pip install -r requirements.txt
fi

# Check if we need to run a setup script
if [ -f "setup.sh" ]; then
  echo "Running setup script..."
  sh setup.sh
fi

# Run the tests with pytest and output to JSON
echo "Running tests..."
python -m pytest test_solution.py -v --json-report --json-report-file=results.json || true

# Check if results.json was created
if [ -f "results.json" ]; then
  echo "Test results generated successfully"
  cat results.json
  
  # Check if any tests failed
  FAILED_TESTS=$(cat results.json | grep -o '"failed":[^,}]*' | cut -d':' -f2)
  if [ "$FAILED_TESTS" = "0" ]; then
    exit 0
  else
    exit 1
  fi
else
  echo "Failed to generate test results"
  
  # Create a basic error report
  cat > results.json << EOF
{
  "tests": [
    {
      "name": "Test Execution Error",
      "outcome": "failed",
      "message": "Failed to execute tests. Check for syntax errors or missing dependencies."
    }
  ],
  "summary": {
    "total": 1,
    "passed": 0,
    "failed": 1,
    "skipped": 0,
    "error": 0,
    "duration": 0
  }
}
EOF
  
  exit 1
fi