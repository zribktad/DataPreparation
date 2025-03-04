using DataPreparation.Data.Setup;
using DataPreparation.Provider;
using OrderService.Models;

namespace OrderService.Boa.ShowCases.Factories;

public class CustomerFactoryAsync: IDataFactoryAsync<Customer>
{
    public Task<Customer> Create(long id, IDataParams? args, CancellationToken token = default)
    {
        var address = new Address() {City = "City", Street = "Street", PostalCode = "ZipCode"};
        return Task.FromResult(new Customer() {Id = id, Name = $"Name{id}", Address = address, Email = $"Email{id}", Phone = $"Phone{id}"});
    }
    
    public Task<bool> Delete(long id, Customer data, IDataParams? args)
    {
      return Task.FromResult(true);
    }
    
}