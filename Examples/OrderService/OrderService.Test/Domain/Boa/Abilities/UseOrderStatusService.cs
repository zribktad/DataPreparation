using Boa.Constrictor.Screenplay;

namespace OrderService.BoaTest.OrderStatusService.Abilities;

public class UseOrderStatusService : IAbility
{
    public Services.IOrderStatusService Service { get; }

    public UseOrderStatusService(Services.IOrderStatusService service)
    {
        Service = service;
    }

    public static UseOrderStatusService With(Services.IOrderStatusService service)
    {
        return new UseOrderStatusService(service);
    }
}