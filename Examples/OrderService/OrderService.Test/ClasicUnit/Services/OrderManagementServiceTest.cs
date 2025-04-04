using System;
using Moq;
using OrderService.DTO;
using OrderService.Exceptions;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;
using Xunit;

namespace OrderService.Test.Services
{
    public class OrderManagementServiceTest
    {
        [Fact]
        public void AddRatingToOrder_ValidOrder_ReturnsNewRating()
        {
            // Arrange
            var orderId = 1;
            var ratingDto = new RatingDTO { NumOfStars = 5, Reason = "Great service" };
            var order = new Order { Id = orderId };

            var mockOrderService = new Mock<IOrderService>();
            mockOrderService.Setup(service => service.GetOrder(orderId)).Returns(order);
            mockOrderService.Setup(service => service.UpdateOrder(orderId, It.IsAny<Order>()));

            var orderManagementService = new OrderManagementService(mockOrderService.Object);

            // Act
            var result = orderManagementService.AddRatingToOrder(orderId, ratingDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ratingDto.NumOfStars, result.NumOfStars);
            Assert.Equal(ratingDto.Reason, result.Reason);
        }

        [Fact]
        public void AddRatingToOrder_OrderNotFound_ThrowsException()
        {
            // Arrange
            var orderId = 1;
            var ratingDto = new RatingDTO { NumOfStars = 5, Reason = "Great service" };

            var mockOrderService = new Mock<IOrderService>();
            mockOrderService.Setup(service => service.GetOrder(orderId)).Throws<InvalidOperationException>();

            var orderManagementService = new OrderManagementService(mockOrderService.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => orderManagementService.AddRatingToOrder(orderId, ratingDto));
        }

        [Fact]
        public void AddComplaintToOrder_ValidOrder_ReturnsNewComplaint()
        {
            // Arrange
            var orderId = 1;
            var complaintDto = new ComplaintDTO { Status = "Pending", Reason = "Issue with the product" };
            var order = new Order { Id = orderId };

            var mockOrderService = new Mock<IOrderService>();
            mockOrderService.Setup(service => service.GetOrder(orderId)).Returns(order);
            mockOrderService.Setup(service => service.UpdateOrder(orderId, It.IsAny<Order>()));

            var orderManagementService = new OrderManagementService(mockOrderService.Object);

            // Act
            var result = orderManagementService.AddComplaintToOrder(orderId, complaintDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(complaintDto.Status, result.Status);
            Assert.Equal(complaintDto.Reason, result.Reason);
        }

        [Fact]
        public void AddComplaintToOrder_OrderNotFound_ThrowsException()
        {
            // Arrange
            var orderId = 1;
            var complaintDto = new ComplaintDTO { Status = "Pending", Reason = "Issue with the product" };

            var mockOrderService = new Mock<IOrderService>();
            mockOrderService.Setup(service => service.GetOrder(orderId)).Throws<InvalidOperationException>();

            var orderManagementService = new OrderManagementService(mockOrderService.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => orderManagementService.AddComplaintToOrder(orderId, complaintDto));
        }

        [Fact]
        public void AddComplaintToOrder_ComplaintAlreadyExists_ThrowsException()
        {
            // Arrange
            var orderId = 1;
            var complaintDto = new ComplaintDTO { Status = "Pending", Reason = "Issue with the product" };
            var order = new Order { Id = orderId, Complaint = new Complaint() };

            var mockOrderService = new Mock<IOrderService>();
            mockOrderService.Setup(service => service.GetOrder(orderId)).Returns(order);

            var orderManagementService = new OrderManagementService(mockOrderService.Object);

            // Act & Assert
            Assert.Throws<AlreadyExistsException>(() => orderManagementService.AddComplaintToOrder(orderId, complaintDto));
        }
        [Fact]
        public void UpdateComplaintStatus_ValidOrderAndComplaint_ReturnsUpdatedComplaint()
        {
            // Arrange
            var orderId = 1;
            var complaintDto = new ComplaintDTO { Status = "Resolved", Reason = "Resolved the issue" };
            var order = new Order { Id = orderId, Complaint = new Complaint { Status = "Pending", Reason = "Issue with the product" } };

            var mockOrderService = new Mock<IOrderService>();
            mockOrderService.Setup(service => service.GetOrder(orderId)).Returns(order);
            mockOrderService.Setup(service => service.UpdateOrder(orderId, It.IsAny<Order>()));

            var orderManagementService = new OrderManagementService(mockOrderService.Object);

            // Act
            var result = orderManagementService.UpdateComplaintStatus(orderId, complaintDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(complaintDto.Status, result.Status);
            Assert.Equal(complaintDto.Reason, result.Reason);
        }


        [Fact]
        public void UpdateComplaintStatus_OrderNotFound_ThrowsException()
        {
            // Arrange
            var orderId = 1;
            var complaintDto = new ComplaintDTO { Status = "Resolved", Reason = "Resolved the issue" };

            var mockOrderService = new Mock<IOrderService>();
            mockOrderService.Setup(service => service.GetOrder(orderId)).Throws<InvalidOperationException>();

            var orderManagementService = new OrderManagementService(mockOrderService.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => orderManagementService.UpdateComplaintStatus(orderId, complaintDto));
        }


        [Fact]
        public void UpdateComplaintStatus_ComplaintNotFound_ThrowsException()
        {
            // Arrange
            var orderId = 1;
            var complaintDto = new ComplaintDTO { Status = "Resolved", Reason = "Resolved the issue" };
            var order = new Order { Id = orderId }; // No complaint

            var mockOrderService = new Mock<IOrderService>();
            mockOrderService.Setup(service => service.GetOrder(orderId)).Returns(order);

            var orderManagementService = new OrderManagementService(mockOrderService.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => orderManagementService.UpdateComplaintStatus(orderId, complaintDto));
        }
    }
}

