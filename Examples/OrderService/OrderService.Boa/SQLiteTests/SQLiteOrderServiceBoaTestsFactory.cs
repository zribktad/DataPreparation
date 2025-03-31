using Boa.Constrictor.Screenplay;
using DataPreparation.Factory.Testing;
using DataPreparation.Provider;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Boa.CustomerService.Abilities;
using OrderService.Boa.CustomerService.Questions;
using OrderService.Boa.CustomerService.Tasks;
using OrderService.Boa.Factories.SQLite;
using OrderService.Boa.OrderService.Abilities;
using OrderService.Boa.OrderService.Questions;
using OrderService.Boa.OrderService.Tasks;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;
using Shouldly;

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
                    .MigrationsAssembly("OrderService")),ServiceLifetime.Transient);
        
        serviceCollection.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        serviceCollection.AddScoped<IOrderService, Services.OrderService>();
        serviceCollection.AddScoped<IOrderManagementService, OrderManagementService>();
        serviceCollection.AddScoped<IOrderStatusService, Services.OrderStatusService>();
        serviceCollection.AddScoped<IOrderItemService, OrderItemService>();
        serviceCollection.AddScoped<ICustomerService, Services.CustomerService>();
        
        var context = serviceCollection.BuildServiceProvider().GetRequiredService<OrderServiceContext>();
        context.Database.EnsureCreated();
        context.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        
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
    
    


    
}