using DataPreparation.Data.Setup;
using DataPreparation.Factory.Testing;
using Moq;
using OrderService.Models;
using OrderService.Repository;

namespace OrderService.BoaTest.ShowCases.Factories;

public class CustomerMockRepositoryFactoryAsync: IDataFactoryAsync<IRepository<Customer>>
{
   
    public Task<IRepository<Customer>> Create(long createId, IDataParams? args, CancellationToken token = default)
    {
        var mockCustomerRepository = new Mock<IRepository<Customer>>();
        if(args?.Find<Customer>(out var customer, c => c.Id > 0 ) == true)
        {
            mockCustomerRepository
                .Setup(repo => repo.GetById(customer.Id, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
                .Returns(customer);
        }
        
        return Task.FromResult(mockCustomerRepository.Object);
    }

    public Task<bool> Delete(long createId, IRepository<Customer> data, IDataParams? args)
    {
      return Task.FromResult(true);
    }
    
}