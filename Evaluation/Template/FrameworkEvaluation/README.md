# Tasks


## Part 1 – Initial Setup

### 1. Set up the framework
- Add the event sourcing framework to the project.
- Configure it using the included `appsettings.json` template.

### 2. Implement and register a `SpotQuote` entity
- Design a `SpotQuote` domain model that includes the following properties:
  - `string CustomerName`
  - `double Price`
- Register the entity with the framework.

### 3. Complete the `SpotQuoteService`
- You are provided with an empty class `SpotQuoteService` containing four method signatures: Create, Read, Update, and Delete.
- Implement these methods using the framework’s repository abstraction (`IRepository<T>`).

## Part 2 – Schema Evolution and Extension

### 4. Implement and register a `Location` entity
- Create a new domain entity `Location` with the following properties:
  - `string City`
  - `string Country`
- Register the entity with the framework.

### 5. Evolve the `SpotQuote` entity
- Create a new version of the `SpotQuote` entity with the following changes:
  - Replace `CustomerName` with a new property: `string CustomerId`. 
  - The property should be marked as personal data.
- Add properties to represent the origin and destination of the quote. These should refer to existing `Location` entities, following the reference-by-ID modeling strategy.
    - `X OriginLocation`
    - `X DestinationLocation`
- Ensure that persisted `SpotQuote` data from the old version remains compatible with the new version by versioning it.

### 6. Apply schema versioning
- Use the framework's support for schema versioning and entity migration to support the new version.

## Part 3 - Projections and Read Models

### 7. Read Model
- There is a `SpotQuoteCountriesReadModel` read model that includes:
    - `Guid SpotQuoteId`
    - `string CountryFrom`
    - `string CountryTo`

### 8. Create Get Method to Retrieve Read Model
- Extend `SpotQuoteService` with a method `GetSpotQuoteCountriesReadModel(Guid id)` that uses a projection to retrieve the `SpotQuoteCountriesReadModel` for a given `SpotQuoteId`. 
- Use projections provided by the framework to:
  - Retrieve only the `OriginLocation` and `DestinationLocation` from the `SpotQuote` entity.
  - Then fetch only the `Country` field from each corresponding `Location` entity.
- Avoid fetching unnecessary data - focus on efficiency through projection.


## Part 4 - External Dependencies (HTTP Calls)

### 9. Get External Origin Location for SpotQuote

- Extent `SpotQuoteService` with a method that retrieves the origin `Location` for a given SpotQuoteId by making a HTTP GET request to: `"/spotquotes/{id}/location/origin"`.
- Use the `IApiGateway` abstraction provided by the framework to perform this call.

## Part 5 - Replay

### 10. Time-Travel Debugging

- Add a method `DebugTo(DateTime pointInTime)` that replays all events up to a given point in time, using `IReplayService`.
    - The method should not automatically return to the present. The system should remain in the past state.
    - The implementation should use snapshots to improve performance.

### 11. State Restoration

- Add another method `ResetEntityStore` that restores the full entity store by replaying all historical events from the beginning, using `IReplayService`.
    - The method should automatically stop at the latest event.
    - It should not use snapshots, forcing a full replay from event 0.

### 12. History

- Add a method `GetSpotQuoteHistory(Guid spotQuoteId)` that retrieves a list of all historical versions of a SpotQuote using the event history, using `IEntityHistoryService`.
    - This should use the framework’s history mechanism to return a timeline of changes.