using Boa.Constrictor.Screenplay;

namespace OrderService.Boa.OrderStatusService.Abilities;

public class UseOrderStatusService : IAbility
{
    public Services.OrderStatusService Service { get; }

    public UseOrderStatusService(Services.OrderStatusService service)
    {
        Service = service;
    }

    public static UseOrderStatusService With(Services.OrderStatusService service)
    {
        return new UseOrderStatusService(service);
    }
}