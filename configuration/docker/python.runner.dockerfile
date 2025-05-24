FROM python:3.11-alpine

# Set working directory
WORKDIR /app

# Copy environment files
COPY ./environments/runners/python/ /app/

# Create a directory for code
RUN mkdir -p /app/code

# Install dependencies
RUN pip install --no-cache-dir pytest pytest-json-report

# Set permissions for scripts
RUN chmod +x /app/run-code.sh

# Set default command
CMD ["python", "--version"]