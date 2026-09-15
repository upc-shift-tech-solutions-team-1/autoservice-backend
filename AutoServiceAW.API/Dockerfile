FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AutoServiceAW.sln ./
COPY AutoServiceAW.API/AutoServiceAW.API.csproj ./AutoServiceAW.API/

RUN dotnet restore AutoServiceAW.sln

COPY . .
WORKDIR /src/AutoServiceAW.API
RUN dotnet publish AutoServiceAW.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "AutoServiceAW.API.dll"]