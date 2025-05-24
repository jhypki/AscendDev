#!/bin/sh

cd /app/code

# Check if we need to install dependencies
if [ -f "package.json" ]; then
  npm install
fi

# Run the JavaScript code
node index.js