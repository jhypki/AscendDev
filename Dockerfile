# Use the official .NET SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ["AscendDev.sln", "./"]
COPY ["AscendDev.Core/AscendDev.Core.csproj", "AscendDev.Core/"]
COPY ["AscendDev.Data/AscendDev.Data.csproj", "AscendDev.Data/"]
COPY ["AscendDev.Services/AscendDev.Services.csproj", "AscendDev.Services/"]
COPY ["AscendDev.API/AscendDev.API.csproj", "AscendDev.API/"]
COPY ["AscendDev.Services.Test/AscendDev.Services.Test.csproj", "AscendDev.Services.Test/"]
COPY ["AscendDev.Data.Test/AscendDev.Data.Test.csproj", "AscendDev.Data.Test/"]

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
ENTRYPOINT ["dotnet", "AscendDev.API.dll"]
