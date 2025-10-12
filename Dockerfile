# Use the official .NET SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files first (for better layer caching)
COPY ["AscendDev.Core/AscendDev.Core.csproj", "AscendDev.Core/"]
COPY ["AscendDev.Data/AscendDev.Data.csproj", "AscendDev.Data/"]
COPY ["AscendDev.Services/AscendDev.Services.csproj", "AscendDev.Services/"]
COPY ["AscendDev.API/AscendDev.API.csproj", "AscendDev.API/"]

# Restore NuGet packages for API project only
RUN dotnet restore AscendDev.API/AscendDev.API.csproj

# Copy the rest of the source code (excluding test projects)
COPY ["AscendDev.Core/", "AscendDev.Core/"]
COPY ["AscendDev.Data/", "AscendDev.Data/"]
COPY ["AscendDev.Services/", "AscendDev.Services/"]
COPY ["AscendDev.API/", "AscendDev.API/"]

# Build and publish the application (only the API project, exclude test projects)
RUN dotnet publish AscendDev.API/AscendDev.API.csproj -c Release -o /app/out

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install Docker CLI only (we'll use the host's Docker daemon)
RUN apt-get update && apt-get install -y --no-install-recommends \
    apt-transport-https \
    ca-certificates \
    curl \
    gnupg \
    lsb-release \
    && curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg \
    && echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" > /etc/apt/sources.list.d/docker.list \
    && apt-get update \
    && apt-get install -y docker-ce-cli \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Install the New Relic agent binaries
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
    && echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
    && wget https://download.newrelic.com/548C16BF.gpg \
    && apt-key add 548C16BF.gpg \
    && apt-get update \
    && apt-get install -y 'newrelic-dotnet-agent' \
    && rm -rf /var/lib/apt/lists/*

# Enable the agent loading mechanism (variables needed by the runtime)
ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so
# Note: NEW_RELIC_LICENSE_KEY and NEW_RELIC_APP_NAME are now expected
# to be provided as environment variables at container runtime (e.g., via docker-compose)

# Create a non-root user to run the application
RUN useradd -ms /bin/bash appuser
# Create app directory owned by appuser (adjust temp directory if needed)
RUN mkdir -p /app && chown -R appuser:appuser /app

# Copy the published app
WORKDIR /app
COPY --from=build --chown=appuser:appuser /app/out .

# Set the working directory
WORKDIR /app

# TODO: Switch to non-root user if docker socket access allows or is not needed
# USER appuser

# Create temp directory for code execution (if app needs it relative to WORKDIR)
# Ensure permissions allow the running user (root or appuser) to write here
ENV TEMP_DIRECTORY=/app/temp
RUN mkdir -p $TEMP_DIRECTORY && chown -R appuser:appuser $TEMP_DIRECTORY # Or root:root if running as root

# Expose API port
EXPOSE 5171

# Run the application
ENTRYPOINT ["dotnet", "AscendDev.API.dll"]