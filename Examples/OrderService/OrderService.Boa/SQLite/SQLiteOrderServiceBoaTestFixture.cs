using Boa.Constrictor.Screenplay;
using DataPreparation.Factory.Testing;
using DataPreparation.Provider;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using FluentAssertions;
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

namespace OrderService.Boa;

[DataPreparationFixture]
[Parallelizable(ParallelScope.All)]
public class SQLiteOrderServiceBoaTestFixture : IDataPreparationTestServices, IDataPreparationLogger, IBeforeTest
{
    private SqliteOrderServiceContext _context;
    private SqliteConnection _connection;


    [OneTimeSetUp]
    public void SetUp()
    {
        
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        // Configure in-memory database for testing
        var options = new DbContextOptionsBuilder<OrderServiceContext>()
            .UseSqlite(_connection, sql => {
                sql
                .MigrationsAssembly("OrderService")
                .MigrationsHistoryTable("__EFMigrationsHistory"); })
            .Options;

        _context = new SqliteOrderServiceContext(options);
        _context.Database.EnsureCreated();

       
    }
    
    
    #region DataPreparationServices

    static SqliteConnection serviceConnection = new("DataSource=:memory:");
    public  void DataPreparationServices(IServiceCollection serviceCollection)
    {
        //_context.Database.EnsureCreated();
        serviceConnection.Open();
        serviceCollection.AddDbContext<OrderServiceContext>(options =>
            options.UseSqlite(serviceConnection, 
                sqliteOptions => sqliteOptions
                    .MigrationsHistoryTable("__EFMigrationsHistory")
                    .MigrationsAssembly("OrderService")));
        
        serviceCollection.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        serviceCollection.AddScoped<IOrderService, Services.OrderService>();
        serviceCollection.AddScoped<IOrderManagementService, OrderManagementService>();
        serviceCollection.AddScoped<IOrderStatusService, Services.OrderStatusService>();
        serviceCollection.AddScoped<IOrderItemService, OrderItemService>();
        serviceCollection.AddScoped<ICustomerService, Services.CustomerService>();
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
        try
        {
            testProvider.GetRequiredService<OrderServiceContext>().Database.EnsureCreated();
        }
        catch (Exception e)
        {
           
        }
        
    }
    
    #endregion
    
  

    
    
    [DataPreparationTest]
    public void CreateOrder_FullOrderDTO_ReturnsOrder()
    {
        IRepository<Customer> customerRepository = new Repository<Customer>(_context);
        IRepository<Order> orderRepository = new Repository<Order>(_context);
        Services.OrderService  orderService = new Services.OrderService(orderRepository, customerRepository,null);
        
        // Arrange
        var address = new Address() { City = "City", Street = "Street", PostalCode = "ZipCode" };
        var customer = new Customer {Name = "Test Customer", Address = address, Email =" ",Phone = "000"};
        _context.Customers.Add(customer);
        _context.SaveChanges();
        
        OrderItem orderItem1 = new() { ItemId = 1, Quantity = 1 };
        OrderItem orderItem2 = new() { ItemId = 2, Quantity = 2 };
        var orderItems = new List<OrderItem> { orderItem1, orderItem2 };
        
        OrderDTO orderDto = new OrderDTO { CustomerId = customer.Id, OrderItems = orderItems };
        
        //FactoryObjects the order service
        Actor actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(orderService));
        // Act
        var createTask = CreateOrderTask.For(orderDto);
        actor.AttemptsTo(createTask);
        Order result = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(orderDto.CustomerId);
        result.OrderItems.Should().NotBeNull();
        result.OrderItems.Should().HaveCount(orderItems.Count);
        
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
        
        createTask.CreatedOrder.Should().NotBeNull();
        Order result = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));
        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(orderDto.CustomerId);
        result.OrderItems.Should().NotBeNull();
        result.OrderItems.Should().HaveCount(orderDto.OrderItems.Count());
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    [DataPreparationTest]
    public async Task CreateCustomer_CreateOrder()
    {
        IRepository<Customer> customerRepository = new Repository<Customer>(_context);
        IRepository<Order> orderRepository = new Repository<Order>(_context);
        Services.OrderService  orderService = new Services.OrderService(orderRepository, customerRepository,null);
        Services.CustomerService  customerService = new Services.CustomerService(customerRepository);
        // *********** Create Customer ***********
        // Arrange customer
        Address address = new Address() { City = "City", Street = "Street", PostalCode = "ZipCode" };
        var customerDto = new CustomerDTO { Name = "John Doe", Email = "john.doe@example.com" , Address = address, Phone = "000"};
        
        var actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(orderService));
        actor.Can(UseCustomerService.With(customerService));
        // Act
        var createCustomerTask = CreateCustomerTask.For(customerDto);
        actor.AttemptsTo(createCustomerTask);
        
        // Assert
        var createdCustomer = actor.AsksFor(new CustomerById(createCustomerTask.CreatedCustomer.Id));
        createdCustomer.Should().NotBeNull()
            .And.BeOfType<Customer>()
            .And.Match<Customer>(c => c.Name == customerDto.Name)
            .And.Match<Customer>(c => c.Email == customerDto.Email);
        
        // *********** Create Order with created customer ***********
        //Aranage Order
        OrderItem orderItem1 = new() { ItemId = 1, Quantity = 1 };
        OrderItem orderItem2 = new() { ItemId = 2, Quantity = 2 };
        
        var orderItems = new List<OrderItem> { orderItem1, orderItem2 };
        OrderDTO orderDto = new OrderDTO { CustomerId = createdCustomer.Id, OrderItems = orderItems };
        
        var createTask = CreateOrderTask.For(orderDto);
        actor.AttemptsTo(createTask);
        Order result = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));
        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(orderDto.CustomerId);
        result.OrderItems.Should().NotBeNull();
        result.OrderItems.Should().HaveCount(orderDto.OrderItems.Count());
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
        factory.Register<Customer, CustomerFactoryAsync>(createCustomerTask.CreatedCustomer); //this is for data dependency using CustomerFactoryAsync
        
        // Assert
        var createdCustomer = actor.AsksFor(new CustomerById(createCustomerTask.CreatedCustomer.Id));
        createdCustomer.Should().NotBeNull()
            .And.BeOfType<Customer>()
            .And.Match<Customer>(c => c.Name == customerDto.Name)
            .And.Match<Customer>(c => c.Email == customerDto.Email);
        
        // *********** Create Order with created customer ***********
        //Aranage Order
        var orderDto = await PreparationContext.GetFactory().NewAsync<OrderDTO, OrderDtoFactoryAsync>();//(ObjectsParams.Use(createdCustomer)); //add customer to order
        var createTask = CreateOrderTask.For(orderDto);
        actor.AttemptsTo(createTask);
        factory.Register<Order, OrderRegisterAsync>(createTask.CreatedOrder, out _); //this is for data dependency using new register
        
        createTask.CreatedOrder.Should().NotBeNull();
        Order result = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));
        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(orderDto.CustomerId);
        result.OrderItems.Should().NotBeNull();
        result.OrderItems.Should().HaveCount(orderDto.OrderItems.Count());
        
    }
    
    
    
    
    [OneTimeTearDown]
    public void Cleanup()
    {
        _context?.Dispose(); // Dispose the context once all tests are done
        _connection?.Dispose(); // Dispose the connection once all tests are done
    }


    
}