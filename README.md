
<p align="left">
  <img src="icon.jpg" alt="Preview" width="400"/>
</p>

# Data Preparation NUnit Framework Extension


[![NuGet](https://img.shields.io/nuget/v/DataPreparation.svg)](https://www.nuget.org/packages/DataPreparation/)
[![License](https://img.shields.io/github/license/zribktad/DataPreparation)](LICENSE)

A C# NUnit Extension library designed to simplify test data preparation and management for your unit tests.

## Overview

DataPreparation is a specialized NUnit extension that provides a structured approach to setting up test data. Unlike other libraries like AutoFixture, DataPreparation focuses on giving developers precise control over how test data is prepared while reducing boilerplate code.

## Key Features

- Streamlined approach to preparing test data with data factories
- Seamless integration with NUnit test framework
- Support for both synchronous and asynchronous data preparation
- Service provider integration for dependency injection in tests
- Support for BDD-style testing with behavior-driven frameworks
- Factory pattern for test data creation and management
- Mocking integration with tools like Moq
- Test data lifecycle management with Before/After test hooks
- SQLite in-memory database support for integration testing
- Screenplay pattern support with Boa Constrictor

## Installation

Install the package via NuGet Package Manager:

```
Install-Package DataPreparation
```

Or via .NET CLI:

```
dotnet add package DataPreparation
```

## Getting Started

To get started with DataPreparation:

1. Add the `[DataPreparationFixture]` attribute to your test class
2. Implement the required interfaces like `IDataPreparationLogger` or `IDataPreparationTestServices`
3. Use `PreparationContext` to access test data factories and service providers
4. Mark your test methods with `[DataPreparationTest]` for automatic data preparation

### Basic Test Setup

```csharp
[DataPreparationFixture]
public class ExampleTextFixture : IDataPreparationLogger, IDataPreparationTestServices
{
    public ILoggerFactory InitializeDataPreparationTestLogger()
    {
        return new NullLoggerFactory();
    }

    public void DataPreparationServices(IServiceCollection serviceCollection)
    {
        // Register your services here
    }

    [DataPreparationTest]
    public void TestExample()
    {
        var sourceFactory = PreparationContext.GetFactory();
        var serviceProvider = PreparationContext.GetProvider();
        
        // Create and use test data
        var testData = sourceFactory.New<ExampleFactory>();
    }
}
```

## Usage Examples

### Factory Pattern for Test Data

Using factories to create test data with control over the creation process:

```csharp
[DataPreparationTest]
public void CreateOrder_FullOrderDTO_ReturnsOrder_Factory() 
{
    // Get the source factory
    var sourceFactory = PreparationContext.GetFactory();
    
    // Create test data using factories
    var customer = sourceFactory.New<Customer, CustomerFactory>();
    var orderDto = sourceFactory.Get<OrderDTO, OrderDtoFactory>();
    
    // Create a service with the test data
    var orderService = sourceFactory.New<OrderService, OrderServiceFactory>(
        ListParams.Use(orderDto, customer));

    // Use the created service and data in tests
    // ...
}
```

### Data Preparation for Tests

Prepare data for specific test cases with custom parameters:

```csharp
[DataPreparationTest]
[UsePreparedDataParamsFor(typeof(CreateOrderTask), [5, "Customer Name", 2])]
public void CreateOrder_FullOrderDTO_ReturnsOrder_Before()
{
    // Access prepared data from the provider
    var provider = PreparationContext.GetProvider();
    var orderDto = provider.GetService<CreationOrderDTO>();
    
    // Access prepared mock repositories
    var mockCustomerRepository = provider.GetService<Mock<IRepository<Customer>>>();
    var mockOrderRepository = provider.GetService<Mock<IRepository<Order>>>();
    
    // Test logic using prepared data
    // ...
}
```

### Async Data Operations

Working with asynchronous data factories and operations:

```csharp
[DataPreparationTest]
public async Task CreateOrder_FullOrderDTO_ReturnsOrderFactoryAsync()
{
    // Create async test data
    var customer = await PreparationContext.GetFactory().NewAsync<Customer, CustomerFactoryAsync>();
    var orderDto = await PreparationContext.GetFactory().GetAsync<OrderDTO, OrderDtoFactoryAsync>();
    
    // Use the async data in tests
    // ...
}
```

### Integration with SQLite for Integration Testing

Use in-memory SQLite for integration tests:

```csharp
[DataPreparationFixture]
public class SqLiteDataPreparationFixture : IDataPreparationTestServices
{
    public void DataPreparationServices(IServiceCollection serviceCollection)
    {
        // Setup SQLite in-memory database
        SqliteConnection databaseConnection = new("DataSource=:memory:");
        databaseConnection.Open();
        serviceCollection.AddDbContext<YourDbContext>(options =>
            options.UseSqlite(databaseConnection));
            
        // Register repositories and services
        serviceCollection.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        serviceCollection.AddScoped<IYourService, YourService>();
    }
}
```

### BDD Testing Pattern

Using DataPreparation with BDD-style tests:

```csharp
[DataPreparationFixture]
[Story(
    AsA = "data preparer",
    IWant = "to execute a complete workflow",
    SoThat = "I can verify the full transition process")]
public class WorkflowBddTest : SqLiteDataPreparationFixture
{
    [DataPreparationTest]
    [UsePreparedDataFor(typeof(UpdateStatusTask))]
    public void CompleteWorkflow_FromCreateToDelivered()
    {
        this.Given(_ => _steps.GivenIHaveActor())
            .And(_ => _steps.GivenActorCanUseSourceFactory())
            .And(_ => _steps.GivenActorCanUseService())
            .When(_ => _steps.WhenICreateEntity())
            .Then(_ => _steps.ThenIChangeStatusTo(Status.PROCESSING))
            .Then(_ => _steps.ThenIChangeStatusTo(Status.DELIVERED))
            .BDDfy();
    }
}
```

### Screenplay Pattern Support

Using DataPreparation with the Boa Constrictor screenplay pattern:

```csharp
[DataPreparationTest]
public async Task CreateEntity_ValidDTO_ReturnsEntity()
{
    // Create an actor with abilities
    var actor = new Actor("Tester");
    actor.Can(UseSourceFactory.FromDataPreparation());
    actor.Can(UseService.FromDataPreparationProvider());
    
    // Actor performs tasks and asks questions
    var dto = await actor.AsksForAsync(NewDtoAsync.WithNoArgs());
    var createTask = CreateEntityTask.For(dto);
    actor.AttemptsTo(createTask);
    
    // Assert on results
    var result = actor.AsksFor(EntityById.WithId(createTask.CreatedEntity.Id));
    result.ShouldNotBeNull();
    result.Name.ShouldBe(dto.Name);
}
```

### Test Data Lifecycle Management

Managing test data setup and teardown:

```csharp
[PreparationClassFor(typeof(TestedClass))]
public class TestedClassData : IBeforePreparation
{
    [UpData]
    public void SetupTestData()
    {
        // Setup code that runs before the test
    }
    
    [DownData]
    public void CleanupTestData()
    {
        // Cleanup code that runs after the test
    }
}
```

## Advanced Features

### Tracking Data History

Access previously created data in your tests:

```csharp
// Get all instances created by a specific factory
var allItems = PreparationContext.GetFactory().Was<ItemFactory>();

// Get the count of items created by a factory
var factoryCount = PreparationContext.GetFactory().Was<OrderItem, OrderItemFactory>().Count();
```

### Data Dependency Registration

Register dependencies between test data objects:

```csharp
// Register a created entity for later retrieval
factory.Register<Order, OrderRegisterAsync>(createdOrder, out var id);

// Access the entity by type later in the test
var registeredOrder = factory.GetById<Order>(id);
```

## Project Structure

The repository consists of two main components:

- **DataPreparation**: The core library containing the implementation of the NUnit extension
- **Examples/OrderService**: Sample project demonstrating how to use the library in real-world scenarios with different testing styles:
  - Classic unit tests with Moq
  - BDD tests with TestStack.BDDfy
  - Screenplay pattern tests with Boa Constrictor
  - SQLite integration tests
  - Various data preparation patterns

## Benefits

- **Cleaner Test Code**: Remove cluttered data setup logic from your tests
- **Reusability**: Define data preparation patterns once and reuse them across multiple tests
- **Maintainability**: Changes to your data model only require updates in one place
- **Flexibility**: Support for multiple testing styles (unit, integration, BDD)
- **Performance**: Efficient data creation and management for fast test execution
- **Integration**: Works with popular testing tools and patterns

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
