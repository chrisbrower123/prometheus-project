# Backend Architecture

The backend lives under `api/` and is split into three production projects:

```text
api/
├── src/
│   ├── StockData.Domain/
│   ├── StockData.Infrastructure/
│   └── StockData.Api/
└── tests/
    ├── StockData.Domain.Tests/
    ├── StockData.Infrastructure.Tests/
    └── StockData.Api.Tests/
```

The separation is intentional. Each project represents a different reason to change:

1. Domain rules change when the meaning of stock data changes.
2. Infrastructure changes when Yahoo Finance or another external provider changes.
3. API changes when the HTTP contract or hosting requirements change.

Keeping those reasons separate reduces coupling and makes each area easier to test.

## The three-project separation

### 1. StockData.Domain

`StockData.Domain` is the center of the application. It owns the business meaning of stock data and contains rules that should remain valid regardless of whether data came from Yahoo Finance, a database, a test fixture, or another provider.

The Domain project deliberately does not reference ASP.NET Core, `HttpClient`, Yahoo DTOs, JSON serialization, Swagger, or web-specific response types.

Its responsibilities include:

- Defining normalized stock symbols.
- Representing intraday bars and series.
- Aggregating intraday bars into daily summaries.
- Defining provider-facing and application-facing interfaces.
- Coordinating the request workflow through domain services.
- Defining meaningful domain exceptions.

The Domain project is not just a collection of data classes. Its models protect invariants at their boundaries so invalid state is rejected close to its source.

### 2. StockData.Infrastructure

`StockData.Infrastructure` implements technical details that are outside the business model.

The current provider is Yahoo Finance, but the Domain layer only sees the `IMarketDataProvider` abstraction. The Domain does not need to know the Yahoo URL format, query parameters, response DTO shape, retry policy, or HTTP status codes.

Infrastructure is responsible for:

- Configuring the Yahoo Finance HTTP client.
- Validating provider options such as base URL, timeout, and user agent.
- Building provider requests for supported intervals.
- URL-encoding symbols and converting request windows to Unix seconds.
- Deserializing Yahoo JSON into infrastructure-only DTOs.
- Mapping provider DTOs into Domain models.
- Resolving exchange time zones and fixed-offset fallbacks.
- Translating provider failures into Domain exceptions.
- Applying resilience behavior to transient provider responses.

The provider boundary is important because external response formats are not application contracts. Yahoo response objects are mapped before they reach the Domain.

### 3. StockData.Api

`StockData.Api` is the delivery and hosting layer. It exposes the application through HTTP and contains web concerns rather than business rules.

It is responsible for:

- Creating and configuring the ASP.NET Core host.
- Registering controllers and dependency injection.
- Defining routes and HTTP verbs.
- Parsing route input through Domain APIs.
- Mapping Domain results to JSON response DTOs.
- Translating exceptions into problem responses.
- Configuring CORS, Swagger, health checks, and middleware order.
- Providing the production process entry point.

Controllers should remain thin. The daily summaries controller parses the symbol, calls `IDailySummaryService`, and maps the result. It does not calculate date windows, call Yahoo directly, aggregate bars, or decide how duplicates are handled.

## Dependency direction

The dependency graph is:

```text
StockData.Api
    ├──> StockData.Domain
    └──> StockData.Infrastructure ───> StockData.Domain
```

The arrows represent compile-time project dependencies.

The Domain does not reference either outer project. This direction gives the business rules the greatest independence:

- Domain tests do not require a web host or network access.
- Infrastructure can be replaced without rewriting aggregation logic.
- The API can change its transport details without changing provider mapping.
- A future provider can implement `IMarketDataProvider` without changing Domain models.

Dependency injection is the composition mechanism that connects the projects at runtime. The API registers Domain services and Infrastructure implementations, but the Domain itself still depends only on abstractions.

## Domain model

### Symbol

`Symbol` is a normalized value object for a market symbol.

Parsing performs the following work:

- Rejects null, empty, and whitespace-only input.
- Trims surrounding whitespace.
- Normalizes letters with `ToUpperInvariant()`.
- Enforces the supported length range.
- Accepts ASCII letters, digits, and supported punctuation.
- Rejects internal whitespace and unsupported characters.
- Rejects punctuation-only values and invalid trailing punctuation.

`Symbol.Parse` throws `InvalidSymbolException` when input is invalid. `Symbol.TryParse` returns `false` and produces the default symbol instead of throwing. Keeping both operations on shared validation rules prevents them from disagreeing.

The API uses `Symbol.Parse` at the boundary so malformed route input is rejected before the provider is called.

### IntradayBar

`IntradayBar` represents one timestamped market observation:

- Timestamp.
- Low price.
- High price.
- Volume.

Prices use `decimal`, not `double`, because price calculations need predictable decimal precision. Low and volume cannot be negative, and high cannot be lower than low. Equal high and low values are valid, as are zero prices and zero-volume bars where the provider data allows them.

### IntradaySeries

`IntradaySeries` groups bars with a symbol and an exchange time zone. It validates required values and defensively copies the input bars so callers cannot mutate the series through the original collection.

The exchange time zone belongs to the series because daily grouping depends on the exchange's calendar, not on the machine running the API.

### DailySummary

`DailySummary` is the Domain output of aggregation:

- A `DateOnly` exchange-local day.
- The average low price for that day.
- The average high price for that day.
- The summed volume as a `long`.

The Domain retains full decimal precision. Presentation rounding is an API or frontend concern.

## Aggregation rules

The daily aggregator is stateless and returns an immutable/read-only result.

Its processing rules are:

1. Reject a null series.
2. Return no summaries for an empty series.
3. Deduplicate bars by their UTC instant, keeping the last input occurrence.
4. Convert each instant into the series exchange time zone.
5. Group bars by the resulting local `DateOnly`.
6. Average lows and highs without rounding.
7. Sum volume as `long`.
8. Sort output ascending by day.
9. Do not create summaries for days with no bars.

This prevents a bar's original offset or the server's local time zone from changing the exchange-local day assigned to the bar.

## Service orchestration

`DailySummaryService` coordinates the use case without becoming a provider implementation.

The service:

1. Validates the symbol before doing other work.
2. Checks cancellation before calculating the request window.
3. Uses the injected `TimeProvider` for the current UTC time.
4. Requests one month of fifteen-minute data through `IMarketDataProvider`.
5. Throws `NoDataAvailableException` when the provider returns no bars.
6. Passes the exact provider series to the aggregator.
7. Returns the aggregator result without wrapping or rewriting provider exceptions.

This makes time and external dependencies injectable in tests while keeping the use-case sequence explicit.

## Domain errors and why they exist

The exception types communicate stable application meanings rather than leaking provider implementation details.

### InvalidSymbolException

Raised when input cannot be represented as a valid `Symbol`. This is a client input problem and becomes HTTP 400 at the API boundary.

### SymbolNotFoundException

Raised when the provider confirms that the requested symbol does not exist. This becomes HTTP 404 and is distinct from a valid symbol for which there is no data.

### NoDataAvailableException

Raised when a valid symbol produces an empty series. This also becomes HTTP 404, but it uses a different problem type so clients can distinguish “unknown symbol” from “known symbol with no data.”

### ProviderRateLimitedException

Raised when the provider returns a rate-limit response. It may carry a positive `RetryAfter` duration, which the API exposes as a `Retry-After` header and the frontend can display to the user.

### ProviderUnavailableException

Raised for provider transport failures, malformed successful responses, unusable provider metadata, and unsuccessful provider responses that are not symbol-not-found or rate-limit cases. It becomes HTTP 502 because the application could not obtain data from its upstream provider.

### Unexpected exceptions

Unexpected exceptions become HTTP 500 through the API exception handler. Response bodies contain safe fixed text and a trace ID, not exception messages, stack traces, URLs, or upstream response bodies.

## End-to-end request flow

```text
HTTP request
    ↓
StockData.Api controller
    ↓ parses route symbol
IDailySummaryService
    ↓ calculates UTC window
IMarketDataProvider
    ↓ Yahoo HTTP request
Yahoo DTO / mapper
    ↓ Domain IntradaySeries
DailyAggregator
    ↓ DailySummary list
API response mapper
    ↓
HTTP JSON response
```

The API error handler sits around this flow and translates known Domain exceptions into safe RFC 7807-style responses.

## Testing strategy

The three test projects mirror the production boundaries:

- Domain tests verify invariants, aggregation, time-zone behavior, and service orchestration without HTTP.
- Infrastructure tests use fixtures and stub handlers, so they verify provider behavior without making Yahoo network calls.
- API tests use a fake provider and verify routes, response contracts, exception handling, CORS, Swagger, and health checks.

This structure lets failures identify the layer that owns the behavior instead of requiring full end-to-end setup for every rule.

## Running the backend

From the repository root:

```powershell
dotnet run --project api/src/StockData.Api/StockData.Api.csproj --urls http://localhost:5087
```

The health endpoint is available at `http://localhost:5087/health`.

The backend can also be started through Docker Compose:

```powershell
docker compose up --build
```

See [backend-test.md](backend-test.md) for test commands and [README.md](../README.md) for both containerized and separate development workflows.
