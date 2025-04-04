using DataPreparation.Data.Factory;
using DataPreparation.Data.Setup;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using OrderService.DTO;
using OrderService.Models;

namespace OrderService.BoaTest.Factories.SQLite;


[FactoryLifetime(ServiceLifetime.Scoped)]
public class CustomerDtoFactory: IDataFactory<CustomerDTO>
{
    public CustomerDtoFactory()
    {
        
    }
    
    public CustomerDtoFactory(OrderServiceContext context)
    {
    }
    public bool Delete(long createId, CustomerDTO data, IDataParams? args)
    {
        return true;
    }

    public CustomerDTO Create(long createId, IDataParams? args)
    {
        
        Address address = args?.Find<Address>(out var retAddress) == true ? retAddress : new Address() {City = "City", Street = "Street", PostalCode = "ZipCode"};
        return new CustomerDTO()
        {
            Name = $"Name {createId}",
            Address = address,
            Email = $"Email {createId}",
            Phone = $"Phone {createId}"
        };
    }
}