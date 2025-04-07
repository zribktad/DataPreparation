using Boa.Constrictor.Screenplay;
using OrderService.BoaTest.OrderService.Abilities;
using OrderService.Models;

namespace OrderService.BoaTest.OrderService.Questions;

public class OrdersByStatus : IQuestion<IEnumerable<Order>>
{
    private readonly Status _status;

    private OrdersByStatus(Status status)
    {
        _status = status;
    }

    public IEnumerable<Order> RequestAs(IActor actor)
    {
        var ability = actor.Using<UseOrderService>();
        return ability.Service.GetOrders().Where(o => o.OrderStatuses.LastOrDefault()?.Status == _status);
    }

    public static OrdersByStatus WithStatus(Status status)
    {
        return new OrdersByStatus(status);
    }
}