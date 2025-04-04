using Boa.Constrictor.Screenplay;
using Moq;
using NUnit.Framework;
using OrderService.BoaTest.OrderStatusService.Abilities;
using OrderService.BoaTest.OrderStatusService.Questions;
using OrderService.BoaTest.OrderStatusService.Tasks;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using Shouldly;

namespace OrderService.BoaTest.OrderStatusService;

[TestFixture]
public class OrderStatusServiceBoaTestFixture
{
    private IActor _actor;
    private Services.OrderStatusService _orderStatusService;

    private Mock<IRepository<Order>> _mockOrderRepository;
    private Mock<IRepository<OrderStatus>> _mockOrderStatusRepository;

    [SetUp]
    public void SetUp()
    {
        // Initialize mocks
        _mockOrderRepository = new Mock<IRepository<Order>>();
        _mockOrderStatusRepository = new Mock<IRepository<OrderStatus>>();

        
        var mockFactoryClient = new Mock<IHttpClientFactory>();
        mockFactoryClient.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
        // Create the service instance
        _orderStatusService = new Services.OrderStatusService(
            _mockOrderStatusRepository.Object, 
            _mockOrderRepository.Object, null, null,mockFactoryClient.Object);

        // Assign the actor
        _actor = new Actor("OrderStatusTester");
        _actor.Can(UseOrderStatusService.With(_orderStatusService));
    }

    [Test]
    public void AddOrderStatus_ValidOrderIdAndStatusDto_ReturnsTrue()
    {
        // Arrange
        var orderId = 1;
        var order = new Order { Id = orderId };
        var statusDto = new OrderStatusInputDTO { OrderStatus = nameof(Status.DELIVERED) };

        _mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns(order);
        _mockOrderStatusRepository.Setup(repo => repo.Insert(It.IsAny<OrderStatus>())).Returns(new OrderStatus { Status =  (Status)Enum.Parse(typeof(Status), statusDto.OrderStatus), StatusDate = DateTime.Now.ToUniversalTime() });
        // Act
        var addTask = AddOrderStatusTask.For(orderId, statusDto);
        _actor.AttemptsTo(addTask);

        // Assert
        addTask.AddResult.ShouldNotBeNull();
        _mockOrderStatusRepository.Verify(repo => repo.Insert(It.IsAny<OrderStatus>()), Times.Once);
    }

    [Test]
    public void AddOrderStatus_InvalidStatus_ThrowsException()
    {
        // Arrange
        var orderId = 1;
        var order = new Order { Id = orderId };
        var statusDto = new OrderStatusInputDTO { OrderStatus = "InvalidStatus" };

        _mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns(order);

        // Act & Assert
        Action act = () => _actor.AttemptsTo(AddOrderStatusTask.For(orderId, statusDto));
        act.ShouldThrow<InvalidOperationException>();
        _mockOrderStatusRepository.Verify(repo => repo.Insert(It.IsAny<OrderStatus>()), Times.Never);
    }

    [Test]
    public void AddOrderStatus_OrderNotFound_ThrowsException()
    {
        // Arrange
        var orderId = 999;
        var statusDto = new OrderStatusInputDTO { OrderStatus = nameof(Status.DELIVERED) };

        _mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns<Order>(null);

        // Act & Assert
        Action act = () => _actor.AttemptsTo(AddOrderStatusTask.For(orderId, statusDto));
        act.ShouldThrow<ArgumentException>();
        _mockOrderStatusRepository.Verify(repo => repo.Insert(It.IsAny<OrderStatus>()), Times.Never);
    }

    [Test]
    public void GetOrderStatuses_ValidOrderId_ReturnsOrderStatuses()
    {
        // Arrange
        var orderId = 1;
        var status = new OrderStatus { Status = Status.SENT, StatusDate = DateTime.Now };
        var order = new Order 
        { 
            Id = orderId, 
            OrderStatuses = new List<OrderStatus> { status }
        };

        _mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns(order);

        // Act
        var orderStatuses = _actor.AsksFor(OrderStatusesForOrderId.ForOrderId(orderId));

        // Assert
        orderStatuses.ShouldNotBeNull();
        orderStatuses.Count().ShouldBe(1);
        orderStatuses.First().OrderStatus.ShouldBe(nameof(Status.SENT));
        orderStatuses.First().StatusDate.ShouldBeLessThanOrEqualTo(DateTime.Now +TimeSpan.FromMinutes(5));
    }

    [Test]
    public void GetOrderStatuses_OrderNotFound_ThrowsException()
    {
        // Arrange
        var orderId = 999;

        _mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns<Order>(null);

        // Act & Assert
        Action act = () => _actor.AsksFor(OrderStatusesForOrderId.ForOrderId(orderId));
        act.ShouldThrow<ArgumentException>();
    }
    
    [Test]
    public void AddOrderStatus_ValidOrderIdAndStatusDto_Full()
    {
        // Arrange
        var orderId = 1;
        var statusDto = new OrderStatusInputDTO { OrderStatus = nameof(Status.DELIVERED) };
        var order = new Order { Id = orderId };
       

        _mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns(order);
        
        _mockOrderStatusRepository.Setup(repo => repo.Insert(It.IsAny<OrderStatus>())).Returns(new OrderStatus { Status =  (Status)Enum.Parse(typeof(Status), statusDto.OrderStatus), StatusDate = DateTime.Now.ToUniversalTime() });
        // Act
        var addStatusTask =   AddOrderStatusTask.For(orderId, statusDto);
        _actor.AttemptsTo(addStatusTask);
        Enum.TryParse<Status>(addStatusTask.AddResult.OrderStatus, out var status);
        var orderstatus = new OrderStatus() {Status = status ,StatusDate = addStatusTask.AddResult.StatusDate };
        order.OrderStatuses = new List<OrderStatus> {orderstatus };
        
        _mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns(() => order);
       
        var orderStatuses = _actor.AsksFor(OrderStatusesForOrderId.ForOrderId(orderId));

       
        // Assert
        _mockOrderStatusRepository.Verify(repo => repo.Insert(It.IsAny<OrderStatus>()), Times.Once);
        orderStatuses.ShouldNotBeNull();
        orderStatuses.Count().ShouldBe(1);
        orderStatuses.First().OrderStatus.ShouldBe(nameof(Status.DELIVERED));
        orderStatuses.First().StatusDate.ShouldBeLessThanOrEqualTo(DateTime.Now +TimeSpan.FromMinutes(5));        
        
        
    }
}