using Boa.Constrictor.Screenplay;
using DataPreparation.Factory.Testing;
using DataPreparation.Provider;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.BoaTest.OrderService.Abilities;
using OrderService.BoaTest.OrderService.Questions;
using OrderService.BoaTest.OrderService.Tasks;
using OrderService.BoaTest.ShowCases.Factories;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using Shouldly;

namespace OrderService.BoaTest;

[DataPreparationFixture]
[Parallelizable(ParallelScope.All)]
public class OrderServiceBoaTestFactoryShowCase_Sync_Async : IDataPreparationLogger, IDataPreparationTestServices
{
    public void DataPreparationServices(IServiceCollection serviceCollection)
    {
      
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
    
    [DataPreparationTest]
    public void CreateOrder_FullOrderDTO_ReturnsOrderFactory() // Show Case for Test Data Preparation Sync
    {
        // Arrange
        var sourceFactory = PreparationContext.GetFactory();
        var customer = sourceFactory.New<Customer,CustomerFactory>();
        OrderDTO orderDto = sourceFactory.Get<OrderDTO,OrderDtoFactory>();
        Services.OrderService orderService = sourceFactory.New<Services.OrderService,OrderServiceFactory>(ListParams.Use(orderDto,customer));
        
        IActor actor = new Actor("OrderTester", new ConsoleLogger());
        if (actor == null) throw new ArgumentNullException(nameof(actor));
        actor.Can(UseOrderService.With(orderService));

        // Act
        var createTask = CreateOrderTask.For(orderDto); //task that will be executed CreateOrder  -  get customer by ID and insert order
        actor.AttemptsTo(createTask);
        Order result = actor.AsksFor(OrderById.WithId(createTask.CreatedOrder.Id)); // there can be find mock for GetByID, or add real test data to DB

        // Assert - converted to Shouldly
        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(orderDto.CustomerId);
        result.OrderItems.ShouldNotBeNull();
        result.OrderItems.Count().ShouldBe(sourceFactory.Was<OrderItem,OrderItemFactory>().ToList().Count);
    }
    
    [DataPreparationTest]
    public async Task CreateOrder_FullOrderDTO_ReturnsOrderFactoryAsync() // Show Case for Test Data Preparation Async
    {
        // Arrange
        var customer = await PreparationContext.GetFactory().NewAsync<Customer,CustomerFactoryAsync>();
        OrderDTO orderDto = await PreparationContext.GetFactory().GetAsync<OrderDTO,OrderDtoFactoryAsync>();
        Services.OrderService orderService = PreparationContext.GetFactory().New<Services.OrderService,OrderServiceFactory>(ListParams.Use(orderDto,customer));

        
        IActor actor = new Actor("OrderTester", new ConsoleLogger());
        if (actor == null) throw new ArgumentNullException(nameof(actor));
        actor.Can(UseOrderService.With(orderService));

        // Act
        var createTask = CreateOrderTask.For(orderDto); //task that will be executed CreateOrder  -  get customer by ID and insert order
        actor.AttemptsTo(createTask);
        Order result = actor.AsksFor(OrderById.WithId(createTask.CreatedOrder.Id)); // there can be find mock for GetByID, or add real test data to DB

        // Assert - converted to Shouldly
        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(orderDto.CustomerId);
        result.OrderItems.ShouldNotBeNull();
        result.OrderItems.Count().ShouldBe(PreparationContext.GetFactory().Was<OrderItem,OrderItemFactoryAsync>().ToList().Count);
    }
}