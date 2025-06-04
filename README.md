# Master Thesis

This repository contains the following projects:

- EventSourcingFramework

  The core library providing full event sourcing functionality.
- SpotQuoteApp

  A sample application that demonstrates how to use the framework in a realistic domain scenario, specifically, for managing spot quotes.

- Mock-api

  A lightweight mock HTTP API used by `SpotQuoteApp` to simulate external buying rate requests

- Example

  A minimal standalone project that supports the _How to Use the Event Sourcing Framework_ guide and the quick-start example from this README.

## Spot Quote App

### Running the App
Start the Spot Quote App locally:

- **Windows**:
  ```
  cd SpotQuoteApp
  ./Start.ps1
  ```
- **Linux/macOS**
  ```
  cd SpotQuoteApp
  ./Start.sh
  ```
  
Access the app at: http://localhost:8080

### Requirements 

- PowerShell or Bash
- Docker & Docker Compose

## Local Development: Spot Quote App

To start the local dev environment:

- **Windows**:
  ```
  cd SpotQuoteApp
  ./DevUp.ps1
  ```
- **Linux/macOS**
  ```
  cd SpotQuoteApp
  ./DevUp.sh
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
  cd EventSourcingFramework
  ./DevUp.ps1
  ```
- **Linux/macOS**
  ```
  cd EventSourcingFramework
  ./DevUp.sh
  ```

This will start a MongoDB instance.

### Running Integration Tests

- **Windows**:
  ```
  cd EventSourcingFramework
  ./RunIntegrationTests.ps1
  ```
- **Linux/macOS**
  ```
  cd EventSourcingFramework
  ./RunIntegrationTests.sh
  ```
  
This will also spin up a MongoDB instance for the tests.


### Requirements
- PowerShell or Bash
- Docker & Docker Compose
- .NET 9 SDK

  
# How to Use the Event Sourcing Framework
This guide walks you through setting up and using the event sourcing framework, including how to define domain objects, configure persistence, and manage versioning and replay.

## Prerequisites

- Basic understanding of C# and .NET.
- MongoDB instance running (locally or remotely).
- Basic familiarity with NoSQL principles:
  - Prefer storing object references as IDs instead of direct nested documents when modeling relationships.
  
    Below you can see an example of this. Instead of embedding the full `Address` object inside the `Order` document, we store only its ID. This keeps the documents lightweight, supports better versioning, and avoids data duplication:
    ```csharp
    // Avoid embedding related objects
    public class Order
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }

        // Embedded object — discouraged
        public Address ShippingAddress { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }

    // Reference by ID
    public class Order
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }

        // Reference by ID — preferred
        public Guid ShippingAddressId { get; set; }
    }

    public class Address
    {
        public Guid Id { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    } 
    ```
## Step 1: Install and Configure the Framework
### 1. Add the Framework to Your Application
Add the following `EventSourcing.CRUD.Infrastructure` nuget package.

In your `Program.cs` or `Startup.cs`, register the framework:
```csharp
using EventSourcingFramework.Infrastructure.DI;

public void ConfigureServices(IServiceCollection services)
{
  var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .Build();
  services.AddEventSourcing(configuration, (schema, migrations, migrator, mongoDbRegistrationService) =>
  {                  
    // Register Entities (Step 2)   
    // Register Migrations (Step 7)
  });
}
```
### 2. Add AppSettings Configuration
Update your `appsettings.json` with the required configuration:
```csharp
{
  "MongoDb": {
    "EventStore": {
      "ConnectionString": "mongodb://localhost:27017",
      "DatabaseName": "EventStore"
    },
    "EntityStore": {
      "ConnectionString": "mongodb://localhost:27017",
      "DatabaseName": "EntityStore"
    },
    "DebugEntityStore": {
      "ConnectionString": "mongodb://localhost:27017",
      "DatabaseName": "EntityStore_debug"
    },
    "PersonalDataStore": {
      "ConnectionString": "mongodb://localhost:27017",
      "DatabaseName": "PersonalDataStore"
    },
    "ApiResponseStore": {
      "ConnectionString": "mongodb://localhost:27017",
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
        "EventThreshold": 1000
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
Remember to set the properties of your `appsettings.json` file so it is copied to the output folder during build.
#### **Explanation of Settings**
MongoDb Section
- EventStore: Stores all emitted domain events
- EntityStore: Stores the current state of entities after events are applied (also includes snaphots)
- DebugEntityStore: A separate database used when replaying in debug mode to prevent modifying production data.
- PersonalDataStore: Store for sensistive personal data, supporting privacy and compliance requirements.
- ApiResponseStore: Caches external API responses for deterministic replay of API calls.

EventSourcing Section
- EnableEventStore: Enables the events store. Set to `false` to disable event tracking entirely.
- EnableEntityStore: Enables the entity store for state reconstruction and queering.
- EnablePersonalDataStore: Enables use of the personal data store.

Snapshot Configuration
- Snapshot.Enabled: Enables automatic snapshotting to optimize replay performance.
- Snapshot.Trigger:
  - Mode: Can be one of:
    - `EventCount`: triggers after a certain number of events.
    - `Time`: triggers based on time interval.
    - `Either`: triggers if either condition is met.
    - `Both`: triggers if both conditions are met.
  - Frequency: Used with `Time` mode. Examples values: `Day`, `Week`, `Month`, `Year`. 
  - EventThreshold: Used with `EventCount` mode. Number of events after which a snapshot is taken.
 - Snapshot.Retention:
    - Strategy: Defines how old snapshots are cleaned up. Options include `Count`, `Time`, or a combination (`All`).
    - MaxCount: Maximum number of snapshots to retain.
    - MaxAgeDays: Maximum age (in days) of a snapshots before it´s considered expired and removed. 

## Step 2: Define Your Domain Objects
All domain entities must inherit from the  `Entity` base class provided by the framework. There is no need to define an `Id` property manually, as it is already included in the Entity base class. 

```csharp
public class Customer : Entity
{
  public string Name { get; set; }
}

public class Order : Entity
{
  public Guid CustomerId { get; set; }
}
```
Next, register your entities during service configuration. This tells the framework which entity types to persist and what MongoDB collection names to use for each.
```csharp
services.AddEventSourcing(configuration, (schema, migrations, migrator, mongoDbRegistrationService) =>
{                  
  mongoDbRegistrationService.Register(
    (typeof(Customer), "Customers"),
    (typeof(Order), "Orders")
  );
});
```
- `typeof(Customer)` and `typeof(Order)` specify the entity types to register.
- `"Customers"` and `"Orders"` define the corresponding MongoDB collection names.
- This registration ensures your entities are tracked and stored correctly in the entity store. It also enables full event sourcing functionality for those types, as it automatically registers the corresponding Create, Update, and Delete events for each entity.

## Step 3: Use Repositories

## CRUD Interface

The repository exposes the following **CRUD** operations for any entity type `T` that implements the `IEntity` interface:

### Create

- `Task CreateAsync(T entity)`
  - Adds a new entity.

### Read

- `Task<T?> ReadByIdAsync(Guid id)`
  - Retrieves an entity by its unique ID.

- `Task<T?> ReadByFilterAsync(Expression<Func<T, bool>> filter)`
  - Retrieves a single entity matching a filter.

- `Task<TProjection?> ReadProjectionByIdAsync<TProjection>(Guid id, Expression<Func<T, TProjection>> projection)`
  - Retrieves a projected view of an entity by ID.

- `Task<TProjection?> ReadProjectionByFilterAsync<TProjection>(Expression<Func<T, bool>> filter, Expression<Func<T, TProjection>> projection)`
  - Retrieves a projected view of an entity that matches the given filter.

- `Task<IReadOnlyCollection<T>> ReadAllAsync()`
  - Retrieves all entities.

- `Task<IReadOnlyCollection<T>> ReadAllByFilterAsync(Expression<Func<T, bool>> filter)`
  - Retrieves all entities that match a given filter.

- `Task<IReadOnlyCollection<TProjection>> ReadAllProjectionsAsync<TProjection>(Expression<Func<T, TProjection>> projection)`
  - Retrieves projections of all entities.

- `Task<IReadOnlyCollection<TProjection>> ReadAllProjectionsByFilterAsync<TProjection>(Expression<Func<T, TProjection>> projection, Expression<Func<T, bool>> filter)`
  - Retrieves projections of entities matching the filter.

### Update

- `Task UpdateAsync(T entity)`
  - Updates an existing entity.

### Delete

- `Task DeleteAsync(T entity)`
  - Deletes an entity.


### Definitions

- A **filter** is a condition used to select specific records.
  ```csharp
  order => order.ShippingAddressId = 1
  ```
- A **projection** transforms each entity into a different form — often by selecting only specific fields or mapping to a DTO.  
  ```csharp
  order => new { order.Id, order.ShippingAddressId }
  ```

## Repository Interaction Options
You can now choose how to interact with your data:

### Option A: Inject Generic Repositories
Inject `IRepository<T>` where you need it:
```csharp
public class CustomerService
{
  private readonly IRepository<Customer> repository;

  public CustomerService(IRepository<Customer> repository)
  {
    this.repository = repository;
  }

  public Task CreateCustomerAsync(string name)
  {
    var customer = new Customer { Name = name };
    return repository.CreateAsync(customer); 
  }
}
```
### Option B: Create a Domain-Specific Repository
If you prefer domain logic encapsulation:
```csharp
public interface IOrderRepository
{
  Task<Guid> CreateOrderAsync(Guid customerId);
  Task UpdateOrderCustomerId(Guid orderId, Guid customerId);
  Task DeleteOrder(Guid orderId);
  Task<IReadOnlyCollection<Guid>> GetAllOrderIdsForCustomerIdAsync(Guid customerId);  
}

public class OrderRepository : IOrderRepository
{
  private readonly IRepository<Order> orderRepository;  

  public OrderRepository(IRepository<Order> orderRepository)
  {
    this.orderRepository = orderRepository;
  } 

  public async Task<Guid> CreateOrderAsync(Guid customerId)
  {
    var order = new Order
    {
      CustomerId = customerId
    };
    await orderRepository.CreateAsync(order);
    return order.Id;
  } 

  public async Task UpdateOrderCustomerId(Guid orderId, Guid customerId)
  {
    var order = await orderRepository.ReadByIdAsync(orderId);
    order!.CustomerId = customerId;
    await orderRepository.UpdateAsync(order);
  } 

  public async Task DeleteOrder(Guid orderId)
  {
    var order = await orderRepository.ReadByIdAsync(orderId);
    await orderRepository.DeleteAsync(order!);
  } 

  public Task<IReadOnlyCollection<Guid>> GetAllOrderIdsForCustomerIdAsync(Guid customerId)
  {
    return orderRepository.ReadAllProjectionsByFilterAsync(o => o.Id, o => o.CustomerId == customerId);
  }
}
```
Finally, register your custom service or domain-specific repository in your application's dependency injection container:
```csharp
services.AddScoped<IOrderRepository, OrderRepository>();
```
## Step 4: Using External API
Use `IApiGateway` to perform HTTP-based integration from within your domain or service logic:
```csharp
public class CustomerService
{
  private readonly IApiGateway apiGateway;

    public CustomerService(IApiGateway apiGateway)
    {
      this.apiGateway = apiGateway;
    }

    public Task<IReadOnlyCollection<Customer>> FetchCustomersExternallyAsync()
    {
      return apiGateway.GetAsync<IReadOnlyCollection<Customer>>("/customers");
    }

    public Task SendCustomerAsync(Customer customer)
    {
      return apiGateway.SendAsync<Customer>(new HttpRequestMessage
      {
        Method = HttpMethod.Post,
        RequestUri = new Uri("/customers")
      });
    }
    
    public Task<CustomerResponse> PostCustomerAsync(CustomerRequest request)
    {
      return apiGateway.PostAsync<CustomerRequest, CustomerResponse>("/customers", request);
    }
}
```

## Step 5: Using Replay
Inject `IReplayService` wherever you need to perform a replay operation, for example, to enable time-travel debugging, view the history of an entity, or completely restore the entity store from events:

```csharp
public class DebugService
{
  private readonly IReplayService replayService;

  public DebugService(IReplayService replayService)
  {
    this.replayService = replayService;
  }
  
  public Task TimeTravelToAsync(DateTime dateTime)
  {
    return replayService.ReplayUntilAsync(dateTime);
  }

  public Task RestoreEntityStoreAsync()
  {
    return replayService.ReplayAllAsync(useSnapshot:false);
  }
  
  public Task<IReadOnlyCollection<T>> GetEntityHistoryAsync<T>(Guid entityId) where T : Entity
  {
    return entityHistoryService.GetEntityHistoryAsync<T>(entityId);
  }
}
```

## Step 6: Snapshots
Snapshots are handled automatically by the framework based on configuration. However, if needed, you can manually take, restore, or delete snapshots using `ISnapshotService`:

```csharp
public class SnapshotTool
{
  private readonly ISnapshotService snapshotService;
  private readonly IEventSequenceGenerator eventSequenceGenerator;

  public SnapshotTool(ISnapshotService snapshotService,IEventSequenceGenerator eventSequenceGenerator)
  {
    this.snapshotService = snapshotService;
    this.eventSequenceGenerator = eventSequenceGenerator;
  }

  public async Task TakeSnapshotAsync()
  {
    var currentEventNumber = await eventSequenceGenerator.GetCurrentSequenceNumberAsync();
    await snapshotService.TakeSnapshotAsync(currentEventNumber);
  }
  
  public async Task RestoreSnapshotAsync(string snapshotId)
  {
    await snapshotService.RestoreSnapshotAsync(snapshotId);
  }
  
  public async Task DeleteSnapshotAsync(string snapshotId)
  {
    await snapshotService.DeleteSnapshotAsync(snapshotId);
  }
}
```

## Step 7: Evolving Your Domain Model
When you need to make breaking changes to your domain objects:

1. Version the current entity:
    - Rename the existing class to e.g. `CustomerV1` and set `SchemaVersion = 1`.
    ```csharp
    public class CustomerV1 : Entity
    {
      public int SchemaVersion { get; set; } = 1; 
      public string FirstName { get; set; }
      public string LastName { get; set; }
    }
    ```
2. Create the new version:
    - Copy the old class to a new class named `Customer` and set `SchemaVersion = 2`
    - Make your desired changes.
    ```csharp
    public class Customer : Entity
    {
      public int SchemaVersion { get; set; } = 2;
      public string Name { get; set; }
    }
    ```
3. Inside your event sourcing registration method, register the entity versions, migrations, and a migration function to upgrade from the old version to the new one:
```csharp
services.AddEventSourcing(configuration, (schema, migrations, migrator, mongoDbRegistrationService) =>
{
  mongoDbRegistrationService.Register(
    (typeof(Customer), "Customers"),
    (typeof(Order), "Orders")
  );
  
  schema.Register(typeof(Customer), 2);
  
  migrations.Register<Customer>(1, typeof(CustomerV1));
  migrations.Register<Customer>(2, typeof(Customer));
  
  migrator.Register<CustomerV1, Customer>(1, v1 => new Customer
  {
    Id = v1.Id,
    Name = $"{v1.FirstName} {v1.LastName}"
  });
});
```
This ensures old events and entities can still be replayed and mapped to the new schema.

## Step 8: Personal Data Handling
To support GDPR compliance, you can annotate sensitive fields with the `[PersonalData]` attribute.
This ensures that the marked properties are automatically stripped from events and stored securely in the dedicated personal data store.

```csharp
public class Customer : Entity
{
  [PersonalData]
  public string Name { get; set; }
}
```
It is the developers responsibility to determine what constitutes personal data based on the domain and applicable regulations (e.g., GDPR).
The framework provides the mechanism, but you must define which fields are sensitive and ensure compliance, including implementing appropriate data retention strategies (e.g., scheduled jobs to purge expired records).


## Step 9: Logging And Auditing (Optional)
Although the framework provides a complete event store behind the scenes, you may want to expose events for operational logging, audit trails, or debugging.

You can easily fetch and log recent events using the IEventStore interface provided by the framework:
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
The developer is in charge of implementing logging and auditing based on the specific needs of the domain and compliance requirements. While the event store provides a complete history of changes, the framework does not automatically expose logs or audit trails. It is up to the developer to decide what to track, how to track it, and how to surface it.
## You Are Ready
Once these steps are complete:

- Events are handled behind the scenes.
- Repositories and APIs are ready to use.
- Replay and snapshot capabilities are fully integrated.
- The system is prepared for versioning and schema evolution.
- Personal data is separated and managed in compliance with GDPR.

# Quick Start Example
Here is a minimal end-to-end example to help you get started quickly.

## Define Your Entity
```csharp
public class Customer : Entity
{
  public string Name { get; set; }
}
```

## Register in Program.cs / Startup.cs

```csharp
services.AddEventSourcing(configuration, (schema, migrations, migrator, mongoDbRegistrationService) =>
{
  mongoDbRegistrationService.Register((typeof(Customer), "Customers"));
});
```

## Use the Repository

```csharp
public class CustomerService
{
  private readonly IRepository<Customer> repository;

  public CustomerService(IRepository<Customer> repository)
  {
    this.repository = repository;
  }

  public async Task<Guid> CreateCustomerAsync(string name)
  {
    var customer = new Customer { Name = name };
    await repository.CreateAsync(customer);
    return customer.Id;
  }

  public Task<Customer?> GetCustomerAsync(Guid id)
  {
    return repository.ReadByIdAsync(id);
  }
}
```

## Register the Service
```csharp
services.AddScoped<CustomerService>();
```
