# EventSourcingFramework

## Local Development

For local development, you can start the MongoDB container manually:

```powershell
docker compose up -d
```

This will start MongoDB in the background.

You can clean up by shutting down the MongoDB container:

```powershell
docker compose down
```

### Requirements

- Docker & Docker Compose
- .NET 9 SDK

## Running Integration Tests

To run the integration tests locally, run:

```powershell
./Run-IntegrationTests.ps1
```

### Requirements

- Powershell
- Docker & Docker Compose
- .NET 9 SDK

### What the Script Does

- Starts a MongoDB container using Docker Compose
- Runs:
  - Application integration tests
  - Infrastructure integration tests
- Cleans up by shutting down the MongoDB container
  