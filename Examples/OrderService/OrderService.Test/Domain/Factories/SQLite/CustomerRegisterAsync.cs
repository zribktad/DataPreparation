using DataPreparation.Data.Setup;
using OrderService.Models;

namespace OrderService.BoaTest.Factories.SQLite;

public class CustomerRegisterAsync : IDataRegisterAsync<Customer>
{
    private OrderServiceContext _context;

    public CustomerRegisterAsync(OrderServiceContext context)
    {
        _context = context;
    }

    public async Task<bool> Delete(long createId, Customer data, IDataParams? args)
    {
        _context.Customers.Remove(data);
        await _context.SaveChangesAsync();
        return true;
    }
}