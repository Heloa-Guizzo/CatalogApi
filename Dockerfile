FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY . .

RUN dotnet restore src/microservices/CatalogAPI/CatalogAPI.csproj

RUN dotnet publish src/microservices/CatalogAPI/CatalogAPI.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /publish .

ENTRYPOINT ["dotnet", "CatalogAPI.dll"]