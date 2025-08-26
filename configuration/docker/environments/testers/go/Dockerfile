FROM golang:1.21-alpine

# Set working directory
WORKDIR /app

# Install git (needed for go modules)
RUN apk add --no-cache git

# Copy environment files
COPY . /app/

# Create a directory for test files
RUN mkdir -p /app/test

# Convert line endings and set permissions for scripts
RUN sed -i 's/\r$//' /app/run-tests.sh && chmod +x /app/run-tests.sh

# Set default command
CMD ["go", "test"]