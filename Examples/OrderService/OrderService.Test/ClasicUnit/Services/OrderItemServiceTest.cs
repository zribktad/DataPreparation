using Moq;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OrderService.Test.Services
{
    public class OrderItemServiceTest
    {
        [Fact]
        public void AddOrderItem_ValidOrder_ReturnsTrue()
        {
            // Arrange
            var orderId = 1;
            var order = new Order { Id = orderId };
            var orderItemDTO = new OrderItemDTO { ItemId = 1, Quantity = 2 };

            var mockOrderRepository = new Mock<IRepository<Order>>();
            mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>())).Returns(order);

            var mockOrderItemRepository = new Mock<IRepository<OrderItem>>();
            mockOrderItemRepository.Setup(repo => repo.Insert(It.IsAny<OrderItem>()));

            var orderItemService = new OrderItemService(mockOrderRepository.Object, mockOrderItemRepository.Object);

            // Act
            var result = orderItemService.AddOrderItem(orderId, orderItemDTO);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetOrderItems_ExistingOrder_ReturnsOrderItems()
        {
            // Arrange
            var orderId = 1;
            var order = new Order
            {
                Id = orderId,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Id = 1, ItemId = 1, Quantity = 2 },
                    new OrderItem { Id = 2, ItemId = 2, Quantity = 3 }
                }
            };

            var mockOrderRepository = new Mock<IRepository<Order>>();
            mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>())).Returns(order);

            var orderItemService = new OrderItemService(mockOrderRepository.Object, Mock.Of<IRepository<OrderItem>>());

            // Act
            var result = orderItemService.GetOrderItems(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetOrderItems_NonExistingOrder_ThrowsException()
        {
            // Arrange
            var orderId = 1;
            var mockOrderRepository = new Mock<IRepository<Order>>();
            mockOrderRepository.Setup(repo => repo.GetById(orderId, null)).Returns((Order)null); // Pass null for optional parameter

            var orderItemService = new OrderItemService(mockOrderRepository.Object, Mock.Of<IRepository<OrderItem>>());

            // Act & Assert
            Assert.Throws<ArgumentException>(() => orderItemService.GetOrderItems(orderId));
        }
    }
}
