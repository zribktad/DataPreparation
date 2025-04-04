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
    public class CustomerControllerIntegrationTest
    {
        private readonly Mock<ILogger<CustomerController>> _loggerMock;
        private readonly Mock<ICustomerService> _customerServiceMock;

        private readonly CustomerController _controller;

        public CustomerControllerIntegrationTest()
        {
            _loggerMock = new Mock<ILogger<CustomerController>>();
            _customerServiceMock = new Mock<ICustomerService>();

            _controller = new CustomerController(_loggerMock.Object, _customerServiceMock.Object);
        }

        [Fact]
        public void Get_ReturnsOkResult_WithListOfCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { Id = 1, Name = "John Doe", Email = "john@example.com" },
                new Customer { Id = 2, Name = "Jane Doe", Email = "jane@example.com" }
            };
            _customerServiceMock.Setup(x => x.GetAllCustomers()).Returns(customers);

            // Act
            var result = _controller.Get() as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var model = Assert.IsAssignableFrom<IEnumerable<Customer>>(result.Value);
            Assert.Equal(customers.Count, model.Count());
        }

        [Fact]
        public void Get_WithValidId_ReturnsOkResult_WithCustomer()
        {
            // Arrange
            long customerId = 1;
            var customer = new Customer { Id = customerId, Name = "John Doe", Email = "john@example.com" };
            _customerServiceMock.Setup(x => x.GetCustomerById(customerId)).Returns(customer);

            // Act
            var result = _controller.Get(customerId) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var model = Assert.IsType<Customer>(result.Value);
            Assert.Equal(customer.Id, model.Id);
        }

        [Fact]
        public void Get_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            long customerId = 1;
            _customerServiceMock.Setup(x => x.GetCustomerById(customerId)).Throws<InvalidOperationException>();

            // Act
            var result = _controller.Get(customerId) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public void Post_WithValidData_ReturnsCreatedAtRouteResult()
        {
            // Arrange
            var customerDTO = new CustomerDTO { Name = "John Doe", Email = "john@example.com" };
            var newCustomer = new Customer { Id = 1, Name = "John Doe", Email = "john@example.com" };
            _customerServiceMock.Setup(x => x.CreateCustomer(customerDTO)).Returns(newCustomer);

            // Act
            var result = _controller.Post(customerDTO) as CreatedAtRouteResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
            Assert.Equal("GetCustomer", result.RouteName);
            Assert.Equal(newCustomer.Id, result.RouteValues["id"]);
            var model = Assert.IsType<Customer>(result.Value);
            Assert.Equal(newCustomer.Id, model.Id);
        }

        [Fact]
        public void Put_WithValidIdAndData_ReturnsOkResult_WithUpdatedCustomer()
        {
            // Arrange
            long customerId = 1;
            var updatedCustomer = new Customer { Id = customerId, Name = "Updated Name", Email = "updated@example.com" };
            _customerServiceMock.Setup(x => x.UpdateCustomer(customerId, It.IsAny<Customer>())).Returns(updatedCustomer);

            // Act
            var result = _controller.Put(customerId, updatedCustomer) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var model = Assert.IsType<Customer>(result.Value);
            Assert.Equal(updatedCustomer.Id, model.Id);
            Assert.Equal(updatedCustomer.Name, model.Name);
            Assert.Equal(updatedCustomer.Email, model.Email);
        }

        [Fact]
        public void Put_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            long customerId = 1;
            _customerServiceMock.Setup(x => x.UpdateCustomer(customerId, It.IsAny<Customer>())).Throws<InvalidOperationException>();

            // Act
            var result = _controller.Put(customerId, new Customer { Id = customerId }) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }
    }
}
