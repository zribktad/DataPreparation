using Moq;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;


namespace OrderService.Test.Services
{
    public class CustomerServiceTest
    {
        [Fact]
        public void GetAllCustomers_Returns_All_Customers()
        {
            var customers = new List<Customer>
            {
                new Customer { Id = 1, Name = "Customer 1", Address = new Address() },
                new Customer { Id = 2, Name = "Customer 2", Address = new Address() }
            };

            var mockRepository = new Mock<IRepository<Customer>>();
            mockRepository.Setup(repo => repo.GetAll(It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
               .Returns(customers);
            CustomerService _customerService = new CustomerService(mockRepository.Object);

            var result = _customerService.GetAllCustomers();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetCustomerById_Returns_Customer_If_Found_In_Database()
        {
            long customerId = 1;
            var expectedCustomer = new Customer { Id = customerId, Name = "Customer 1", Address = new Address() };

            var mockRepository = new Mock<IRepository<Customer>>();
            mockRepository.Setup(repo => repo.GetById(customerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                          .Returns(expectedCustomer);

            var _customerService = new CustomerService(mockRepository.Object);

            var result = _customerService.GetCustomerById(customerId);

            Assert.NotNull(result);
            Assert.Equal(expectedCustomer.Id, result.Id);
            Assert.Equal(expectedCustomer.Name, result.Name);
        }

        [Fact]
        public void GetCustomerById_Throws_Exception_If_Not_Found_In_Database()
        {
            long nonExistentCustomerId = 0;

            var mockRepository = new Mock<IRepository<Customer>>();
            mockRepository.Setup(repo => repo.GetById(nonExistentCustomerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                          .Returns((Customer)null);

            var _customerService = new CustomerService(mockRepository.Object);

            Assert.Throws<InvalidOperationException>(() => _customerService.GetCustomerById(nonExistentCustomerId));
        }

        [Fact]
        public void CreateCustomer_Returns_New_Customer()
        {
            var customerDTO = new CustomerDTO {  };

            var mockRepository = new Mock<IRepository<Customer>>();
            mockRepository.Setup(repo => repo.Insert(It.IsAny<Customer>()))
                          .Callback<Customer>(c => c.Id = 1); 

            var _customerService = new CustomerService(mockRepository.Object);

            var result = _customerService.CreateCustomer(customerDTO);

            Assert.NotNull(result);
        }

        [Fact]
        public void UpdateCustomer_Returns_Updated_Customer()
        {
            long customerId = 1;
            var updatedCustomer = new Customer
            {
                Id = customerId,
                Name = "Updated Name",
                Email = "updated@example.com",
                Phone = "1234567890",
                Address = new Address
                {
                    Street = "Updated Street",
                    City = "Updated City",
                    PostalCode = "12345"
                }
            };

            var mockRepository = new Mock<IRepository<Customer>>();
            mockRepository.Setup(repo => repo.GetById(customerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                          .Returns(updatedCustomer);
            mockRepository.Setup(repo => repo.Update(It.IsAny<Customer>()));

            var _customerService = new CustomerService(mockRepository.Object);

            var result = _customerService.UpdateCustomer(customerId, updatedCustomer);

            Assert.NotNull(result);
            Assert.Equal(customerId, result.Id);
            Assert.Equal(updatedCustomer.Name, result.Name);
            Assert.Equal(updatedCustomer.Email, result.Email);
            Assert.Equal(updatedCustomer.Phone, result.Phone);
            Assert.Equal(updatedCustomer.Address.Street, result.Address.Street);
            Assert.Equal(updatedCustomer.Address.City, result.Address.City);
            Assert.Equal(updatedCustomer.Address.PostalCode, result.Address.PostalCode);
        }

    }
}
