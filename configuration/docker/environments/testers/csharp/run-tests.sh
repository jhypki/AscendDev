#!/bin/sh

# Change to test directory
cd /app/test

# Check if we need to install dependencies
if [ -f "install-deps.sh" ]; then
  echo "Installing additional dependencies..."
  sh install-deps.sh
fi

# Create a test project if it doesn't exist
if [ ! -f "TestProject.csproj" ]; then
  echo "Creating test project..."
  cat > TestProject.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
  </ItemGroup>

</Project>
EOF
fi

# Check if there's a custom test config
if [ -f "test-config.json" ]; then
  echo "Using custom test configuration"
  TEST_TIMEOUT=$(cat test-config.json | grep -o '"timeout":[^,}]*' | cut -d':' -f2)
  echo "Test timeout: $TEST_TIMEOUT ms"
else
  echo "Using default test configuration"
fi

# Run the tests with xUnit and output to JSON
echo "Running tests..."
dotnet test --logger "trx;LogFileName=results.trx" || true

# Convert TRX to JSON
echo "Converting test results to JSON..."
dotnet /app/TrxToJson.dll /app/test/TestResults/results.trx /app/test/results.json

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
  exit 1
fi