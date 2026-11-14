FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CatalogApi/CatalogAPI.csproj", "CatalogApi/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "CatalogApi/CatalogAPI.csproj"

COPY ["CatalogApi/", "CatalogApi/"]
COPY ["Shared/", "Shared/"]
WORKDIR "/src/CatalogApi"
RUN dotnet publish "CatalogAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CatalogAPI.dll"]