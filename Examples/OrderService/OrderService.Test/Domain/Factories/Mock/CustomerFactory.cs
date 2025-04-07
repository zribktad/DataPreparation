using DataPreparation.Data.Setup;
using DataPreparation.Provider;
using OrderService.Models;

namespace OrderService.BoaTest.ShowCases.Factories;

public class CustomerFactory : IDataFactory<Customer>
{
    public Customer Create(long id, IDataParams? args)
    {
        var address = new Address() { City = "City", Street = "Street", PostalCode = "ZipCode" };
        return new Customer()
            { Id = id, Name = $"Name{id}", Address = address, Email = $"Email{id}", Phone = $"Phone{id}" };
    }

    public bool Delete(long id, Customer data, IDataParams? args)
    {
        return true;
    }
}