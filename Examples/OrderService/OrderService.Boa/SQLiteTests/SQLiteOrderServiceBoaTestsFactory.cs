using System.ComponentModel.DataAnnotations;
using Boa.Constrictor.Screenplay;
using DataPreparation.Factory.Testing;
using DataPreparation.Provider;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Boa.Boa.Abilities;
using OrderService.Boa.CustomerService.Abilities;
using OrderService.Boa.CustomerService.Questions;
using OrderService.Boa.CustomerService.Tasks;
using OrderService.Boa.Factories.SQLite;
using OrderService.Boa.OrderService.Abilities;
using OrderService.Boa.OrderService.Questions;
using OrderService.Boa.OrderService.Tasks;
using OrderService.Boa.OrderStatusService.Abilities;
using OrderService.Boa.TestFakeModels;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;
using Shouldly;
using Steeltoe.Discovery;

namespace OrderService.Boa;

[DataPreparationFixture]
public class SqLiteOrderServiceBoaTestsFactory : IDataPreparationTestServices, IDataPreparationLogger, IBeforeTest
{
    
    public  void DataPreparationServices(IServiceCollection serviceCollection)
    {
        
        SqliteConnection databaseConnection = new("DataSource=:memory:");
        databaseConnection.Open();
        serviceCollection.AddDbContext<OrderServiceContext>(options =>
            options.UseSqlite(databaseConnection, 
                sqliteOptions => sqliteOptions
                    .MigrationsHistoryTable("__EFMigrationsHistory")
                    .MigrationsAssembly("OrderService")));
        
        serviceCollection.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        serviceCollection.AddScoped<IOrderService, Services.OrderService>();
        serviceCollection.AddScoped<IOrderManagementService, OrderManagementService>();
        serviceCollection.AddScoped<IOrderStatusService, Services.OrderStatusService>();
        serviceCollection.AddScoped<IOrderItemService, OrderItemService>();
        serviceCollection.AddScoped<ICustomerService, Services.CustomerService>();
        serviceCollection.AddScoped<IDiscoveryClient, FakeDiscoveryClient>();
        serviceCollection.AddScoped<IHttpClientFactory,FakeHttpClientFactory>();
        
        var context = serviceCollection.BuildServiceProvider().GetRequiredService<OrderServiceContext>();
        context.Database.EnsureCreated();
    }
    
    public  ILoggerFactory InitializeDataPreparationTestLogger()
    {
        return LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddDebug();
            builder.AddConsole();
        });
        
    }

    public void BeforeTest(IServiceProvider testProvider)
    {
       // code to run before each test
    }
    
    
    
    [DataPreparationTest]
    public async Task CreateOrder_FullOrderDTO_ReturnsOrder_Factory()
    {
        var factory = PreparationContext.GetFactory();
        #region Other Variant
        
        var orderItems = await  factory.GetAsync<OrderItem,OrderItemFactoryAsync>(2); 
        var customer =await factory.GetAsync<Customer,CustomerFactoryAsync>();
        OrderDTO orderDto = new OrderDTO { CustomerId = customer.Id, OrderItems = orderItems };
      
        #endregion
        orderDto = await factory.GetAsync<OrderDTO,OrderDtoFactoryAsync>();
        //FactoryObjects the order service
        Actor actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));
        // Act
        var createTask = CreateOrderTask.For(orderDto);
        actor.AttemptsTo(createTask);
        factory.Register<Order, OrderRegisterAsync>(createTask.CreatedOrder, out _); //this is for data dependency
        
        createTask.CreatedOrder.ShouldNotBeNull();;
        Order result = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));
        // Assert

        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(orderDto.CustomerId);
        result.OrderItems.ShouldNotBeNull();
        result.OrderItems.Count().ShouldBe(orderDto.OrderItems.Count());
    }
    
    
    [DataPreparationTest]
    public async Task CreateCustomer_CreateOrder_Factory()
    {
        // *********** Create Customer ***********
        // Arrange customer
        var factory = PreparationContext.GetFactory();
        var customerDto = factory.Get<CustomerDTO,CustomerDtoFactory>();

        var actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));
        actor.Can(UseCustomerService.With(PreparationContext.GetProvider().GetRequiredService<ICustomerService>()));
        
        // Act
        var createCustomerTask = CreateCustomerTask.For(customerDto);
        actor.AttemptsTo(createCustomerTask);
        PreparationContext.GetFactory().Register<Customer, CustomerFactoryAsync>(createCustomerTask.CreatedCustomer); //this is for data dependency using CustomerFactoryAsync

        
        // Assert
        var createdCustomer = actor.AsksFor(new CustomerById(createCustomerTask.CreatedCustomer.Id));
        createdCustomer.ShouldNotBeNull();
        createdCustomer.ShouldBeOfType<Customer>();
        createdCustomer.Name.ShouldBe(customerDto.Name);
        createdCustomer.Email.ShouldBe(customerDto.Email);
        
        // *********** Create Order with created customer ***********
        //Aranage Order
        var orderDto = await PreparationContext.GetFactory().NewAsync<OrderDTO, OrderDtoFactoryAsync>();//(ListParams.Use(createdCustomer)); //add customer to order
        var createTask = CreateOrderTask.For(orderDto);
        actor.AttemptsTo(createTask);
        factory.Register<Order, OrderRegisterAsync>(createTask.CreatedOrder, out _); //this is for data dependency using new register
        
        createTask.CreatedOrder.ShouldNotBeNull();
        Order result = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));
        // Assert
        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(orderDto.CustomerId);
        result.OrderItems.ShouldNotBeNull();
        result.OrderItems.Count().ShouldBe(orderDto.OrderItems.Count());
        
    }
    
    [DataPreparationTest]
    public async Task CancelOrder_ValidOrder_ChangesOrderStatus()
    {
        var factory = PreparationContext.GetFactory();
        var orderDto = await factory.GetAsync<OrderDTO, OrderDtoFactoryAsync>();

        Actor actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));
        actor.Can(UseOrderStatusService.With(PreparationContext.GetProvider().GetRequiredService<IOrderStatusService>()));
    
        // Vytvoření objednávky
        var createTask = CreateOrderTask.For(orderDto);
        actor.AttemptsTo(createTask);
        factory.Register<Order, OrderRegisterAsync>(createTask.CreatedOrder, out _);
    
        // Zrušení objednávky
        var cancelTask = CancelOrderTask.For(createTask.CreatedOrder.Id);
        actor.AttemptsTo(cancelTask);
    
        // Získání aktualizované objednávky
        var canceledOrder = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));
    
        // Assert
        canceledOrder.ShouldNotBeNull();
        canceledOrder.OrderStatuses.ShouldContain(item => item.Status == Status.CANCELED);
        canceledOrder.OrderStatuses.ShouldContain(item => item.Status == Status.CANCELED);
    }
    
    [DataPreparationTest]
    public async Task GetOrdersByCustomer_MultipleOrders_ReturnsOrdersForCustomer()
    {
        var factory = PreparationContext.GetFactory();
        var customer = await factory.GetAsync<Customer, CustomerFactoryAsync>();
    
        Actor actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));
    
        // Vytvoření více objednávek pro stejného zákazníka
        var orderCount = 3;
        for (int i = 0; i < orderCount; i++)
        {
            var orderDto = await factory.NewAsync<OrderDTO, OrderDtoFactoryAsync>(ListParams.Use(customer));
            var createTask = CreateOrderTask.For(orderDto);
            actor.AttemptsTo(createTask);
            factory.Register<Order, OrderRegisterAsync>(createTask.CreatedOrder, out _);
        }
    
        // Získání objednávek podle zákazníka
        var customerOrders = actor.AsksFor(OrdersByCustomer.WithId(customer.Id));
    
        // Assert
        customerOrders.ShouldNotBeNull();
        customerOrders.Count().ShouldBe(orderCount);
        customerOrders.ShouldAllBe(o => o.CustomerId == customer.Id);
    }
    
    [DataPreparationTest]
    public async Task CreateOrder_EmptyItems_ThrowsValidationException()
    {
        var factory = PreparationContext.GetFactory();
        var customer = await factory.GetAsync<Customer, CustomerFactoryAsync>();
    
        // Vytvoření prázdného seznamu položek
        var orderDto = new OrderDTO { CustomerId = customer.Id, OrderItems = new List<OrderItem>() };
    
        Actor actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));
    
        // Pokus o vytvoření objednávky bez položek by měl selhat
        Should.Throw<ValidationException>(() => actor.AttemptsTo(CreateOrderTask.For(orderDto)));
    }
    
    
    [DataPreparationTest]
    public async Task UpdateOrderStatus_ValidStatus_ChangesStatus()
    {
        var factory = PreparationContext.GetFactory();
        var orderDto = await factory.GetAsync<OrderDTO, OrderDtoFactoryAsync>();

        Actor actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));
        actor.Can(UseOrderStatusService.With(PreparationContext.GetProvider().GetRequiredService<IOrderStatusService>()));
    
        // Vytvoření objednávky
        var createTask = CreateOrderTask.For(orderDto);
        actor.AttemptsTo(createTask);
        factory.Register<Order, OrderRegisterAsync>(createTask.CreatedOrder, out _);
    
        // Změna stavu objednávky na "Zpracovává se"
        var updateStatusTask = UpdateOrderStatusTask.For(createTask.CreatedOrder.Id, Status.PROCESSING);
        actor.AttemptsTo(updateStatusTask);
    
        // Získání aktualizované objednávky
        var updatedOrder = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));
    
        // Assert
        updatedOrder.ShouldNotBeNull();
        updatedOrder.OrderStatuses.LastOrDefault().Status.ShouldBe( Status.PROCESSING);
    }

    [DataPreparationTest]
    [UsePreparedDataFor(typeof(UpdateOrderStatusTask),true)]
    public async Task CompleteOrderWorkflow_FromCreateToShipped()
    {
        var factory = PreparationContext.GetFactory();
        var orderDto = await factory.GetAsync<OrderDTO, OrderDtoFactoryAsync>();

        Actor actor = new Actor("OrderProcessor", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));
        actor.Can(UseOrderStatusService.With(PreparationContext.GetProvider().GetRequiredService<IOrderStatusService>()));
        actor.Can(UseOrderManagementService.With(PreparationContext.GetProvider().GetRequiredService<IOrderManagementService>()));
    
        // Vytvoření objednávky
        var createTask = CreateOrderTask.For(orderDto);
        actor.AttemptsTo(createTask);
        factory.Register<Order, OrderRegisterAsync>(createTask.CreatedOrder, out _);
    
        // Ověření výchozího stavu
        var initialOrder = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));
        initialOrder.OrderStatuses.LastOrDefault()!.Status.ShouldBe(Status.CREATED);
    
        actor.AttemptsTo(UpdateOrderStatusTask.For(createTask.CreatedOrder.Id, Status.PROCESSING));
        actor.AttemptsTo(UpdateOrderStatusTask.For(createTask.CreatedOrder.Id,  Status.SENT));
        actor.AttemptsTo(UpdateOrderStatusTask.For(createTask.CreatedOrder.Id, Status.DELIVERING));
        actor.AttemptsTo(UpdateOrderStatusTask.For(createTask.CreatedOrder.Id, Status.DELIVERED));
        
        var finalOrder = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));
        finalOrder.OrderStatuses.LastOrDefault()!.Status.ShouldBe(Status.DELIVERED);
    }

    [DataPreparationTest]
    public async Task GetAllOrders_MultipleOrders_ReturnsAllOrders()
    {
        var factory = PreparationContext.GetFactory();
    
        Actor actor = new Actor("OrderAdmin", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));
    
        var initialOrders = actor.AsksFor(AllOrders.FromService());
        var initialCount = initialOrders.Count();
    
        var orderCount = 3;
        for (int i = 0; i < orderCount; i++)
        {
            var orderDto = await factory.GetAsync<OrderDTO, OrderDtoFactoryAsync>();
            var createTask = CreateOrderTask.For(orderDto);
            actor.AttemptsTo(createTask);
            factory.Register<Order, OrderRegisterAsync>(createTask.CreatedOrder, out _);
        }
    
        var allOrders = actor.AsksFor(AllOrders.FromService());
    
        // Assert
        allOrders.ShouldNotBeNull();
        allOrders.Count().ShouldBe(initialCount + orderCount);
    }
    
    [DataPreparationTest]
    public async Task GetOrdersByStatus_FilterByStatus_ReturnsFilteredOrders()
    {
        var factory = PreparationContext.GetFactory();
    
        Actor actor = new Actor("OrderAnalyst", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));
        actor.Can(UseOrderStatusService.With(PreparationContext.GetProvider().GetRequiredService<IOrderStatusService>()));
    
        // Vytvoření objednávek s různými stavy
        var orderDto1 = await factory.GetAsync<OrderDTO, OrderDtoFactoryAsync>();
        var createTask1 = CreateOrderTask.For(orderDto1);
        actor.AttemptsTo(createTask1);
        factory.Register<Order, OrderRegisterAsync>(createTask1.CreatedOrder, out _);
    
        var orderDto2 = await factory.GetAsync<OrderDTO, OrderDtoFactoryAsync>();
        var createTask2 = CreateOrderTask.For(orderDto2);
        actor.AttemptsTo(createTask2);
        factory.Register<Order, OrderRegisterAsync>(createTask2.CreatedOrder, out _);
    
        actor.AttemptsTo(UpdateOrderStatusTask.For(createTask2.CreatedOrder.Id, Status.PROCESSING));
    
        var createdOrders = actor.AsksFor(OrdersByStatus.WithStatus( Status.CREATED));
        var processingOrders = actor.AsksFor(OrdersByStatus.WithStatus(Status.PROCESSING));
    
        // Assert
        createdOrders.ShouldContain(o => o.Id == createTask1.CreatedOrder.Id);
        processingOrders.ShouldContain(o => o.Id == createTask2.CreatedOrder.Id);
    }
}