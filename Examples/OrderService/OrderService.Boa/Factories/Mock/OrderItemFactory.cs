using DataPreparation.Data.Setup;
using OrderService.Models;

namespace OrderService.Boa.ShowCases.Factories;

public class OrderItemFactory: IDataFactory<OrderItem>
{

    public OrderItem Create(long id, IDataParams? args)
    {
        return new OrderItem() {Id = id, ItemId = id, Quantity = (int)id};
    }

    public bool Delete(long id, OrderItem data, IDataParams? args)
    {
        return true;
    }
}