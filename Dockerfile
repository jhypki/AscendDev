# Use the official .NET SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ["ElearningPlatform.sln", "./"]
COPY ["ElearningPlatform.Core/ElearningPlatform.Core.csproj", "ElearningPlatform.Core/"]
COPY ["ElearningPlatform.Data/ElearningPlatform.Data.csproj", "ElearningPlatform.Data/"]
COPY ["ElearningPlatform.Services/ElearningPlatform.Services.csproj", "ElearningPlatform.Services/"]
COPY ["ElearningPlatform.API/ElearningPlatform.API.csproj", "ElearningPlatform.API/"]
COPY ["ElearningPlatform.Services.Test/ElearningPlatform.Services.Test.csproj", "ElearningPlatform.Services.Test/"]
COPY ["ElearningPlatform.Data.Test/ElearningPlatform.Data.Test.csproj", "ElearningPlatform.Data.Test/"]

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out


# Build the application
RUN dotnet publish -c Release -o /app/out

# Use a lightweight runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Set environment variables
ENV ASPNETCORE_URLS=http://0.0.0.0:5171

# Expose API port
EXPOSE 5171

# Run the API
ENTRYPOINT ["dotnet", "ElearningPlatform.API.dll"]
