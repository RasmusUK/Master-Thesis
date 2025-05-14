# EventSourcingFramework

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
  