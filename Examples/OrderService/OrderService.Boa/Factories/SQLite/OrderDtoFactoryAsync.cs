using DataPreparation.Data.Setup;
using DataPreparation.Provider;
using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Boa.Factories.SQLite;

public class OrderDtoFactoryAsync : IDataFactoryAsync<OrderDTO>
{
    
    public async Task<OrderDTO> Create(long createId, IDataParams? args, CancellationToken token = default)
    {
        var factory = PreparationContext.GetFactory();
        //var customerId = args?.Find<Customer>(out var result) == true ? result.Id :  (await factory.GetAsync<Customer, CustomerFactoryAsync>(token)).Id;
        return new OrderDTO()
        {
            OrderItems = await factory.GetAsync<OrderItem, OrderItemFactoryAsync>(2, token),
            CustomerId = (await factory.GetAsync<Customer, CustomerFactoryAsync>(token)).Id
        };

    }

    public Task<bool> Delete(long createId, OrderDTO data, IDataParams? args)
    {
        return Task.FromResult(true);
    }
}
    