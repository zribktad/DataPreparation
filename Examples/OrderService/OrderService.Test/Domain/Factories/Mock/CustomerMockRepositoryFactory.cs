using DataPreparation.Data.Setup;
using DataPreparation.Factory.Testing;
using Moq;
using OrderService.Models;
using OrderService.Repository;

namespace OrderService.BoaTest.ShowCases.Factories;

public class CustomerMockRepositoryFactory: IDataFactory<IRepository<Customer>>
{
    public IRepository<Customer> Create(long id, IDataParams? args)
    {
        var mockCustomerRepository = new Mock<IRepository<Customer>>();
        if(args?.Find<Customer>(out var customer, c => c.Id > 0 ) == true)
        {
            mockCustomerRepository
                .Setup(repo => repo.GetById(customer.Id, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                .Returns(customer);
        }
        
        return mockCustomerRepository.Object;
    }

    public bool Delete(long id, IRepository<Customer> data, IDataParams? args)
    {
        return true;
    }
}