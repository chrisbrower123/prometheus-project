FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Packages.props ./
COPY Directory.Build.props ./
COPY src/StockData.Api/StockData.Api.csproj src/StockData.Api/
COPY src/StockData.Domain/StockData.Domain.csproj src/StockData.Domain/
COPY src/StockData.Infrastructure/StockData.Infrastructure.csproj src/StockData.Infrastructure/
RUN dotnet restore src/StockData.Api/StockData.Api.csproj

COPY src ./src
RUN dotnet publish src/StockData.Api/StockData.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
RUN apk add --no-cache wget
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StockData.Api.dll"]
