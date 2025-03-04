using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;
using Xunit;

namespace OrderService.Test.Services
{
    public class OrderStatusServiceTest
    {
        [Fact]
        public void AddOrderStatus_ValidOrderIdAndStatusDto_ReturnsTrue()
        {
            // Arrange
            var orderId = 1;
            var statusDto = new OrderStatusInputDTO { OrderStatus = nameof(Status.DELIVERED) };
            var order = new Order { Id = orderId };

            var mockOrderRepository = new Mock<IRepository<Order>>();
            mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>())).Returns(order);

            var mockOrderStatusRepository = new Mock<IRepository<OrderStatus>>();
            mockOrderStatusRepository.Setup(repo => repo.Insert(It.IsAny<OrderStatus>())).Returns(new OrderStatus { Status =  (Status)Enum.Parse(typeof(Status), statusDto.OrderStatus), StatusDate = DateTime.Now.ToUniversalTime() });
            var orderStatusService = new OrderStatusService(mockOrderStatusRepository.Object, mockOrderRepository.Object, null, null);

            // Act
            var result = orderStatusService.AddOrderStatus(orderId, statusDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.OrderStatus, statusDto.OrderStatus);
            mockOrderStatusRepository.Verify(repo => repo.Insert(It.IsAny<OrderStatus>()), Times.Once);
        }


        [Fact]
        public void AddOrderStatus_InvalidStatus_ThrowsException()
        {
            // Arrange
            var orderId = 1;
            var statusDto = new OrderStatusInputDTO { OrderStatus = "InvalidStatus" };
            var order = new Order { Id = orderId };

            var mockOrderRepository = new Mock<IRepository<Order>>();
            mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>())).Returns(order);

            var mockOrderStatusRepository = new Mock<IRepository<OrderStatus>>();

            var orderStatusService = new OrderStatusService(mockOrderStatusRepository.Object, mockOrderRepository.Object, null, null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => orderStatusService.AddOrderStatus(orderId, statusDto));
            mockOrderStatusRepository.Verify(repo => repo.Insert(It.IsAny<OrderStatus>()), Times.Never);
        }


        [Fact]
        public void AddOrderStatus_OrderNotFound_ThrowsException()
        {
            // Arrange
            var orderId = 1;
            var statusDto = new OrderStatusInputDTO { OrderStatus = nameof(Status.DELIVERED) };

            var mockOrderRepository = new Mock<IRepository<Order>>();
            mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>())).Returns<Order>(null);

            var mockOrderStatusRepository = new Mock<IRepository<OrderStatus>>();

            var orderStatusService = new OrderStatusService(mockOrderStatusRepository.Object, mockOrderRepository.Object, null, null);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => orderStatusService.AddOrderStatus(orderId, statusDto));
            mockOrderStatusRepository.Verify(repo => repo.Insert(It.IsAny<OrderStatus>()), Times.Never);
        }

        [Fact]
        public void GetOrderStatuses_ValidOrderId_ReturnsOrderStatuses()
        {
            // Arrange
            var orderId = 1;
            var order = new Order { Id = orderId, OrderStatuses = new List<OrderStatus> { new OrderStatus { Status = Status.SENT, StatusDate = DateTime.Now } } };

            var mockOrderRepository = new Mock<IRepository<Order>>();
            mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>())).Returns(order);

            var mockOrderStatusRepository = new Mock<IRepository<OrderStatus>>();

            var orderStatusService = new OrderStatusService(mockOrderStatusRepository.Object, mockOrderRepository.Object, null, null);

            // Act
            var result = orderStatusService.GetOrderStatuses(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(order.OrderStatuses.First().Status.ToString(), result.First().OrderStatus);
            Assert.Equal(order.OrderStatuses.First().StatusDate, result.First().StatusDate);
        }


        [Fact]
        public void GetOrderStatuses_OrderNotFound_ThrowsException()
        {
            // Arrange
            var orderId = 1;

            var mockOrderRepository = new Mock<IRepository<Order>>();
            mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>())).Returns<Order>(null);

            var mockOrderStatusRepository = new Mock<IRepository<OrderStatus>>();

            var orderStatusService = new OrderStatusService(mockOrderStatusRepository.Object, mockOrderRepository.Object, null, null);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => orderStatusService.GetOrderStatuses(orderId));
        }
    }
}
