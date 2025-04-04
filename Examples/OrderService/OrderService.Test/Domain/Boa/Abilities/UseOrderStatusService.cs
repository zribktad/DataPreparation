using Boa.Constrictor.Screenplay;
using DataPreparation.Provider;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Services;

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

    public static UseOrderStatusService FromDataPreparationProvider()
    {
        return new UseOrderStatusService(PreparationContext.GetProvider().GetRequiredService<IOrderStatusService>());
    }
}