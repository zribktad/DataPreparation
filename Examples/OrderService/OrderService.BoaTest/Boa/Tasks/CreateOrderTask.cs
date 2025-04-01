using Boa.Constrictor.Screenplay;
using OrderService.BoaTest.OrderService.Abilities;
using OrderService.DTO;
using OrderService.Models;

namespace OrderService.BoaTest.OrderService.Tasks;

public class CreateOrderTask : ITask//, IReturn<Order>
{
    private readonly OrderDTO _orderDto;
    public Order CreatedOrder { get; private set; }
    public CreateOrderTask(OrderDTO orderDto) => _orderDto = orderDto;
    
    public void PerformAs(IActor actor)
    {
        var ability = actor.Using<UseOrderService>();
        CreatedOrder = ability.Service.CreateOrder(_orderDto);
        
    }
    
    public Order GetResult() => CreatedOrder;
    public static CreateOrderTask For(OrderDTO orderDto) => new CreateOrderTask(orderDto);
}