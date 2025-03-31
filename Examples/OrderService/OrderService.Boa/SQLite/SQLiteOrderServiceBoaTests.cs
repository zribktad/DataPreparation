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

public class SqLiteOrderServiceBoaTests
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
    [Test]
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
        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(orderDto.CustomerId);
        result.OrderItems.ShouldNotBeNull();
        result.OrderItems.Count().ShouldBe(orderItems.Count);
        
    }

    [Test]
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
        createdCustomer.ShouldNotBeNull();
        createdCustomer.ShouldBeOfType<Customer>();
        createdCustomer.Name.ShouldBe(customerDto.Name);
        createdCustomer.Email.ShouldBe(customerDto.Email);
        
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
        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(orderDto.CustomerId);
        result.OrderItems.ShouldNotBeNull();
        result.OrderItems.Count().ShouldBe(orderDto.OrderItems.Count());
    }
    
    
    [OneTimeTearDown]
    public void Cleanup()
    {
        _context?.Dispose(); // Dispose the context once all tests are done
        _connection?.Dispose(); // Dispose the connection once all tests are done
    }


    
}