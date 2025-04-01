using Boa.Constrictor.Screenplay;
using OrderService.Services;

namespace OrderService.Boa.OrderService.Abilities;

public class UseOrderItemService : IAbility
{
    public IOrderItemService Service { get; }

    private UseOrderItemService(IOrderItemService service)
    {
        Service = service;
    }

    public static UseOrderItemService With(IOrderItemService service)
    {
        return new UseOrderItemService(service);
    }
}