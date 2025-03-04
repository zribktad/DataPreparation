using DataPreparation.Data.Setup;
using OrderService.Models;

namespace OrderService.Boa.Factories.SQLite;

public class OrderRegisterAsync : IDataRegisterAsync<Order>
{
    OrderServiceContext _context;
    public OrderRegisterAsync(OrderServiceContext context )
    {
        _context = context;
    }
    public async Task<bool> Delete(long createId, Order data, IDataParams? args)
    {
        _context.Orders.Remove(data);
        await _context.SaveChangesAsync();
        return true;
    }
}