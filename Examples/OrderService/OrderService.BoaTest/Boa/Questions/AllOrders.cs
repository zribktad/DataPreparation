using Boa.Constrictor.Screenplay;
using OrderService.BoaTest.OrderService.Abilities;
using OrderService.Models;

namespace OrderService.BoaTest.OrderService.Questions;

public class AllOrders : IQuestion<IEnumerable<Order>>
{
    public IEnumerable<Order> RequestAs(IActor actor)
    {
        var ability = actor.Using<UseOrderService>();
        return ability.Service.GetOrders();
    }
    public static AllOrders FromService() => new();
}