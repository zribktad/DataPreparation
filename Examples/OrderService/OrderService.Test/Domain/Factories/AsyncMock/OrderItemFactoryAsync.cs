using DataPreparation.Data.Setup;
using OrderService.Models;

namespace OrderService.BoaTest.ShowCases.Factories;

public class OrderItemFactoryAsync : IDataFactoryAsync<OrderItem>
{
    public Task<OrderItem> Create(long createId, IDataParams? args, CancellationToken token = default)
    {
        return Task.FromResult(new OrderItem() { Id = createId, ItemId = createId, Quantity = (int)createId });
    }

    public Task<bool> Delete(long createId, OrderItem data, IDataParams? args)
    {
        return Task.FromResult(true);
    }
}