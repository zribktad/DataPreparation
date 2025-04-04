using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Controllers;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Test.Controllers
{
    public class OrderControllerIntegrationTest
    {
        [Fact]
        public void AddStatus_ReturnsOkResult_And_ChangesOrderStatus()
        {
            // Arrange
            long orderId = 1;
            var statusDto = new OrderStatusInputDTO { OrderStatus = "DELIVERING" };
            var order = new Order { Id = orderId, OrderStatuses = new List<OrderStatus>() };
            var newOrderStatus = new OrderStatusOutputDTO { OrderStatus = "DELIVERING", StatusDate = DateTime.Now };

            var loggerMock = new Mock<ILogger<CustomerController>>();
            var orderServiceMock = new Mock<IOrderService>();
            var orderManagementServiceMock = new Mock<IOrderManagementService>();
            var orderStatusServiceMock = new Mock<IOrderStatusService>();
            var orderItemServiceMock = new Mock<IOrderItemService>();
            var customerServiceMock = new Mock<ICustomerService>();

            orderServiceMock.Setup(x => x.GetOrder(orderId)).Returns(order);
            orderStatusServiceMock.Setup(x => x.AddOrderStatus(orderId, statusDto)).Returns(newOrderStatus);

            var controller = new OrderController(loggerMock.Object, orderServiceMock.Object, orderManagementServiceMock.Object, orderStatusServiceMock.Object, orderItemServiceMock.Object, customerServiceMock.Object);

            // Act
            var result = controller.AddStatus(orderId, statusDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            // Check if the new order status was added
            orderStatusServiceMock.Verify(x => x.AddOrderStatus(orderId, statusDto), Times.Once);

            // Check if the result is OK
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void GetOrders_ReturnsOkResult_WithListOfOrders()
        {
            var _orderServiceMock = new Mock<IOrderService>();
            var _controller = new OrderController(null, _orderServiceMock.Object, null, null, null, null);
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = 1, CustomerId = 1, OrderDate = DateTime.Now },
                new Order { Id = 2, CustomerId = 1, OrderDate = DateTime.Now }
            };
            _orderServiceMock.Setup(x => x.GetOrders()).Returns(orders);

            // Act
            var result = _controller.GetOrders() as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var model = Assert.IsAssignableFrom<IEnumerable<Order>>(result.Value);
            Assert.Equal(orders.Count, model.Count());
        }

        [Fact]
        public void GetOrder_WithValidId_ReturnsOkResult_WithOrder()
        {
            var _orderServiceMock = new Mock<IOrderService>();
            var _controller = new OrderController(null, _orderServiceMock.Object, null, null, null, null);
            // Arrange
            long orderId = 1;
            var order = new Order { Id = orderId, CustomerId = 1, OrderDate = DateTime.Now };
            _orderServiceMock.Setup(x => x.GetOrder(orderId)).Returns(order);

            // Act
            var result = _controller.GetOrder(orderId) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var model = Assert.IsType<Order>(result.Value);
            Assert.Equal(order.Id, model.Id);
        }

        [Fact]
        public void GetOrder_WithInvalidId_ReturnsNotFoundResult()
        {
            var _orderServiceMock = new Mock<IOrderService>();
            var _controller = new OrderController(null, _orderServiceMock.Object, null, null, null, null);
            // Arrange
            long orderId = 1;
            _orderServiceMock.Setup(x => x.GetOrder(orderId)).Throws<ArgumentException>();

            // Act
            var result = _controller.GetOrder(orderId) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public void PostOrder_WithValidData_ReturnsCreatedAtRouteResult()
        {
            var _orderServiceMock = new Mock<IOrderService>();
            var _controller = new OrderController(null, _orderServiceMock.Object, null, null, null, null);
            // Arrange
            var orderDTO = new OrderDTO { CustomerId = 1 };
            var newOrder = new Order { Id = 1, CustomerId = 1, OrderDate = DateTime.Now };
            _orderServiceMock.Setup(x => x.CreateOrder(orderDTO)).Returns(newOrder);

            // Act
            var result = _controller.PostOrder(orderDTO) as CreatedAtRouteResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
            Assert.Equal("GetOrder", result.RouteName);
            Assert.Equal(newOrder.Id, result.RouteValues["id"]);
            var model = Assert.IsType<Order>(result.Value);
            Assert.Equal(newOrder.Id, model.Id);
        }

        [Fact]
        public void PostOrder_WithNullData_ReturnsBadRequestResult()
        {
            var _orderServiceMock = new Mock<IOrderService>();
            var _controller = new OrderController(null, _orderServiceMock.Object, null, null, null, null);
            // Act
            var result = _controller.PostOrder(null) as BadRequestResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }


        [Fact]
        public void PostRating_WithValidData_ReturnsCreatedAtRouteResult()
        {
            var _orderManagementServiceMock = new Mock<IOrderManagementService>();
            var _controller = new OrderController(null, null, _orderManagementServiceMock.Object, null, null, null);
            // Arrange
            long orderId = 1;
            var rating = new RatingDTO { NumOfStars = 5 };
            var newRating = new Rating { OrderId = orderId, NumOfStars = rating.NumOfStars };
            _orderManagementServiceMock.Setup(x => x.AddRatingToOrder(orderId, rating)).Returns(newRating);

            // Act
            var result = _controller.PostRating(orderId, rating) as CreatedAtRouteResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
            Assert.Equal("GetOrder", result.RouteName);
            Assert.Equal(orderId, result.RouteValues["id"]);
            var model = Assert.IsType<Rating>(result.Value);
            Assert.Equal(newRating.OrderId, model.OrderId);
        }

        [Fact]
        public void PostRating_WithInvalidData_ReturnsNotFoundResult()
        {
            var _orderManagementServiceMock = new Mock<IOrderManagementService>();
            var _controller = new OrderController(null, null, _orderManagementServiceMock.Object, null, null, null);
            // Arrange
            long orderId = 1;
            var rating = new RatingDTO { NumOfStars = 5 };
            _orderManagementServiceMock.Setup(x => x.AddRatingToOrder(orderId, rating)).Throws<InvalidOperationException>();

            // Act
            var result = _controller.PostRating(orderId, rating) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public void PostComplaint_WithValidData_ReturnsCreatedAtRouteResult()
        {
            var _orderManagementServiceMock = new Mock<IOrderManagementService>();
            var _controller = new OrderController(null, null, _orderManagementServiceMock.Object, null, null, null);
            // Arrange
            long orderId = 1;
            var complaint = new ComplaintDTO { Reason = "Bad quality" };
            var newComplaint = new Complaint { OrderId = orderId, Reason = complaint.Reason };
            _orderManagementServiceMock.Setup(x => x.AddComplaintToOrder(orderId, complaint)).Returns(newComplaint);

            // Act
            var result = _controller.PostComplaint(orderId, complaint) as CreatedAtRouteResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
            Assert.Equal("GetOrder", result.RouteName);
            Assert.Equal(orderId, result.RouteValues["id"]);
            var model = Assert.IsType<Complaint>(result.Value);
            Assert.Equal(newComplaint.OrderId, model.OrderId);
        }

        [Fact]
        public void PostComplaint_WithInvalidData_ReturnsNotFoundResult()
        {
            var _orderManagementServiceMock = new Mock<IOrderManagementService>();
            var _controller = new OrderController(null, null, _orderManagementServiceMock.Object, null, null, null);
            // Arrange
            long orderId = 1;
            var complaint = new ComplaintDTO { Reason = "Bad quality" };
            _orderManagementServiceMock.Setup(x => x.AddComplaintToOrder(orderId, complaint)).Throws<InvalidOperationException>();

            // Act
            var result = _controller.PostComplaint(orderId, complaint) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public void PutComplaint_WithValidData_ReturnsCreatedAtRouteResult()
        {
            var _orderManagementServiceMock = new Mock<IOrderManagementService>();
            var _controller = new OrderController(null, null, _orderManagementServiceMock.Object, null, null, null);
            // Arrange
            long orderId = 1;
            var complaint = new ComplaintDTO { Reason = "Bad quality" };
            var updatedComplaint = new Complaint { OrderId = orderId, Reason = complaint.Reason };
            _orderManagementServiceMock.Setup(x => x.UpdateComplaintStatus(orderId, complaint)).Returns(updatedComplaint);

            // Act
            var result = _controller.PutComplaint(orderId, complaint) as CreatedAtRouteResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
            Assert.Equal("GetOrder", result.RouteName);
            Assert.Equal(orderId, result.RouteValues["id"]);
            var model = Assert.IsType<Complaint>(result.Value);
            Assert.Equal(updatedComplaint.OrderId, model.OrderId);
        }

        [Fact]
        public void PutComplaint_WithInvalidData_ReturnsNotFoundResult()
        {
            var _orderManagementServiceMock = new Mock<IOrderManagementService>();
            var _controller = new OrderController(null, null, _orderManagementServiceMock.Object, null, null, null);
            // Arrange
            long orderId = 1;
            var complaint = new ComplaintDTO { Reason = "Bad quality" };
            _orderManagementServiceMock.Setup(x => x.UpdateComplaintStatus(orderId, complaint)).Throws<InvalidOperationException>();

            // Act
            var result = _controller.PutComplaint(orderId, complaint) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public void AddItem_WithValidData_ReturnsOkResult()
        {
            var _orderItemServiceMock = new Mock<IOrderItemService>();
            var _controller = new OrderController(null, null, null, null, _orderItemServiceMock.Object, null);
            // Arrange
            long orderId = 1;
            var orderItemDTO = new OrderItemDTO { ItemId = 1, Quantity = 2 };
            var Order = new Order { Id = orderId, CustomerId = 1 };
            var orderItem = new OrderItem { Order = Order, ItemId = orderItemDTO.ItemId, Quantity = orderItemDTO.Quantity };
            _orderItemServiceMock.Setup(x => x.AddOrderItem(orderId, orderItemDTO)).Returns(true);

            // Act
            var result = _controller.AddItem(orderId, orderItemDTO) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public void AddItem_WithInvalidData_ReturnsNotFoundResult()
        {
            var _orderItemServiceMock = new Mock<IOrderItemService>();
            var _controller = new OrderController(null, null, null, null, _orderItemServiceMock.Object, null);
            // Arrange
            long orderId = 1;
            var orderItemDTO = new OrderItemDTO { ItemId = 1, Quantity = 2 };
            _orderItemServiceMock.Setup(x => x.AddOrderItem(orderId, orderItemDTO)).Throws<ArgumentException>();

            // Act
            var result = _controller.AddItem(orderId, orderItemDTO) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public void GetItems_ReturnsOkResult_WithListOfItems()
        {
            var _orderItemServiceMock = new Mock<IOrderItemService>();
            var _controller = new OrderController(null, null, null, null, _orderItemServiceMock.Object, null);
            // Arrange
            long orderId = 1;
            var order = new Order { Id = orderId, CustomerId = 1 };
            var orderItems = new List<OrderItem>
            {
                new OrderItem { Order = order, ItemId = 1, Quantity = 2, Cost = 10 },
                new OrderItem { Order = order, ItemId = 2, Quantity = 1, Cost = 20 }
            };
            _orderItemServiceMock.Setup(x => x.GetOrderItems(orderId)).Returns(orderItems);

            // Act
            var result = _controller.GetItems(orderId) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderItem>>(result.Value);
            Assert.Equal(orderItems.Count, model.Count());
        }

        [Fact]
        public void GetItems_WithInvalidId_ReturnsNotFoundResult()
        {
            var _orderItemServiceMock = new Mock<IOrderItemService>();
            var _controller = new OrderController(null, null, null, null, _orderItemServiceMock.Object, null);
            // Arrange
            long orderId = 1;
            _orderItemServiceMock.Setup(x => x.GetOrderItems(orderId)).Throws<ArgumentException>();

            // Act
            var result = _controller.GetItems(orderId) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public void GetReceipt_ReturnsOkResult_WithReceipt()
        {
            var _orderServiceMock = new Mock<IOrderService>();
            var _customerServiceMock = new Mock<ICustomerService>();
            var _orderItemServiceMock = new Mock<IOrderItemService>();

            var _controller = new OrderController(null, _orderServiceMock.Object, null, null, _orderItemServiceMock.Object, _customerServiceMock.Object);
            // Arrange
            long orderId = 1;
            var order = new Order { Id = orderId, CustomerId = 1 };
            var orderItems = new List<OrderItem>
            {
                new OrderItem { ItemId = 1, Quantity = 2, Cost = 10 },
                new OrderItem { ItemId = 2, Quantity = 1, Cost = 20 }
            };
            var customer = new Customer { Id = 1, Name = "John Doe" };

            _orderServiceMock.Setup(x => x.GetOrder(orderId)).Returns(order);
            _orderItemServiceMock.Setup(x => x.GetOrderItems(orderId)).Returns(orderItems);
            _customerServiceMock.Setup(x => x.GetCustomerById(order.CustomerId)).Returns(customer);

            // Act
            var result = _controller.GetReceipt(orderId) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var receipt = result.Value as string;
            Assert.Contains(customer.Name, receipt);
            Assert.Contains(orderId.ToString(), receipt);
            foreach (var item in orderItems)
            {
                Assert.Contains(item.ItemId.ToString(), receipt);
                Assert.Contains(item.Quantity.ToString(), receipt);
                Assert.Contains(item.Cost.ToString(), receipt);
                Assert.Contains((item.Quantity * item.Cost).ToString(), receipt);
            }
        }

        [Fact]
        public void GetReceipt_WithInvalidId_ReturnsNotFoundResult()
        {
            var _orderServiceMock = new Mock<IOrderService>();
            var _customerServiceMock = new Mock<ICustomerService>();
            var _orderItemServiceMock = new Mock<IOrderItemService>();

            var _controller = new OrderController(null, _orderServiceMock.Object, null, null, _orderItemServiceMock.Object, _customerServiceMock.Object);
            // Arrange
            long orderId = 1;

            _orderServiceMock.Setup(x => x.GetOrder(orderId)).Throws<ArgumentException>();

            // Act
            var result = _controller.GetReceipt(orderId) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

    }
}
