using Boa.Constrictor.Screenplay;
using OrderService.Boa.OrderService.Abilities;

namespace OrderService.Boa.Boa.Questions;

public class IsAllOrders(long size) : IQuestion<bool>
{
    public bool RequestAs(IActor actor)
    {
        var ability = actor.Using<UseOrderService>();
        var orders =  ability.Service.GetOrders();
        return orders.Count() == size;
        
    }
    public static IsAllOrders FromService( long size ) => new(size);
}