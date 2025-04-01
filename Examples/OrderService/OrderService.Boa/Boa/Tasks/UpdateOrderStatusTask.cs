using Boa.Constrictor.Screenplay;
using OrderService.Boa.OrderService.Abilities;
using OrderService.Boa.OrderStatusService.Abilities;
using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Boa.OrderService.Tasks;

public class UpdateOrderStatusTask : ITask
{
    private readonly long _orderId;
    private readonly Status _status;
    public bool UpdateResult { get; private set; }

    private UpdateOrderStatusTask(long orderId, Status status)
    {
        _orderId = orderId;
        _status = status;
    }

    public void PerformAs(IActor actor)
    {
        var ability = actor.Using<UseOrderStatusService>();
        var status = new OrderStatusInputDTO() { OrderStatus = _status.ToString() };
        var res = ability.Service.AddOrderStatus(_orderId, status);
        UpdateResult = res.OrderStatus == _status.ToString();
    }

    public static UpdateOrderStatusTask For(long orderId, Status status) => new UpdateOrderStatusTask(orderId, status);
}