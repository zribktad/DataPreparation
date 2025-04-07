using Boa.Constrictor.Screenplay;
using OrderService.BoaTest.OrderService.Abilities;
using OrderService.Models;

namespace OrderService.BoaTest.OrderService.Tasks;

public class UpdateOrderTask : ITask
{
    private readonly int _orderId;
    private readonly Order _updatedOrder;
    public bool UpdateResult { get; private set; }

    public UpdateOrderTask(int orderId, Order updatedOrder)
    {
        _orderId = orderId;
        _updatedOrder = updatedOrder;
    }

    public void PerformAs(IActor actor)
    {
        var ability = actor.Using<UseOrderService>();
        UpdateResult = ability.Service.UpdateOrder(_orderId, _updatedOrder);
    }

    public static UpdateOrderTask For(int orderId, Order updatedOrder)
    {
        return new UpdateOrderTask(orderId, updatedOrder);
    }
}