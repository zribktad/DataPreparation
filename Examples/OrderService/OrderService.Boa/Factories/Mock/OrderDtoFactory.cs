using DataPreparation.Data.Setup;
using DataPreparation.Provider;
using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Boa.ShowCases.Factories;

public class OrderDtoFactory: IDataFactory<OrderDTO>
{
    public OrderDTO Create(long id, IDataParams? args)
    { var factory = PreparationContext.GetFactory();
       return new OrderDTO(){OrderItems = factory.Get<OrderItem,OrderItemFactory>(2),CustomerId = factory.Get<Customer,CustomerFactory>().Id};
    }

    public bool Delete(long id, OrderDTO data, IDataParams? args)
    {
        return true;
    }
}