#!/bin/sh

cd /app/code

# Check if we need to install dependencies
if [ -f "package.json" ]; then
  npm install
fi

# Create a tsconfig.json file if not exists
if [ ! -f "tsconfig.json" ]; then
  echo "{
  \"compilerOptions\": {
    \"target\": \"ES2022\",
    \"module\": \"NodeNext\",
    \"moduleResolution\": \"NodeNext\",
    \"esModuleInterop\": true,
    \"strict\": true
  }
}" > tsconfig.json
fi

# Compile the TypeScript code
tsc --noEmit 2> compilation.txt

# Check if compilation was successful
if [ $? -eq 0 ]; then
  # Run the code with ts-node
  ts-node index.ts
else
  cat compilation.txt
  exit 1
fi