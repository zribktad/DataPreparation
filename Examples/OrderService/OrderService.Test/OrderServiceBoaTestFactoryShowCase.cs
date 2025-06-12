// -----------------------------------------------------------------------------
// OrderServiceBoaTestFactoryShowCase.cs
//
// This file contains integration and unit tests for the OrderService using the
// Boa.Constrictor Screenplay pattern, NUnit, Moq, and custom data preparation utilities.
//
// Main Class:
//   OrderServiceBoaTestFactoryShowCase
//
// Purpose:
//   - Verifies the creation of orders in the OrderService.
//   - Demonstrates different test data setup strategies: manual mocks, DI, and factories.
//   - Uses the Screenplay pattern for test actions and assertions.
//
// Key Features:
//   - [DataPreparationFixture] and [Parallelizable] for advanced test setup and parallelism.
//   - Dependency injection for test data and service mocks.
//   - Custom attributes for data preparation and parameterized tests.
//   - Fluent assertions with Shouldly.
//
// Test Methods:
//   - CreateOrder_FullOrderDTO_ReturnsOrder: Manual mock setup.
//   - CreateOrder_FullOrderDTO_ReturnsOrder_Before: Uses DI and prepared data.
//   - CreateOrder_FullOrderDTO_ReturnsOrder_Factory: Uses factories for test data/services.
//
// Technologies:
//   - Boa.Constrictor.Screenplay, NUnit, Moq, Shouldly, custom DataPreparation utilities.
// -----------------------------------------------------------------------------

using Boa.Constrictor.Screenplay;
using DataPreparation.Factory.Testing;
using DataPreparation.Provider;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OrderService.BoaTest.OrderService.Abilities;
using OrderService.BoaTest.OrderService.Questions;
using OrderService.BoaTest.OrderService.Tasks;
using OrderService.BoaTest.ShowCases.Factories;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Test.Domain.BddSteps;
using OrderService.Test.Domain.Boa.Abilities;
using OrderService.Test.Domain.Boa.Questions;
using OrderService.Test.Domain.Factories.AsyncMock;
using OrderService.Test.Domain.TestFakeModels;
using Shouldly;
using TestStack.BDDfy;

namespace OrderService.Test;

[DataPreparationFixture]
[Parallelizable(ParallelScope.All)]
public class OrderServiceBoaTestFactoryShowCase : IDataPreparationLogger, IDataPreparationTestServices, IBeforeTest, IAfterTest
{
    #region Fixture Setup
    public void DataPreparationServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<Mock<IRepository<Customer>>>();
        serviceCollection.AddScoped<Mock<IRepository<Order>>>();
        serviceCollection.AddScoped<CreationOrderDTO>();
    }
    public ILoggerFactory InitializeDataPreparationTestLogger()
    {
        return LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddDebug();
            builder.AddConsole();
        });
    }
    public void BeforeTest(IServiceProvider testProvider)
    {
        
    }
    public void AfterTest(IServiceProvider testProvider)
    {
    }
    
    #endregion
    
    // -----------------------------------------------------------------------------
    // Test Descriptions
    // -----------------------------------------------------------------------------
    // CreateOrder_FullOrderDTO_ReturnsOrder:
    //   Tests order creation using manually constructed mocks for repositories and DTOs.
    //   Verifies that the created order matches the input data.
    //
    // CreateOrder_FullOrderDTO_ReturnsOrder_Before:
    //   Uses dependency injection to provide prepared data and mocks.
    //   Tests order creation and checks that the result matches the expected customer and order items.
    //
    // CreateOrder_FullOrderDTO_ReturnsOrder_Factory:
    //   Uses a factory to generate test data and services.
    //   Verifies that the order is created correctly and the returned data matches the input.
    //
    // CreateOrder_FullOrderDTO_ReturnsOrder_FactoryBetter:
    //   Demonstrates a more advanced factory-based approach using actor abilities.
    //   Retrieves DTOs and services from the actor and verifies order creation.
    //
    // Order_BDD:
    //   Implements a BDD-style test using BDDfy. Steps are defined in a separate steps class,
    //   and the test verifies the full order creation workflow.
    //
    // CreateOrder_FullOrderDTO_ReturnsOrderFactoryAsync:
    //   Asynchronous test using async factories to generate test data.
    //   Verifies that the order is created as expected and the number of order items matches the prepared data.
    // -----------------------------------------------------------------------------

    [Test]
    public void CreateOrder_FullOrderDTO_ReturnsOrder()
    {
        // Arrange
        var address = new Address { City = "City", Street = "Street", PostalCode = "ZipCode" };
        var customer = new Customer { Id = 1, Name = "Name Surname", Address = address };

        var orderItems = new List<OrderItem>
        {
            new() { Id = 1, ItemId = 1, Quantity = 1 },
            new() { Id = 2, ItemId = 2, Quantity = 2 }
        };

        var orderDto = new OrderDTO
            { CustomerId = customer.Id, OrderItems = orderItems };
        var mockCustomerRepository =
            new Mock<IRepository<Customer>>();
        mockCustomerRepository
            .Setup(repo =>
                repo.GetById(orderDto.CustomerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
            .Returns(customer);

        var mockOrderRepository = new Mock<IRepository<Order>>();
        mockOrderRepository.Setup(repo => repo.Insert(It.IsAny<Order>()))
            .Returns<Order>((order) => order);
        mockOrderRepository.Setup(repo =>
                repo.GetById(It.IsAny<long>(), It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns((long id, Func<IQueryable<Order>, IQueryable<Order>> _) =>
                new Order
                {
                    Id = id, CustomerId = orderDto.CustomerId, OrderItems = orderItems
                });
        var orderService = new OrderService.Services.OrderService(
            mockOrderRepository.Object, mockCustomerRepository.Object);

        var actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(orderService));

        // Act
        var createTask = CreateOrderTask.For(orderDto);
        actor.AttemptsTo(createTask);
        var result = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));

        // Assert
        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(orderDto.CustomerId);
        result.OrderItems.ShouldNotBeNull();
        result.OrderItems.Count().ShouldBe(orderItems.Count);
    }

    [DataPreparationTest] 
    [UsePreparedDataParamsFor(typeof(CreateOrderTask),[5,"Customer Name",2])]
    public void CreateOrder_FullOrderDTO_ReturnsOrder_Before()
    {
        // Arrange
        var provider = PreparationContext.GetProvider();
        var orderDto = provider.GetService<CreationOrderDTO>();
        
        var mockCustomerRepository = provider.GetService<Mock<IRepository<Customer>>>();
        var mockOrderRepository = provider.GetService<Mock<IRepository<Order>>>();
        var orderService = new OrderService.Services.OrderService(
            mockOrderRepository.Object, mockCustomerRepository.Object); 

        var actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(orderService));

        // Act
        var createTask = CreateOrderTask.For(orderDto);
        actor.AttemptsTo(createTask);
        var result = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));

        // Assert
        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(orderDto.CustomerId);
        result.OrderItems.ShouldNotBeNull();
        result.OrderItems.Count().ShouldBe(2);
    }

    [DataPreparationTest]
    public void CreateOrder_FullOrderDTO_ReturnsOrder_Factory() 
    {
        // Arrange
        var sourceFactory = PreparationContext.GetFactory();
        var customer = sourceFactory.New<Customer, CustomerFactory>();
        var orderDto = sourceFactory.Get<OrderDTO, OrderDtoFactory>();
        var orderService =
            sourceFactory.New<OrderService.Services.OrderService, OrderServiceFactory>(ListParams.Use(orderDto, customer));

        IActor actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(orderService));

        // Act
        var createTask = CreateOrderTask.For(orderDto); 
        actor.AttemptsTo(createTask);
        var result = actor.AsksFor(OrderById.WithId(createTask.CreatedOrder.Id));

        // Assert -  Shouldly
        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(orderDto.CustomerId);
        result.OrderItems.ShouldNotBeNull();
        result.OrderItems.Count().ShouldBe(orderDto.OrderItems.Count);
    }
    
    [DataPreparationTest]
    public void CreateOrder_FullOrderDTO_ReturnsOrder_FactoryBetter() 
    {
        // Arrange
        IActor actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.FromMockFactory());
        actor.Can( UseSourceFactory.FromDataPreparation());

        // Act
        var orderDto = actor.AsksFor(GetOrderDto.One());
        var createTask = CreateOrderTask.For(orderDto); 
        actor.AttemptsTo(createTask);
        var result = actor.AsksFor(OrderById.WithId(createTask.CreatedOrder.Id));

        // Assert -  Shouldly
        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(orderDto.CustomerId);
        result.OrderItems.ShouldNotBeNull();
        result.OrderItems.Count().ShouldBe(orderDto.OrderItems.Count);
    }
    

    [DataPreparationTest]
    public void Order_BDD()
    {
        OrderServiceStepsMock steps = new();
        this.Given(_ => steps.GivenIHaveUser())
            .And(_ => steps.GivenUserCanCreateOrder())
            .When(_ => steps.WhenUserCreatesOrderData())
            .Then(_ => steps.ThanUserCreatesOrder())
            .When(_ => steps.WhenUserLookAtOrder())
            .Then(_ => steps.ThenOrderShouldBeCreated())
            .BDDfy();
    }
   
    
    //Example of async test using async factory
    [DataPreparationTest]
    public async Task CreateOrder_FullOrderDTO_ReturnsOrderFactoryAsync()
    {
        // Arrange
        var customer = await PreparationContext.GetFactory().NewAsync<Customer, CustomerFactoryAsync>();
        var orderDto = await PreparationContext.GetFactory().GetAsync<OrderDTO, OrderDtoFactoryAsync>();
        var orderService = PreparationContext.GetFactory()
            .New<OrderService.Services.OrderService, OrderServiceFactory>(ListParams.Use(orderDto, customer));


        IActor actor = new Actor("OrderTester", new ConsoleLogger());
        if (actor == null) throw new ArgumentNullException(nameof(actor));
        actor.Can(UseOrderService.With(orderService));

        // Act
        var createTask = CreateOrderTask.For(orderDto);
        actor.AttemptsTo(createTask);
        var result = actor.AsksFor(OrderById.WithId(createTask.CreatedOrder.Id)); 

        // Assert - converted to Shouldly
        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(orderDto.CustomerId);
        result.OrderItems.ShouldNotBeNull();
        result.OrderItems.Count()
            .ShouldBe(PreparationContext.GetFactory().Was<OrderItem, OrderItemFactoryAsync>().ToList().Count);
    }


}
