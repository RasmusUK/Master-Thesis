# Master-Thesis

## Spot Quote App

For local development, you can start the MongoDB container manually:

```powershell
docker compose up -d --build
```

This will start:

- MongoDB 
- Spot Quote App
- Mocked API Endpoint

You can clean up by shutting down the containers:

```powershell
docker compose down
```

You can access the Spot Quote App on <localhost:8080>.

### Requirements

- Docker & Docker Compose
