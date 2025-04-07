using Boa.Constrictor.Screenplay;
using OrderService.BoaTest.OrderStatusService.Abilities;
using OrderService.DTO;
using OrderService.Services;

namespace OrderService.BoaTest.OrderStatusService.Questions;

public class OrderStatusesForOrderId : IQuestion<IEnumerable<OrderStatusOutputDTO>>
{
    private readonly int _orderId;

    public OrderStatusesForOrderId(int orderId)
    {
        _orderId = orderId;
    }

    public IEnumerable<OrderStatusOutputDTO> RequestAs(IActor actor)
    {
        var ability = actor.Using<UseOrderStatusService>();
        return ability.Service.GetOrderStatuses(_orderId);
    }

    public static OrderStatusesForOrderId ForOrderId(int orderId)
    {
        return new OrderStatusesForOrderId(orderId);
    }
}