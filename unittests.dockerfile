FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine

WORKDIR /app

# Create NuGet.config with additional package sources
RUN mkdir -p /root/.nuget/NuGet && \
    echo '<?xml version="1.0" encoding="utf-8"?>\
    <configuration>\
    <packageSources>\
    <clear />\
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />\
    <add key="dotnet-public" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json" />\
    <add key="dotnet8" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet8/nuget/v3/index.json" />\
    </packageSources>\
    </configuration>' > /root/.nuget/NuGet/NuGet.Config

# Copy the solution file and project files first to optimize Docker caching
COPY AscendDev.sln .
COPY AscendDev.API/*.csproj ./AscendDev.API/
COPY AscendDev.API.Test/*.csproj ./AscendDev.API.Test/
COPY AscendDev.Core/*.csproj ./AscendDev.Core/
COPY AscendDev.Core.Test/*.csproj ./AscendDev.Core.Test/
COPY AscendDev.Data/*.csproj ./AscendDev.Data/
COPY AscendDev.Data.Test/*.csproj ./AscendDev.Data.Test/
COPY AscendDev.Services/*.csproj ./AscendDev.Services/
COPY AscendDev.Services.Test/*.csproj ./AscendDev.Services.Test/

# Clear NuGet caches and restore dependencies with additional options
RUN dotnet nuget locals all --clear && \
    dotnet restore --disable-parallel --force

# Copy the rest of the code
COPY . .

# Build the solution with verbosity
RUN dotnet build -c Release -v normal

# Set environment variables for tests
ENV DOTNET_ENVIRONMENT=Test
ENV ASPNETCORE_ENVIRONMENT=Test

# Create directory for test results
RUN mkdir -p /app/TestResults

# Default command to run all tests and generate results
ENTRYPOINT ["dotnet", "test", "-c", "Release", "--logger", "trx;LogFileName=TestResults.trx", "--results-directory", "/app/TestResults"]