using DataPreparation.Data.Setup;
using DataPreparation.Provider;
using OrderService.BoaTest.Factories.SQLite;
using OrderService.Models;

namespace OrderService.Test.Domain.Factories.SQLite;

public class CustomerFactoryAsync(OrderServiceContext context) : IDataFactoryAsync<Customer>
{
    public async Task<Customer> Create(long id, IDataParams? args, CancellationToken token)
    {
        var factory = PreparationContext.GetFactory();
        var customer = new Customer
        {
            Name = $"Name {id}", Address = await factory.GetAsync<Address, AddressFactoryAsync>(token),
            Email = "@ " + id, Phone = id.ToString()
        };
        context.Addresses.Attach(customer.Address);
        await context.Customers.AddAsync(customer, token);
        await context.SaveChangesAsync(token);
        return customer;
    }

    public async Task<bool> Delete(long id, Customer data, IDataParams? args)
    {
        context.Customers.Remove(data);
        await context.SaveChangesAsync();
        return true;
    }
}