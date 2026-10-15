# Frontend Architecture

The frontend is a Vue 3 and TypeScript application built with Vite. It is located under `frontend/`.

```text
frontend/
├── src/
│   ├── components/
│   ├── composables/
│   ├── resources/
│   ├── services/
│   ├── types/
│   ├── utils/
│   ├── views/
│   ├── App.vue
│   └── main.ts
├── index.html
├── package.json
└── vite.config.ts
```

## Project responsibilities

### Views

`StockDashboard.vue` is the page-level composition layer. It connects the stock-data composable to the search form, alert, chart, and table.

### Components

- `StockSearch.vue` owns the accessible symbol input form and emits search requests.
- `StockChart.vue` renders the price averages and volume using Chart.js through `vue-chartjs`.
- `StockDataTable.vue` provides an accessible, non-chart representation of all daily values.
- `AlertMessage.vue` translates backend problem types into user-facing messages and exposes support references when available.

Components receive data through props or emit events. They do not own the backend request lifecycle.

### Composables

`useStockData.ts` owns request state:

- Current summaries.
- Loading state.
- Current symbol.
- Problem details and retry information.
- Cancellation of an earlier request when a new search starts.
- Protection against stale responses overwriting newer results.

### Services

`apiClient.ts` is the HTTP boundary. It reads `VITE_API_BASE_URL`, URL-encodes symbols, calls the backend endpoint, parses successful responses, and translates RFC 7807 or fallback error responses into `ApiError` instances.

### Types

`types/stock.ts` contains the frontend representations of `DailySummary`, `ProblemDetails`, and known backend problem-type URNs.

### Utilities

`utils/formatters.ts` contains local-calendar date parsing, price formatting, volume formatting, and client-side symbol validation.

## Data flow

```text
StockSearch
    └── emits symbol
        └── StockDashboard
            └── useStockData
                └── apiClient
                    └── .NET API
                └── data/error state
                    ├── StockChart
                    ├── StockDataTable
                    └── AlertMessage
```

## Configuration

When running the frontend separately, create `frontend/.env.local`:

```env
VITE_API_BASE_URL=http://localhost:5087
```

When running through Docker Compose, the frontend is built with `VITE_API_BASE_URL=/` and Nginx proxies `/api` requests to the API container. This keeps browser requests same-origin.

## Running the frontend

From `frontend/`:

```powershell
npm install
npm run dev
```

The production build can be checked with:

```powershell
npm run build
```

## Accessibility and presentation

The application provides an accessible table in addition to the chart, labeled form controls, live loading/error messages, keyboard-friendly controls, and responsive desktop/mobile layouts. Chart colors use the Prometheus Group navy and red palette, while the table remains the authoritative way to inspect exact values.
