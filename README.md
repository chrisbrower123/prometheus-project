# prometheus-project
My take-home project from Prometheus, an intraday stock visualization tool.

## Run with Docker

Prerequisite: Docker Desktop must be running with the Linux container engine enabled. Verify it with:

```powershell
docker info
```

If Docker reports that it cannot connect to `dockerDesktopLinuxEngine`, start Docker Desktop and wait until it reports that the engine is running.

From the repository root:

```powershell
docker compose up --build
```

Open the frontend at `http://localhost:5173`. The frontend container serves the Vue application and proxies `/api` requests to the API container. The API is also available directly at `http://localhost:5087`, with health status at `/health`.

Stop the containers with:

```powershell
docker compose down
```

## Run Separately

### Backend

From the repository root, start the API:

```powershell
dotnet run --project api/src/StockData.Api/StockData.Api.csproj --urls http://localhost:5087
```

The backend is available at `http://localhost:5087`. Health status is available at `http://localhost:5087/health`.

### Frontend

In a second terminal, create `frontend/.env.local` with:

```env
VITE_API_BASE_URL=http://localhost:5087
```

Then start the Vue development server:

```powershell
cd frontend
npm install
npm run dev
```

Open the Vite URL shown in the terminal, normally `http://localhost:5173`.

To validate the frontend production build:

```powershell
cd frontend
npm run build
```
