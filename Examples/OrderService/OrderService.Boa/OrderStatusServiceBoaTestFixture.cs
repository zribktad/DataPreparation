using System;
using System.Collections.Generic;
using System.Linq;
using Boa.Constrictor.Screenplay;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using OrderService.Boa.OrderStatusService.Abilities;
using OrderService.Boa.OrderStatusService.Questions;
using OrderService.Boa.OrderStatusService.Tasks;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;

namespace OrderService.Boa.OrderStatusService;

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

        // Create the service instance
        _orderStatusService = new Services.OrderStatusService(
            _mockOrderStatusRepository.Object, 
            _mockOrderRepository.Object, null, null);

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
        addTask.AddResult.Should().NotBeNull();
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
        act.Should().Throw<InvalidOperationException>();
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
        act.Should().Throw<ArgumentException>();
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
        orderStatuses.Should().NotBeNull();
        orderStatuses.Should().HaveCount(1);
        orderStatuses.First().OrderStatus.Should().Be(nameof(Status.SENT));
        orderStatuses.First().StatusDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(5)); // Adjust delta as needed
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
        act.Should().Throw<ArgumentException>();
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
        orderStatuses.Should().NotBeNull();
        orderStatuses.Should().HaveCount(1);
        orderStatuses.First().OrderStatus.Should().Be(nameof(Status.DELIVERED));
        orderStatuses.First().StatusDate.Should().BeCloseTo(DateTime.Now.ToUniversalTime(), TimeSpan.FromMinutes(5)); // Adjust delta as needed
        
        
        
    }
}