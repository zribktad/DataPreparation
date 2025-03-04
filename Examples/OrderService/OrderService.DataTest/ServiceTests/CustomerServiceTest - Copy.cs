using DataPreparation.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrderService.DataTest.Data;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;

namespace OrderService.Test.Services
{
    public class CustomerServiceTestCopy 
    {
        
        private Mock<IRepository<Customer>> _mockRepository;
        private CustomerService _customerService;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<IRepository<Customer>>();
            _customerService = new CustomerService(_mockRepository.Object);
        }

        [Test]
        public void GetAllCustomers_Returns_All_Customers()
        {
            var customers = new List<Customer>
            {
                new Customer { Id = 1, Name = "Customer 1", Address = new Address() },
                new Customer { Id = 2, Name = "Customer 2", Address = new Address() }
            };

            _mockRepository.Setup(repo => repo.GetAll(It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                           .Returns(customers);

            var result = _customerService.GetAllCustomers();

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetCustomerById_Returns_Customer_If_Found_In_Database()
        {
            long customerId = 1;
            var expectedCustomer = new Customer { Id = customerId, Name = "Customer 1", Address = new Address() };

            _mockRepository.Setup(repo => repo.GetById(customerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                           .Returns(expectedCustomer);

            var result = _customerService.GetCustomerById(customerId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(expectedCustomer.Id));
            Assert.That(result.Name, Is.EqualTo(expectedCustomer.Name));
        }

        [Test]
        public void GetCustomerById_Throws_Exception_If_Not_Found_In_Database()
        {
            long nonExistentCustomerId = 0;

            _mockRepository.Setup(repo => repo.GetById(nonExistentCustomerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                           .Returns((Customer)null);

            Assert.Throws<InvalidOperationException>(() => _customerService.GetCustomerById(nonExistentCustomerId));
        }

        [Test]
        public void CreateCustomer_Returns_New_Customer()
        {
            var customerDTO = new CustomerDTO { };

            _mockRepository.Setup(repo => repo.Insert(It.IsAny<Customer>()))
                           .Callback<Customer>(c => c.Id = 1);

            var result = _customerService.CreateCustomer(customerDTO);

            Assert.That(result, Is.Not.Null);
        }

        [Test]
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

            _mockRepository.Setup(repo => repo.GetById(customerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                           .Returns(updatedCustomer);
            _mockRepository.Setup(repo => repo.Update(It.IsAny<Customer>()));

            var result = _customerService.UpdateCustomer(customerId, updatedCustomer);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(customerId));
            Assert.That(result.Name, Is.EqualTo(updatedCustomer.Name));
            Assert.That(result.Email, Is.EqualTo(updatedCustomer.Email));
            Assert.That(result.Phone, Is.EqualTo(updatedCustomer.Phone));
            Assert.That(result.Address.Street, Is.EqualTo(updatedCustomer.Address.Street));
            Assert.That(result.Address.City, Is.EqualTo(updatedCustomer.Address.City));
            Assert.That(result.Address.PostalCode, Is.EqualTo(updatedCustomer.Address.PostalCode));
        }


     
    }
}
