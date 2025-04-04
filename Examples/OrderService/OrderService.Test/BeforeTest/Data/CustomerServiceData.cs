using Castle.Core.Resource;
using DataPreparation.Data;
using Moq;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;

namespace OrderService.DataTest.Data
{
    [PreparationClassFor(typeof(CustomerService))]
    public class CustomerServiceData: IBeforePreparationTask
    {

        public Task UpData()
        {
            Console.WriteLine("CustomerServiceData up data ");
           return Task.CompletedTask;
        }

        public Task DownData()
        {
            Console.WriteLine("CustomerServiceData Down Data ");
            return Task.CompletedTask;
        }


        [PreparationMethodFor(typeof(CustomerService), nameof(CustomerService.GetAllCustomers))]
        public class GetAllCustomersBefore : IBeforePreparation
        {
            private readonly Mock<IRepository<Customer>> _customerRepo;
            public GetAllCustomersBefore(Mock<IRepository<Customer>> customerRepo)
            {
                _customerRepo = customerRepo;
            }

            public void UpData()
            {

                Console.WriteLine("GetAllCustomersBefore up data ");
                var customers = new List<Customer>
                {
                    new Customer { Id = 1, Name = "Customer 1", Address = new Address() },
                    new Customer { Id = 2, Name = "Customer 2", Address = new Address() }
                };

                _customerRepo.Setup(repo => repo.GetAll(It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                    .Returns(customers);

             
            }

            public void DownData()
            {
                Console.WriteLine("GetAllCustomersBefore Down Data ");
                _customerRepo.Setup(repo => repo.GetAll(It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                    .Returns([]);
            }
        }

        [PreparationMethodFor(typeof(CustomerService), nameof(CustomerService.GetCustomerById))]
        public class GetCustomerByIdData(Mock<IRepository<Customer>> customerRepo)
        {
            [UpData]
            
            public void TestUpData(long customerId , string customerName)
            {
                var expectedCustomer = new Customer { Id = customerId, Name = customerName, Address = new Address() };

                customerRepo.Setup(repo => repo.GetById(customerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                    .Returns(expectedCustomer);
                Console.WriteLine("GetCustomerByIdData up data ");
            }
            [DownData]
            public void TestDownData(long customerId)
            {
                customerRepo.Setup(repo => repo.GetById(customerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                    .Returns((Customer)null);
                Console.WriteLine("GetCustomerByIdData Down Data ");
            }
        }
        
        [PreparationMethodFor(typeof(CustomerService), nameof(CustomerService.UpdateCustomer))]
        public class UpdateCustomerData(Mock<IRepository<Customer>> customerRepo)
        {
            [UpData]
            public Task TestUpData()
            {
                customerRepo.Setup(repo => repo.Update(It.IsAny<Customer>())).Returns((Customer c) => c);
                return Task.CompletedTask;
            }
            [DownData]
            public async Task TestDownData()
            {
               
            }
        }

    }
}
