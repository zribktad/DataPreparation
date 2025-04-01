using Boa.Constrictor.Screenplay;
using OrderService.Boa.OrderService.Abilities;
using OrderService.Boa.OrderStatusService.Abilities;
using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Boa.OrderService.Tasks;

public class CancelOrderTask : ITask
{
    private readonly long _orderId;
    public bool CancelResult { get; private set; }

    private CancelOrderTask(long orderId)
    {
        _orderId = orderId;
    }

    public void PerformAs(IActor actor)
    {
        var ability = actor.Using<UseOrderStatusService>();
        var ret = ability.Service.AddOrderStatus(_orderId, new OrderStatusInputDTO(){OrderStatus = Status.CANCELED.ToString()});
        CancelResult =  ret.OrderStatus == Status.CANCELED.ToString();
     
    }

    public static CancelOrderTask For(long orderId) => new CancelOrderTask(orderId);
}