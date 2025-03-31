using Boa.Constrictor.Screenplay;
using DataPreparation.Factory.Testing;
using DataPreparation.Provider;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Boa.OrderService.Abilities;
using OrderService.Boa.OrderService.Questions;
using OrderService.Boa.OrderService.Tasks;
using OrderService.Boa.ShowCases.Factories;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using Shouldly;

namespace OrderService.Boa;

[DataPreparationFixture]
[Parallelizable(ParallelScope.All)]
public class OrderServiceBoaTestFactoryFixture : IDataPreparationLogger, IDataPreparationTestServices
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
    public void CreateOrder_FullOrderDTO_ReturnsOrderFactory() // Show Case for Test Data Preparation
    {
        // Arrange
        var customer = PreparationContext.GetFactory().New<Customer,CustomerFactory>();
        OrderDTO orderDto = PreparationContext.GetFactory().Get<OrderDTO,OrderDtoFactory>();
        Services.OrderService orderService = new Services.OrderService(
            PreparationContext.GetFactory().New<IRepository<Order>,OrderMockRepositoryFactory>(ListParams.Use(orderDto)), 
            PreparationContext.GetFactory().New<IRepository<Customer>,CustomerMockRepositoryFactory>(ListParams.Use(customer)), 
            null);
        
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
        result.OrderItems.Count().ShouldBe((int)PreparationContext.GetFactory().Was<OrderItem,OrderItemFactory>().ToList().Count);
    }
    
    [DataPreparationTest]
    public async Task CreateOrder_FullOrderDTO_ReturnsOrderFactoryAsync() // Show Case for Test Data Preparation
    {
        // Arrange
        var customer = await PreparationContext.GetFactory().NewAsync<Customer,CustomerFactoryAsync>();
        OrderDTO orderDto = await PreparationContext.GetFactory().GetAsync<OrderDTO,OrderDtoFactoryAsync>();
        Services.OrderService orderService = new Services.OrderService(
            await PreparationContext.GetFactory().NewAsync<IRepository<Order>,OrderMockRepositoryFactoryAsync>(ListParams.Use(orderDto)), 
            await PreparationContext.GetFactory().NewAsync<IRepository<Customer>,CustomerMockRepositoryFactoryAsync>(ListParams.Use(customer)), 
            null);
        
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