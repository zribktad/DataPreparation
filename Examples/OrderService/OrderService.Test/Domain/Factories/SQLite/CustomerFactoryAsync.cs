using DataPreparation.Data.Setup;
using DataPreparation.Provider;
using OrderService.Models;

namespace OrderService.BoaTest.Factories.SQLite;

public class CustomerFactoryAsync : IDataFactoryAsync<Customer>
{
    private OrderServiceContext _context;

    public CustomerFactoryAsync(OrderServiceContext context)
    {
        _context = context;
    }

    public async Task<Customer> Create(long id, IDataParams? args, CancellationToken token)
    {
        var factory = PreparationContext.GetFactory();
        var customer = new Customer
        {
            Name = $"Name {id}", Address = await factory.GetAsync<Address, AddressFactoryAsync>(token),
            Email = "@ " + id, Phone = id.ToString()
        };
        _context.Addresses.Attach(customer.Address);
        await _context.Customers.AddAsync(customer, token);
        await _context.SaveChangesAsync(token);
        return customer;
    }

    public async Task<bool> Delete(long id, Customer data, IDataParams? args)
    {
        var isTracked = _context.ChangeTracker.Entries<Customer>().Any(e => e.Entity == data);
        Console.WriteLine($"Is customer tracked? {isTracked}");

        _context.Customers.Remove(data);
        await _context.SaveChangesAsync();
        return true;
    }
}