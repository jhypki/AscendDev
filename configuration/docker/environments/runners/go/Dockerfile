FROM golang:1.21-alpine

# Set working directory
WORKDIR /app

# Install git (needed for go modules)
RUN apk add --no-cache git

# Copy environment files
COPY . /app/

# Create a directory for code files
RUN mkdir -p /app/code

# Convert line endings and set permissions for scripts
RUN sed -i 's/\r$//' /app/run-code.sh && chmod +x /app/run-code.sh

# Set default command
CMD ["go", "run", "."]