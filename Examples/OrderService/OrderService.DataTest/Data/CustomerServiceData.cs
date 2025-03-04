using Castle.Core.Resource;
using DataPreparation.Data;
using Moq;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;

namespace OrderService.DataTest.Data
{
    [DataClassPreparationFor(typeof(CustomerService))]
    public class CustomerServiceData: IClassDataPreparation
    {

        public void TestUpData()
        {
            Console.WriteLine("CustomerServiceData up data ");
        }

        public void TestDownData()
        {
            Console.WriteLine("CustomerServiceData Down Data ");
        }


        [DataMethodPreparationFor(typeof(CustomerService), nameof(CustomerService.GetAllCustomers))]
        public class GetAllCustomersData : IMethodDataPreparation
        {
            private readonly Mock<IRepository<Customer>> _customerRepo;
            public GetAllCustomersData(Mock<IRepository<Customer>> customerRepo)
            {
                _customerRepo = customerRepo;
            }

            public void TestUpData()
            {

                Console.WriteLine("GetAllCustomersData up data ");
                var customers = new List<Customer>
                {
                    new Customer { Id = 1, Name = "Customer 1", Address = new Address() },
                    new Customer { Id = 2, Name = "Customer 2", Address = new Address() }
                };

                _customerRepo.Setup(repo => repo.GetAll(It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                    .Returns(customers);

             
            }

            public void TestDownData()
            {
                Console.WriteLine("GetAllCustomersData Down Data ");
                _customerRepo.Setup(repo => repo.GetAll(It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                    .Returns([]);
            }
        }

        [DataMethodPreparationFor(typeof(CustomerService), nameof(CustomerService.GetCustomerById))]
        public class GetCustomerByIdData(Mock<IRepository<Customer>> customerRepo)
        {
            [UpData]
            public void TestUpData(long id , string name)
            {
                var expectedCustomer = new Customer { Id = id, Name = name, Address = new Address() };

                customerRepo.Setup(repo => repo.GetById(id, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                    .Returns(expectedCustomer);
                Console.WriteLine("GetCustomerByIdData up data ");
            }
            [DownData]
            public void TestDownData(long id)
            {
                customerRepo.Setup(repo => repo.GetById(id, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                    .Returns((Customer)null);
                Console.WriteLine("GetCustomerByIdData Down Data ");
            }
        }
        
        [DataMethodPreparationFor(typeof(CustomerService), nameof(CustomerService.UpdateCustomer))]
        public class UpdateCustomerData(Mock<IRepository<Customer>> customerRepo)
        {
            [UpData]
            public void TestUpData()
            {
                customerRepo.Setup(repo => repo.Update(It.IsAny<Customer>())).Returns((Customer c) => c);
            }
            [DownData]
            public void TestDownData()
            {
            }
        }

    }
}
