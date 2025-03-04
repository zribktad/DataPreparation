using Boa.Constrictor.Screenplay;
using OrderService.Boa.OrderStatusService.Abilities;
using OrderService.DTO;
using OrderService.Services;

namespace OrderService.Boa.OrderStatusService.Tasks;

public class AddOrderStatusTask : ITask
{
    private readonly int _orderId;
    private readonly OrderStatusInputDTO _statusDto;
    public OrderStatusOutputDTO AddResult { get; private set; }

    public AddOrderStatusTask(int orderId, OrderStatusInputDTO statusDto)
    {
        _orderId = orderId;
        _statusDto = statusDto;
    }

    public void PerformAs(IActor actor)
    {
        var ability = actor.Using<UseOrderStatusService>();
        AddResult = ability.Service.AddOrderStatus(_orderId, _statusDto);
    }

    public static AddOrderStatusTask For(int orderId, OrderStatusInputDTO statusDto)
    {
        return new AddOrderStatusTask(orderId, statusDto);
    }
}