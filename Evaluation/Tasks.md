# Tasks

## 1. Add the Event Sourcing Framework
Before using the framework’s features, you need to install it and register the necessary services in your application. For the purposes of this evaluation, MongoDB setup is assumed to be preconfigured or handled outside the scope of the task, as it is not required for simply building and compiling the application.

### Steps
1. Install the NuGet package.     
    Use the terminal to add the required dependency:
   ```bash
    dotnet add package EventSourcing.CRUD.Infrastructure
    ```
    Or install it via the NuGet package manager in your IDE.

2. Add configuration to `appsettings.json`  
    Include the necessary configuration section for the framework:
    ```json
    {
      "MongoDb": {
        "EventStore": {
          "ConnectionString": "mongodb://localhost:27022",
          "DatabaseName": "EventStore"
        },
        "EntityStore": {
          "ConnectionString": "mongodb://localhost:27022",
          "DatabaseName": "EntityStore"
        },
        "DebugEntityStore": {
          "ConnectionString": "mongodb://localhost:27022",
          "DatabaseName": "EntityStore_debug"
        },
        "PersonalDataStore": {
          "ConnectionString": "mongodb://localhost:27022",
          "DatabaseName": "PersonalDataStore"
        },
        "ApiResponseStore": {
          "ConnectionString": "mongodb://localhost:27022",
          "DatabaseName": "ApiResponseStore"
        }
      },
      "EventSourcing": {
        "EnableEventStore": true,
        "EnableEntityStore": true,
        "EnablePersonalDataStore": true,
        "Snapshot": {
          "Enabled": true,
          "Trigger": {
            "Mode": "Either",
            "Frequency": "Week",
            "EventThreshold": 100000
          },
          "Retention": {
            "Strategy": "Count",
            "MaxCount": 20,
            "MaxAgeDays": 180
          }
        }
      }
    }    
    ```
3.  Load configuration in `Startup.cs`  
    In your `Startup.cs` (or program file), load the configuration from the JSON file inside the `ConfigureServices`:
    ```csharp
    var configuration = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", false, true)
      .Build();
    ```
3. Register the event sourcing services  
  Still in `Startup.cs` inside `ConfigureServices`, register the framework with the DI container:
    ```csharp
    using EventSourcingFramework.Infrastructure.DI;

    ...

    services.AddEventSourcing(configuration, (schema, migrations, migrator, mongoDbRegistrationService) =>
    {       
      // Register entities
      // Register migrations
    });
    ```
## 2. Identify and Model Key Domain Entities
The key domain entities must be identified and modelled following the guidelines:

- All entities must inherit from the `Entity` base class, which includes a `Guid Id`.
- Always reference other entities using their `Id` (e.g., `CustomerId`), not by embedding full objects.
- You do **not** need to define any events yourself. The framework auto-generates events like `SpotQuoteCreated`, `CustomerUpdated`, etc.
- Start simple. Implementing 1–2 entities is enough to get started.
- You can apply the method gradually. Event-sourced entities can live alongside traditional ones.


### Example
```csharp
using EventSourcingFramework.Core.Models.Entity;

public class Car : Entity
{
  public string Model { get; set; }
  public int Year { get; set; }
  
  public Car(string model, int year)
  {
    Model = model;
    Year = year;  
  }
}

public class Order : Entity
{
  public Guid CarId { get; set; }
  
  public Order(Guid carId)
  {
    CarId = carId;
  }
}
```

### Task
Start by defining the core business objects for your domain. In this task, that includes:

- `Customer` – represents the customer requesting a quote.  
- `SpotQuote` – represents a quote and references a `Customer` by ID.

### Customer Properties:
- `string FirstName`
- `string LastName`

### SpotQuote Properties:
- `Guid CustomerId`
- `double Price`

## 3. Register Your Entities
Each domain entity must be explicitly registered with the framework to enable proper persistence, versioning, and change tracking. Registration allows the framework to:

- Automatically generate events for create, update, and delete operations.
- Track the full event history of each entity.
- Associate each entity with its corresponding MongoDB collection.
- Support schema evolution through versioning and future migrations.
- Enable the use of repositories.

### Example

Entity registration is done within the `AddEventSourcing(...)` setup block in your `Startup.cs` or program configuration:
```csharp
services.AddEventSourcing(configuration, (schema, migrations, migrator, mongoDbRegistrationService) =>
{        
  mongoDbRegistrationService.Register(
    (typeof(Car), "Cars"),
    (typeof(Order), "Orders")
  );
});
```

### Task
Register the `SpotQuote` and the `Customer`.

## 4. Interact with Your Domain Through Repositories

Domain entities are accessed and modified through a repository abstraction that exposes standard asynchronous create, read, update, and delete (CRUD) operations. The underlying event sourcing is abstracted away. Each change is automatically captured as an immutable event, maintaining full traceability without extra effort from the developer.

Repositories enable you to:

- Persist and retrieve event-sourced entities using a familiar interface.
- Automatically generate and store domain events for every change.
- Seamlessly integrate into modern async applications and services.
- Introduce transactional operations for atomic updates across multiple entities.

### Example
You can inject the repository wherever needed, such as in services:

```csharp
using EventSourcingFramework.Core.Interfaces;

public class CarService
{
  private readonly IRepository<Car> carRepository;
  
  public CarService(IRepository<Car> carRepository)
  {
    this.carRepository = carRepository;
  }

  public async Task<Guid> CreateCar(string model, int year)
  {
    var car = new Car(model, year);
    await carRepository.CreateAsync(car);
    return car.Id;
  }

  public async Task<Car?> GetCarById(Guid carId)
  {
    return await carRepository.ReadByIdAsync(carId);
  }

  public async Task UpdateCarYear(Guid carId, int year)
  {
    var car = await carRepository.ReadByIdAsync(carId);
    car.Year = year;
    await carRepository.UpdateAsync(car);
  }

  public async Task DeleteCar(Guid carId)
  {
    var car = await carRepository.ReadByIdAsync(carId);
    await carRepository.DeleteAsync(car);
  }
}
```

### Task 

Go to the file `SpotQuoteService.cs` and implement the required CRUD operations using `IRepository<SpotQuote>`.

Specifically, complete the following inside the /* Part 4 */ section:

- `CreateSpotQuote(...)`
- `GetSpotQuote(...)`
- `UpdateSpotQuotePrice(...)`
- `DeleteSpotQuote(...)`


## 5. Define Projections and Read Models for Optimized Queries
To support efficient querying, define lightweight read models tailored to specific use cases. These are built from your persisted entities using filtering and projections, returning only the data needed for a given view or operation.

Unlike domain entities, read models are optimized for performance and avoid overfetching, enabling more scalable and responsive read operations.

### Example
Suppose you need to find all car models for a given year. The projection `car => car.Model` ensures only the model is fetched, and the filter `car => car.Year == year` limits results to the relevant year. You can also project directly into a read model object if needed.

```csharp
public async Task<IReadOnlyCollection<string>> GetCarModelsForYear(int year)
{
    return await carRepository.ReadAllProjectionsByFilterAsync
    (
      car => car.Model, //Projection
      car => car.Year == year //Filter
    );
}
```

### Task
Navigate to `SpotQuoteService.cs` and implement the method `GetAllSpotQuoteIdsForCustomer(...)` in the /* Part 5 */ section. It should return a list of spot quote IDs belonging to a given customer.


## 6. Use the API Gateway for All Your HTTP Calls
All outbound HTTP requests should be routed through the framework’s API gateway abstraction. This ensures:

- Deterministic behavior during replays by caching responses and returning the same results for identical requests.
- Full traceability of external service interactions.
- Safe debugging and testing without impacting live systems.

### Example
Suppose you want to fetch external car data from an endpoint. You can do this by injecting and using the `IApiGateway` interface. The example below uses the `GetAsync` method, but the framework also supports native `PostAsync` and a flexible `SendAsync` method that accepts a complete `HttpRequestMessage` for advanced scenarios:
```csharp
using EventSourcingFramework.Application.Abstractions.ApiGateway;

public class CarService
{
  private readonly IApiGateway apiGateway;

  public CarService(IApiGateway apiGateway)
  {
    this.apiGateway = apiGateway;
  }

  public async Task <IReadOnlyCollection<Car>> FetchCarsExternally()
  {
    return await apiGateway.GetAsync<IReadOnlyCollection<Car>>("/cars");
  }
}
```

### Task
Go to the method `FetchExternalSpotQuotes(...)` in `SpotQuoteService.cs` and implement it to fetch external spot quotes from the endpoint `/spotquotes`.


## 7. Use Replay and Debugging Tools to Analyze and Evolve Your System Safely

The framework provides powerful tools for debugging and safe system evolution:
- **Time-travel debugging**: Rewind the system to any historical point.
- **Historical state restoration**: Observe the exact entity state after any event.
- **Controlled replay**: Test logic changes using real event history without affecting production data.

These capabilities help you:

- Diagnose bugs in production without disrupting live operations.
- Validate the effect of new business logic safely.
- Explore how entities evolved over time with full traceability.

### Example
Inject `IReplayService` wherever you need to initiate replay operations. This allows for debugging or full system restoration. The `IEntityHistoryService` allows for history inspection.

- `UseSnapshot`: Set to true to speed up replay using snapshots (default for most cases).
- `AutoStop`: Set to true to return automatically to current state after replay (useful when restoring state).

```csharp
using EventSourcingFramework.Application.Abstractions.EntityHistory;
using EventSourcingFramework.Application.Abstractions.Replay;

public class DebugService
{
  private readonly IReplayService replayService;
  private readonly IEntityHistoryService entityHistoryService;

  public DebugService(IReplayService replayService, 
    IEntityHistoryService entityHistoryService)
  {
    this.replayService = replayService;
    this.entityHistoryService = entityHistoryService;
  }
  
  public async Task DebugTo(DateTime dateTime)
  {
    await replayService.ReplayUntilAsync(dateTime, 
      useSnapshot: true, 
      autoStop: false
    );
  }

  public async Task RestoreEntityStore()
  {
    await replayService.ReplayAllAsync(
      useSnapshot:false,
      autoStop: true
    );
  }
  
  public async Task<IReadOnlyCollection<Car>> GetCarHistory(Guid carId)
  {
    return await entityHistoryService.GetEntityHistoryAsync<Car>(carId);
  }
}
```

### Task
In `SpotQuoteService.cs` under /* Part 7 */ do the following:
1. Implement `GetSpotQuoteHistory(...)`. 

    It should return all historical states of the specified spot quote ID.
2. Implement `DebugTo(...)`
    
    It should replay the system to the given point in time.

3. Implement `ResetEntityStore(...)`

    This should completely rebuild the entity store from events (without using snapshots, and with auto-stop enabled).

## 8. Evolve Your Domain Model Using Versioning and Migrations     

As your domain model changes, you can safely update it without breaking existing data by using the framework’s built-in schema versioning and migration tools.

- Older events remain replayable even after schema changes.
- Legacy entity data can be upgraded to match the current schema using custom migration logic.
- This ensures long-term maintainability and consistency across your system.

### Example
Suppose we want to upgrade our `Car` entity and change the model to an enum instead. Assume we have created all models as enums and added a custom `Model.ParseFromString` method. 

1. First we change the name of our existing class and set the `SchemaVersion = 1`. 

    Do not use refactoring tools for it since all existing used of `Car` should still be `Car` and not `CarV1`.
    ```csharp
    public class CarV1 : Entity
    {
      public string Model { get; set; }
      public int Year { get; set; }
      public new int SchmeaVersion { get; set; } = 1;
      
      public CarV1(string model, int year)
      {
        Model = model;
        Year = year;  
      }
    }
    ```
2. Create the new `Car` entity with the changes and set the `SchemaVersion = 2`:
    ```csharp
    public class Car : Entity
    {
      public Model Model { get; set; }
      public int Year { get; set; }
      public int SchmeaVersion { get; set; } = 2;
      
      public Car(Model model, int year)
      {
        Model = model;
        Year = year;  
      }
    }
    ```
3. Inside the event sourcing registration method in `Startup.cs` or equivalent, register the entity versions, migrations, and a migration function to upgrade from the old version to the new one:    
    ```csharp
    services.AddEventSourcing(configuration, (schema, migrations, migrator, mongoDbRegistrationService) =>
    {
        mongoDbRegistrationService.Register(
          (typeof(Car), "Cars"),
          (typeof(Order), "Orders")
        );

        schema.Register(typeof(Car), 2);

        migrations.Register<Car>(1, typeof(CarV1));
        migrations.Register<Car>(2, typeof(Car));

        migrator.Register<CarV1, Car>(1, v1 => 
        new Car(Model.ParseFromString(v1.Model), v1.Year));
    });
    ```

### Task

1. Update existing `Customer` to `CustomerV1`.
2. Create a new `Customer` version with a `Name` property instead of `FirstName` and `LastName`. 
2. Register the schema version, the migrations, and the migration function in `Startup.cs`.


## 9. Handle Personal Data Securely and in Compliance with Regulations
To support GDPR compliance and ensure secure handling of sensitive user data, the framework allows you to mark personally identifiable fields with the `[PersonalData]` attribute. These fields are automatically excluded from the event store and stored separately in a dedicated personal data store.

### Example
Suppose that we have an `User` entity with the following properties:
- First name
- Last name
- Email
- CPR

To ensure these values are not stored in the immutable event log, but instead handled seperately, annotate them like this:
```csharp
public class User : Entity
{
  [PersonalData]
  public string FirstName { get; set; }

  [PersonalData]
  public string LastName { get; set; }

  [PersonalData]
  public string Email { get; set; }

  [PersonalData]
  public string CPR { get; set; }
}
```

### Task
Update the Customer entity you previously created to mark the `Name` property as `[PersonalData]`.


## 10. Implement Operational Logging and Auditing as Needed

While the framework captures all domain changes in an immutable event store, it does not enforce any specific operational logging or audit trail mechanisms. Instead, it provides a foundation upon which you can build domain-specific logging and monitoring features.

### Example
Suppose you want to analyze recent system activity. Since all domain changes are events, you can simply query the event store and transform the data however you like. Here's an example that logs the most recent 50 events:
```csharp
public class AuditLogger
{
  private readonly IEventStore eventStore;
  private readonly ILogger<AuditLogger> logger;

  public AuditLogger(IEventStore eventStore, ILogger<AuditLogger> logger)
  {
    this.eventStore = eventStore;
    this.logger = logger;
  }

  public async Task LogRecentEventsAsync(int count = 50)
  {
    var events = await eventStore.GetEventsAsync();
    var recentEvents = events
      .OrderByDescending(e => e.Timestamp)
      .Take(count);

    foreach (var e in recentEvents)
    {
      logger.LogInformation("Event: {Type} | Event Id: {EventId} | Entity Id: {EntityId} | Timestamp: {Timestamp}",
        e.Typename,
        e.Id,
        e.EntityId,
        e.Timestamp);
    }
  }
}
```

### Task
Imagine you want to understand system activity by tracking how many events occurred recently.
Implement the method `NrOfEventsTheLastHour()` in `SpotQuoteService.cs`. It should return the number of events that occurred in the past hour.

**Tip**: Use `DateTime.UtcNow.AddHours(-1)` to define the lower bound of the query.
