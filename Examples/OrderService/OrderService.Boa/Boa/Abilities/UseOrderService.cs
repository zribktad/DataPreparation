using Boa.Constrictor.Screenplay;

namespace OrderService.Boa.OrderService.Abilities;

public class UseOrderService(Services.IOrderService service) : IAbility
{
    public Services.IOrderService Service { get; } = service;
    public static UseOrderService With(Services.IOrderService service) => new UseOrderService(service);
}