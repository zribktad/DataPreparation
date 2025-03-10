using DataPreparation.Data.Setup;
using DataPreparation.Provider;
using OrderService.Models;

namespace OrderService.Boa.Factories.SQLite;

public class CustomerFactoryAsync: IDataFactoryAsync<Customer>
{
    OrderServiceContext _context;
    public CustomerFactoryAsync(OrderServiceContext context )
    {
        context.Database.EnsureCreated();
        _context = context;
    }
    public async Task<Customer> Create(long id, IDataParams? args, CancellationToken token = default)
    {
       
        var factory = PreparationContext.GetFactory();
        var customer = new Customer {Name = $"Name {id}", Address =await factory.GetAsync<Address,AddressFactoryAsync>(token), Email ="@ " + id,Phone = id.ToString()};
        await  _context.Customers.AddAsync(customer, token);
        await _context.SaveChangesAsync(token);
        return customer;
    }
    
    public async Task<bool> Delete(long id, Customer data, IDataParams? args)
    {
         _context.Customers.Remove(data);
         await _context.SaveChangesAsync();
         return true;
    }
    
}