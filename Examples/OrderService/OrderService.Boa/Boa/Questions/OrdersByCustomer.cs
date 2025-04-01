using Boa.Constrictor.Screenplay;
using OrderService.Boa.OrderService.Abilities;
using OrderService.Models;

namespace OrderService.Boa.OrderService.Questions;

public class OrdersByCustomer : IQuestion<IEnumerable<Order>>
{
    private readonly long _customerId;

    private OrdersByCustomer(long customerId)
    {
        _customerId = customerId;
    }

    public IEnumerable<Order> RequestAs(IActor actor)
    {
        var ability = actor.Using<UseOrderService>();
        return ability.Service.GetOrders().Where(o => o.CustomerId == _customerId);
    }

    public static OrdersByCustomer WithId(long customerId) => new OrdersByCustomer(customerId);
}