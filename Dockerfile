# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/server/CrossWords.csproj", "src/server/"]
COPY ["src/server/Directory.Packages.props", "src/server/"]
COPY ["src/server/global.json", "src/server/"]
RUN dotnet restore "src/server/CrossWords.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/server"
RUN dotnet build "CrossWords.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CrossWords.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
COPY --from=build /src/src/client /app/client

ENTRYPOINT ["dotnet", "CrossWords.dll"]
