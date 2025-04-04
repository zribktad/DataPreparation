using DataPreparation.Data.Setup;
using DataPreparation.Provider;
using OrderService.DTO;
using OrderService.Models;

namespace OrderService.BoaTest.ShowCases.Factories;

public class OrderDtoFactoryAsync : IDataFactoryAsync<OrderDTO>
{


    public async Task<OrderDTO> Create(long createId, IDataParams? args, CancellationToken token = default)
    {
        var factory = PreparationContext.GetFactory();
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
    