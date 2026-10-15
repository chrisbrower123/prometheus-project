# Backend Tests

Backend tests are under `api/tests/` and are grouped by the production boundary they verify:

```text
api/tests/
├── StockData.Domain.Tests/
├── StockData.Infrastructure.Tests/
└── StockData.Api.Tests/
```

The repository currently targets .NET 10. Check the installed SDK with:

```powershell
dotnet --version
```

## Run the complete backend suite

From the repository root:

```powershell
dotnet test api/StockData.sln --filter "Category!=TimezoneReview"
```

The `TimezoneReview` tests remain excluded because they are intentionally reserved for separate review.

## Run one test project

```powershell
dotnet test api/tests/StockData.Domain.Tests/StockData.Domain.Tests.csproj
dotnet test api/tests/StockData.Infrastructure.Tests/StockData.Infrastructure.Tests.csproj
dotnet test api/tests/StockData.Api.Tests/StockData.Api.Tests.csproj
```

## Run one test class

Use the VSTest fully-qualified-name filter:

```powershell
dotnet test api/tests/StockData.Api.Tests/StockData.Api.Tests.csproj --filter "FullyQualifiedName~HostingTests"
dotnet test api/tests/StockData.Infrastructure.Tests/StockData.Infrastructure.Tests.csproj --filter "FullyQualifiedName~YahooMarketDataProviderTests"
```

Replace the class name after `FullyQualifiedName~` with the class you want to run.

## Build without running tests

```powershell
dotnet build api/StockData.sln
```

The solution treats warnings as errors, so a successful build should report zero warnings and zero errors.

## Test boundaries

- Domain tests verify business rules, validation, aggregation, and service orchestration.
- Infrastructure tests use fixtures and stub HTTP handlers. They do not make network calls to Yahoo Finance.
- API tests use a fake market-data provider and exercise controller, error handling, routing, CORS, Swagger, and health-check behavior.

Tests should be changed only when a contradictory or impossible assertion has been explicitly reviewed and approved.
