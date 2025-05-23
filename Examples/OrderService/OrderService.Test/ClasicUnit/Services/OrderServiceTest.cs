﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;
using Assert = Xunit.Assert;

namespace OrderService.Test.Services;

public class OrderServiceTest
{
    [Test]
    public void GetOrders_ReturnsOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { Id = 1, CustomerId = 1 },
            new() { Id = 2, CustomerId = 2 }
        };

        var mockOrderRepository = new Mock<IRepository<Order>>();
        mockOrderRepository.Setup(repo => repo.GetAll(It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns(orders);

        var orderService = new OrderService.Services.OrderService(mockOrderRepository.Object, null);

        // Act
        var result = orderService.GetOrders();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orders.Count, result.Count());
    }

    [Test]
    public void GetOrder_ValidId_ReturnsOrder()
    {
        // Arrange
        var orderId = 1;
        var order = new Order { Id = orderId, CustomerId = 1 };

        var mockOrderRepository = new Mock<IRepository<Order>>();
        mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns(order);

        var orderService = new OrderService.Services.OrderService(mockOrderRepository.Object, null);

        // Act
        var result = orderService.GetOrder(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
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

        var orderService = new OrderService.Services.OrderService(mockOrderRepository.Object, null);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => orderService.GetOrder(invalidId));
    }


    [Test]
    public void CreateOrder_ValidOrderDTO_ReturnsOrder()
    {
        // Arrange
        var customer = new Customer { Id = 1 };
        var orderDto = new OrderDTO
            { CustomerId = customer.Id, OrderItems = new List<OrderItem> { new() { ItemId = 1, Quantity = 1 } } };


        var mockCustomerRepository = new Mock<IRepository<Customer>>();
        mockCustomerRepository.Setup(repo =>
                repo.GetById(orderDto.CustomerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
            .Returns(customer);

        var mockOrderRepository = new Mock<IRepository<Order>>();
        mockOrderRepository.Setup(repo => repo.Insert(It.IsAny<Order>()))
            .Returns(new Order { Id = 1, CustomerId = orderDto.CustomerId });

        var orderService =
            new OrderService.Services.OrderService(mockOrderRepository.Object, mockCustomerRepository.Object);

        // Act
        var result = orderService.CreateOrder(orderDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderDto.CustomerId, result.CustomerId);
        mockOrderRepository.Verify(repo => repo.Insert(It.IsAny<Order>()), Times.Once);
    }

    [Test]
    public void CreateOrder_InvalidCustomerId_ThrowsException()
    {
        // Arrange
        var orderDto = new OrderDTO { CustomerId = 999 };

        var mockCustomerRepository = new Mock<IRepository<Customer>>();
        mockCustomerRepository
            .Setup(repo =>
                repo.GetById(orderDto.CustomerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
            .Returns<Customer>(null);

        var mockOrderRepository = new Mock<IRepository<Order>>();

        var orderService =
            new OrderService.Services.OrderService(mockOrderRepository.Object, mockCustomerRepository.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => orderService.CreateOrder(orderDto));
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
        mockOrderRepository.Setup(repo => repo.GetById(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns(existingOrder);

        var orderService = new OrderService.Services.OrderService(mockOrderRepository.Object, null);

        // Act
        var result = orderService.UpdateOrder(orderId, updatedOrder);

        // Assert
        Assert.True(result);
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

        var orderService = new OrderService.Services.OrderService(mockOrderRepository.Object, null);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => orderService.UpdateOrder(invalidId, updatedOrder));
        mockOrderRepository.Verify(repo => repo.Update(It.IsAny<Order>()), Times.Never);
    }
}