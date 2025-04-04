using Boa.Constrictor.Screenplay;
using OrderService.BoaTest.OrderService.Abilities;
using OrderService.Models;

namespace OrderService.BoaTest.OrderService.Questions;

public class OrderById(long orderId) : IQuestion<Order>
{
    public Order RequestAs(IActor actor)
    {
        var ability = actor.Using<UseOrderService>();
        return ability.Service.GetOrder(orderId);
    }
    public static OrderById WithId(long orderId) => new OrderById(orderId);
}