using DataPreparation.Data.Setup;
using DataPreparation.Factory.Testing;
using DataPreparation.Provider;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;

namespace OrderService.BoaTest.ShowCases.Factories;

public class OrderServiceFactory: IDataFactory<Services.OrderService>
{
    public bool Delete(long createId, Services.OrderService data, IDataParams? args)
    {
        return true;
    }

    public Services.OrderService Create(long createId, IDataParams? args)
    {
        var sourceFactory = PreparationContext.GetFactory();
        OrderDTO? orderDto = null;
        Customer? customer = null;
        args?.Find<OrderDTO>(out orderDto);
        args?.Find<Customer>(out  customer);
        return new Services.OrderService(
            sourceFactory.New<IRepository<Order>, OrderMockRepositoryFactory>(new ObjectParam(orderDto)), 
            sourceFactory.New<IRepository<Customer>,CustomerMockRepositoryFactory>(new ObjectParam(customer)));
    }
}