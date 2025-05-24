#!/bin/sh

cd /app/code

# Check if we need to install dependencies
if [ -f "requirements.txt" ]; then
  pip install -r requirements.txt
fi

# Run the code
python main.py