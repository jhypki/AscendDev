FROM python:3.11-alpine

# Set working directory
WORKDIR /app

# Copy environment files
COPY . /app/

# Install dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Create a directory for test files
RUN mkdir -p /app/test

# Set permissions for scripts
RUN chmod +x /app/run-tests.sh

# Set default command
CMD ["pytest"]