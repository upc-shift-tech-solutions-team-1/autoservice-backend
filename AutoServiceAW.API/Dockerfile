
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AutoServiceAW.sln ./
COPY AutoServiceAW.API/AutoServiceAW.API.csproj ./AutoServiceAW.API/

RUN dotnet restore AutoServiceAW.API/AutoServiceAW.API.csproj

COPY . .
WORKDIR /src/AutoServiceAW.API

RUN dotnet publish AutoServiceAW.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Configure the HTTP port used by Render
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Run the application as a non-root user
USER $APP_UID

ENTRYPOINT ["dotnet", "AutoServiceAW.API.dll"]
