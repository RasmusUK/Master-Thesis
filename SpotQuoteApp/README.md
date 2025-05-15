# EventSourcingFramework

## Spot Quote App

You can start the app with:

```powershell
./Start.ps1
```

You can access the Spot Quote App on <localhost:8080>.

## Local Development

For local development, you can start the MongoDB and Mock-api manually:

```powershell
./DevUp.ps1
```

This will start MongoDB and the Mock-api in the background.

You can clean up by shutting down the containers:

```powershell
docker compose down
```

### Requirements

- Powershell
- Docker & Docker Compose
- .NET 9 SDK
