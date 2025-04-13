using DataPreparation.Data.Setup;
using DataPreparation.Provider;
using OrderService.BoaTest.Factories.SQLite;
using OrderService.Models;

namespace OrderService.Test.Domain.Factories.SQLite;

public class OrderFactoryAsync(OrderServiceContext context) : IDataFactoryAsync<Order>
{
    public async Task<Order> Create(long id, IDataParams? args, CancellationToken token)
    {
        var factory = PreparationContext.GetFactory();
   
        var order = new Order
        {
            CustomerId =  (await factory.GetAsync<Customer, CustomerFactoryAsync>(token)).Id,
            OrderItems = await factory.GetAsync<OrderItem, OrderItemFactoryAsync>(2, token),
            OrderStatuses =  [
            
                new() { Status = Status.CREATED}
            ]
        };
        
        await context.Set<Order>().AddAsync(order, token);
        await context.SaveChangesAsync(token);
        return order;
    }

    public async Task<bool> Delete(long id, Order data, IDataParams? args)
    {
        context.Set<Order>().Remove(data);
       
        await context.SaveChangesAsync();
        return true;
    }
}