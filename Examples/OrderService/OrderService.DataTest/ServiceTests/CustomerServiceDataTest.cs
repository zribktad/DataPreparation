using Castle.Core.Configuration;
using DataPreparation.Analyzers.Test;
using DataPreparation.Provider;
using DataPreparation.Testing;
using DataPreparation.Unums.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Npgsql;
using OrderService.DataTest.Data;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;
using FluentAssertions;


namespace OrderService.Test.Services
{

    [DataPreparationFixture]
    public class CustomerServiceDataTest : IDataPreparationTestServices
    {


        private static Mock<IRepository<Customer>> _mockRepository;
        private static IConfigurationRoot _configuration;
        private static string _connectionString;
        public CustomerServiceDataTest()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            _configuration = configurationBuilder.Build();

            _connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? _configuration.GetConnectionString("DefaultConnection");
            
        }
        public  void DataPreparationServices(IServiceCollection serviceCollection)
        {
            
            serviceCollection.AddDbContext<OrderServiceContext>(options =>
                options.UseNpgsql(_connectionString));
            
            _mockRepository = new Mock<IRepository<Customer>>();
            serviceCollection.AddSingleton(_ => _mockRepository);
        }
        
 //[UsePreparedData(typeof(CustomerServiceData.GetCustomerByIdData))]
        //[DataPreparationAutoAnalyze]
        [Test] 
        [UsePreparedDataFor(typeof(CustomerService), nameof(CustomerService.GetAllCustomers))]
       
        public void GetAllCustomers_Returns_All_Customers()
        {
            Console.WriteLine("Test GetAllCustomers ");
            CustomerService _customerService = new(_mockRepository.Object);
            var result = _customerService.GetAllCustomers();

            // Assert.That(result.Count(), Is.EqualTo(2));
            // Assert.That(result, Is.All.Not.Null);
            // Assert.That(result, Has.Exactly(1).Matches<Customer>(c => c.Id == 1L));
            // Assert.That(result, Has.Exactly(1).Matches<Customer>(c => c.Id == 2L));
            // Assert.That(result, Has.Exactly(1).Matches<Customer>(c => c.Name == "Customer 1"));
            var s = "Customer 1";
            var Name = "s";
            
            //result.Count().Should().Be(2);
            result.Should().HaveCount(2);
            // result.Should().NotBeNullOrEmpty();
            // result.Should().ContainSingle(c => c.Name == "Customer 1" );
            // result.Should().ContainSingle(c => c.Name == "Customer 2");
            // result.Should().ContainSingle(c => c.Id == 1L);
            // result.Should().ContainSingle(c => c.Id == 2L);
           
            
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result, Is.All.Not.Null);
            Assert.That(result, Has.Exactly(1).Matches<Customer>(c => c.Id == 1L));
            Assert.That(result, Has.Exactly(1).Matches<Customer>(c => c.Id == 2L));
            Assert.That(result, Has.Exactly(1).Matches<Customer>(c => c.Name == "Customer 1"));

            
        }


        [Test , UsePreparedDataParams(typeof(CustomerServiceData.GetCustomerByIdData),[5L, "Customer 1",],[5L])]
        public void GetCustomerById_Returns_Customer_If_Found_In_Database()
        {
            Console.WriteLine("Test GetCustomerById ");
            var mockRepo = PreparationContext.GetProvider().GetService<Mock<IRepository<Customer>>>();;

                CustomerService _customerService = new(mockRepo.Object);
            var result = _customerService.GetCustomerById(5L);
            
            
            result.Should().NotBeNull();
            result.Id.Should().Be(5L);
            result.Name.Should().Be("Customer 1");
            //
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(5L));
            Assert.That(result.Name, Is.EqualTo("Customer 1"));
        }
        
        
        [Test] 
        [UsePreparedDataParamsFor(typeof(CustomerService), nameof(CustomerService.GetCustomerById),[1L, "Customer 1"],[1L])]  
        [UsePreparedDataParams(typeof(CustomerServiceData.UpdateCustomerData))]
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

          
            var mockRepo = PreparationContext.GetProvider().GetService<Mock<IRepository<Customer>>>();;
            CustomerService customerService = new(mockRepo.Object);
            var result = customerService.UpdateCustomer(customerId, updatedCustomer);

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
