using Boa.Constrictor.Screenplay;
using OrderService.BoaTest.Factories.SQLite;
using OrderService.BoaTest.OrderService.Abilities;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Test.Domain.Boa.Abilities;

namespace OrderService.BoaTest.OrderService.Tasks;

public class CreateOrderAndRegisterTask : ITask//, IReturn<Order>
{
    private readonly OrderDTO _orderDto;
    public Order CreatedOrder { get; private set; }
    public CreateOrderAndRegisterTask(OrderDTO orderDto) => _orderDto = orderDto;
    
    public void PerformAs(IActor actor)
    {
        var ability = actor.Using<UseOrderService>();
        CreatedOrder = ability.Service.CreateOrder(_orderDto);
        var factoryAbility = actor.Using<UseSourceFactory>();
        
        factoryAbility.Factory.Register<Order, OrderRegisterAsync>(CreatedOrder, out _); //this is for data dependency
    }
    
    public Order GetResult() => CreatedOrder;
    public static CreateOrderAndRegisterTask For(OrderDTO orderDto) => new CreateOrderAndRegisterTask(orderDto);
}