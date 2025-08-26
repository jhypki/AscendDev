FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine

# Set working directory
WORKDIR /app

# Copy environment files
COPY . /app/

# Create a directory for test files
RUN mkdir -p /app/test

# Set permissions for scripts
RUN chmod +x /app/run-tests.sh

# Set default command
CMD ["dotnet", "test"]