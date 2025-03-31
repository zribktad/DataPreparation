using DataPreparation.Data.Setup;
using OrderService.Models;

namespace OrderService.Boa.Factories.SQLite;

public class OrderItemFactoryAsync: IDataFactoryAsync<OrderItem>
{
    OrderServiceContext _context;
    public OrderItemFactoryAsync( OrderServiceContext context )
    {
        _context = context;
    }
    public Task<OrderItem> Create(long createId, IDataParams? args, CancellationToken token = default)
    {
        return Task.FromResult( new OrderItem() {ItemId = createId, Quantity = (int)createId});
    }

    public Task<bool> Delete(long createId, OrderItem data, IDataParams? args)
    {
        return Task.FromResult(true);
    }
    
}