# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY InfiniteTavern.sln ./
COPY src/InfiniteTavern.API/*.csproj ./src/InfiniteTavern.API/
COPY src/InfiniteTavern.Application/*.csproj ./src/InfiniteTavern.Application/
COPY src/InfiniteTavern.Domain/*.csproj ./src/InfiniteTavern.Domain/
COPY src/InfiniteTavern.Infrastructure/*.csproj ./src/InfiniteTavern.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . ./

# Build the project
RUN dotnet publish src/InfiniteTavern.API/InfiniteTavern.API.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Render uses the PORT environment variable
ENV ASPNETCORE_URLS=http://+:${PORT:-5000}
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE ${PORT:-5000}

ENTRYPOINT ["dotnet", "InfiniteTavern.API.dll"]
