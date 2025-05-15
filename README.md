# Master Thesis

This repository contains the following projects:

- EventSourcingFramework
- Mock-api
- SpotQuoteApp

## Spot Quote App

### Running the App
Start the Spot Quote App locally:

- **Windows**:
  ```
  ./SpotQuoteApp/Start.ps1
  ```
- **Linux/macOS**
  ```
  ./SpotQuoteApp/Start.sh
  ```
  
Access the app at: http://localhost:8080

### Requirements 

- PowerShell or Bash
- Docker & Docker Compose

## Local Development: Spot Quote App

To start the local dev environment:

- **Windows**:
  ```
  ./SpotQuoteApp/DevUp.ps1
  ```
- **Linux/macOS**
  ```
  ./SpotQuote/DevUp.sh
  ```

This will start:
- A MongoDB instance
- A mock API serving a buying rate search endpoint

### Requirements
- PowerShell or Bash
- Docker & Docker Compose
- .NET 9 SDK


## Event Sourcing Framework

### Local Development

To start the local dev environment:

- **Windows**:
  ```
  ./EventSourcingFramework/DevUp.ps1
  ```
- **Linux/macOS**
  ```
  ./EventSourcingFramework/DevUp.sh
  ```

This will start a MongoDB instance.

### Running Integration Tests

- **Windows**:
  ```
  ./EventSourcingFramework/RunIntegrationTests.ps1
  ```
- **Linux/macOS**
  ```
  ./EventSourcingFramework/RunIntegrationTests.sh
  ```
  
This will also spin up a MongoDB instance for the tests.


### Requirements
- PowerShell or Bash
- Docker & Docker Compose
- .NET 9 SDK

  
