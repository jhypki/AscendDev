FROM python:3.11-alpine

# Set working directory
WORKDIR /app

# Copy environment files
COPY . /app/

# Create a directory for code
RUN mkdir -p /app/code

# Install dependencies
RUN pip install --no-cache-dir pytest pytest-json-report

# Convert line endings and set permissions for scripts
RUN sed -i 's/\r$//' /app/run-code.sh && chmod +x /app/run-code.sh

# Set default command
CMD ["python", "--version"]