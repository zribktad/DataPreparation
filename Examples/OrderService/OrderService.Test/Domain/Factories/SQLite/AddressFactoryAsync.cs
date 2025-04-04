using DataPreparation.Data.Setup;
using DataPreparation.Provider;
using OrderService.Models;

namespace OrderService.BoaTest.Factories.SQLite;

public class AddressFactoryAsync: IDataFactoryAsync<Address>
{
    readonly OrderServiceContext _context;
    public AddressFactoryAsync(OrderServiceContext context )
    {
        _context = context;
    }

    public async Task<Address> Create(long createId, IDataParams? args, CancellationToken token = default)
    {
        Address address = new() {City = $"City {createId}", Street = $"Street {createId}", PostalCode = $"ZipCode {createId}"};
        await  _context.Addresses.AddAsync(address, token);
        await _context.SaveChangesAsync(token);
        return address;
    }

    public async Task<bool> Delete(long createId, Address data, IDataParams? args)
    {
       _context.Addresses.Remove(data);
       await _context.SaveChangesAsync();
       return true;
    }
}
