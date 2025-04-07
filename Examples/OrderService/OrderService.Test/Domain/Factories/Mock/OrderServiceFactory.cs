using DataPreparation.Data.Setup;
using DataPreparation.Factory.Testing;
using DataPreparation.Provider;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;

namespace OrderService.BoaTest.ShowCases.Factories;

public class OrderServiceFactory : IDataFactory<Services.OrderService>
{
    public bool Delete(long createId, Services.OrderService data, IDataParams? args)
    {
        return true;
    }

    public Services.OrderService Create(long createId, IDataParams? args)
    {
        var sourceFactory = PreparationContext.GetFactory();
        
        if (args?.Find(out OrderDTO? orderDto) != true)
        {
            orderDto = sourceFactory.Get<OrderDTO, OrderDtoFactory>();
        }

        if (args?.Find(out Customer? customer) != true)
        {
            customer = sourceFactory.Get<Customer, CustomerFactory>();
        }
        
        return new Services.OrderService(
            sourceFactory.New<IRepository<Order>, OrderMockRepositoryFactory>(new ObjectParam(orderDto)),
            sourceFactory.New<IRepository<Customer>, CustomerMockRepositoryFactory>(new ObjectParam(customer)));
    }
}