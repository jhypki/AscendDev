#!/bin/sh

# Change to test directory
cd /app/test

# Print current directory and list files for debugging
echo "Current directory: $(pwd)"
echo "Files in directory:"
ls -la

# Check if we need to install dependencies
if [ -f "install-deps.sh" ]; then
  echo "Installing additional dependencies..."
  sh install-deps.sh
fi

# Initialize go module if go.mod doesn't exist
if [ ! -f "go.mod" ]; then
  echo "Initializing Go module..."
  go mod init solution
fi

# Check if there's a custom test config
if [ -f "test-config.json" ]; then
  echo "Using custom test configuration"
  TEST_TIMEOUT=$(cat test-config.json | grep -o '"timeout":[^,}]*' | cut -d':' -f2 | tr -d ' ')
  echo "Test timeout: $TEST_TIMEOUT seconds"
  # Validate timeout is a number
  if ! echo "$TEST_TIMEOUT" | grep -q '^[0-9]\+$'; then
    echo "Invalid timeout value, using default"
    TEST_TIMEOUT=30
  fi
else
  echo "Using default test configuration"
  TEST_TIMEOUT=30
fi

# Run the tests with Go test and output to JSON
echo "Running tests..."
go test -json -timeout=${TEST_TIMEOUT}s ./... > test_output.json 2>&1

# Process the Go test JSON output and convert to our format
echo "Processing test results..."
cat > process_results.go << 'EOF'
package main

import (
	"bufio"
	"encoding/json"
	"fmt"
	"os"
	"strings"
)

type GoTestEvent struct {
	Time    string  `json:"Time"`
	Action  string  `json:"Action"`
	Package string  `json:"Package"`
	Test    string  `json:"Test"`
	Elapsed float64 `json:"Elapsed"`
	Output  string  `json:"Output"`
}

type TestResult struct {
	Tests   []TestCase   `json:"tests"`
	Summary TestSummary  `json:"summary"`
}

type TestCase struct {
	Name   string  `json:"name"`
	Action string  `json:"action"`
	Elapsed float64 `json:"elapsed"`
	Output string  `json:"output"`
}

type TestSummary struct {
	Total    int     `json:"total"`
	Passed   int     `json:"passed"`
	Failed   int     `json:"failed"`
	Skipped  int     `json:"skipped"`
	Duration float64 `json:"duration"`
}

func main() {
	file, err := os.Open("test_output.json")
	if err != nil {
		fmt.Printf("Error opening test output: %v\n", err)
		os.Exit(1)
	}
	defer file.Close()

	var tests []TestCase
	var summary TestSummary
	testMap := make(map[string]*TestCase)

	scanner := bufio.NewScanner(file)
	for scanner.Scan() {
		var event GoTestEvent
		if err := json.Unmarshal(scanner.Bytes(), &event); err != nil {
			continue
		}

		if event.Test != "" {
			testName := event.Test
			if testMap[testName] == nil {
				testMap[testName] = &TestCase{
					Name:   testName,
					Action: "run",
					Output: "",
				}
			}

			switch event.Action {
			case "run":
				testMap[testName].Action = "run"
			case "pass":
				testMap[testName].Action = "pass"
				testMap[testName].Elapsed = event.Elapsed
				summary.Passed++
			case "fail":
				testMap[testName].Action = "fail"
				testMap[testName].Elapsed = event.Elapsed
				summary.Failed++
			case "skip":
				testMap[testName].Action = "skip"
				testMap[testName].Elapsed = event.Elapsed
				summary.Skipped++
			case "output":
				if strings.TrimSpace(event.Output) != "" {
					testMap[testName].Output += event.Output
				}
			}
		}
	}

	// Convert map to slice
	for _, test := range testMap {
		tests = append(tests, *test)
		summary.Total++
		summary.Duration += test.Elapsed
	}

	result := TestResult{
		Tests:   tests,
		Summary: summary,
	}

	jsonData, err := json.MarshalIndent(result, "", "  ")
	if err != nil {
		fmt.Printf("Error marshaling JSON: %v\n", err)
		os.Exit(1)
	}

	err = os.WriteFile("results.json", jsonData, 0644)
	if err != nil {
		fmt.Printf("Error writing results: %v\n", err)
		os.Exit(1)
	}

	fmt.Println("Test results processed successfully")
}
EOF

# Run the result processor
go run process_results.go

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
  cat > results.json << 'EOF'
{
  "tests": [
    {
      "name": "Test Execution Error",
      "action": "fail",
      "elapsed": 0,
      "output": "Failed to execute tests. Check for compilation errors or missing dependencies."
    }
  ],
  "summary": {
    "total": 1,
    "passed": 0,
    "failed": 1,
    "skipped": 0,
    "duration": 0
  }
}
EOF
  
  exit 1
fi