using DataPreparation.Data.Setup;
using DataPreparation.Provider;
using OrderService.BoaTest.ShowCases.Factories;
using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Test.Domain.Factories.AsyncMock;

public class OrderDtoFactoryAsync : IDataFactoryAsync<OrderDTO>
{


    public async Task<OrderDTO> Create(long createId, IDataParams? args, CancellationToken token = default)
    {
        var factory = PreparationContext.GetFactory();
        return new OrderDTO()
        {
            OrderItems = await factory.GetAsync<OrderItem, OrderService.BoaTest.Factories.SQLite.OrderItemFactoryAsync>(2, token),
            CustomerId = (await factory.GetAsync<Customer, OrderService.BoaTest.Factories.SQLite.CustomerFactoryAsync>(token)).Id
        };

    }

    public Task<bool> Delete(long createId, OrderDTO data, IDataParams? args)
    {
        return Task.FromResult(true);
    }
}
    