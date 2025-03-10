using Boa.Constrictor.Screenplay;
using DataPreparation.Factory.Testing;
using DataPreparation.Provider;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrderService.Boa.OrderService.Abilities;
using OrderService.Boa.OrderService.Questions;
using OrderService.Boa.OrderService.Tasks;
using OrderService.Boa.ShowCases.Factories;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using ILogger = NUnit.Framework.Internal.ILogger;
using Microsoft.Extensions.Logging;

namespace OrderService.Boa;

[DataPreparationFixture]
public class OrderServiceBoaTestFixture:IDataPreparationLogger
{
    public  ILoggerFactory InitializeDataPreparationTestLogger()
    {
        return LoggerFactory.Create(builder =>
        {
            builder.AddDebug();
            builder.AddConsole();
        });
        
    }
    
    [Test]
    public void CreateOrder_FullOrderDTO_ReturnsOrder()
    {
        // Arrange
        var orderItems = new List<OrderItem>
        {
            new() { Id = 1, ItemId = 1, Quantity = 1 },
            new() { Id = 2, ItemId = 2, Quantity = 2 }
        };        //This data have to be prepared in a way that the order can be created
  
        var customer = new Customer { Id = 1 };      //This data have to be prepared in a way that the order can be created
       
        OrderDTO orderDto = new OrderDTO { CustomerId = customer.Id, OrderItems = orderItems }; //This are data that will be used to create the order
        
        var mockCustomerRepository = new Mock<IRepository<Customer>>();//FactoryObjects the customer repository, Mock the GetById method to return the customer with the same id as the input
        mockCustomerRepository
            .Setup(repo => repo.GetById(orderDto.CustomerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
            .Returns(customer);
        
        var mockOrderRepository = new Mock<IRepository<Order>>();  //FactoryObjects the order repository
      
        mockOrderRepository.Setup(repo => repo.Insert(It.IsAny<Order>())).Returns<Order>( (order) => order);  //Mock the Insert method to return the order with the same id as the input
        mockOrderRepository.Setup(repo => repo.GetById(It.IsAny<long>(), It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))       
            .Returns((long id, Func<IQueryable<Order>, IQueryable<Order>> _) =>
                new Order { Id = id, CustomerId = orderDto.CustomerId, OrderItems = orderItems }); //Mock the GetById method to return the order with the same id as the input
    
        Services.OrderService orderService = new Services.OrderService(
            mockOrderRepository.Object, mockCustomerRepository.Object, null);    //FactoryObjects the order service
        
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
        mockOrderRepository.Verify(repo => repo.Insert(It.IsAny<Order>()), Times.Once);
    }
    
    public void CreateOrder_FullOrderDTO_ReturnsOrder_PrepareConcept() // Show Case for Test Data Preparation
    {
       //only for IDE to recognize the class
      //this will be static method that will return prepared data, maybe we can define it for each DTO
        var testData = PreparationContext.GetFactory();
        // Arrange
        //for each NEXT generate new CustomerId, for GET(2) return 2 OrderItem (add to DB and after text remove), this use  generic interface, New, Next, Get, Was
        OrderDTO orderDto = new OrderDTO { CustomerId = testData.New<Customer,CustomerFactory>().Id , OrderItems = testData.New<OrderItem,OrderItemFactory>(2) };
        //or we can use PreparationContext.OrderDTO.Get() - PreparationContext.OrderItems.Get(2) and prepItems.CustomerId.Next() - but it must be defined in PreparationContext
        orderDto = testData.New<OrderDTO,OrderDtoFactory>();
    
        //how to ppair CustomerIde with CustomerRepository, OrderItems with OrderRepository?
        Services.OrderService orderService = new Services.OrderService(
            testData.New<IRepository<Order>,OrderMockRepositoryFactory>(new ObjectsParams(orderDto)), testData.New<IRepository<Customer>,CustomerMockRepositoryFactory>(), null);
        //or if we use injection setup orderReposiroty and customerRepository according the UseOrderService (Can)
        orderService = PreparationContext.GetProvider().GetService<Services.OrderService>(); // how to setup to return Order with good ID
        //orderDTO is not in OrderRepository, but OrderRepository is  defined method for insert and get by id and for CustomerRepository is defined method for get by id
        
        Actor actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(orderService));

        // Act
        //set up Customer to Repository
        var createTask = CreateOrderTask.For(orderDto); //task that will be executed CreateOrder  -  get customer by ID and insert orderm 
        actor.AttemptsTo(createTask);
        Order result = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id)); // there can be find mock for GetByID, or add real test data to DB
        //maybe we can define some info about function that need mock or data in Screenplay pattern, somethink like interface, PrepareDataFor. //how to define if we use mock or not? SingleTone Scope for mock

        
        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(orderDto.CustomerId);
        result.OrderItems.Should().NotBeNull();
        // we can also use the history of the data
        result.OrderItems.Should().HaveCount( testData.Was<OrderItem,OrderItemFactory>().ToList().Count);
    }
    public void CreateOrder_FullOrderDTO_ReturnsOrder_PrepareConceptShortExample() // Show Case for Test Data Preparation
    {
        // Arrange
        var customer = PreparationContext.GetFactory().New<Customer,CustomerFactory>();
        OrderDTO orderDto = PreparationContext.GetFactory().New<OrderDTO,OrderDtoFactory>();
        
        Services.OrderService _orderService = new Services.OrderService(
                    PreparationContext.GetFactory().New<IRepository<Order>,OrderMockRepositoryFactory>(ObjectsParams.Use(orderDto)), 
                 PreparationContext.GetFactory().New<IRepository<Customer>,CustomerMockRepositoryFactory>(ObjectsParams.Use(customer)), 
                    null);
        IActor _actor = new Actor("OrderTester", new ConsoleLogger());
        _actor.Can(UseOrderService.With(_orderService));

        // Act
        var createTask = CreateOrderTask.For(orderDto); //task that will be executed CreateOrder  -  get customer by ID and insert order
        _actor.AttemptsTo(createTask);
        Order result = _actor.AsksFor( OrderById.WithId(createTask.CreatedOrder.Id)); // there can be find mock for GetByID, or add real test data to DB

        
        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(orderDto.CustomerId);
        result.OrderItems.Should().NotBeNull();
        result.OrderItems.Should().HaveCount( (int)PreparationContext.GetFactory().Was<OrderItem,OrderItemFactory>().ToList().Count);
    }
    
    [DataPreparationTest]
    public void CreateOrder_FullOrderDTO_ReturnsOrderFactory() // Show Case for Test Data Preparation
    {
        // Arrange
        var factory = PreparationContext.GetFactory(); // get factory for data preparation
        var customer = factory.New<Customer,CustomerFactory>();
        OrderDTO orderDto = factory.Get<OrderDTO,OrderDtoFactory>();
        Services.OrderService orderService = new Services.OrderService(
            factory.New<IRepository<Order>, OrderMockRepositoryFactory>(ObjectsParams.Use(orderDto)), 
            factory.New<IRepository<Customer>, CustomerMockRepositoryFactory>(ObjectsParams.Use(customer)), 
            null);
        
        IActor actor = new Actor("OrderTester", new ConsoleLogger());
        if (actor == null) throw new ArgumentNullException(nameof(actor));
        actor.Can(UseOrderService.With(orderService));

        // Act
        var createTask = CreateOrderTask.For(orderDto); //task that will be executed CreateOrder  -  get customer by ID and insert order
        actor.AttemptsTo(createTask);
        Order result = actor.AsksFor( OrderById.WithId(createTask.CreatedOrder.Id)); // there can be find mock for GetByID, or add real test data to DB
        
        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(orderDto.CustomerId);
        result.OrderItems.Should().NotBeNull();
        result.OrderItems.Should().HaveCount( (int)PreparationContext.GetFactory().Was<OrderItem,OrderItemFactory>().ToList().Count);
    }
    
    
    
    
    
    
    
    
    
    
    
    
    public void CreateOrder_FullOrderDTO_ReturnsOrder_PrepareNaming() // Show Case for Test Data Preparation
    {
        // Arrange
        OrderDTO orderDto = PreparationContext.GetFactory().New<OrderDTO,OrderDtoFactory>();
        Services.OrderService _orderService = null;
       //_orderService = PreparationContext.GetFactory().NewDataPreparationService<Services.OrderService>(orderDto);//how to presetup service to return Order with good ID if using mock? //TODO this line
        //maybe fisrt process and add all get, new data to DB and after that create service
        IActor actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(_orderService));

        // Act
        var createTask = CreateOrderTask.For(orderDto); //what about to change For naming which will describe order object
        actor.AttemptsTo(createTask);
        Order result = actor.AsksFor( OrderById.WithId(createTask.CreatedOrder.Id));
        
        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(orderDto.CustomerId);
        result.OrderItems.Should().NotBeNull();
        result.OrderItems.Should().HaveCount( (int)PreparationContext.GetFactory().Was<OrderItem,OrderItemFactory>().ToList().Count);
    }

    

    [Test]
    public void GetOrders_ReturnsOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order { Id = 1, CustomerId = 1 },
            new Order { Id = 2, CustomerId = 2 }
        };

        var mockOrderRepository = new Mock<IRepository<Order>>();
        mockOrderRepository
            .Setup(repo => repo.GetAll(It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns(orders);

        Services.OrderService _orderService = new Services.OrderService(mockOrderRepository.Object, null, null);
        IActor _actor = new Actor("OrderTester");
        _actor.Can(UseOrderService.With(_orderService));

        // Act
        var result = _actor.AsksFor(AllOrders.FromService());

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().Be(orders.Count);
    }

    [Test]
    public void GetOrder_ValidId_ReturnsOrder()
    {
        // Arrange
        var orderId = 1;
        var order = new Order { Id = orderId, CustomerId = 1 };

        var mockOrderRepository = new Mock<IRepository<Order>>();
        mockOrderRepository
            .Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns(order);

        Services.OrderService _orderService = new Services.OrderService(mockOrderRepository.Object, null, null);
        IActor _actor = new Actor("OrderTester");
        _actor.Can(UseOrderService.With(_orderService));

        // Act
        var result = _actor.AsksFor(OrderById.WithId(orderId));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
    }

    [Test]
    public void GetOrder_InvalidId_ThrowsException()
    {
        // Arrange
        var invalidId = 999;
        var mockOrderRepository = new Mock<IRepository<Order>>();
        mockOrderRepository
            .Setup(repo => repo.GetById(invalidId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns<Order>(null);

        Services.OrderService _orderService = new Services.OrderService(mockOrderRepository.Object, null, null);
        IActor _actor = new Actor("OrderTester");
        _actor.Can(UseOrderService.With(_orderService));

        // Act & Assert
        Action act = () => _actor.AsksFor(OrderById.WithId(invalidId));
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CreateOrder_ValidOrderDTO_ReturnsOrder()
    {
        // Arrange
        var orderDto = new OrderDTO { CustomerId = 1 };
        var customer = new Customer { Id = 1 };

        var mockCustomerRepository = new Mock<IRepository<Customer>>();
        mockCustomerRepository
            .Setup(repo => repo.GetById(orderDto.CustomerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
            .Returns(customer);

        var mockOrderRepository = new Mock<IRepository<Order>>();
        mockOrderRepository.Setup(repo => repo.Insert(It.IsAny<Order>())).Returns<Order>( (order) => order);
        mockOrderRepository
            .Setup(repo => repo.GetById(It.IsAny<long>(), It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns((long id, Func<IQueryable<Order>, IQueryable<Order>> query) =>
                new Order { Id = id, CustomerId = orderDto.CustomerId });
        
        Services.OrderService _orderService = new Services.OrderService(
            mockOrderRepository.Object, mockCustomerRepository.Object, null);
        IActor _actor = new Actor("OrderTester");
        _actor.Can(UseOrderService.With(_orderService));

        // Act
        var createTask = CreateOrderTask.For(orderDto);
        _actor.AttemptsTo(createTask);
        var result = _actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(orderDto.CustomerId);
        mockOrderRepository.Verify(repo => repo.Insert(It.IsAny<Order>()), Times.Once);
    }

    [Test]
    public void CreateOrder_InvalidCustomerId_ThrowsException()
    {
        // Arrange
        var orderDto = new OrderDTO { CustomerId = 999 };

        var mockCustomerRepository = new Mock<IRepository<Customer>>();
        mockCustomerRepository
            .Setup(repo => repo.GetById(orderDto.CustomerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
            .Returns<Customer>(null);

        var mockOrderRepository = new Mock<IRepository<Order>>();

        Services.OrderService _orderService = new Services.OrderService(
            mockOrderRepository.Object, mockCustomerRepository.Object, null);
        IActor _actor = new Actor("OrderTester");
        _actor.Can(UseOrderService.With(_orderService));

        // Act & Assert
        Action act = () => _orderService.CreateOrder(orderDto);
        act.Should().Throw<InvalidOperationException>();
        mockOrderRepository.Verify(repo => repo.Insert(It.IsAny<Order>()), Times.Never);
    }


   

    [Test]
    public void UpdateOrder_ValidIdAndOrder_ReturnsTrue()
    {
        // Arrange
        var orderId = 1;
        var existingOrder = new Order { Id = orderId };
        var updatedOrder = new Order { Id = orderId, CustomerId = 2 };

        var mockOrderRepository = new Mock<IRepository<Order>>();
        mockOrderRepository
            .Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns(existingOrder);

        Services.OrderService _orderService = new Services.OrderService(
            mockOrderRepository.Object, null, null);
        IActor _actor = new Actor("OrderTester");
        _actor.Can(UseOrderService.With(_orderService));

        // Act
        var updateTask = UpdateOrderTask.For(orderId, updatedOrder);
        _actor.AttemptsTo(updateTask);
        var result = updateTask.UpdateResult;

        // Assert
        result.Should().BeTrue();
        mockOrderRepository.Verify(repo => repo.Update(It.IsAny<Order>()), Times.Once);
    }

    [Test]
    public void UpdateOrder_InvalidId_ThrowsException()
    {
        // Arrange
        var invalidId = 999;
        var updatedOrder = new Order { Id = invalidId, CustomerId = 2 };

        var mockOrderRepository = new Mock<IRepository<Order>>();
        mockOrderRepository
            .Setup(repo => repo.GetById(invalidId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns<Order>(null);

        Services.OrderService _orderService = new Services.OrderService(
            mockOrderRepository.Object,null, null);
        IActor _actor = new Actor("OrderTester");
        _actor.Can(UseOrderService.With(_orderService));

        // Act & Assert
        Action act = () => _actor.AttemptsTo(UpdateOrderTask.For(invalidId, updatedOrder));
        act.Should().Throw<ArgumentException>();
        mockOrderRepository.Verify(repo => repo.Update(It.IsAny<Order>()), Times.Never);
    }

   
}