#!/bin/sh

cd /app/code

# Create a new console project if not exists
if [ ! -f "Program.cs" ]; then
  echo "Program.cs not found. Exiting."
  exit 1
fi

# Create a temporary project file
echo "<Project Sdk=\"Microsoft.NET.Sdk\">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>" > Program.csproj

# Compile the code
dotnet build -c Release 2> compilation.txt

# Check if compilation was successful
if [ $? -eq 0 ]; then
  # Run the code
  dotnet run -c Release
else
  echo "Compilation failed. See compilation.txt for details."
  cat compilation.txt
  exit 1
fi